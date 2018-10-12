using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gibbed.IO;
using TF.Core.Entities;

namespace TF.Core.Projects.Yakuza0.Files
{
    public class StreetNameFile : TFFile
    {
        private class Section
        {
            public short NextOffset;
            public string Name;
            public List<ItemData> Strings;

            public Section()
            {
                Strings = new List<ItemData>();
            }
        }

        private class ItemData
        {
            public byte Index;
            public byte Length;
            public TFString Data;
        }

        private int _header;
        private List<Section> _sections;

        public StreetNameFile(string fileName) : base(fileName)
        {
            _sections = new List<Section>();
        }

        public override Endian Endianness => Endian.Little;
        public override Encoding Encoding => Encoding.UTF8;

        public override string FileType => "Street Names";
        
        public override IList<TFString> Strings {
            get
            {
                var result = new List<TFString>();

                foreach (var section in _sections)
                {
                    foreach (var s in section.Strings)
                    {
                        result.Add(s.Data);
                    }
                }
            
                return result;
            }
        }

        public override void Read(Stream s)
        {
            while (s.Position < s.Length)
            {
                var section = new Section {NextOffset = s.ReadValueS16(Endianness), Name = s.ReadStringZ(Encoding)};

                var tmp = s.PeekValueS16(Endianness);
                while (tmp != -1)
                {
                    var item = new ItemData {Index = s.ReadValueU8(), Length = s.ReadValueU8(), Data = ReadString(s, section.Name)};

                    section.Strings.Add(item);

                    tmp = s.PeekValueS16(Endianness);
                }

                _sections.Add(section);
                s.Seek(2, SeekOrigin.Current);
            }
        }

        private TFString ReadString(Stream s, string section)
        {
            var value = new TFString {FileId = Id, Section = section, Offset = (int)s.Position, Visible = false};

            var str = s.ReadStringZ(Encoding);

            if (!string.IsNullOrEmpty(str.TrimEnd('\0')))
            {
                str = Yakuza0Project.ReadingReplacements(str);

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
                foreach (var section in _sections)
                {
                    var offsetPos = fs.Position;
                    fs.WriteValueS16(0, Endianness);
                    fs.WriteStringZ(section.Name, Encoding);

                    foreach (var itemData in section.Strings)
                    {
                        fs.WriteValueU8(itemData.Index);

                        var tfString = strings.FirstOrDefault(x => x.Offset == itemData.Data.Offset);

                        string str;
                        Encoding enc;
                        bool replaceChars;

                        if (tfString.Original == tfString.Translation)
                        {
                            str = tfString.Original;
                            enc = Encoding;
                            replaceChars = false;
                        }
                        else
                        {
                            str = tfString.Translation;
                            enc = options.SelectedEncoding;
                            replaceChars = options.CharReplacement != 0;
                        }

                        if (!string.IsNullOrEmpty(str))
                        {
                            if (replaceChars)
                            {
                                str = Utils.ReplaceChars(str, options.CharReplacementList);
                            }

                            str = Yakuza0Project.WritingReplacements(str);

                            var length = str.GetLength(enc);
                            fs.WriteValueU8((byte)length);
                            fs.WriteStringZ(str, enc);
                        }
                        else
                        {
                            // Hay que escribir solo el 0 del final
                            fs.WriteValueU8(0); // longitud
                            fs.WriteValueU8(0); // cadena
                        }
                    }

                    fs.WriteValueS16(-1, Endianness);

                    var pos = fs.Position;
                    fs.Seek(offsetPos, SeekOrigin.Begin);
                    fs.WriteValueS16((short)pos, Endianness);
                    fs.Seek(pos, SeekOrigin.Begin);
                }
            }
        }
    }
}
