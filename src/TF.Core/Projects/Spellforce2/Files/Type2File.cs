using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gibbed.IO;
using TF.Core.Entities;

namespace TF.Core.Projects.Spellforce2.Files
{
    public class Type2File : TFFile
    {
        private class ItemData
        {
            public ushort ItemId;
            public ushort Unknown;
            public byte Gender; // 01 = Masc, 02 = Fem, 03 = Neutro?
            public byte Number; // 00 = Sing, 01 = Plural
            public TFString Data;
        }

        private List<ItemData> _strings;

        public Type2File(string fileName) : base(fileName)
        {
            _strings = new List<ItemData>();
        }

        public override Endian Endianness => Endian.Little;
        public override Encoding Encoding => Encoding.GetEncoding("Unicode");

        public override string FileType => "Spellforce 2 Type 2 Text File";
        
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
            var numElems = s.ReadValueS32(Endianness);
            
            for (var i = 0; i < numElems; i++)
            {
                var str = new ItemData
                {
                    ItemId = s.ReadValueU16(Endianness),
                    Unknown = s.ReadValueU16(Endianness),
                    Gender = s.ReadValueU8(),
                    Number = s.ReadValueU8()
                };

                var dataLength = s.ReadValueU32(Endianness);
                str.Data = ReadString(s, dataLength * 2, $"{str.ItemId:0000}-{str.Gender}-{str.Number}");
                
                _strings.Add(str);
            }
        }

        private TFString ReadString(Stream s, uint length, string section)
        {
            var value = new TFString {FileId = Id, Offset = (int) s.Position, Visible = true, Section = section};

            var str = s.ReadString(length, false, Encoding);

            str = Spellforce2Project.ReadingReplacements(str);

            value.Original = str;
            value.Translation = str;

            return value;
        }
        
        public override void Save(string fileName, byte[] originalContent, IList<TFString> strings,
            ExportOptions options)
        {
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                fs.WriteValueS32(strings.Count, Endianness);

                foreach (var item in _strings)
                {
                    fs.WriteValueU16(item.ItemId, Endianness);
                    fs.WriteValueU16(item.Unknown, Endianness);
                    fs.WriteValueU8(item.Gender);
                    fs.WriteValueU8(item.Number);

                    var tfString = strings.FirstOrDefault(x => x.Offset == item.Data.Offset);

                    var str = tfString.Translation;

                    if (options.CharReplacement == 1)
                    {
                        str = Utils.ReplaceChars(str, options.CharReplacementList);
                    }

                    str = Spellforce2Project.WritingReplacements(str);

                    var length = (uint)str.GetLength(options.SelectedEncoding) / 2;
                    fs.WriteValueU32(length, Endianness);
                    fs.WriteString(str, options.SelectedEncoding);
                }
            }
        }
    }
}
