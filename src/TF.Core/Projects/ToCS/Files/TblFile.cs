using System.Collections.Generic;
using System.IO;
using System.Text;
using Gibbed.IO;
using TF.Core.Entities;

namespace TF.Core.Projects.ToCS.Files
{
    public class TblFile : TFFile
    {
        private class ItemData
        {
            public string Id { get; set; }
            public byte[] Unknown { get; set; }
            public TFString Value { get; set; }
        }

        private List<ItemData> _strings;

        public TblFile(string fileName) : base(fileName)
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
                    result.Add(item.Value);
                }
            
                return result;
            }
        }

        public override string FileType => "ToCS tbl";
        
        public override void Read(Stream s)
        {
            var count = s.ReadValueU16(Endianness);

            _strings = new List<ItemData>(count);

            for (var i = 0; i < count; i++)
            {
                var id = s.ReadStringZ(Encoding);
                var unknown = s.ReadBytes(4);
                var value = ReadString(s);

                var itemData = new ItemData
                {
                    Id = id, 
                    Unknown = unknown,
                    Value = value
                };

                _strings.Add(itemData);
            }
        }

        private TFString ReadString(Stream s)
        {
            var offset = (int)s.Position;
            var str = s.ReadStringZ(Encoding);

            var value = new TFString {FileId = Id, Offset = offset, Visible = false};

            if (!string.IsNullOrEmpty(str.TrimEnd('\0')))
            {
                str = ToCSProject.ReadingReplacements(str);
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
                    //fs.WriteString(_strings[i].Japanese, options.SelectedEncoding);
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
                writeStr = ToCSProject.WritingReplacements(writeStr);
                s.WriteString(writeStr, Encoding);
            }
            else
            {
                if (options.CharReplacement != 0)
                {
                    writeStr = Utils.ReplaceChars(writeStr, options.CharReplacementList);
                }

                writeStr = ToCSProject.WritingReplacements(writeStr);
                s.WriteString(writeStr, options.SelectedEncoding);
            }
        }
    }
}
