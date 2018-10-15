using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gibbed.IO;
using TF.Core.Entities;

namespace TF.Core.Projects.MegaManLegends.Files
{
    public class BinFile : TFFile
    {
        private class PackedFile
        {
            public int Offset;
            public byte[] Data;
        }

        private static readonly char[] CharArray = { 
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ' ', ',', '·', '!', '?', '·', 
            '.', ';', '·', ':', '-', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 
            'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 
            'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 
            'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '(', ')', '┌', '┘', '\'', '"', '+', 
            '-', '=', '/', '$', '&', '─', '○', '△', '╳', '□', '[', ']', '★' , '\\', 'Ω', 'α'
        };

        private List<PackedFile> _files;
        private List<TFString> _strings;

        public BinFile(string fileName) : base(fileName)
        {
            _files = new List<PackedFile>();
            _strings = new List<TFString>();
        }

        public override Endian Endianness => Endian.Little;
        public override Encoding Encoding => Encoding.GetEncoding("ISO-8859-1");

        public override string FileType => "MegaMan Legends BIN";
        
        public override IList<TFString> Strings {
            get
            {
                var result = new List<TFString>();

                foreach (var s in _strings)
                {
                    result.Add(s);
                }
                
            
                return result;
            }
        }

        public override void Read(Stream s)
        {
            var firstOffset = s.ReadValueS32(Endianness);
            var numFiles = firstOffset / 4 - 1;
            s.Seek(0, SeekOrigin.Begin);
            
            var offsets = new int[numFiles + 1];
                
            for (var i = 0; i <= numFiles; i++)
            {
                offsets[i] = s.ReadValueS32(Endian.Little);
            }

            for (var i = 0; i < numFiles; i++)
            {
                var size = offsets[i + 1] - offsets[i];
                s.Seek(offsets[i], SeekOrigin.Begin);

                var file = new PackedFile {Offset = offsets[i], Data = s.ReadBytes(size)};

                _files.Add(file);
            }

            foreach (var file in _files)
            {
                var data = file.Data;

                if (data[0] == 0x42 && data[1] == 0x44 && data[2] == 0x1F && data[3] == 0x00)
                {
                    ProcessBDFile(file);
                }
            }
        }

        private void ProcessBDFile(PackedFile file)
        {
            using (var s = new MemoryStream(file.Data))
            {
                s.Seek(8, SeekOrigin.Begin);
                var firstOffset = s.ReadValueS16(Endianness);
                var numItems = firstOffset / 2;
                s.Seek(8, SeekOrigin.Begin);

                var offsets = new short[numItems + 1];
                for (var i = 0; i < numItems; i++)
                {
                    offsets[i] = (short) (s.ReadValueS16(Endian.Little) + 8);
                }

                offsets[numItems] = (short) (s.Length);

                for (var i = 0; i < numItems; i++)
                {
                    var size = offsets[i + 1] - offsets[i];
                    s.Seek(offsets[i], SeekOrigin.Begin);

                    var data = s.ReadBytes(size);

                    var readStrings = ProcessItem(data);

                    _strings.AddRange(readStrings);
                }
            }
        }

        private IList<TFString> ProcessItem(byte[] item)
        {
            var result = new List<TFString>();

            using (var s = new MemoryStream(item))
            {
                var startPos = s.FindPattern(new byte[] {0x8F, 0x00, 0x08});

                if (startPos != -1)
                {
                    s.Seek(startPos, SeekOrigin.Begin);

                    while (s.Position < s.Length)
                    {
                        var type = s.ReadValueU8();

                        switch (type)
                        {
                            case 0x82:
                            case 0xE9:
                            {
                                var tfString = ReadString(s);
                                if (tfString.Original != null)
                                {
                                    result.Add(tfString);
                                }
                                break;
                            }
                            case 0x8B:
                            {
                                s.Seek(4, SeekOrigin.Current);
                                var tfString = ReadString(s);
                                if (tfString.Original != null)
                                {
                                    result.Add(tfString);
                                }
                                break;
                            }
                            
                            case 0x83:
                            case 0x86:
                            case 0x87:
                            case 0x8A:
                            case 0x9C:
                            {
                                s.Seek(2, SeekOrigin.Current);
                                var tfString = ReadString(s);
                                if (tfString.Original != null)
                                {
                                    result.Add(tfString);
                                }
                                break;
                            }
                            
                            case 0xA0:
                            {
                                s.Seek(3, SeekOrigin.Current);
                                var tfString = ReadString(s);
                                if (tfString.Original != null)
                                {
                                    result.Add(tfString);
                                }
                                break;
                            }

                            case 0x85:
                            case 0xAB:
                            {
                                s.Seek(1, SeekOrigin.Current);
                                break;
                            }

                            case 0x8F:
                            case 0x92:
                            case 0x95:
                            case 0x96:
                            {
                                s.Seek(2, SeekOrigin.Current);
                                break;
                            }
                            
                            case 0x93:
                            case 0x9B:
                            case 0xFF:
                            {
                                s.Seek(3, SeekOrigin.Current);
                                break;
                            }

                            case 0xBB:
                            case 0xA5:
                            {
                                s.Seek(4, SeekOrigin.Current);
                                break;
                            }

                            case 0x88:
                            {
                                s.Seek(6, SeekOrigin.Current);
                                break;
                            }
                            
                            case 0xCC:
                            {
                                s.Seek(7, SeekOrigin.Current);
                                break;
                            }
                            default:
                            {
                                if (type > 0x09 && type < 0x60)
                                {
                                    s.Seek(-1, SeekOrigin.Current);
                                    var tfString = ReadString(s);
                                    if (tfString.Original != null)
                                    {
                                        result.Add(tfString);
                                    }
                                }

                                break;
                            }
                        }
                    }
                }
            }

            return result;
        }

        private TFString ReadString(Stream s)
        {
            var sb = new StringBuilder();
            var input = s.ReadValueU8();
            while (input < 0x60 || input == 0x82)
            {
                if (input == 0x82)
                {
                    sb.Append("\\n");
                }
                else
                {
                    sb.Append(CharArray[input]);
                }
                input = s.ReadValueU8();
            }

            s.Seek(-1, SeekOrigin.Current);

            var str = sb.ToString();

            var value = new TFString
            {
                FileId = Id,
                Offset = (int) s.Position,
                Visible = false,
            };

            if (!string.IsNullOrEmpty(str))
            {
                value.Visible = true;
                value.Original = str;
                value.Translation = str;
            }

            return value;
        }
        
        public override void Save(string fileName, byte[] originalContent, IList<TFString> strings,
            ExportOptions options)
        {
            
        }
    }
}
