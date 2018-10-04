using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gibbed.IO;
using TF.Core.Entities;

namespace TF.Core.Projects.Yakuza0.Files
{
    public class BarFile : TFFile
    {
        private class ItemData
        {
            public int[] Unknown;
            public List<TFString> Data;

            public ItemData()
            {
                Unknown = new int[22];
                Data = new List<TFString>();
            }
        }

        public BarFile(string fileName) : base(fileName)
        {
            
        }

        public override Endian Endianness => Endian.Big;
        public override Encoding Encoding => Encoding.UTF8;
        public override string FileType => "bar000x.bin";
        
        public override IList<TFString> Strings {
            get
            {
                var result = new List<TFString>();

                foreach (var item in _dataList1)
                {
                    result.Add(item);
                }

                result.Add(_barkeeper);

                foreach (var item in _dataList2)
                {
                    for (var i = 0; i < item.Data.Count; i++)
                    {
                        result.Add(item.Data[i]);
                    }
                }
            
                return result;
            }
        }

        private int _unknown1;
        private short _unknown2;
        private short _num;
        private int _unknown3;
        private int _table2Offset;
        private byte[] _remainder1;
        private byte[] _remainder2;

        private List<TFString> _dataList1;
        private TFString _barkeeper;
        private List<ItemData> _dataList2;

        public override void Read(Stream s)
        {
            _unknown1 = s.ReadValueS32(Endianness);
            _unknown2 = s.ReadValueS16(Endianness);
            _num = s.ReadValueS16(Endianness);
            _unknown3 = s.ReadValueS32(Endianness);
            _table2Offset = s.ReadValueS32(Endianness);

            _dataList1 = new List<TFString>();
            _dataList2 = new List<ItemData>();

            var firstString = -1;

            for (var i = 0; i < 43; i++)
            {
                var str = ReadString(s);
                _dataList1.Add(str);

                if (firstString == -1)
                {
                    firstString = str.Offset;
                }
            }

            _remainder1 = s.ReadBytes(0x54);

            s.Seek(-8, SeekOrigin.Current);
            _barkeeper = ReadString(s);
            s.Seek(4, SeekOrigin.Current);

            for (var i = 0; i < _num; i++)
            {
                var item = new ItemData();
                for (var j = 0; j < 22; j++)
                {
                    item.Unknown[j] = s.ReadValueS32(Endianness);
                }

                if (firstString == -1)
                {
                    firstString = item.Unknown[8];
                }

                item.Data.Add(ReadString(s, item.Unknown[8]));
                item.Data.Add(ReadString(s, item.Unknown[14]));
                item.Data.Add(ReadString(s, item.Unknown[15]));
                item.Data.Add(ReadString(s, item.Unknown[16]));
                item.Data.Add(ReadString(s, item.Unknown[17]));
                item.Data.Add(ReadString(s, item.Unknown[18]));
                item.Data.Add(ReadString(s, item.Unknown[19]));
                item.Data.Add(ReadString(s, item.Unknown[20]));
                item.Data.Add(ReadString(s, item.Unknown[21]));
                
                _dataList2.Add(item);
            }

            var remainderSize = (int) (firstString - s.Position);

            if (remainderSize > 0)
            {
                _remainder2 = ReadRemainder(s, remainderSize);
            }
        }

