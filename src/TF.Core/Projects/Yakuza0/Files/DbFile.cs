using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gibbed.IO;
using TF.Core.Entities;

namespace TF.Core.Projects.Yakuza0.Files
{
    public class DbFile : TFFile
    {
        private class DataItem
        {
            public string Name;
            public int DataType;
            public int DataCount;
            public int DataLength;

            public byte[] DataRemainder; // Bytes que quedan después del último dato. No se si hay que guardarlos o no

            public List<TFString> Data;
            public List<short> AdditionalValues;
        }

        private int _signature;
        private int _itemCount;
        private int _unknown1;
        private int _unknown2;

        private List<DataItem> _dataList;

        public DbFile(string fileName) : base(fileName)
        {
            
        }

        public override Endian Endianness => Endian.Big;
        public override Encoding Encoding => Encoding.UTF8;

        public override IList<TFString> Strings {
            get
            {
                var result = new List<TFString>();

                foreach (var item in _dataList)
                {
                    if (item.DataType > 0x02)
                    {
                        // Solo tienen texto los tipos 0, 1 y 2
                        continue;
                    }

                    for (var i = 0; i < item.DataCount; i++)
                    {
                        result.Add(item.Data[i]);
                    }
                }
            
                return result;
            }
        }

        public override string FileType => "Database";
        
        public override void Read(Stream s)
        {
            ReadHeader(s);
            
            ReadFields(s);

            ReadData(s);
        }

        private void ReadHeader(Stream s)
        {
            _signature = s.ReadValueS32(Endianness); // 20 07 03 19
            _itemCount = s.ReadValueS32(Endianness);
            _unknown1 = s.ReadValueS32(Endianness);
            _unknown2 = s.ReadValueS32(Endianness); // 0
        }

        private void ReadFields(Stream s)
        {
            _dataList = new List<DataItem>(_itemCount);

            for (var i = 0; i < _itemCount; i++)
            {
                var pos = s.Position;
                var field = new DataItem
                {
                    Name = s.ReadStringZ(Encoding)
                };

                s.Seek(pos + 48, SeekOrigin.Begin);

                field.DataType = s.ReadValueS32(Endianness);
                field.DataCount = s.ReadValueS32(Endianness);
                field.DataLength = s.ReadValueS32(Endianness);
                
                s.ReadValueS32(Endianness); // Leemos el 0 que hay al final

                field.Data = new List<TFString>();
                field.AdditionalValues = new List<short>();
                
                _dataList.Add(field);
            }
        }

        private void ReadData(Stream s)
        {
            foreach (var item in _dataList)
            {
                var dataType = item.DataType;

                if (dataType == 0 || dataType == 1)
                {
                    var startPos = s.Position;
                    var numData = 0;
                    while (numData < item.DataCount)
                    {
                        var tfString = GetString(s, item.Name);
                        item.Data.Add(tfString);
                        item.AdditionalValues.Add(0);
                        numData++;
                    }

                    var endPos = s.Position;
                    var readBytes = (int)(endPos - startPos);
                    if (readBytes < item.DataLength)
                    {
                        // quedan bytes antes del próximo campo
                        item.DataRemainder = s.ReadBytes(item.DataLength - readBytes);
                    }
                }
                else if (dataType == 2)
                {
                    // cada cadena va precedida de 2 bytes con un valor
                    var startPos = s.Position;
                    var numData = 0;
                    while (numData < item.DataCount)
                    {
                        var key = s.ReadValueS16(Endianness);

                        var tfString = GetString(s, item.Name);

                        item.Data.Add(tfString);
                        item.AdditionalValues.Add(key);
                        numData++;
                    }

                    var endPos = s.Position;
                    var readBytes = (int)(endPos - startPos);
                    if (readBytes < item.DataLength)
                    {
                        // quedan bytes antes del próximo campo
                        item.DataRemainder = s.ReadBytes(item.DataLength - readBytes);
                    }
                }
                else
                {
                    item.DataRemainder = s.ReadBytes(item.DataLength);
                }
            }
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

                WriteFields(fs, strings, options);

                WriteData(fs, strings, options);
            }
        }

        private void WriteHeader(Stream s)
        {
            s.WriteValueS32(_signature, Endianness);
            s.WriteValueS32(_itemCount, Endianness);
            s.WriteValueS32(_unknown1, Endianness);
            s.WriteValueS32(_unknown2, Endianness);
        }

        private void WriteFields(Stream s, IList<TFString> strings, ExportOptions options)
        {
            foreach (var dataField in _dataList)
            {
                s.WriteStringZ(dataField.Name, Encoding);
                var zeros = new byte[48 - (dataField.Name.GetLength(Encoding) + 1)];
                s.WriteBytes(zeros);
                
                s.WriteValueS32(dataField.DataType, Endianness);
                s.WriteValueS32(dataField.DataCount, Endianness);

                if (dataField.DataType > 0x02)
                {
                    s.WriteValueS32(dataField.DataLength, Endianness);
                }
                else
                {
                    var size = 0;

                    if (dataField.DataRemainder != null)
                    {
                        size += dataField.DataRemainder.Length;
                    }

                    foreach (var tfString in strings.Where(x => x.Section == dataField.Name))
                    {
                        var writeStr = tfString.Translation;
                        if (tfString.Original.Equals(tfString.Translation))
                        {
                            writeStr = Yakuza0Project.WritingReplacements(writeStr);
                            size += writeStr.GetLength(Encoding) + 1;
                        }
                        else
                        {
                            writeStr = Yakuza0Project.WritingReplacements(writeStr);
                            size += writeStr.GetLength(options.SelectedEncoding) + 1;
                        }
                    }

                    if (dataField.DataType == 0x02)
                    {
                        size += 2 * dataField.DataCount;
                    }

                    s.WriteValueS32(size, Endianness);
                }
                s.WriteValueS32(0);
            }
        }

        private void WriteData(Stream s, IList<TFString> strings, ExportOptions options)
        {
            foreach (var item in _dataList)
            {
                var dataType = item.DataType;

                if (dataType == 0x00 || dataType == 0x01)
                {
                    foreach (var value in strings)
                    {
                        if (value.Section != item.Name)
                        {
                            continue;
                        }

                        WriteString(s, value, options);
                    }
                }
                else if (dataType == 2)
                {
                    var i = 0;
                    foreach (var value in strings)
                    {
                        if (value.Section != item.Name)
                        {
                            continue;
                        }

                        s.WriteValueS16(item.AdditionalValues[i], Endianness);

                        WriteString(s, value, options);

                        i++;
                    }
                }
                
                if (item.DataRemainder != null)
                {
                    s.WriteBytes(item.DataRemainder);
                }
            }
        }

        private void WriteString(Stream s, TFString str, ExportOptions options)
        {
            var writeStr = str.Translation;
            if (str.Original.Equals(str.Translation))
            {
                writeStr = Yakuza0Project.WritingReplacements(writeStr);
                s.WriteStringZ(writeStr, Encoding);
            }
            else
            {
                if (options.CharReplacement != 0)
                {
                    writeStr = Utils.ReplaceChars(writeStr, options.CharReplacementList);
                }

                writeStr = Yakuza0Project.WritingReplacements(writeStr);
                s.WriteStringZ(writeStr, options.SelectedEncoding);
            }
        }
    }
}
