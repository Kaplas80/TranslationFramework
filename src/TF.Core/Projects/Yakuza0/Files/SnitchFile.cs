using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gibbed.IO;
using TF.Core.Entities;

namespace TF.Core.Projects.Yakuza0.Files
{
    public class SnitchFile : TFFile
    {
        private class ItemData
        {
            public byte[] Unknown;
            public TFString Data;
            public byte[] Remainder;
        }

        public SnitchFile(string fileName) : base(fileName)
        {
            
        }

        public override Endian Endianness => Endian.Big;
        public override Encoding Encoding => Encoding.UTF8;
        public override string FileType => "snitch.bin";
        
        public override IList<TFString> Strings {
            get
            {
                var result = new List<TFString>();

                foreach (var item in _dataList)
                {
                    result.Add(item.Data);
                }
            
                return result;
            }
        }

        private byte[] _unknown;
        private List<ItemData> _dataList;

        public override void Read(Stream s)
        {
            _unknown = s.ReadBytes(0x18);

            _dataList = new List<ItemData>();

            s.Seek(8, SeekOrigin.Current);
            var firstStringOffset = s.ReadValueS32(Endianness);
            s.Seek(-12, SeekOrigin.Current);

            var stringOffsets = new List<int>();
            var remainderOffsets = new List<int>();
            var remainderSizes = new List<int>();

            while (s.Position < firstStringOffset)
            {
                s.Seek(8, SeekOrigin.Current);
                var offset = s.ReadValueS32(Endianness);
                stringOffsets.Add(offset);
                offset = s.ReadValueS32(Endianness);
                remainderOffsets.Add(offset);
            }

            var i = 0;
            for (i = 0; i < stringOffsets.Count - 1; i++)
            {
                remainderSizes.Add(stringOffsets[i + 1] - remainderOffsets[i]);
            }

            remainderSizes.Add((int) (s.Length - remainderOffsets[remainderOffsets.Count - 1]));

            s.Seek(0x18, SeekOrigin.Begin);

            i = 0;
            while (s.Position < firstStringOffset)
            {
                var value = new ItemData();
                value.Unknown = s.ReadBytes(8);
                value.Data = ReadString(s);
                value.Remainder = ReadRemainder(s, remainderSizes[i]);
                
                _dataList.Add(value);
                i++;
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

        private byte[] ReadRemainder(Stream s, int size)
        {
            var remainderOffset = s.ReadValueS32(Endianness);
            var pos = s.Position;

            var result = new byte[size];

            if (remainderOffset != 0)
            {
                s.Seek(remainderOffset, SeekOrigin.Begin);

                result = s.ReadBytes(size);
            }

            s.Seek(pos, SeekOrigin.Begin);

            return result;
        }

        public override void Save(string fileName, byte[] originalContent, IList<TFString> strings, ExportOptions options)
        {
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                fs.WriteBytes(_unknown);

                var currentOffset = _unknown.Length + 16 * strings.Count;

                for (var i = 0; i < _dataList.Count; i++)
                {
                    var item = _dataList[i];

                    fs.Seek(_unknown.Length + i * 16, SeekOrigin.Begin);

                    fs.WriteBytes(item.Unknown);

                    fs.WriteValueS32(currentOffset, Endianness);

                    var posAux = fs.Position;

                    fs.Seek(currentOffset, SeekOrigin.Begin);

                    var tfString = strings.FirstOrDefault(x => x.Offset == item.Data.Offset);

                    if (tfString != null)
                    {
                        var str = tfString.Translation;

                        if (!string.IsNullOrEmpty(str))
                        {
                            if (!str.Equals(tfString.Original))
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

                    fs.Seek(posAux, SeekOrigin.Begin);
                    fs.WriteValueS32(currentOffset, Endianness);
                    fs.Seek(currentOffset, SeekOrigin.Begin);

                    fs.WriteBytes(item.Remainder);
                    currentOffset += item.Remainder.Length;
                }
            }
        }
    }
}