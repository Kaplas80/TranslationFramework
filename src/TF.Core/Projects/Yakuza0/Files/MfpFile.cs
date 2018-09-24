using System.Collections.Generic;
using System.IO;
using System.Text;
using Gibbed.IO;
using TF.Core.Entities;

namespace TF.Core.Projects.Yakuza0.Files
{
    public class MfpFile : TFFile
    {
        private const int MAX_SIZE = 0xC0;

        private List<TFString> _dataList;

        public MfpFile(string fileName) : base(fileName)
        {
            
        }

        public override Endian Endianness => Endian.Big;
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

        public override string FileType => "mfp";

        public override void Read(Stream s)
        {
            _dataList = new List<TFString>();

            s.Seek(0x24, SeekOrigin.Begin);

            var pointer1 = s.ReadValueS32(Endianness);

            if (pointer1 != -1)
            {
                s.Seek(pointer1, SeekOrigin.Begin);

                var pointer2 = s.ReadValueS32(Endianness);

                s.Seek(pointer2, SeekOrigin.Begin);

                var str = s.ReadStringZ(Encoding);
                if (!string.IsNullOrEmpty(str.TrimEnd('\0')))
                {
                    str = Yakuza0Project.ReadingReplacements(str);
                }

                var item = new TFString
                {
                    FileId = Id,
                    Offset = pointer2,
                    Section = pointer1.ToString("X8"),
                    Original = str,
                    Translation = str,
                    Visible = !string.IsNullOrEmpty(str.TrimEnd('\0'))
                };

                _dataList.Add(item);
            }
        }

        public override void Save(string fileName, byte[] originalContent, IList<TFString> strings, ExportOptions options)
        {
            File.WriteAllBytes(fileName, originalContent);

            using (var fs = new FileStream(fileName, FileMode.Open))
            {
                for (var i = 0; i < _dataList.Count; i++)
                {
                    var item = _dataList[i];

                    fs.Seek(item.Offset, SeekOrigin.Begin);

                    var str = strings[i].Translation;

                    if (strings[i].Visible && !string.IsNullOrEmpty(str))
                    {
                        var zeros = new byte[MAX_SIZE];

                        fs.WriteBytes(zeros);

                        if (options.CharReplacement != 0)
                        {
                            str = Utils.ReplaceChars(str, options.CharReplacementList);
                        }

                        str = Yakuza0Project.WritingReplacements(str);

                        if (str.GetLength(options.SelectedEncoding) >= MAX_SIZE)
                        {
                            str = str.Substring(0, MAX_SIZE - 1);
                        }
                        
                        fs.Seek(item.Offset, SeekOrigin.Begin);
                        fs.WriteStringZ(str, options.SelectedEncoding);
                    }
                }
            }
        }
    }
}
