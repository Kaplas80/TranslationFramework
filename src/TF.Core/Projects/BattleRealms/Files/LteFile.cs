using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gibbed.IO;
using TF.Core.Entities;

namespace TF.Core.Projects.BattleRealms.Files
{
    public class LteFile : TFFile
    {
        private class ItemData
        {
            public int Index;
            public byte Unknown;
            public int Length;
            public TFString Data;
        }

        private int _header;
        private List<ItemData> _strings;

        public LteFile(string fileName) : base(fileName)
        {
            _strings = new List<ItemData>();
        }

        public override Endian Endianness => Endian.Little;
        public override Encoding Encoding => Encoding.GetEncoding("Unicode");

        public override string FileType => "Battle Realm LTE";
        
        public override IList<TFString> Strings {
            get
            {
                var result = new List<TFString>();

                foreach (var item in _strings)
                {
                    result.Add(item.Data);
                }
            
                return result;
            }
        }

        public override void Read(Stream s)
        {
            _header = s.ReadValueS32(Endianness);
            var lineCount = s.ReadValueS32(Endianness);
            
            for (var i = 0; i < lineCount; i++)
            {
                var str = new ItemData
                {
                    Index = s.ReadValueS32(Endianness), 
                    Unknown = s.ReadValueU8(), 
                    Length = s.ReadValueS32(Endianness),
                    Data = ReadString(s)
                };

                _strings.Add(str);
            }
        }

        private TFString ReadString(Stream s)
        {
            var value = new TFString {FileId = Id, Offset = (int)s.Position, Visible = false};

            var str = s.ReadStringZ(Encoding);

            if (!string.IsNullOrEmpty(str.TrimEnd('\0')))
            {
                str = BattleRealmsProject.ReadingReplacements(str);

                value.Original = str;
                value.Translation = str;
                value.Visible = true;
            }

            return value;
        }
        
        public override void Save(string fileName, byte[] originalContent, IList<TFString> strings,
            ExportOptions options)
        {
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                fs.WriteValueS32(_header, Endianness);
                fs.WriteValueS32(strings.Count, Endianness);


                foreach (var item in _strings)
                {
                    fs.WriteValueS32(item.Index, Endianness);
                    fs.WriteValueU8(item.Unknown);

                    var tfString = strings.FirstOrDefault(x => x.Offset == item.Data.Offset);

                    var str = tfString.Translation;

                    if (!string.IsNullOrEmpty(str))
                    {
                        var length = str.GetLength(options.SelectedEncoding) / 2 + 1;
                        fs.WriteValueS32(length, Endianness);

                        if (options.CharReplacement == 1)
                        {
                            str = Utils.ReplaceChars(str, options.CharReplacementList);
                        }

                        str = BattleRealmsProject.WritingReplacements(str);

                        fs.WriteStringZ(str, options.SelectedEncoding);
                    }
                    else
                    {
                        fs.WriteValueS32(1, Endianness);
                        fs.WriteByte(0);
                        fs.WriteByte(0);
                    }
                }
            }
        }
    }
}
