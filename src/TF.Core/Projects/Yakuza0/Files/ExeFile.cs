using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Gibbed.IO;
using TF.Core.Entities;

namespace TF.Core.Projects.Yakuza0.Files
{
    public class ExeFile : TFFile
    {
        private static readonly long FILE_BASE = 0x0140001600;
        private static readonly long OUTPUT_BASE = 0x0140003800;
        
        private List<TFString> _dataList;

        private List<int> _offsets;

        public ExeFile(string fileName) : base(fileName)
        {
            var content = File.ReadAllLines("ExeOffsets.txt");
            _offsets = new List<int>(content.Length);
            foreach (var s in content)
            {
                _offsets.Add(Convert.ToInt32(s, 16));
            }
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

        public override void Read(Stream s)
        {
            ReadDataItems(s);
        }

        private void ReadDataItems(Stream s)
        {
            _dataList = new List<TFString>();
            
            var readStrings = new List<long>();

            foreach (var offset in _offsets)
            {
                s.Seek(offset, SeekOrigin.Begin);

                var stringOffset = s.ReadValueS64(Endianness);
                var correctedOffset = stringOffset - ((stringOffset >= 0x014E042000)?OUTPUT_BASE:FILE_BASE);

                TFString tfString;

                if (readStrings.Contains(stringOffset))
                {
                    tfString = new TFString
                    {
                        FileId = Id,
                        Offset = (int) correctedOffset,
                        Section = offset.ToString("X8"),
                        Original = string.Empty,
                        Translation = string.Empty,
                        Visible = false
                    };
                }
                else
                {
                    var pos = s.Position;
                    s.Seek(correctedOffset, SeekOrigin.Begin);

                    tfString = ReadString(s, (int)correctedOffset);
                    tfString.Section = offset.ToString("X8");
                    readStrings.Add(stringOffset);
                    s.Seek(pos, SeekOrigin.Begin);
                }
            
                _dataList.Add(tfString);
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

        public override void Save(string fileName, byte[] originalContent, IList<TFString> strings, ExportOptions options)
        {
            File.WriteAllBytes(fileName, originalContent);

            using (var fs = new FileStream(fileName, FileMode.Open))
            {
                ClearSections(fs);
                WriteDataItems(fs, strings, options);
            }
        }

        private void ClearSections(Stream s)
        {
            var zeroes = new byte[s.Length - 0x0E03E800];
            
            s.Seek(0x0E03E800, SeekOrigin.Begin);
            s.WriteBytes(zeroes);
        }

        private void WriteDataItems(Stream s, IList<TFString> strings, ExportOptions options)
        {
            var written = new Dictionary<int, long>();

            var stringOffset = 0x0E03E800;
            foreach (var tfString in strings)
            {
                var indexOffset = Convert.ToInt32(tfString.Section, 16);

                s.Seek(indexOffset, SeekOrigin.Begin);

                if (written.ContainsKey(tfString.Offset))
                {
                    s.WriteValueS64(written[tfString.Offset], Endianness);
                }
                else
                {
                    var correctedOffset = stringOffset + OUTPUT_BASE;

                    s.WriteValueS64(correctedOffset, Endianness);

                    s.Seek(stringOffset, SeekOrigin.Begin);

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

                        s.WriteStringZ(str, enc);

                        stringOffset += str.GetLength(enc) + 1;
                    }
                    else
                    {
                        // Hay que escribir solo el 0 del final
                        s.WriteString("\0");
                        stringOffset++;
                    }

                    written.Add(tfString.Offset, correctedOffset);
                }
            }
        }
    }
}
