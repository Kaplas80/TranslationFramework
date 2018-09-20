using System.Collections.Generic;
using System.IO;
using System.Text;
using Gibbed.IO;
using TF.Core.Entities;

namespace TF.Core.Projects.Yakuza0.Files
{
    public class ExeFile : TFFile
    {
        private class ExeStringTable
        {
            public string Name;
            public int TableOffset;
            public int StringCount;

            public long OutputOffset;

            public int SectionSize;
        }

        private static readonly long FILE_BASE = 0x0140001600;
        private static readonly long OUTPUT_BASE = 0x0140003800;

        private static ExeStringTable[] _sections = {
            new ExeStringTable
            {
                Name = "SECCION_01",
                TableOffset = 0x01053B20,
                StringCount = 105,
                //StringsOffset = 0x00DC72B0,
                OutputOffset = 0x0E03E800,
                SectionSize = 0x1000
            },
            new ExeStringTable
            {
                Name = "SECCION_02",
                TableOffset = 0x01053E70,
                StringCount = 132,
                //StringsOffset = 0x00DC7780,
                OutputOffset = 0x0E03F800,
                SectionSize = 0x1000
            },
            new ExeStringTable
            {
                Name = "SECCION_03",
                TableOffset = 0x010543C8,
                StringCount = 389,
                //StringsOffset = 0x00DC81F8,
                OutputOffset = 0x0E040800,
                SectionSize = 0x4000
            },
            new ExeStringTable
            {
                Name = "SECCION_04",
                TableOffset = 0x01055000,
                StringCount = 18,
                //StringsOffset = 0x00DC81F8,
                OutputOffset = 0x0E044800,
                SectionSize = 0x1000
            },
            new ExeStringTable
            {
                Name = "SECCION_05",
                TableOffset = 0x01055098,
                StringCount = 26,
                //StringsOffset = 0x00DC81F8,
                OutputOffset = 0x0E045800,
                SectionSize = 0x1000
            },
            new ExeStringTable
            {
                Name = "SECCION_06",
                TableOffset = 0x01055D50,
                StringCount = 43,
                //StringsOffset = 0x00DD0850,
                OutputOffset = 0x0E045800,
                SectionSize = 0x4000
            },
            new ExeStringTable
            {
                Name = "SECCION_07",
                TableOffset = 0x01055EB0,
                StringCount = 253,
                //StringsOffset = 0x00DCCFD8,
                OutputOffset = 0x0E049800,
                SectionSize = 0x6000
            },
            new ExeStringTable
            {
                Name = "SECCION_08",
                TableOffset = 0x010566A0,
                StringCount = 28,
                //StringsOffset = 0x00DCCFD8,
                OutputOffset = 0x0E04F800,
                SectionSize = 0x2000
            },
            
            /*new ExeStringTable
            {
                Name = "SECCION_06",
                TableOffset = 0x,
                StringCount = ,
                StringsOffset = 0x00E85B38,
                OutputOffset = 0x0E050800,
                SectionSize = 0x20000
            },*/
        };

        private List<TFString> _dataList;

        public ExeFile(string fileName) : base(fileName)
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
                    result.Add(item);
                }
            
                return result;
            }
        }

        public override string FileType => "Yakuza0.exe";

        public override void Read()
        {
            using (var fs = new FileStream(Path, FileMode.Open))
            {
                ReadDataItems(fs);
            }
        }

        private void ReadDataItems(Stream s)
        {
            _dataList = new List<TFString>();
            
            var readStrings = new List<long>();

            foreach (var section in _sections)
            {
                var i = 0;

                s.Seek(section.TableOffset, SeekOrigin.Begin);
                while (i < section.StringCount)
                {
                    var stringOffset = s.ReadValueS64(Endianness);
                    var correctedOffset = stringOffset - ((stringOffset >= 0x014E042000)?OUTPUT_BASE:FILE_BASE);

                    TFString tfString;

                    if (readStrings.Contains(stringOffset))
                    {
                        tfString = new TFString
                        {
                            FileId = Id,
                            Offset = (int) correctedOffset,
                            Section = section.Name,
                            Original = string.Empty,
                            Translation = string.Empty,
                            Visible = false,
                        };
                    }
                    else
                    {
                        var pos = s.Position;
                        s.Seek(correctedOffset, SeekOrigin.Begin);

                        tfString = ReadString(s, (int)correctedOffset);
                        tfString.Section = section.Name;
                        readStrings.Add(stringOffset);
                        s.Seek(pos, SeekOrigin.Begin);
                    }
                
                    _dataList.Add(tfString);
                    i++;
                }
            }
        }

        private TFString ReadString(Stream s, int stringOffset)
        {
            var value = new TFString {FileId = Id, Offset = stringOffset, Visible = false};

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

        public override void Save(string fileName, IList<TFString> strings, ExportOptions options)
        {
            File.Copy(Path, fileName, true);

            using (var fs = new FileStream(fileName, FileMode.Open))
            {
                ClearSections(fs);
                WriteDataItems(fs, strings, options);
            }
        }

        private void ClearSections(Stream s)
        {
            foreach (var section in _sections)
            {
                var zeroes = new byte[section.SectionSize];
                s.Seek(section.OutputOffset, SeekOrigin.Begin);
                s.WriteBytes(zeroes);
            }
        }

        private void WriteDataItems(Stream s, IList<TFString> strings, ExportOptions options)
        {
            var written = new Dictionary<int, long>();

            var tableOffset = new Dictionary<string, long>();
            var stringOffset = new Dictionary<string, long>();
            var sections = new Dictionary<string, ExeStringTable>();

            foreach (var section in _sections)
            {
                tableOffset[section.Name] = section.TableOffset;
                stringOffset[section.Name] = section.OutputOffset;
                sections[section.Name] = section;
            }

            foreach (var tfString in strings)
            {
                var currentTable = sections[tfString.Section];
                
                s.Seek(tableOffset[currentTable.Name], SeekOrigin.Begin);

                if (written.ContainsKey(tfString.Offset))
                {
                    s.WriteValueS64(written[tfString.Offset], Endianness);
                    tableOffset[currentTable.Name] = s.Position;
                }
                else
                {
                    var correctedOffset = stringOffset[tfString.Section] + OUTPUT_BASE;
                    
                    s.WriteValueS64(correctedOffset, Endianness);
                    tableOffset[currentTable.Name] = s.Position;

                    s.Seek(stringOffset[tfString.Section], SeekOrigin.Begin);

                    var str = tfString.Translation;

                    if (!string.IsNullOrEmpty(str))
                    {
                        if (options.CharReplacement != 0)
                        {
                            str = Utils.ReplaceChars(str, options.CharReplacementList);
                        }

                        str = Yakuza0Project.WritingReplacements(str);

                        s.WriteStringZ(str, options.SelectedEncoding);

                        stringOffset[tfString.Section] += str.GetLength(options.SelectedEncoding) + 1;
                    }
                    else
                    {
                        // Hay que escribir solo el 0 del final
                        s.WriteString("\0");
                        stringOffset[tfString.Section]++;
                    }

                    written.Add(tfString.Offset, correctedOffset);
                }
            }
        }
    }
}
