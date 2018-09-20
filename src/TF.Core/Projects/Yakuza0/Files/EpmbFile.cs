using System.Collections.Generic;
using System.IO;
using System.Text;
using Gibbed.IO;
using TF.Core.Entities;

namespace TF.Core.Projects.Yakuza0.Files
{
    public class EpmbFile : TFFile
    {
        private class DataItem
        {
            public TFString Data;
            public int AdditionalValue;
        }

        private int _signature;
        private int _unknown1;
        private int _unknown2;
        private int _unknown3;
        private int _itemCount;

        private List<DataItem> _dataList;

        public EpmbFile(string fileName) : base(fileName)
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
                    result.Add(item.Data);
                }
            
                return result;
            }
        }

        public override string FileType => "EPMB";
        
        public override void Read()
        {
            using (var fs = new FileStream(Path, FileMode.Open))
            {
                ReadHeader(fs);
                
                ReadData(fs);
            }
        }

        private void ReadHeader(Stream s)
        {
            _signature = s.ReadValueS32(Endianness); 
            _unknown1 = s.ReadValueS32(Endianness); // 65536
            _unknown2 = s.ReadValueS32(Endianness); // 1
            _unknown3 = s.ReadValueS32(Endianness); // 0
            _itemCount = s.ReadValueS32(Endianness);
            
        }

        private void ReadData(Stream s)
        {
            _dataList = new List<DataItem>(_itemCount);

            for (var i = 0; i < _itemCount; i++)
            {
                var key = s.ReadValueS32(Endianness);

                var pos = s.Position;

                var tfString = GetString(s);
                var item = new DataItem {Data = tfString, AdditionalValue = key};
                _dataList.Add(item);
                
                s.Seek(pos + 64, SeekOrigin.Begin);
            }
        }

        private TFString GetString(Stream s)
        {
            var pos = s.Position;
            var str = s.ReadStringZ(Encoding);
            str = Yakuza0Project.ReadingReplacements(str);

            var tfString = new TFString
            {
                FileId = Id,
                Original = str,
                Translation = str,
                Offset = (int) pos,
                Section = string.Empty,
                Visible = !string.IsNullOrWhiteSpace(str)
            };

            return tfString;
        }

        public override void Save(string fileName, IList<TFString> strings, ExportOptions options)
        {
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                WriteHeader(fs);

                WriteData(fs, strings, options);
            }
        }

        private void WriteHeader(Stream s)
        {
            s.WriteValueS32(_signature, Endianness);
            s.WriteValueS32(_unknown1, Endianness);
            s.WriteValueS32(_unknown2, Endianness);
            s.WriteValueS32(_unknown3, Endianness);
            s.WriteValueS32(_itemCount, Endianness);
        }

        private void WriteData(Stream s, IList<TFString> strings, ExportOptions options)
        {
            for (var i = 0; i < _itemCount; i++)
            {
                var item = _dataList[i];
                s.WriteValueS32(item.AdditionalValue, Endianness);
                WriteString(s, strings[i].Translation, options);
            }
        }

        private static void WriteString(Stream s, string str, ExportOptions options)
        {
            if (options.CharReplacement != 0)
            {
                str = Utils.ReplaceChars(str, options.CharReplacementList);
            }
            str = Yakuza0Project.WritingReplacements(str);

            if (str.GetLength(options.SelectedEncoding) >= 64)
            {
                str = str.Substring(0, 63);
            }

            s.WriteStringZ(str, options.SelectedEncoding);

            var zeros = new byte[64 - (str.GetLength(options.SelectedEncoding) + 1)];
            s.WriteBytes(zeros);
        }
    }
}
