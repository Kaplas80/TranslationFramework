using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gibbed.IO;
using TF.Core.Entities;

namespace TF.Core.Projects.SAO_HF.Files
{
    public class Ofs3File : TFFile
    {
        private struct FileInfo
        {
            public uint Offset;
            public uint Size;
        }

        private class Ofs3Data
        {
            public long StartOffset { get; set; }

            public uint Signature { get; set; }
            public uint HeaderSize { get; set; }
            public ushort Type { get; set; }
            public byte Padding { get; set; }
            public byte SubType { get; set; }
            public uint Size { get; set; }
            public uint FileCount { get; set; }

            public FileInfo[] FilesInfo { get; set; }

            public Ofs3Data[] Files { get; set; }
        }

        private Ofs3Data _root;
        private List<TFString> _strings { get; set; }

        public Ofs3File(string fileName) : base(fileName)
        {
            _strings = new List<TFString>();
        }

        public override Endian Endianness => Endian.Little;
        public override Encoding Encoding => Encoding.UTF8;

        public override string FileType => "OFS3";
        
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
            _root = new Ofs3Data();
            ReadFile(s, _root);
        }

        private void ReadFile(Stream s, Ofs3Data file)
        {
            var signature = s.ReadValueU32(Endianness);
            s.Seek(-4, SeekOrigin.Current);
            if (signature == 0x3353464F)
            {
                ReadOfs3(s, file);
            }
            else
            {
                ReadStrings(s, file);
            }
        }

        private void ReadOfs3(Stream s, Ofs3Data file)
        {
            file.StartOffset = s.Position;

            file.Signature = s.ReadValueU32(Endianness);
            file.HeaderSize = s.ReadValueU32(Endianness);
            file.Type = s.ReadValueU16(Endianness);
            file.Padding = s.ReadValueU8();
            file.SubType = s.ReadValueU8();
            file.Size = s.ReadValueU32(Endianness);
            file.FileCount = s.ReadValueU32(Endianness);

            file.FilesInfo = new FileInfo[file.FileCount];

            for (var i = 0; i < file.FileCount; i++)
            {
                var offset = s.ReadValueU32(Endianness) + file.HeaderSize;
                var size = s.ReadValueU32(Endianness);

                var t = new FileInfo {Offset = offset, Size = size};

                file.FilesInfo[i] = t;
            }

            file.Files = new Ofs3Data[file.FileCount];
            for (var i = 0; i < file.FileCount; i++)
            {
                file.Files[i] = new Ofs3Data();
                var offset = file.FilesInfo[i].Offset;
                s.Seek(offset + file.StartOffset, SeekOrigin.Begin);
                ReadFile(s, file.Files[i]);
            }
        }

        private void ReadStrings(Stream s, Ofs3Data file)
        {
            file.StartOffset = s.Position;
            file.Padding = 0x04;

            var stringOffset = (int) s.Position;
            
            var value = new TFString {FileId = Id, Offset = stringOffset, Visible = false};

            if (stringOffset != 0)
            {
                var str = s.ReadStringZ(Encoding);

                if (!string.IsNullOrEmpty(str.TrimEnd('\0')))
                {
                    str = SAOProject.ReadingReplacements(str);
                }

                value.Original = str;
                value.Translation = str;
                value.Visible = !string.IsNullOrWhiteSpace(str);
            }

            _strings.Add(value);
        }

        public override void Save(string fileName, byte[] originalContent, IList<TFString> strings, ExportOptions options)
        {
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                WriteFile(fs, strings, options, _root);
            }
        }

        private int WriteFile(Stream s, IList<TFString> strings, ExportOptions options, Ofs3Data file)
        {
            if (file.Signature == 0x3353464F)
            {
                return WriteOfs3(s, strings, options, file);
            }
            else
            {
                return WriteStrings(s, strings, options, file);
            }
        }

        private int WriteOfs3(Stream s, IList<TFString> strings, ExportOptions options, Ofs3Data file)
        {
            var fileStart = s.Position;

            s.WriteValueU32(file.Signature, Endianness);
            s.WriteValueU32(file.HeaderSize, Endianness);
            s.WriteValueU16(file.Type, Endianness);
            s.WriteValueU8(file.Padding);
            s.WriteValueU8(file.SubType);

            var sizeOffset = s.Position;
            s.WriteValueU32(0, Endianness); // Tamaño total, todavía desconocido

            s.WriteValueU32(file.FileCount);

            var result = 4; //El filecount entra dentro del tamaño del fichero.

            var tableOffset = s.Position;
            for (var i = 0; i < file.FileCount; i++)
            {
                s.WriteValueU32(0, Endianness);

                if (file.SubType == 1)
                {
                    s.WriteValueU32(file.FilesInfo[i].Size);
                }
                else
                {
                    s.WriteValueU32(0, Endianness);
                }

                result += 8;
            }

            while (s.Position % file.Padding != 0)
            {
                s.WriteValueU8(0);
                result++;
            }

            // Estoy en la posición para escribir el primer fichero hijo
            for (var i = 0; i < file.FileCount; i++)
            {
                var offset = (int)(s.Position - fileStart - file.HeaderSize);
                var size = WriteFile(s, strings, options, file.Files[i]);
                result += size;

                var aux = s.Position;

                s.Seek(tableOffset, SeekOrigin.Begin);
                s.WriteValueS32(offset, Endianness);

                if (file.SubType != 1)
                {
                    s.WriteValueS32(size, Endianness);
                }
                else
                {
                    s.Seek(4, SeekOrigin.Current);
                }

                tableOffset = s.Position;

                s.Seek(aux, SeekOrigin.Begin);
            }

            var pos = s.Position;
            s.Seek(sizeOffset, SeekOrigin.Begin);
            s.WriteValueS32(result, Endianness);
            s.Seek(pos, SeekOrigin.Begin);

            return (int)(result + file.HeaderSize);
        }

        private int WriteStrings(Stream s, IList<TFString> strings, ExportOptions options, Ofs3Data file)
        {
            int result = 0;
            var tfString = strings.FirstOrDefault(x => x.Offset == file.StartOffset);

            if (tfString != null)
            {
                var str = tfString.Translation;

                if (tfString.Visible && !string.IsNullOrEmpty(str))
                {
                    if (options.CharReplacement != 0)
                    {
                        str = Utils.ReplaceChars(str, options.CharReplacementList);
                    }

                    str = SAOProject.WritingReplacements(str);

                    s.WriteStringZ(str, options.SelectedEncoding);

                    result = str.GetLength(options.SelectedEncoding) + 1;
                }
                else
                {
                    s.WriteStringZ(tfString.Original, options.SelectedEncoding);

                    result = tfString.Original.GetLength(options.SelectedEncoding) + 1;
                }

                while (s.Position % file.Padding != 0)
                {
                    s.WriteValueU8(0);

                    result++;
                }
            }

            return result;
        }
    }
}
