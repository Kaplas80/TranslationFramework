using System.Collections.Generic;
using System.IO;
using System.Text;
using Gibbed.IO;
using TF.Core.Entities;

namespace TF.Core.Projects.Shenmue.Files
{
    public class SubFile : TFFile
    {
        private class DataIndex
        {
            public byte[] Id;
            public byte[] Id2;
            public int StringOffset;
        }

        private class DataItem
        {
            public DataIndex Index;
            public TFString Data;

            public DataItem()
            {
                Index = new DataIndex();
                Data = new TFString();
            }
        }

        private int _signature;

        private List<DataItem> _dataList;

        public SubFile(string fileName) : base(fileName)
        {
            
        }

        public override Endian Endianness => Endian.Little;
        public override Encoding Encoding => Encoding.UTF8;

        public override IList<TFString> Strings {
            get
            {
                var result = new List<TFString>();

                foreach (var item in _dataList)
                {
                    result.Add(item.Data);
                }
            
                return result;
            }
        }

        public override string FileType => "Shenmue Sub";
        
        public override void Read(Stream s)
        {
            ReadDataItems(s);
        }

        private void ReadDataItems(Stream s)
        {
            _signature = s.ReadValueS32(Endianness);
            var elementCount = s.ReadValueS32(Endianness);

            var indexBase = 16;
            var stringBase = indexBase + elementCount * 0x1C;
            _dataList = new List<DataItem>();
            var i = 0;
            while (i < elementCount)
            {
                s.Seek(indexBase + i * 0x1C, SeekOrigin.Begin);
                var item = new DataItem();
                item.Index.Id = s.ReadBytes(0xC);
                item.Index.Id2 = s.ReadBytes(0xC);
                item.Index.StringOffset = s.ReadValueS32(Endianness);

                s.Seek(stringBase + item.Index.StringOffset, SeekOrigin.Begin);

                item.Data = ReadString(s);

                _dataList.Add(item);
                i++;
            }
        }

        private TFString ReadString(Stream s)
        {
            var stringOffset = (int) s.Position;
            
            var value = new TFString {FileId = Id, Offset = stringOffset, Visible = false};

            if (stringOffset != 0)
            {
                s.Seek(stringOffset, SeekOrigin.Begin);

                var str = s.ReadStringZ(Encoding);

                if (!string.IsNullOrEmpty(str.TrimEnd('\0')))
                {
                    str = ShenmueProject.ReadingReplacements(str);
                }

                value.Original = str;
                value.Translation = str;
                value.Visible = !string.IsNullOrWhiteSpace(str);
            }

            return value;
        }

        public override void Save(string fileName, byte[] originalContent, IList<TFString> strings, ExportOptions options)
        {
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                WriteDataItems(fs, strings, options);
            }
        }

        private void WriteDataItems(Stream s, IList<TFString> strings, ExportOptions options)
        {
            var elementCount = _dataList.Count;
            s.WriteValueS32(_signature, Endianness);
            s.WriteValueS32(elementCount, Endianness);
            s.Seek(8, SeekOrigin.Current);
            
            var indexBase = 16;
            var stringBase = indexBase + elementCount * 0x1C;
            var stringOffset = 0;

            for (var i = 0; i < elementCount; i++)
            {
                var item = _dataList[i];
                s.Seek(indexBase + i * 0x1C, SeekOrigin.Begin);
                s.WriteBytes(item.Index.Id);
                s.WriteBytes(item.Index.Id2);
                s.WriteValueS32(stringOffset);

                s.Seek(stringBase + stringOffset, SeekOrigin.Begin);

                string str;
                if (strings[i].Visible)
                {
                    str = strings[i].Translation;

                    if (options.CharReplacement != 0)
                    {
                        str = Utils.ReplaceChars(str, options.CharReplacementList);
                    }

                    str = ShenmueProject.WritingReplacements(str);
                }
                else
                {
                    str = item.Data.Original;
                }

                var bytes = options.SelectedEncoding.GetBytes(str);
                var strLength = bytes.Length;

                for (var j = 0; j < strLength; j++)
                {
                    if (bytes[j] == '\r')
                    {
                        bytes[j] = 0xA1;
                    }

                    if (bytes[j] == '\n')
                    {
                        bytes[j] = 0xF5;
                    }
                }

                s.WriteBytes(bytes);
                s.WriteByte(0);

                stringOffset += strLength + 1;
            }
        }
    }
}
