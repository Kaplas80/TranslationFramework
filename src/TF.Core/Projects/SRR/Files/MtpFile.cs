using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gibbed.IO;
using TF.Core.Entities;

namespace TF.Core.Projects.SRR.Files
{
    public class MtpFile : TFFile
    {
        private class MtpData
        {
            public MtpHeader Header;
            public MtpContent Content;
            public byte[] Ending;

            public MtpData()
            {
                Header = new MtpHeader();
                Content = new MtpContent();
            }
        }

        private class MtpHeader
        {
            public byte[] Signature;
            public int Size1;
            public int ContentOffset;
            public int Unknown1;
            public int Unknown2;
            public int ContentSize;
            public long Unknown3;
        }

        private class MtpContent
        {
            public int Unknown1;
            public int StringCount1;
            public int Array2ItemSize;
            public int StringCount2;
            public int Unknown3;
            public int Unknown4;
            public int Language; // 4 para inglés

            public IList<DataItem> Strings;

            public MtpContent()
            {
                Strings = new List<DataItem>();
            }
        }

        private class StringIndex
        {
            public int Unknown1;
            public int Unknown2;
            public int StringOffset;
        }

        private class DataItem
        {
            public int Index1;
            public StringIndex Index2;
            public int Lenght;
            public TFString Data;

            public DataItem()
            {
                Index2 = new StringIndex();
            }
        }

        private MtpData _root;

        public MtpFile(string fileName) : base(fileName)
        {
            
        }

        public override Endian Endianness => Endian.Little;
        public override Encoding Encoding => Encoding.UTF8;

        public override string FileType => "MTP";
        
        public override IList<TFString> Strings {
            get
            {
                return _root.Content.Strings.Select(item => item.Data).ToList();
            }
        }

        public override void Read(Stream s)
        {
            _root = new MtpData();

            ReadHeader(s);
            ReadContent(s);
            ReadEnding(s);
        }

        private void ReadHeader(Stream s)
        {
            s.Seek(0, SeekOrigin.Begin);

            var header = _root.Header;

            header.Signature = s.ReadBytes(4);
            header.Size1 = s.ReadValueS32(Endianness);
            header.ContentOffset = s.ReadValueS32(Endianness);
            header.Unknown1 = s.ReadValueS32(Endianness);
            header.Unknown2 = s.ReadValueS32(Endianness);
            header.ContentSize = s.ReadValueS32(Endianness);
            header.Unknown3 = s.ReadValueS64(Endianness);
        }

        private void ReadContent(Stream s)
        {
            s.Seek(0x20, SeekOrigin.Begin);

            var content = _root.Content;

            content.Unknown1 = s.ReadValueS32(Endianness);
            content.StringCount1 = s.ReadValueS32(Endianness);
            content.Array2ItemSize = s.ReadValueS32(Endianness);
            content.StringCount2 = s.ReadValueS32(Endianness);
            content.Unknown3 = s.ReadValueS32(Endianness);
            if (content.Array2ItemSize == 3)
            {
                content.Unknown4 = s.ReadValueS32(Endianness);
            }
            content.Language = s.ReadValueS32(Endianness);

            var array1Base = s.Position;
            var array2Base = array1Base + 4 * content.StringCount1;
            var stringsBase = array2Base + content.Array2ItemSize * 4 * content.StringCount1;

            for (var i = 0; i < content.StringCount1; i++)
            {
                var item = new DataItem();

                s.Seek(array1Base + 4 * i, SeekOrigin.Begin);
                item.Index1 = s.ReadValueS32(Endianness);

                s.Seek(array2Base + 4 * item.Index1, SeekOrigin.Begin);
                item.Index2.Unknown1 = s.ReadValueS32(Endianness);
                if (content.Array2ItemSize == 3)
                {
                    item.Index2.Unknown2 = s.ReadValueS32(Endianness);
                }
                item.Index2.StringOffset = s.ReadValueS32(Endianness);

                s.Seek(stringsBase + item.Index2.StringOffset, SeekOrigin.Begin);
                item.Lenght = s.ReadValueS32(Endianness);
                item.Data = ReadString(s);

                content.Strings.Add(item);
            }
        }

