using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Gibbed.IO;
using TF.Core.Entities;

namespace TF.Core.Projects.Yakuza0.Files
{
    public class MsgFileNew : TFFile
    {
        private class Property
        {
            public ushort Type { get; set; }
            public byte[] Unknown1 { get; set; }
            public byte[] Unknown2 { get; set; }
            public ushort Position { get; set; }
            public byte[] Unknown3 { get; set; }

            public bool IsPause()
            {
                return Type == 0x0209; 
            }
        }

        private class Item
        {
            public TFString String { get; set; }
            public List<Property> StringProperties { get; }
            public byte Unknown { get; set; }

            public Item()
            {
                StringProperties = new List<Property>();
            }

            public bool HasPauses()
            {
                return StringProperties.Any(x => x.Type == 0x0209);
            }
        }

        private class StringGroup
        {
            public List<Item> Items { get; }
            public byte[] Unknown { get; set; }

            public StringGroup()
            {
                Items = new List<Item>();
            }
        }

        private byte[] _magic;
        private List<StringGroup> _stringGroups;
        private byte[] _table1;
        private List<TFString> _tableTitles;
        private byte[] _remainder;

        public MsgFileNew(string fileName) : base(fileName)
        {
            _stringGroups = new List<StringGroup>();
            _tableTitles = new List<TFString>();
        }

        public override Endian Endianness => Endian.Big;
        public override Encoding Encoding => Encoding.UTF8;

        public override IList<TFString> Strings {
            get
            {
                var result = new List<TFString>();

                foreach (var s in _tableTitles)
                {
                    result.Add(s);
                }

                foreach (var g in _stringGroups)
                {
                    foreach (var i in g.Items)
                    {
                        result.Add(i.String);
                    }
                }

                return result;
            }
        }

        public override string FileType => "Msg";
        
        public override void Read(Stream s)
        {
            _magic = s.ReadBytes(3);

            var stringGroupsCount = s.ReadByte();
            var stringGroupStartPointer = s.ReadValueU32(Endianness);
            var table1Pointer = s.ReadValueU32(Endianness);
            var table1Count = s.ReadValueU16(Endianness);
            var titlesCount = s.ReadValueU16(Endianness);
            var titlesPointer = s.ReadValueU32(Endianness);
            var remainderPointer = s.ReadValueU32(Endianness);

            for (var i = 0; i < stringGroupsCount; i++)
            {
                s.Seek(stringGroupStartPointer + i * 16, SeekOrigin.Begin);
                var strGroup = ReadStringGroup(s);
                _stringGroups.Add(strGroup);
            }

            s.Seek(table1Pointer, SeekOrigin.Begin);
            _table1 = s.ReadBytes(table1Count * 16);

            for (var i = 0; i < titlesCount; i++)
            {
                s.Seek(titlesPointer + i * 4, SeekOrigin.Begin);
                var title = ReadTitle(s);
                _tableTitles.Add(title);
            }

            s.Seek(remainderPointer, SeekOrigin.Begin);
            _remainder = s.ReadBytes((int) (s.Length - remainderPointer));
        }

        private StringGroup ReadStringGroup(Stream s)
        {
            var result = new StringGroup();

            var groupStartPointer = s.ReadValueS64(Endianness);
            var strCount = s.ReadValueU16(Endianness);
            result.Unknown = s.ReadBytes(6);

            for (var i = 0; i < strCount; i++)
            {
                s.Seek(groupStartPointer + (i * 12), SeekOrigin.Begin);
                var item = ReadItem(s);
                result.Items.Add(item);
            }

            return result;
        }

