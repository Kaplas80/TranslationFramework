using System.Collections.Generic;
using System.IO;
using System.Text;
using Gibbed.IO;
using TF.Core.Entities;

namespace TF.Core.Projects.Yakuza0.Files
{
    public class AiPopupFile : TFFile
    {
        public AiPopupFile(string fileName) : base(fileName)
        {
            
        }

        public override Endian Endianness => Endian.Big;
        public override Encoding Encoding => Encoding.UTF8;
        public override string FileType => "ai_popup.bin";
        
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

        private byte[] _unknown;
        private List<TFString> _dataList;

        public override void Read(Stream s)
        {
            _unknown = s.ReadBytes(0x1AC);

            _dataList = new List<TFString>();
            
            var firstStringOffset = s.ReadValueS32(Endianness);
            s.Seek(-4, SeekOrigin.Current);

            while (s.Position < firstStringOffset)
            {
                var str = ReadString(s);
                _dataList.Add(str);
            }
        }

        private TFString ReadString(Stream s)
        {
            var stringOffset = s.ReadValueS32(Endianness);
            var pos = s.Position;

            var value = new TFString {FileId = Id, Offset = stringOffset, Visible = false};

            if (stringOffset != 0)
            {
                s.Seek(stringOffset, SeekOrigin.Begin);

                var str = s.ReadStringZ(Encoding);

                if (!string.IsNullOrEmpty(str.TrimEnd('\0')))
                {
                    str = Yakuza0Project.ReadingReplacements(str);

                    value.Original = str;
                    value.Translation = str;
                    value.Visible = true;
                }
            }
            else
            {
                value.Visible = false;
            }

            s.Seek(pos, SeekOrigin.Begin);

            return value;
        }

        public override void Save(string fileName, byte[] originalContent, IList<TFString> strings, ExportOptions options)
        {
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                fs.WriteBytes(_unknown);

                var currentOffset = _unknown.Length + 4 * strings.Count;

                for (var i = 0; i < strings.Count; i++)
                {
                    fs.Seek(_unknown.Length + i * 4, SeekOrigin.Begin);

                    if (strings[i].Offset == 0)
                    {
                        fs.WriteValueS32(0);
                    }
                    else
                    {
                        fs.WriteValueS32(currentOffset, Endianness);

                        fs.Seek(currentOffset, SeekOrigin.Begin);

                        var str = strings[i].Translation;

                        if (!string.IsNullOrEmpty(str))
                        {
                            if (!str.Equals(strings[i].Original))
                            {
                                if (options.CharReplacement != 0)
                                {
                                    str = Utils.ReplaceChars(str, options.CharReplacementList);
                                }

                                str = Yakuza0Project.WritingReplacements(str);

                                fs.WriteStringZ(str, options.SelectedEncoding);

                                currentOffset += str.GetLength(options.SelectedEncoding) + 1;
                            }
                            else
                            {
                                str = Yakuza0Project.WritingReplacements(str);

                                fs.WriteStringZ(str, Encoding);

                                currentOffset += str.GetLength(Encoding) + 1;
                            }
                        }
                        else
                        {
                            // Hay que escribir solo el 0 del final
                            fs.WriteString("\0");
                            currentOffset++;
                        }
                    }
                }
            }
        }
    }
}