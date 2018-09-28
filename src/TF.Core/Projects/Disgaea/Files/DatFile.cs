using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gibbed.IO;
using TF.Core.Entities;

namespace TF.Core.Projects.Disgaea.Files
{
    public class DatFile : TFFile
    {
        private class DatData
        {
            public int ItemCount;

            public IList<ItemData> Data;

            public DatData()
            {
                Data = new List<ItemData>();
            }
        }

        private class ItemData
        {
            public int Offset;
            public byte[] Flags;
            public int Size;

            public byte[] BinaryContent;
        }

        private List<TFString> _strings;
        private DatData _root;

        public DatFile(string fileName) : base(fileName)
        {
            _strings = new List<TFString>();
        }

        public override Endian Endianness => Endian.Little;
        public override Encoding Encoding => Encoding.GetEncoding("shift-jis");

        public override string FileType => "Disgaea TALK.DAT";
        
        public override IList<TFString> Strings {
            get
            {
                var result = new List<TFString>();

                foreach (var item in _strings)
                {
                    result.Add(item);
                }
            
                return result;
            }
        }

        public override void Read(Stream s)
        {
            _root = new DatData();

            _root.ItemCount = s.ReadValueS32(Endianness);

            s.ReadValueS32(Endianness);

            var stringBase = 8 + _root.ItemCount * 32;

            for (var i = 0; i < _root.ItemCount; i++)
            {
                var item = new ItemData
                {
                    Offset = stringBase + s.ReadValueS32(Endianness), 
                    Flags = s.ReadBytes(28)
                };

                _root.Data.Add(item);
            }

            for (var i = 0; i < _root.ItemCount - 1; i++)
            {
                _root.Data[i].Size = _root.Data[i + 1].Offset - _root.Data[i].Offset;
            }
            
            _root.Data[_root.ItemCount - 1].Size = (int)(s.Length - _root.Data[_root.ItemCount - 1].Offset);

            for (var i = 0; i < _root.ItemCount; i++)
            {
                var item = _root.Data[i];
                s.Seek(item.Offset, SeekOrigin.Begin);
                item.BinaryContent = s.ReadBytes(item.Size);

                ExtractStrings(item);
            }
        }

        private void ExtractStrings(ItemData item)
        {
            byte[] stringStart = { 0x01 };

            var currentIndex = Utils.FindPattern(item.BinaryContent, stringStart, 0);

            while (currentIndex != -1)
            {
                if (currentIndex < item.Size - 2)
                {
                    if (IsValidShiftJis(item.BinaryContent[currentIndex + 1], item.BinaryContent[currentIndex + 2]))
                    {
                        var tfString = ReadString(item, currentIndex + 1);
                        _strings.Add(tfString);
                    }
                }

                currentIndex = Utils.FindPattern(item.BinaryContent, stringStart, currentIndex + 1);
            }
        }

        private TFString ReadString(ItemData item, int index)
        {
            var stringOffset = item.Offset + index;
            
            var value = new TFString {FileId = Id, Offset = stringOffset, Visible = false};

            var array = new byte[item.Size - index];
            Array.Copy(item.BinaryContent, index, array, 0, array.Length);
            using (var ms = new MemoryStream(array))
            {
                var str = ms.ReadStringZ(Encoding);

                if (!string.IsNullOrEmpty(str.TrimEnd('\0')))
                {
                    str = DisgaeaProject.ReadingReplacements(str);
                }

                value.Original = str;
                value.Translation = ReplaceToAscii(str);
                value.Visible = !string.IsNullOrWhiteSpace(str);
            }

            return value;
        }

        private bool IsValidShiftJis(byte b1, byte b2)
        {
            if (b1 < 0x81 || (b1 > 0x9F && b1 < 0xE0) || (b1 > 0xEA && b1 < 0xED) || (b1 > 0xEE && b1 < 0xF0) || b1 > 0xFB)
            {
                return false;
            }

            if (b2 < 0x3F || b2 > 0xEE)
            {
                return false;
            }

            return true;
        }

        private string ReplaceToAscii(string input)
        {
            var JASCII = "\u3000ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ０１２３４５６７８９ー？！（）．，’”／＜＞＋＾＃＆＊｀～＄＠％：";
            var ASCII = " abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-?!().,'\"/<>+^#&*`~$@%:";

            var result = input;
            for (var i = 0; i < JASCII.Length; i++)
            {
                result = result.Replace(JASCII[i], ASCII[i]);
            }

            return result;
        }

        public override void Save(string fileName, byte[] originalContent, IList<TFString> strings,
            ExportOptions options)
        {
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                fs.WriteValueS32(_root.ItemCount, Endianness);
                fs.WriteValueS32(_root.ItemCount, Endianness);

                var tableBase = fs.Position;
                var stringBase = 8 + _root.ItemCount * 32;
                var currentOffset = _root.Data[0].Offset - stringBase;

                for (var i = 0; i < _root.ItemCount; i++)
                {
                    var item = _root.Data[i];

                    fs.Seek(tableBase + i * 32, SeekOrigin.Begin);
                    fs.WriteValueS32(currentOffset, Endianness);
                    fs.WriteBytes(item.Flags);

                    fs.Seek(stringBase + currentOffset, SeekOrigin.Begin);

                    var itemLength = WriteItemContent(fs, item, strings, options);

                    currentOffset += itemLength;
                }
            }
        }

        private int WriteItemContent(Stream s, ItemData item, IList<TFString> strings, ExportOptions options)
        {
            var result = 0;
            const byte stringStart = 0x01;

            var i = 0;

            while (i < item.Size)
            {
                if (item.BinaryContent[i] != stringStart)
                {
                    s.WriteByte(item.BinaryContent[i]);
                    result++;
                    i++;
                }
                else
                {
                    if (i < item.Size - 2) 
                    {
                        if (IsValidShiftJis(item.BinaryContent[i + 1], item.BinaryContent[i + 2]))
                        {
                            var tfString = strings.FirstOrDefault(x => x.Offset == item.Offset + i + 1);

                            if (tfString.Visible)
                            {
                                var str = tfString.Translation;

                                if (options.CharReplacement != 0)
                                {
                                    str = Utils.ReplaceChars(str, options.CharReplacementList);
                                }

                                str = DisgaeaProject.WritingReplacements(str);

                                str = ReplaceToShiftJis(str);

                                var bytes = Encoding.GetBytes(str);

                                s.WriteByte(stringStart);
                                s.WriteBytes(bytes);
                                s.WriteByte(0);

                                var originalLength = tfString.Original.GetLength(Encoding) + 1;
                                var newLength = bytes.Length + 2;

                                i = i + originalLength + 1;
                                result = result + newLength;
                            }
                            else
                            {
                                s.WriteByte(item.BinaryContent[i]);
                                result++;
                                i++;
                            }
                        }
                        else
                        {
                            s.WriteByte(item.BinaryContent[i]);
                            result++;
                            i++;
                        }
                    }
                    else
                    {
                        s.WriteByte(item.BinaryContent[i]);
                        result++;
                        i++;
                    }
                }
            }

            return result;
        }

        private string ReplaceToShiftJis(string input)
        {
            var JASCII = "\u3000ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ０１２３４５６７８９ー？！（）．，’”／＜＞＋＾＃＆＊｀～＄＠％：";
            var ASCII = " abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-?!().,'\"/<>+^#&*`~$@%:";

            var result = input;
            for (var i = 0; i < ASCII.Length; i++)
            {
                result = result.Replace(ASCII[i], JASCII[i]);
            }

            return result;
        }
    }
}
