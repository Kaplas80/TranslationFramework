using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gibbed.IO;
using TF.Core.Entities;

namespace TF.Core.Projects.Infliction.Files
{
    public class TextFile : TFFile
    {
        private List<TFString> _strings;

        public TextFile(string fileName) : base(fileName)
        {
            _strings = new List<TFString>();
        }

        public override Endian Endianness => Endian.Little;
        public override Encoding Encoding => Encoding.GetEncoding("ISO-8859-1");
        public Encoding Encoding2 => Encoding.Unicode;

        protected virtual byte[] PATTERN => new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x21, 0x00, 0x00, 0x00 };

        public override string FileType => "Infliction Text";

        public override IList<TFString> Strings
        {
            get
            {
                var result = new List<TFString>();

                foreach (var item in _strings)
                {
                    result.Add(item);
                }

                return result;
            }
        }

        public override void Read(Stream s)
        {
            var pos = s.FindPattern(PATTERN);

            while (pos != -1)
            {
                s.Seek(pos + 16 + 0x21, SeekOrigin.Begin);

                var length = s.ReadValueS32(Endianness);

                if (length > 0)
                {
                    var item = ReadString(s, Encoding);
                    _strings.Add(item);
                }
                else
                {
                    var item = ReadString(s, Encoding2);
                    _strings.Add(item);
                }

                pos = s.FindPattern(PATTERN);
            }

        }

        private TFString ReadString(Stream s, Encoding enc)
        {
            var value = new TFString { FileId = Id, Offset = (int)s.Position, Visible = false };

            var str = s.ReadStringZ(enc);

            if (!string.IsNullOrEmpty(str.TrimEnd('\0')))
            {
                //str = InflictionProject.ReadingReplacements(str);
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
                var originalPos = 0;
                var stringPos = Utils.FindPattern(originalContent, PATTERN, originalPos);

                while (stringPos != -1)
                {
                    stringPos += 16 + 0x21;
                    var copyBytes = new byte[stringPos - originalPos];
                    Array.Copy(originalContent, originalPos, copyBytes, 0, copyBytes.Length);
                    fs.WriteBytes(copyBytes);

                    var tfString = strings.FirstOrDefault(x => x.Offset == stringPos + 4);

                    var originalLength = BitConverter.ToInt32(originalContent, stringPos);

                    if (tfString.Original == tfString.Translation)
                    {
                        fs.WriteValueS32(originalLength, Endianness);
                        fs.WriteStringZ(tfString.Original, originalLength > 0 ? Encoding : Encoding2);
                    }
                    else
                    {
                        if (originalLength > 0)
                        {
                            var length = tfString.Translation.GetLength(Encoding) + 1;
                            fs.WriteValueS32(length, Endianness);
                            fs.WriteStringZ(tfString.Translation, Encoding);
                        }
                        else
                        {
                            var length = tfString.Translation.GetLength(Encoding2)/2 + 2;
                            fs.WriteValueS32(-length, Endianness);
                            fs.WriteStringZ(tfString.Translation, Encoding2);
                        }
                    }

                    if (originalLength > 0)
                    {
                        originalPos = stringPos + 4 + originalLength;
                    }
                    else
                    {
                        originalPos = stringPos + 4 - originalLength * 2;
                    }
                    
                    stringPos = Utils.FindPattern(originalContent, PATTERN, originalPos);
                }

                var endBytes = new byte[originalContent.Length - originalPos];
                Array.Copy(originalContent, originalPos, endBytes, 0, endBytes.Length);
                fs.WriteBytes(endBytes);
            }
        }
    }
}
