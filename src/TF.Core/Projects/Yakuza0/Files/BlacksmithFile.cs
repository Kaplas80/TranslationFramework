using System.Collections.Generic;
using System.IO;
using System.Text;
using Gibbed.IO;
using TF.Core.Entities;

namespace TF.Core.Projects.Yakuza0.Files
{
    public class BlacksmithFile : TFFile
    {
        public BlacksmithFile(string fileName) : base(fileName)
        {
            
        }

        public override Endian Endianness => Endian.Big;
        public override Encoding Encoding => Encoding.UTF8;
        public override string FileType => "blacksmith.bin";
        
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

        private List<TFString> _dataList;
        private int _remainderStart;
        private int _unknown;
        private byte[] _remainder;

        public override void Read(Stream s)
        {
            _dataList = new List<TFString>();

            _remainderStart = s.ReadValueS32(Endianness);
            _unknown = s.ReadValueS32(Endianness);
            
            var firstStringOffset = s.ReadValueS32(Endianness);
            s.Seek(-4, SeekOrigin.Current);

            while (s.Position < _remainderStart)
            {
                var str = ReadString(s);
                _dataList.Add(str);
            }

            _remainder = s.ReadBytes(firstStringOffset - _remainderStart);
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

            s.Seek(pos, SeekOrigin.Begin);

            return value;
        }

        public override void Save(string fileName, byte[] originalContent, IList<TFString> strings, ExportOptions options)
        {
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                fs.WriteValueS32(_remainderStart, Endianness);
                fs.WriteValueS32(_unknown, Endianness);

                var currentOffset = 8 + 4 * strings.Count + _remainder.Length;

                for (var i = 0; i < strings.Count; i++)
                {
                    fs.Seek(8 + i * 4, SeekOrigin.Begin);
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

                fs.Seek(_remainderStart, SeekOrigin.Begin);
                fs.WriteBytes(_remainder);
            }
        }
    }
}