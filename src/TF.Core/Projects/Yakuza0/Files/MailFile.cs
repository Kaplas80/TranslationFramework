using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gibbed.IO;
using TF.Core.Entities;

namespace TF.Core.Projects.Yakuza0.Files
{
    public class MailFile : TFFile
    {
        private class Sender
        {
            public int OriginalOffset;
            public int NewOffset;
            public TFString Name;
        }

        private class DataItem
        {
            public int unknown1;
            public Sender Sender;
            public List<TFString> Message;
            public TFString SubMessage;
            public int unknown2;
            public int unknown3;

            public DataItem()
            {
                Message = new List<TFString>();
            }
        }

        private int _unknown1;
        private int _unknown2;
        private int _unknown3;
        private int _itemCount;

        private List<DataItem> _dataList;
        private List<Sender> _senders;

        public MailFile(string fileName) : base(fileName)
        {
            
        }

        public override Endian Endianness => Endian.Big;
        public override Encoding Encoding => Encoding.UTF8;

        public override IList<TFString> Strings {
            get
            {
                var result = new List<TFString>();

                foreach (var item in _senders)
                {
                    result.Add(item.Name);
                }
            
                foreach (var item in _dataList)
                {
                    foreach (var s in item.Message)
                    {
                        result.Add(s);
                    }
                    
                    result.Add(item.SubMessage);
                }

                return result;
            }
        }

        public override string FileType => "EPMB";
        
        public override void Read(Stream s)
        {
            ReadHeader(s);
            
            ReadData(s);
        }

        private void ReadHeader(Stream s)
        {
            _unknown1 = s.ReadValueS32(Endianness); 
            _itemCount = s.ReadValueS32(Endianness);
            _unknown2 = s.ReadValueS32(Endianness); 
            _unknown3 = s.ReadValueS32(Endianness); 
        }

        private void ReadData(Stream s)
        {
            _dataList = new List<DataItem>(_itemCount);
            _senders = new List<Sender>();

            for (var i = 0; i < _itemCount; i++)
            {
                var item = GetDataItem(s);
                _dataList.Add(item);
            }
        }

        private DataItem GetDataItem(Stream s)
        {
            const int itemLength = 0x44;
            var startPos = s.Position;

            var result = new DataItem {unknown1 = s.ReadValueS32(Endianness)};

            var senderPointer = s.ReadValueS32(Endianness);
            var returnPos = s.Position;
            
            if (_senders.Any(x => x.OriginalOffset == senderPointer))
            {
                result.Sender = _senders.First(x => x.OriginalOffset == senderPointer);
            }
            else
            {
                s.Seek(senderPointer, SeekOrigin.Begin);
                var name = GetString(s, "SENDER");
                s.Seek(returnPos, SeekOrigin.Begin);

                var sender = new Sender {OriginalOffset = senderPointer, NewOffset = 0, Name = name};

                result.Sender = sender;
                _senders.Add(sender);
            }

            var messageFirstPointer = s.ReadValueS32(Endianness);
            var numMessageLines = s.ReadValueS32(Endianness);
            returnPos = s.Position;
            s.Seek(messageFirstPointer, SeekOrigin.Begin);
            
            result.Message = new List<TFString>(numMessageLines);
            
            //_numStringPointer += numMessageLines;

            for (var i = 0; i < numMessageLines; i++)
            {
                var messageSecondPointer = s.ReadValueS32(Endianness);
                var returnPos2 = s.Position;
                s.Seek(messageSecondPointer, SeekOrigin.Begin);

                var temp = GetString(s, startPos.ToString("X8"));
                
                result.Message.Add(temp);
                
                s.Seek(returnPos2, SeekOrigin.Begin);
            }
            s.Seek(returnPos, SeekOrigin.Begin);

            s.ReadValueS32(Endianness); //0

            var submessagePointer = s.ReadValueS32(Endianness);
            returnPos = s.Position;
            s.Seek(submessagePointer, SeekOrigin.Begin);

            result.SubMessage = GetString(s, startPos.ToString("X8"));
            
            s.Seek(returnPos, SeekOrigin.Begin);

            result.unknown2 = s.ReadValueS32(Endianness);            
            result.unknown3 = s.ReadValueS32(Endianness);

            s.Seek(startPos + itemLength, SeekOrigin.Begin);
            return result;
        }

