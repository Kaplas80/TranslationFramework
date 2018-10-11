using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Gibbed.IO;
using TF.Core.Entities;

namespace TF.Core.Projects.Yakuza0.Files
{
    public class ImbFile : TFFile
    {
        private const int MAX_SIZE = 0xC0;

        private TFString _title;
        private TFString _description;
        private TFString _dds;

        public ImbFile(string fileName) : base(fileName)
        {
            
        }

        public override Endian Endianness => Endian.Big;
        public override Encoding Encoding => Encoding.UTF8;

        public override IList<TFString> Strings {
            get
            {
                var result = new List<TFString>();

                if (_title != null)
                {
                    result.Add(_title);
                }

                if (_description != null)
                {
                    result.Add(_description);
                }

                if (_dds != null)
                {
                    result.Add(_dds);
                }
            
                return result;
            }
        }

        public override string FileType => "imb";

        public override void Read(Stream s)
        {
            s.Seek(0x20, SeekOrigin.Begin);

            var titlePointer = s.ReadValueS32(Endianness);
            var descriptionPointer = s.ReadValueS32(Endianness);

            s.Seek(0x40, SeekOrigin.Begin);

            var ddsPointer = s.ReadValueS32(Endianness);
            if (titlePointer != 0)
            {
                s.Seek(titlePointer, SeekOrigin.Begin);

                var str = s.ReadStringZ(Encoding);
                if (!string.IsNullOrEmpty(str.TrimEnd('\0')))
                {
                    str = Yakuza0Project.ReadingReplacements(str);
                }

                var item = new TFString
                {
                    FileId = Id,
                    Offset = titlePointer,
                    Section = "32",
                    Original = str,
                    Translation = str,
                    Visible = !string.IsNullOrEmpty(str.TrimEnd('\0'))
                };

                _title = item;
            }

            if (descriptionPointer != 0)
            {
                s.Seek(descriptionPointer, SeekOrigin.Begin);

                var str = s.ReadStringZ(Encoding);
                if (!string.IsNullOrEmpty(str.TrimEnd('\0')))
                {
                    str = Yakuza0Project.ReadingReplacements(str);
                }

                var item = new TFString
                {
                    FileId = Id,
                    Offset = descriptionPointer,
                    Section = "36",
                    Original = str,
                    Translation = str,
                    Visible = !string.IsNullOrEmpty(str.TrimEnd('\0'))
                };

                _description = item;
            }

            if (ddsPointer != 0)
            {
                s.Seek(ddsPointer, SeekOrigin.Begin);

                var str = s.ReadStringZ(Encoding);
                if (!string.IsNullOrEmpty(str.TrimEnd('\0')))
                {
                    str = Yakuza0Project.ReadingReplacements(str);
                }

                var item = new TFString
                {
                    FileId = Id,
                    Offset = ddsPointer,
                    Section = "64",
                    Original = str,
                    Translation = str,
                    Visible = false
                };

                _dds = item;
            }
        }

        public override void Save(string fileName, byte[] originalContent, IList<TFString> strings, ExportOptions options)
        {
            File.WriteAllBytes(fileName, originalContent);

            using (var fs = new FileStream(fileName, FileMode.Open))
            {
                var stringOffset = strings[0].Offset;

                var i = 0;
                foreach (var tfString in strings)
                {
                    var pointer = Convert.ToInt32(tfString.Section);
                    fs.Seek(pointer, SeekOrigin.Begin);
                    fs.WriteValueS32(stringOffset, Endianness);

                    fs.Seek(stringOffset, SeekOrigin.Begin);

                    stringOffset = WriteString(fs, stringOffset, tfString, options, i != 2);

                    i++;
                }

                while (fs.Position % 16 != 0)
                {
                    fs.WriteByte(0);
                }
            }
        }

        private int WriteString(Stream s, int offset, TFString tfString, ExportOptions options, bool writeZero)
        {
            var result = offset;
            s.Seek(offset, SeekOrigin.Begin);

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
                result += str.GetLength(enc) + 1;

                if (writeZero)
                {
                    s.WriteByte(0);
                    result++;
                }
            }

            return result;
        }
    }
}