        private TFString ReadString(Stream s)
        {
            var stringOffset = s.ReadValueS32(Endianness);
            var pos = s.Position;

            var value = new TFString {FileId = Id, Offset = stringOffset, Visible = false};

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

        private TFString ReadString(Stream s, int offset)
        {
            var pos = s.Position;

            var value = new TFString {FileId = Id, Offset = offset, Visible = false};

            if (offset != 0)
            {
                s.Seek(offset, SeekOrigin.Begin);

                var str = s.ReadStringZ(Encoding);

                if (!string.IsNullOrEmpty(str.TrimEnd('\0')))
                {
                    str = Yakuza0Project.ReadingReplacements(str);

                    value.Original = str;
                    value.Translation = str;
                    value.Visible = true;
                }
            }
            else
            {
                value.Visible = false;
            }

            s.Seek(pos, SeekOrigin.Begin);

            return value;
        }

        private byte[] ReadRemainder(Stream s, int size)
        {
            var pos = s.Position;

            var result = s.ReadBytes(size);

            s.Seek(pos, SeekOrigin.Begin);

            return result;
        }

        public override void Save(string fileName, byte[] originalContent, IList<TFString> strings, ExportOptions options)
        {
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                fs.WriteValueS32(_unknown1, Endianness);
                fs.WriteValueS16(_unknown2, Endianness);
                fs.WriteValueS16(_num, Endianness);
                fs.WriteValueS32(_unknown3, Endianness);
                fs.WriteValueS32(_table2Offset, Endianness);

                var r2Size = 0;
                if (_remainder2 != null)
                {
                    r2Size = _remainder2.Length;
                }
                
                var currentOffset = 16 + _dataList1.Count * 4 + _remainder1.Length + _dataList2.Count * 88 + r2Size;
                
                for (var i = 0; i < _dataList1.Count; i++)
                {
                    var item = _dataList1[i];
                    
                    fs.Seek(16 + i * 4, SeekOrigin.Begin);
                    if (item.Offset == 0)
                    {
                        fs.WriteValueS32(0);
                    }
                    else
                    {
                        fs.WriteValueS32(currentOffset, Endianness);
                        currentOffset = WriteString(fs, currentOffset, item, strings, options);
                    }
                }

                fs.Seek(16 + _dataList1.Count * 4, SeekOrigin.Begin);
                fs.WriteBytes(_remainder1);

                fs.Seek(-8, SeekOrigin.Current);
                fs.WriteValueS32(currentOffset, Endianness);
                currentOffset = WriteString(fs, currentOffset, _barkeeper, strings, options);
                fs.Seek(4, SeekOrigin.Current);

                for (var i = 0; i < _dataList2.Count; i++)
                {
                    var item = _dataList2[i];
                    
                    fs.Seek(_table2Offset + i * 88, SeekOrigin.Begin);

                    for (var j = 0; j < 8; j++)
                    {
                        fs.WriteValueS32(item.Unknown[j], Endianness);
                    }

                    currentOffset = WriteItem(fs, currentOffset, item.Data[0], strings, options);

                    for (var j = 9; j < 14; j++)
                    {
                        fs.WriteValueS32(item.Unknown[j], Endianness);
                    }

                    for (var j = 0; j < 8; j++)
                    {
                        currentOffset = WriteItem(fs, currentOffset, item.Data[j + 1], strings, options);
                    }
                }

                if (_remainder2 != null)
                {
                    fs.Seek(_table2Offset + _num * 48, SeekOrigin.Begin);
                    fs.WriteBytes(_remainder2);
                }
            }
        }

        private int WriteItem(Stream s, int offset, TFString item, IList<TFString> strings, ExportOptions options)
        {
            var result = offset;
            if (item.Offset != 0)
            {
                s.WriteValueS32(result, Endianness);
                result = WriteString(s, result, item, strings, options);
            }
            else
            {
                s.WriteValueS32(0);
            }

            return result;
        }

        private int WriteString(Stream s, int offset, TFString item, IList<TFString> strings, ExportOptions options)
        {
            var pos = s.Position;

            var result = offset;

            var tfString = strings.FirstOrDefault(x => x.Offset == item.Offset);

            s.Seek(offset, SeekOrigin.Begin);

            var str = tfString.Translation;

            if (!string.IsNullOrEmpty(str))
            {
                if (!str.Equals(tfString.Original))
                {
                    if (options.CharReplacement != 0)
                    {
                        str = Utils.ReplaceChars(str, options.CharReplacementList);
                    }

                    str = Yakuza0Project.WritingReplacements(str);

                    s.WriteStringZ(str, options.SelectedEncoding);

                    result += str.GetLength(options.SelectedEncoding) + 1;
                }
                else
                {
                    str = Yakuza0Project.WritingReplacements(str);

                    s.WriteStringZ(str, Encoding);

                    result += str.GetLength(Encoding) + 1;
                }
            }
            else
            {
                // Hay que escribir solo el 0 del final
                s.WriteString("\0");
                result++;
            }

            s.Seek(pos, SeekOrigin.Begin);
            return result;
        }
    }
}