        private Item ReadItem(Stream s)
        {
            var result = new Item();
            var strLength = s.ReadValueU16(Endianness);
            var propCount = s.ReadByte();
            result.Unknown = (byte)s.ReadByte();

            var section = s.Position.ToString("X8");
            var strPointer = s.ReadValueU32(Endianness);
            var propPointer = s.ReadValueU32(Endianness);

            s.Seek(strPointer, SeekOrigin.Begin);
            result.String = GetString(s, section);

            s.Seek(propPointer, SeekOrigin.Begin);
            for (var i = 0; i < propCount; i++)
            {
                var prop = new Property
                {
                    Type = s.ReadValueU16(Endianness),
                    Unknown1 = s.ReadBytes(2),
                    Unknown2 = s.ReadBytes(2),
                    Position = s.ReadValueU16(Endianness),
                    Unknown3 = s.ReadBytes(8)
                };
                result.StringProperties.Add(prop);
            }

            return result;
        }

        private TFString ReadTitle(Stream s)
        {
            var pointer = s.ReadValueU32(Endianness);
            s.Seek(pointer, SeekOrigin.Begin);
            return GetString(s, "NAMES");
        }

        private TFString GetString(Stream s, string section)
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
                Section = section,
                Visible = !string.IsNullOrWhiteSpace(str),
            };

