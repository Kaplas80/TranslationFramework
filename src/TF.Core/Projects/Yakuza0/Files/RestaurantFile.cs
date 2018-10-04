using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gibbed.IO;
using TF.Core.Entities;

namespace TF.Core.Projects.Yakuza0.Files
{
    public class RestaurantFile : TFFile
    {
        private class ItemData
        {
            public int[] Unknown;
            public TFString Data;

            public ItemData()
            {
                Unknown = new int[12];
            }
        }

        public RestaurantFile(string fileName) : base(fileName)
        {
            
        }

        public override Endian Endianness => Endian.Big;
        public override Encoding Encoding => Encoding.UTF8;
        public override string FileType => "restaurant .bin";
        
        public override IList<TFString> Strings {
            get
            {
                var result = new List<TFString>();

                foreach (var item in _dataList1)
                {
                    result.Add(item);
                }

                foreach (var item in _dataList2)
                {
                    result.Add(item.Data);
                }
            
                return result;
            }
        }

        private int _unknown1;
        private short _unknown2;
        private short _num;
        private int _unknown3;
        private int _table2Offset;
        private byte[] _remainder1;
        private byte[] _remainder2;

        private List<TFString> _dataList1;
        private List<ItemData> _dataList2;

        public override void Read(Stream s)
        {
            _unknown1 = s.ReadValueS32(Endianness);
            _unknown2 = s.ReadValueS16(Endianness);
            _num = s.ReadValueS16(Endianness);
            _unknown3 = s.ReadValueS32(Endianness);
            _table2Offset = s.ReadValueS32(Endianness);

            _dataList1 = new List<TFString>();
            _dataList2 = new List<ItemData>();

            var firstString = -1;

            for (var i = 0; i < 43; i++)
            {
                var str = ReadString(s);
                _dataList1.Add(str);

                if (firstString == -1)
                {
                    firstString = str.Offset;
                }
            }

            _remainder1 = s.ReadBytes(0x54);
            
            for (var i = 0; i < _num; i++)
            {
                var item = new ItemData();
                for (var j = 0; j < 12; j++)
                {
                    item.Unknown[j] = s.ReadValueS32(Endianness);
                }

                if (firstString == -1)
                {
                    firstString = item.Unknown[8];
                }

                item.Data = ReadString(s, item.Unknown[8]);
                
                _dataList2.Add(item);
            }

            var remainderSize = (int) (firstString - s.Position);

            if (remainderSize > 0)
            {
                _remainder2 = ReadRemainder(s, remainderSize);
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

        private TFString ReadString(Stream s, int offset)
        {
            var pos = s.Position;

            var value = new TFString {FileId = Id, Offset = offset, Visible = false};

            if (offset != 0)
            {
                s.Seek(offset, SeekOrigin.Begin);

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
            var pos = s.Position;

            var result = s.ReadBytes(size);

            s.Seek(pos, SeekOrigin.Begin);

            return result;
        }

        public override void Save(string fileName, byte[] originalContent, IList<TFString> strings, ExportOptions options)
        {
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                fs.WriteValueS32(_unknown1, Endianness);
                fs.WriteValueS16(_unknown2, Endianness);
                fs.WriteValueS16(_num, Endianness);
                fs.WriteValueS32(_unknown3, Endianness);
                fs.WriteValueS32(_table2Offset, Endianness);

                var r2Size = 0;
                if (_remainder2 != null)
                {
                    r2Size = _remainder2.Length;
                }
                
                var currentOffset = 16 + _dataList1.Count * 4 + _remainder1.Length + _dataList2.Count * 48 + r2Size;
                
                for (var i = 0; i < _dataList1.Count; i++)
                {
                    var item = _dataList1[i];
                    
                    fs.Seek(16 + i * 4, SeekOrigin.Begin);
                    if (item.Offset == 0)
                    {
                        fs.WriteValueS32(0);
                    }
                    else
                    {
                        var tfString = strings.FirstOrDefault(x => x.Offset == item.Offset);
                        fs.WriteValueS32(currentOffset, Endianness);

                        fs.Seek(currentOffset, SeekOrigin.Begin);

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
                }

                fs.Seek(16 + _dataList1.Count * 4, SeekOrigin.Begin);
                fs.WriteBytes(_remainder1);

                for (var i = 0; i < _dataList2.Count; i++)
                {
                    var item = _dataList2[i];
                    
                    fs.Seek(_table2Offset + i * 48, SeekOrigin.Begin);

                    for (var j = 0; j < 8; j++)
                    {
                        fs.WriteValueS32(item.Unknown[j], Endianness);
                    }
                    fs.WriteValueS32(currentOffset, Endianness);
                    for (var j = 9; j < 12; j++)
                    {
                        fs.WriteValueS32(item.Unknown[j], Endianness);
                    }

                    var tfString = strings.FirstOrDefault(x => x.Offset == item.Data.Offset);

                    fs.Seek(currentOffset, SeekOrigin.Begin);

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

                if (_remainder2 != null)
                {
                    fs.Seek(_table2Offset + _num * 48, SeekOrigin.Begin);
                    fs.WriteBytes(_remainder2);
                }
            }
        }
    }
}