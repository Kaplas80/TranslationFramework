using System.Collections.Generic;
using System.IO;
using System.Text;
using Gibbed.IO;
using TF.Core.Entities;

namespace TF.Core.Projects.SAO_HF.Files
{
    public class ScriptFile : TFFile
    {
        private static readonly byte[] PATTERN = {0x00, 0x00, 0x08, 0x00};

        private class DataItem
        {
            public TFString Data { get; set; }
            public int RelativeOffsetPosition { get; set; }
            public int RelativeOffset { get; set; }
        }

        private List<DataItem> _strings { get; }
        private int _firstStringOffset;

        public ScriptFile(string fileName) : base(fileName)
        {
            _strings = new List<DataItem>();
        }

        public override Endian Endianness => Endian.Little;
        public override Encoding Encoding => Encoding.UTF8;

        public override string FileType => "Script";
        
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
            s.Seek(8, SeekOrigin.Begin);
            _firstStringOffset = s.ReadValueS32(Endianness);

            var currentIndex = s.FindPattern(PATTERN);

            while (currentIndex != -1)
            {
                var localOffset = s.ReadValueS32(Endianness);
                var control = s.ReadValueS16(Endianness);

                if (control == 0 || control == 8)
                {
                    // Es un texto que hay que añadir
                    var item = new DataItem
                    {
                        RelativeOffset = localOffset,
                        RelativeOffsetPosition = (int) (s.Position - 6)
                    };

                    var pos = s.Position;
                    s.Seek(_firstStringOffset + localOffset, SeekOrigin.Begin);
                    
                    item.Data = ReadString(s);

                    s.Seek(currentIndex + 4, SeekOrigin.Begin);

                    _strings.Add(item);
                }

                currentIndex = s.FindPattern(PATTERN);
            }
        }


        private TFString ReadString(Stream s)
        {
            var stringOffset = (int) s.Position;
            
            var value = new TFString {FileId = Id, Offset = stringOffset, Visible = false};

            var str = s.ReadStringZ(Encoding);

            if (!string.IsNullOrEmpty(str.TrimEnd('\0')))
            {
                str = SAOProject.ReadingReplacements(str);
            }

            value.Original = str;
            value.Translation = str;
            value.Visible = !string.IsNullOrWhiteSpace(str);

            return value;
        }

        public override void Save(string fileName, byte[] originalContent, IList<TFString> strings,
            ExportOptions options)
        {
            File.WriteAllBytes(fileName, originalContent);

            var writeOffset = _firstStringOffset + _strings[0].RelativeOffset;
            using (var fs = new FileStream(fileName, FileMode.Open))
            {
                foreach (var item in _strings)
                {
                    fs.Seek(item.RelativeOffsetPosition, SeekOrigin.Begin);
                    fs.WriteValueS32(writeOffset - _firstStringOffset);

                    fs.Seek(writeOffset, SeekOrigin.Begin);
                    var str = item.Data.Translation;

                    if (item.Data.Visible && !string.IsNullOrEmpty(str))
                    {
                        if (options.CharReplacement != 0)
                        {
                            str = Utils.ReplaceChars(str, options.CharReplacementList);
                        }

                        str = SAOProject.WritingReplacements(str);

                        fs.WriteStringZ(str, options.SelectedEncoding);

                        writeOffset += str.GetLength(options.SelectedEncoding) + 1;
                    }
                    else
                    {
                        fs.WriteStringZ(item.Data.Original);
                        writeOffset = item.Data.Original.GetLength(options.SelectedEncoding) + 1;
                    }
                }

                var strSize = (uint) (fs.Position - _firstStringOffset);

                while (fs.Position % 4 != 0)
                {
                    fs.WriteValueU8(0);
                }

                fs.Seek(0x0C, SeekOrigin.Begin);
                fs.WriteValueU32(strSize, Endianness);
            }
        }
    }
}
