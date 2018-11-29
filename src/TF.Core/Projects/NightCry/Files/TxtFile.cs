using System.Collections.Generic;
using System.IO;
using System.Text;
using Gibbed.IO;
using TF.Core.Entities;

namespace TF.Core.Projects.NightCry.Files
{
    public class TxtFile : TFFile
    {
        private class ItemData
        {
            public string Id { get; set; }
            public TFString English { get; set; }
            public string Japanese { get; set; }
        }

        private List<ItemData> _strings;

        public TxtFile(string fileName) : base(fileName)
        {
            
        }

        public override Endian Endianness => Endian.Little;
        public override Encoding Encoding => Encoding.UTF8;

        public override IList<TFString> Strings {
            get
            {
                var result = new List<TFString>();

                foreach (var item in _strings)
                {
                    result.Add(item.English);
                }
            
                return result;
            }
        }

        public override string FileType => "NightCry Txt";
        
        public override void Read(Stream s)
        {
            _strings = new List<ItemData>();

            var str = s.ReadStringZ(Encoding);
            
            var items = str.Split('■');

            foreach (var item in items)
            {
                var split = item.Split('◆');

                if (split.Length > 1)
                {
                    var itemData = new ItemData
                    {
                        Id = split[0], 
                        English = ReadString(split[1]), 
                        Japanese = split[2]
                    };

                    _strings.Add(itemData);
                }
            }
        }

        private TFString ReadString(string str)
        {
            var value = new TFString {FileId = Id, Offset = 0, Visible = false};

            if (!string.IsNullOrEmpty(str.TrimEnd('\0')))
            {
                str = NightCryProject.ReadingReplacements(str);
            }

            value.Original = str;
            value.Translation = str;
            value.Visible = !string.IsNullOrWhiteSpace(str);

            return value;
        }

        public override void Save(string fileName, byte[] originalContent, IList<TFString> strings, ExportOptions options)
        {
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                for (var i = 0; i < strings.Count; i++)
                {
                    var str = strings[i];
                    fs.WriteString(_strings[i].Id, options.SelectedEncoding);
                    fs.WriteString("◆", options.SelectedEncoding);
                    WriteString(fs, str, options);
                    fs.WriteString("◆", options.SelectedEncoding);
                    fs.WriteString(_strings[i].Japanese, options.SelectedEncoding);
                    fs.WriteString("■", options.SelectedEncoding);
                }
                fs.WriteString("\r\n");
            }
        }

        private void WriteString(Stream s, TFString str, ExportOptions options)
        {
            var writeStr = str.Translation;
            if (str.Original.Equals(str.Translation))
            {
                writeStr = NightCryProject.WritingReplacements(writeStr);
                s.WriteString(writeStr, Encoding);
            }
            else
            {
                if (options.CharReplacement != 0)
                {
                    writeStr = Utils.ReplaceChars(writeStr, options.CharReplacementList);
                }

                writeStr = NightCryProject.WritingReplacements(writeStr);
                s.WriteString(writeStr, options.SelectedEncoding);
            }
        }
    }
}
