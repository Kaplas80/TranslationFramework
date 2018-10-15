using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gibbed.IO;
using TF.Core.Entities;

namespace TF.Core.Projects.Yakuza0.Files
{
    // Agrupación de ficheros .msg
    public class PacFile : TFFile
    {
        private class File
        {
            public int Unknown;
            
            public int OffsetMsg;
            public short SizeMsg;
            public MsgFile Msg;
            public byte[] MsgBytes;

            public int OffsetRemainder;
            public short SizeRemainder;
            public byte[] Remainder;
        }

        public override Endian Endianness => Endian.Big;
        public override Encoding Encoding => Encoding.UTF8;
        public override string FileType => "Pac Bin";

        private List<File> _files;

        public PacFile(string fileName) : base(fileName)
        {
            _files = new List<File>();
        }

        public override IList<TFString> Strings {
            get
            {
                var result = new List<TFString>();

                foreach (var file in _files)
                {
                    if (file.SizeMsg > 0)
                    {
                        result.AddRange(file.Msg.Strings);
                    }
                }

                return result;
            }
        }

        public override void Read(Stream s)
        {
            var numFiles = s.ReadValueS16(Endianness);

            for (var i = 0; i < numFiles; i++)
            {
                s.Seek(8 + i * 16, SeekOrigin.Begin);

                var f = new File
                {
                    Unknown = s.ReadValueS32(Endianness),
                    OffsetMsg = s.ReadValueS32(Endianness),
                    OffsetRemainder = s.ReadValueS32(Endianness),
                    SizeMsg = s.ReadValueS16(Endianness),
                    SizeRemainder = s.ReadValueS16(Endianness)
                };

                if (f.OffsetMsg != 0)
                {
                    s.Seek(f.OffsetMsg, SeekOrigin.Begin);
                    f.MsgBytes = s.ReadBytes(f.SizeMsg);

                    var msgFile = new MsgFile(string.Empty);
                    using (var ms = new MemoryStream(f.MsgBytes))
                    {
                        msgFile.Read(ms);
                    }

                    if (msgFile.Strings.Count > 0)
                    {
                        foreach (var tfString in msgFile.Strings)
                        {
                            tfString.Section = f.OffsetMsg.ToString("X8");
                            tfString.FileId = Id;
                        }
                    }

                    f.Msg = msgFile;
                }

                if (f.OffsetRemainder != 0)
                {
                    s.Seek(f.OffsetRemainder, SeekOrigin.Begin);
                    f.Remainder = s.ReadBytes(f.SizeRemainder);
                }

                _files.Add(f);
            }
        }

        public override void Save(string fileName, byte[] originalContent, IList<TFString> strings, ExportOptions options)
        {
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                fs.WriteValueS16((short) _files.Count, Endianness);
                fs.WriteValueS16(0);
                fs.WriteValueS32(8, Endianness);

                var fileOffset = 8 + _files.Count * 16 + 8;

                for (var i = 0; i < _files.Count; i++)
                {
                    fs.Seek(8 + i * 16, SeekOrigin.Begin);

                    var file = _files[i];
                    fs.WriteValueS32(file.Unknown, Endianness);

                    short newMsgSize;
                    if (file.SizeMsg > 0)
                    {
                        fs.WriteValueS32(fileOffset, Endianness);
                        
                        var pos = fs.Position;
                        fs.Seek(fileOffset, SeekOrigin.Begin);
                        if (file.Msg.Strings.Count > 0)
                        {
                            var msgStrings = strings.Where(x => x.Section == file.OffsetMsg.ToString("X8")).ToList();
                            byte[] newMsg;
                            using (var ms = new MemoryStream())
                            {
                                file.Msg.Save(ms, file.MsgBytes, msgStrings, options);

                                ms.Seek(0, SeekOrigin.End);
                                while (ms.Position % 4 != 0)
                                {
                                    ms.WriteByte(0);
                                }

                                newMsg = ms.ToArray();
                            }
                            
                            fs.WriteBytes(newMsg);
                            newMsgSize = (short) newMsg.Length;
                        }
                        else
                        {
                            fs.WriteBytes(file.MsgBytes);
                            newMsgSize = file.SizeMsg;
                        }

                        fs.Seek(pos, SeekOrigin.Begin);
                    }
                    else
                    {
                        fs.WriteValueS32(0);
                        newMsgSize = 0;
                    }

                    fileOffset += newMsgSize;

                    if (file.SizeRemainder > 0)
                    {
                        fs.WriteValueS32(fileOffset, Endianness);
                        var pos = fs.Position;
                        fs.Seek(fileOffset, SeekOrigin.Begin);
                        fs.WriteBytes(file.Remainder);

                        while (fs.Position % 4 != 0)
                        {
                            fs.WriteByte(0);
                            fileOffset++;
                        }

                        fs.Seek(pos, SeekOrigin.Begin);
                    }
                    else
                    {
                        fs.WriteValueS32(0);
                    }

                    fileOffset += file.SizeRemainder;
                    fs.WriteValueS16(newMsgSize, Endianness);
                    fs.WriteValueS16(file.SizeRemainder, Endianness);
                }
            }
        }
    }
}
