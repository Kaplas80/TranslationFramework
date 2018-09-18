using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gibbed.IO;
using TF.Core.Entities;

namespace TF.Core.Projects.Yakuza0.Files
{
    public class StringTblFile : TFFile
    {
        private class DataItem
        {
            public int DataCount;
            public List<TFString> Data;
        }

        private int _elementCount;
        private int _unknown;

        private List<DataItem> _dataList;

        public StringTblFile(string fileName) : base(fileName)
        {
            
        }

        public override Endian Endianness => Endian.Big;
        public override Encoding Encoding => Encoding.UTF8;

        public override IList<TFString> Strings {
            get
            {
                var result = new List<TFString>();

                foreach (var item in _dataList)
                {
                    for (var i = 0; i<item.DataCount; i++)
                    {
                        result.Add(item.Data[i]);
                    }
                }
            
                return result;
            }
        }

        public override string FileType => "string_tbl.bin";
        
        public override void Read()
        {
            using (var fs = new FileStream(Path, FileMode.Open))
            {
                ReadHeader(fs);
                
                ReadDataItems(fs);
            }
        }

        private void ReadHeader(Stream s)
        {
            _elementCount = s.ReadValueS32(Endianness);
            _unknown = s.ReadValueS32(Endianness);
        }

        private void ReadDataItems(Stream s)
        {
            _dataList = new List<DataItem>();
            var i = 0;
            while (i < _elementCount)
            {
                var dataItem = ReadDataItem(s);

                _dataList.Add(dataItem);
                i++;
            }
        }

        private DataItem ReadDataItem(Stream s)
        {
            var dataItem = new DataItem
            {
                DataCount = s.ReadValueS32(Endianness), 
                Data = new List<TFString>()
            };

            ReadDataItemStrings(dataItem, s);

            return dataItem;
        }

        private void ReadDataItemStrings(DataItem dataItem, Stream s)
        {
            var pointer = s.ReadValueS32(Endianness);
                
            var pos = s.Position;
                
            s.Seek(pointer, SeekOrigin.Begin);
            var j = 0;
            while (j < dataItem.DataCount)
            {
                var value = ReadString(s);
                value.Section = pointer.ToString("X8");

                dataItem.Data.Add(value);

                j++;
            }

            s.Seek(pos, SeekOrigin.Begin);
        }

        private TFString ReadString(Stream s)
        {
            var stringOffset = s.ReadValueS32(Endianness);
            var pos = s.Position;

            var value = new TFString {Offset = stringOffset, Visible = false};

            if (stringOffset != 0)
            {
                s.Seek(stringOffset, SeekOrigin.Begin);

                var str = s.ReadStringZ(Encoding);

                if (!string.IsNullOrEmpty(str.TrimEnd('\0')))
                {
                    str = Yakuza0Project.ReadingReplacements(str);

                    value.Original = str;
                    value.Translation = str;
                    value.Visible = true;
                }
            }

            s.Seek(pos, SeekOrigin.Begin);

            return value;
        }

        public override void Save(string fileName, IList<TFString> strings, ExportOptions options)
        {
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                WriteHeader(fs);

                WriteDataItems(fs, strings, options);
            }
        }

        private void WriteHeader(Stream s)
        {
            s.WriteValueS32(_elementCount, Endianness);
            s.WriteValueS32(_unknown, Endianness);
        }

        private void WriteDataItems(Stream s, IList<TFString> strings, ExportOptions options)
        {
            var strCount = _dataList.Sum(x => x.DataCount);
            
            var offset = 8 + _dataList.Count * 8; // Primer valor de la primera indireccion
            var offset2 = offset + strCount * 4; // Primer valor de la segunda indireccion

            var stringIndex = 0;
            foreach (var item in _dataList)
            {
                var pos = s.Position;

                s.WriteValueS32(item.DataCount, Endianness);
                s.WriteValueS32(offset, Endianness);
                
                s.Seek(offset, SeekOrigin.Begin);
                
                for (var i = 0; i < item.DataCount; i++)
                {
                    if (item.Data[i].Offset == 0)
                    {
                        s.WriteValueS32(0, Endianness);
                    }
                    else
                    {
                        s.WriteValueS32(offset2, Endianness);
                        s.Seek(offset2, SeekOrigin.Begin);

                        var str = strings[stringIndex].Translation;

                        if (!string.IsNullOrEmpty(str))
                        {
                            if (options.CharReplacement != 0)
                            {
                                str = Utils.ReplaceChars(str, options.CharReplacementList);
                            }

                            str = Yakuza0Project.WritingReplacements(str);

                            s.WriteStringZ(str, options.SelectedEncoding);

                            offset2 += str.GetLength(options.SelectedEncoding) + 1;
                        }
                        else
                        {
                            // Hay que escribir solo el 0 del final
                            s.WriteString("\0");
                            offset2++;
                        }
                    }

                    s.Seek(offset + (i + 1) * 4, SeekOrigin.Begin);
                    stringIndex++;
                }

                offset = offset + item.DataCount * 4;
                s.Seek(pos + 8, SeekOrigin.Begin);
            }
        }
    }
}
