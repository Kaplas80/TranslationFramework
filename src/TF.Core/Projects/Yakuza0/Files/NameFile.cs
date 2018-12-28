using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Gibbed.IO;
using TF.Core.Entities;

namespace TF.Core.Projects.Yakuza0.Files
{
    public class NameFile : TFFile
    {
        public NameFile(string fileName) : base(fileName)
        {
            
        }

        public override Endian Endianness => Endian.Big;
        public override Encoding Encoding => Encoding.UTF8;
        public override string FileType => "name_?.bin";
        
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

        public override void Read(Stream s)
        {
            _dataList = new List<TFString>();

            s.Seek(0x38, SeekOrigin.Begin);
            
            var tableOffset = s.ReadValueS32(Endianness);
            var stringCount = s.ReadValueS32(Endianness);

            s.Seek(tableOffset, SeekOrigin.Begin);

            for (var i = 0; i < stringCount; i++)
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

            s.Seek(pos, SeekOrigin.Begin);

            return value;
        }

        public override void Save(string fileName, byte[] originalContent, IList<TFString> strings, ExportOptions options)
        {
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                var header = new byte[0x174];
                Array.Copy(originalContent, header, 0x174);
                fs.WriteBytes(header);

                var remainder = new byte[4200];
                var originalRemainderOffset = (originalContent[0x28] << 24) + (originalContent[0x29] << 16) + 
                                              (originalContent[0x2A] << 8) + originalContent[0x2B];  
                
                Array.Copy(originalContent, originalRemainderOffset, remainder, 0, 4200);
                
                var stringOffsets = new List<int>();

                for (var i = 0; i < strings.Count; i++)
                {
                    stringOffsets.Add((int)fs.Position);
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
                        }
                        else
                        {
                            str = Yakuza0Project.WritingReplacements(str);

                            fs.WriteStringZ(str, Encoding);
                        }
                    }
                    else
                    {
                        // Hay que escribir solo el 0 del final
                        fs.WriteString("\0");
                    }
                }

                while (fs.Position % 16 != 0)
                {
                    fs.WriteByte(0);
                }

                var offsetsPointer = fs.Position;
                
                foreach (var offset in stringOffsets)
                {
                    fs.WriteValueS32(offset, Endianness);
                }

                var remainderPointer = fs.Position;
                fs.WriteBytes(remainder);

                fs.Seek(-4, SeekOrigin.Current);
                var lastPointer = fs.Position;
                fs.WriteValueS32((int)(remainderPointer + 8), Endianness);
                
                while (fs.Position % 16 != 0)
                {
                    fs.WriteByte(0);
                }

                var totalLength = fs.Position;

                fs.Seek(0x0C, SeekOrigin.Begin);
                fs.WriteValueS32((int) totalLength, Endianness);
                
                fs.Seek(0x28, SeekOrigin.Begin);
                fs.WriteValueS32((int) remainderPointer, Endianness);
                fs.WriteValueS32((int) lastPointer, Endianness);
                
                fs.Seek(0x34, SeekOrigin.Begin);
                fs.WriteValueS32((int) (remainderPointer + 4), Endianness);
                fs.WriteValueS32((int) offsetsPointer, Endianness);
            }
        }
    }
}