        private TFString GetString(Stream s, string section)
        {
            var pos = s.Position;
            var str = s.ReadStringZ(Encoding);
            str = Yakuza0Project.ReadingReplacements(str);

            var tfString = new TFString
            {
                FileId = Id,
                Original = str,
                Translation = str,
                Offset = (int) pos,
                Section = section,
                Visible = !string.IsNullOrWhiteSpace(str)
            };

            return tfString;
        }

        public override void Save(string fileName, byte[] originalContent, IList<TFString> strings, ExportOptions options)
        {
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                WriteHeader(fs);

                WriteData(fs, strings, options);
            }

            foreach (var sender in _senders)
            {
                // Vuelvo a poner a 0 los offset por si el usuario vuelve a guardar el fichero en la misma sesión
                sender.NewOffset = 0;
            }
        }

        private void WriteHeader(Stream s)
        {
            s.WriteValueS32(_unknown1, Endianness);
            s.WriteValueS32(_itemCount, Endianness);
            s.WriteValueS32(_unknown2, Endianness);
            s.WriteValueS32(_unknown3, Endianness);
        }

        private void WriteData(Stream s, IList<TFString> strings, ExportOptions options)
        {
            const int itemLength = 0x44;

            var lutOffset = 16 + _itemCount * 0x44; // Posicion de las indirecciones de los mensajes
            var numStringPointer = _dataList.Sum(x => x.Message.Count);
            var stringsOffset = lutOffset + numStringPointer * 4; // Posición de la primera cadena de texto

            for (var i = 0; i < _itemCount; i++)
            {
                var item = _dataList[i];
                
                var pos = s.Position;
                s.WriteValueS32(item.unknown1, Endianness);

                if (item.Sender.NewOffset == 0)
                {
                    item.Sender.NewOffset = stringsOffset;
                    s.WriteValueS32(stringsOffset, Endianness); // Puntero al sender
                    s.Seek(stringsOffset, SeekOrigin.Begin);

                    var name = FindString(strings, "SENDER", item.Sender.OriginalOffset);
                    WriteString(s, name, options);
                    
                    stringsOffset += name.GetLength(options.SelectedEncoding) + 1;
                    s.Seek(pos + 8, SeekOrigin.Begin);
                }
                else
                {
                    s.WriteValueS32(item.Sender.NewOffset, Endianness); // Puntero al sender
                }

                s.WriteValueS32(lutOffset, Endianness);
                s.WriteValueS32(item.Message.Count, Endianness);
                s.WriteValueS32(0, Endianness);

                if (string.IsNullOrEmpty(item.SubMessage.Original))
                {
                    s.WriteValueS32(0, Endianness);
                }
                else
                {
                    s.WriteValueS32(stringsOffset, Endianness);
                    s.Seek(stringsOffset, SeekOrigin.Begin);

                    var str = FindString(strings, string.Empty, item.SubMessage.Offset);
                    WriteString(s, str, options);
                    
                    stringsOffset += str.GetLength(options.SelectedEncoding) + 1;
                }

                s.Seek(pos + 24, SeekOrigin.Begin);
                
                s.WriteValueS32(item.unknown2, Endianness);
                s.WriteValueS32(item.unknown3, Endianness);

                foreach (var value in item.Message)
                {
                    s.Seek(lutOffset, SeekOrigin.Begin);
                    s.WriteValueS32(stringsOffset, Endianness);
                    lutOffset += 4;
                    s.Seek(stringsOffset, SeekOrigin.Begin);
                    
                    var str = FindString(strings, string.Empty, value.Offset);
                    WriteString(s, str, options);
                    stringsOffset += str.GetLength(options.SelectedEncoding) + 1;
                }
                
                s.Seek(pos + itemLength, SeekOrigin.Begin);
            }
        }

        private static string FindString(IList<TFString> strings, string section, int offset)
        {
            if (string.IsNullOrEmpty(section))
            {
                foreach (var s in strings)
                {
                    if (s.Offset == offset)
                    {
                        return s.Translation;
                    }
                }
            }
            else
            {
                foreach (var s in strings)
                {
                    if (s.Section == section && s.Offset == offset)
                    {
                        return s.Translation;
                    }
                }
            }

            return string.Empty;
        }

        private static void WriteString(Stream s, string str, ExportOptions options)
        {
            if (options.CharReplacement != 0)
            {
                str = Utils.ReplaceChars(str, options.CharReplacementList);
            }
            str = Yakuza0Project.WritingReplacements(str);

            s.WriteStringZ(str, options.SelectedEncoding);
        }
    }
}