        private void ReadEnding(Stream s)
        {
            s.Seek(_root.Header.ContentSize + 0x20, SeekOrigin.Begin);

            _root.Ending = s.ReadBytes(0x80);
        }

        private TFString ReadString(Stream s)
        {
            var stringOffset = (int) s.Position;
            
            var value = new TFString {FileId = Id, Offset = stringOffset, Visible = false};

            var str = s.ReadStringZ(Encoding);

            if (!string.IsNullOrEmpty(str.TrimEnd('\0')))
            {
                str = SRRProject.ReadingReplacements(str);
            }

            value.Original = str;
            value.Translation = str;
            value.Visible = !string.IsNullOrWhiteSpace(str);

            return value;
        }

        public override void Save(string fileName, byte[] originalContent, IList<TFString> strings,
            ExportOptions options)
        {
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                fs.Seek(0x20, SeekOrigin.Begin);

                var content = _root.Content;
                fs.WriteValueS32(content.Unknown1, Endianness);
                fs.WriteValueS32(content.StringCount1, Endianness);
                fs.WriteValueS32(content.Array2ItemSize, Endianness);
                fs.WriteValueS32(content.StringCount2, Endianness);
                fs.WriteValueS32(content.Unknown3, Endianness);

                if (content.Array2ItemSize == 3)
                {
                    fs.WriteValueS32(content.Unknown4, Endianness);
                }
                fs.WriteValueS32(content.Language, Endianness);

                var array1Base = fs.Position;
                var array2Base = array1Base + 4 * content.StringCount1;
                var stringsBase = array2Base + content.Array2ItemSize * 4 * content.StringCount1;

                var stringOffset = 0;

                for (var i = 0; i < content.StringCount1; i++)
                {
                    var item = content.Strings[i];

                    fs.Seek(array1Base + 4 * i, SeekOrigin.Begin);
                    fs.WriteValueS32(item.Index1, Endianness);

                    fs.Seek(array2Base + 4 * item.Index1, SeekOrigin.Begin);
                    fs.WriteValueS32(item.Index2.Unknown1, Endianness);
                    if (content.Array2ItemSize == 3)
                    {
                        fs.WriteValueS32(item.Index2.Unknown2, Endianness);
                    }
                    fs.WriteValueS32(stringOffset, Endianness);

                    fs.Seek(stringsBase + stringOffset, SeekOrigin.Begin);

                    var tfString = strings[i];
                    var str = tfString.Original;

                    if (tfString.Visible && !string.IsNullOrEmpty(str))
                    {
                        str = tfString.Translation;
                    }

                    if (options.CharReplacement != 0)
                    {
                        str = Utils.ReplaceChars(str, options.CharReplacementList);
                    }

                    str = SRRProject.WritingReplacements(str);

                    var length = str.GetLength(options.SelectedEncoding);
                    fs.WriteValueS32(length, Endianness);

                    fs.WriteStringZ(str, options.SelectedEncoding);

                    stringOffset = (int) (fs.Position - stringsBase);

                    while (fs.Position % 4 != 0)
                    {
                        fs.WriteValueU8(0);

                        stringOffset++;
                    }
                }

                while (fs.Position % 16 != 0)
                {
                    fs.WriteValueU8(0);
                }

                var contentSize = (int) (fs.Position - 0x20);

                fs.WriteBytes(_root.Ending);

                WriteHeader(fs, contentSize);
            }
        }

        private void WriteHeader(Stream s, int contentSize)
        {
            s.Seek(0, SeekOrigin.Begin);
            s.WriteBytes(_root.Header.Signature);
            s.WriteValueS32(contentSize + 0x60, Endianness); 
            s.WriteValueS32(_root.Header.ContentOffset, Endianness);
            s.WriteValueS32(_root.Header.Unknown1, Endianness);
            s.WriteValueS32(_root.Header.Unknown2, Endianness);
            s.WriteValueS32(contentSize, Endianness); 
            s.WriteValueS64(_root.Header.Unknown3, Endianness);
        }
    }
}