            return tfString;
        }

        public override void Save(string fileName, byte[] originalContent, IList<TFString> strings, ExportOptions options)
        {
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                fs.WriteBytes(_magic);
                fs.WriteByte((byte)_stringGroups.Count);
                fs.WriteValueS32(0x00000018, Endianness);
                fs.WriteValueS32(0); //Puntero Tabla1
                fs.WriteValueU16((ushort) (_table1.Length / 16), Endianness);
                fs.WriteValueU16((ushort) _tableTitles.Count, Endianness);
                fs.WriteValueS32(0); //Puntero Tabla Nombres
                fs.WriteValueS32(0); //Puntero Resto

                var totalStringsCount = 0;
                var totalPropertiesCount = 0;
                foreach (var sg in _stringGroups)
                {
                    totalStringsCount += sg.Items.Count;
                    foreach (var item in sg.Items)
                    {
                        var str = FindString(item.String, strings);

                        totalPropertiesCount += item.StringProperties.Count;
                    }
                }

                var stringGroupPointer = 0x18;
                var itemPointer = stringGroupPointer + 16 * _stringGroups.Count;
                var propertiesPointer = itemPointer + 12 * totalStringsCount;
                var stringsPointer = propertiesPointer + 16 * totalPropertiesCount;

                for (var i = 0; i < _stringGroups.Count; i++)
                {
                    var sg = _stringGroups[i];

                    fs.Seek(stringGroupPointer + i * 16, SeekOrigin.Begin);
                    
                    fs.WriteValueU64((ulong) itemPointer, Endianness);
                    fs.WriteValueU16((ushort) sg.Items.Count, Endianness);
                    fs.WriteBytes(sg.Unknown);

                    for (var j = 0; j < sg.Items.Count; j++)
                    {
                        var item = sg.Items[j];
                        var str = FindString(item.String, strings);
                        var strLength = GetLength(str, options);

                        fs.Seek(itemPointer + j * 12, SeekOrigin.Begin);
                        fs.WriteValueU16((ushort) strLength, Endianness);
                        fs.WriteByte((byte) item.StringProperties.Count);
                        fs.WriteByte(item.Unknown);

                        fs.WriteValueS32(stringsPointer, Endianness);
                        fs.WriteValueS32(propertiesPointer, Endianness);

                        fs.Seek(propertiesPointer, SeekOrigin.Begin);

                        WriteProperties(fs, item, str);

                        fs.Seek(stringsPointer, SeekOrigin.Begin);

                        //WriteString(fs, str, options);

                        propertiesPointer += item.StringProperties.Count * 16;
                        stringsPointer = (int) fs.Position;
                    }

                    itemPointer += sg.Items.Count * 12;
                }

                fs.Seek(0, SeekOrigin.End);
                while (fs.Position % 8 != 0)
                {
                    fs.WriteByte(0);
                }

                var table1Pointer = fs.Position;
                fs.WriteBytes(_table1);

                var table2Pointer = fs.Position;
                
                for (var i = 0; i < _tableTitles.Count; i++)
                {
                    fs.Seek(table2Pointer + i * 4, SeekOrigin.Begin);
                    //remainderPointer = SaveTitle(_tableTitles[i], strings, options);
                }


                fs.Seek(0, SeekOrigin.End);
                var remainderPointer = fs.Position;
                fs.WriteBytes(_remainder);

                /*fs.Seek(0x08, SeekOrigin.Begin);
                fs.WriteValueS32((int)table1Pointer, Endianness);
                fs.Seek(0x16, SeekOrigin.Begin);
                fs.WriteValueS32((int)table2Pointer, Endianness);
                fs.WriteValueS32((int)remainderPointer, Endianness);*/
            }
        }

        public void Save(Stream s, byte[] originalContent, IList<TFString> strings, ExportOptions options)
        {
            
        }

        private void WriteProperties(Stream s, Item item, TFString tfStr)
        {
            if (tfStr.Original == tfStr.Translation)
            {
                foreach (var p in item.StringProperties)
                {
                    s.WriteValueU16(p.Type, Endianness);
                    s.WriteBytes(p.Unknown1);
                    s.WriteBytes(p.Unknown2);
                    s.WriteValueU16(p.Position, Endianness);
                    s.WriteBytes(p.Unknown3);
                }
            }
            else
            {
                // Como la línea ha cambiado, las propiedades también
                var newProperties = new List<Property>();
                if (item.HasPauses())
                {
                    var newPauses = ParsePauses(tfStr.Translation);
                    foreach (var property in item.StringProperties)
                    {
                        if (!property.IsPause())
                        {
                            newProperties.Add(property);
                        }
                    }
                    newProperties.InsertRange(newProperties.Count - 1, newPauses);
                }
                else
                {
                    foreach (var property in item.StringProperties)
                    {
                        newProperties.Add(property);
                    }
                }

                foreach (var p in newProperties)
                {
                    s.WriteValueU16(p.Type, Endianness);
                    s.WriteBytes(p.Unknown1);
                    s.WriteBytes(p.Unknown2);
                    s.WriteValueU16(p.Position, Endianness);
                    s.WriteBytes(p.Unknown3);
                }
            }
        }

        private static IList<Property> ParsePauses(string str)
        {
            var result = new List<Property>();
            var shortPattern = @"(,\s)";
            var longPattern = @"(\.\s|\!|\?)";

            var temp = RemoveTags(str);

            var matches = Regex.Matches(temp, $@"{shortPattern}|{longPattern}");

            foreach (Match match in matches)
            {
                var duration = Regex.IsMatch(match.Value, shortPattern) ? 0x0a : 0x14;

                var p = new Property
                {
                    Type = 0x0209,
                    Unknown1 = new byte[] {0x00, (byte) duration},
                    Unknown2 = new byte[] {0x00, 0x00},
                    Position = (ushort) (match.Index + 1),
                    Unknown3 = new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}
                };

                result.Add(p);
            }

            return result;
        }

        private static TFString FindString(TFString original, IList<TFString> strings)
        {
            return strings.FirstOrDefault(s => s.Offset == original.Offset);
        }

        private int GetLength(TFString tfStr, ExportOptions options)
        {
            if (tfStr.Original == tfStr.Translation)
            {
                var temp = tfStr.Original.Replace("\\r\\n", "\r\n");

                return temp.GetLength(Encoding);
            }
            else
            {
                var temp = tfStr.Translation.Replace("\\r\\n", "\r\n");

                return temp.GetLength(options.SelectedEncoding);
            }
        }

        private static string RemoveTags(string strOriginal)
        {
            var temp = strOriginal.Replace("\\r\\n", " ");
            temp = temp.Replace("\\n", " ");
            temp = Regex.Replace(temp, @"<Color[^>]*>", string.Empty);
            return Regex.Replace(temp, @"<[^>]*>", " ");
        }
    }
}
