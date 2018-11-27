using System.Collections.Generic;
using System.IO;
using System.Text;
using Gibbed.IO;
using TF.Core.Entities;

namespace TF.Core.Projects.JJMacfield.Files
{
    public class TxtFile : TFFile
    {
        private int _offsetLengths;
        private int _offset1;
        private int _offset2;
        private int _offsetStrings1;
        private int _offsetStrings2;

        private List<TFString> _strings;

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
                    result.Add(item);
                }
            
                return result;
            }
        }

        public override string FileType => "JJMacfield Txt";
        
        public override void Read(Stream s)
        {
            s.Seek(0x0C, SeekOrigin.Begin);
            _offsetLengths = s.ReadValueS32(Endianness);
            _offset1 = s.ReadValueS32(Endianness);
            _offsetStrings1 = s.ReadValueS32(Endianness);
            _offset2 = s.ReadValueS32(Endianness);
            _offsetStrings2 = s.ReadValueS32(Endianness);

            s.Seek(0x30, SeekOrigin.Begin);
            var numStrings = s.ReadValueS32(Endianness);

            _strings = new List<TFString>();

            for (var i = 0; i < numStrings; i++)
            {
                s.Seek(_offset1 + i * 4, SeekOrigin.Begin);
                var strOffset = s.ReadValueS32(Endianness);
                s.Seek(_offsetStrings1 + strOffset, SeekOrigin.Begin);

                var str = ReadString(s);
                _strings.Add(str);
            }
        }

        private TFString ReadString(Stream s)
        {
            var stringOffset = (int) s.Position;
            
            var value = new TFString {FileId = Id, Offset = stringOffset, Visible = false};

            var str = s.ReadStringZ(Encoding);

            if (!string.IsNullOrEmpty(str.TrimEnd('\0')))
            {
                str = JJMacfieldProject.ReadingReplacements(str);
            }

            value.Original = str;
            value.Translation = str;
            value.Visible = !string.IsNullOrWhiteSpace(str);

            return value;
        }

        public override void Save(string fileName, byte[] originalContent, IList<TFString> strings, ExportOptions options)
        {
            var lengths = new Dictionary<int, short>();
            using (var original = new MemoryStream(originalContent))
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                // El único offset que hace falta corregir es el offsetCadenas2, los demás están antes de cualquier modificación
                var bytes = original.ReadBytes(_offset1);
                fs.WriteBytes(bytes);

                var strOffset = 0;
                for (var i = 0; i < strings.Count; i++)
                {
                    fs.Seek(_offset1 + i * 4, SeekOrigin.Begin);
                    fs.WriteValueS32(strOffset, Endianness);

                    fs.Seek(_offsetStrings1 + strOffset, SeekOrigin.Begin);

                    var str = strings[i];

                    var length = WriteString(fs, str, options);

                    strOffset += length + 1; // Hay que añadir el 00 del final de linea

                    lengths.Add(i, (short) length);
                }

                while (fs.Position % 0x10 != 0)
                {
                    fs.WriteByte(0);
                }

                var newOffsetStrings2 = (int)fs.Position;

                original.Seek(0x34, SeekOrigin.Begin);
                var numStrings2 = original.ReadValueS32(Endianness);

                original.Seek(_offset2, SeekOrigin.Begin);
                bytes = original.ReadBytes(numStrings2 * 4);
                fs.Seek(_offset2, SeekOrigin.Begin);
                fs.WriteBytes(bytes);

                original.Seek(_offsetStrings2, SeekOrigin.Begin);
                bytes = original.ReadBytes((int) (original.Length - _offsetStrings2));

                fs.Seek(newOffsetStrings2, SeekOrigin.Begin);
                fs.WriteBytes(bytes);

                fs.Seek(0x1C, SeekOrigin.Begin);
                fs.WriteValueS32(newOffsetStrings2);

                original.Seek(0x2c, SeekOrigin.Begin);
                var numLengths = original.ReadValueS32(Endianness);

                for (var i = 0; i < numLengths; i++)
                {
                    fs.Seek(_offsetLengths + i * 16, SeekOrigin.Begin);
                    var id = fs.ReadValueS32(Endianness);
                    fs.WriteValueS16(lengths[id]);
                }
            }
        }

        private int WriteString(Stream s, TFString str, ExportOptions options)
        {
            var result = 0;
            var writeStr = str.Translation;
            if (str.Original.Equals(str.Translation))
            {
                writeStr = JJMacfieldProject.WritingReplacements(writeStr);
                s.WriteStringZ(writeStr, Encoding);
                result = writeStr.GetLength(Encoding);
            }
            else
            {
                if (options.CharReplacement != 0)
                {
                    writeStr = Utils.ReplaceChars(writeStr, options.CharReplacementList);
                }

                writeStr = JJMacfieldProject.WritingReplacements(writeStr);
                s.WriteStringZ(writeStr, options.SelectedEncoding);

                result = writeStr.GetLength(options.SelectedEncoding);
            }

            return result;
        }
    }
}
