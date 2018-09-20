using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Gibbed.IO;
using TF.Core.Entities;

namespace TF.Core.Projects.Yakuza0.Files
{
    public class MsgFile : TFFile
    {
        private class DataItem
        {
            public TFString Data;
            public int PropertiesOffset;
            public int unknown;
        }

        private List<DataItem> _dataList;
        private List<TFString> _namesList;

        private byte[] _header;
        private byte[] _stringProperties;
        private byte[] _section1;
        private byte[] _section3;

        private int _firstStringOffset;
        private int _firstPropertyOffset;

        private int _totalStringLength;
        private int _totalNamesLength;

        public MsgFile(string fileName) : base(fileName)
        {
            
        }

        public override Endian Endianness => Endian.Big;
        public override Encoding Encoding => Encoding.UTF8;

        public override IList<TFString> Strings {
            get
            {
                var result = new List<TFString>();

                foreach (var item in _namesList)
                {
                    result.Add(item);
                }
            
                foreach (var item in _dataList)
                {
                    result.Add(item.Data);
                }

                return result;
            }
        }

        public override string FileType => "Msg";
        
        public override void Read()
        {
            using (var fs = new FileStream(Path, FileMode.Open))
            {
                ReadHeader(fs);
                ReadData(fs);
                ReadRemainder(fs);
            }
        }

        private void ReadHeader(Stream s)
        {
            s.ReadBytes(3);
            var num1 = s.ReadValueS8();
            var headerLength = num1 * 16 + 28;

            s.Seek(0, SeekOrigin.Begin);

            _header = s.ReadBytes(headerLength);
        }

        private void ReadData(Stream s)
        {
            _dataList = new List<DataItem>();
            _totalStringLength = 0;

            _firstStringOffset = s.ReadValueS32(Endianness);
            _firstPropertyOffset = s.ReadValueS32(Endianness);

            s.Seek(-8, SeekOrigin.Current);

            var start = s.Position;
            while (start < _firstPropertyOffset)
            {
                var item = new DataItem();
                var stringOffset = s.ReadValueS32(Endianness);
                item.PropertiesOffset = s.ReadValueS32(Endianness);
                item.unknown = s.ReadValueS32(Endianness);

                var pos = s.Position;
                
                s.Seek(item.PropertiesOffset, SeekOrigin.Begin);
                var pauses = GetPauses(s);

                s.Seek(stringOffset, SeekOrigin.Begin);
                var str = GetString(s, start.ToString("X8"));
                s.Seek(pos, SeekOrigin.Begin);

                InsertPauses(str, pauses);

                item.Data = str;
                _dataList.Add(item);
                _totalStringLength += item.Data.Original.GetLength(Encoding) + 1;

                start = s.Position;
            }

            s.Seek(_firstPropertyOffset, SeekOrigin.Begin);
            _stringProperties = s.ReadBytes(_firstStringOffset - _firstPropertyOffset);
        }

        private void InsertPauses(TFString str, IList<byte> pauses)
        {
            var strClean = RemoveTags(str.Original);
            var strPauses = AddPauses(strClean, pauses);
            var strResult = AddTags(strPauses, str.Original);

            str.Original = strResult;
            str.Translation = strResult;
        }

        private string AddTags(string strPauses, string strOriginal)
        {
            var sb = new StringBuilder();

            int i = 0;
            int j = 0;

            while (i < strPauses.Length && j < strOriginal.Length)
            {
                if (strPauses[i] == strOriginal[j])
                {
                    sb.Append(strOriginal[j]);
                    i++;
                    j++;
                }
                else if (strPauses[i] =='^' && strPauses[i + 1] =='^')
                {
                    sb.Append("^^");
                    i = i+2;
                }
                else if (strOriginal[j] == '<')
                {
                    while (strOriginal[j] != '>')
                    {
                        sb.Append(strOriginal[j]);
                        j++;
                    }
                    sb.Append(strOriginal[j]);
                    j++;
                }
                else if (strOriginal[j] == '\\' && strOriginal[j + 1] == 'r')
                {
                    sb.Append(strOriginal[j]);
                    sb.Append(strOriginal[j + 1]);
                    j = j + 2;
                }
                else if (strOriginal[j] == '\\' && strOriginal[j + 1] == 'n')
                {
                    sb.Append(strOriginal[j]);
                    sb.Append(strOriginal[j + 1]);
                    j = j + 2;
                    i++;
                }
            }

            if (i < strPauses.Length)
            {
                while (i < strPauses.Length)
                {
                    sb.Append(strPauses[i]);
                    i++;
                }
            }

            if (j < strOriginal.Length)
            {
                while (j < strOriginal.Length)
                {
                    sb.Append(strOriginal[j]);
                    j++;
                }
            }

            return sb.ToString();
        }

        private string AddPauses(string strClean, IList<byte> pauses)
        {
            var result = strClean;
            for (var i = pauses.Count - 1; i >= 0; i--)
            {
                result = result.Insert(pauses[i], "^^");
            }

            return result;
        }

        private string RemoveTags(string strOriginal)
        {
            var temp = strOriginal.Replace("\\r\\n", " ");
            return Regex.Replace(temp, @"<[^>]*>", string.Empty);
        }

        private IList<byte> GetPauses(Stream s)
        {
            var result = new List<byte>();
            var temp = s.ReadBytes(16);

            while (temp[0] != 0x01 || temp[1] != 0x01)
            {
                if (temp[0] == 0x02 && temp[1] == 0x09)
                {
                    var pause = temp[7];
                    result.Add(pause);
                }

                temp = s.ReadBytes(16);
            }

            return result;
        }

        private void ReadRemainder(Stream s)
        {
            _namesList = new List<TFString>();
            _totalNamesLength = 0;

            var pointer1 = (_header[8] << 24) + (_header[9] << 16) + (_header[10] << 8) + _header[11];
            var pointer2 = (_header[16] << 24) + (_header[17] << 16) + (_header[18] << 8) + _header[19];
            var pointer3 = (_header[20] << 24) + (_header[21] << 16) + (_header[22] << 8) + _header[23];
            var section1Size = ((_header[12] << 8) + _header[13]) * 16;
            var section2Count = ((_header[14] << 8) + _header[15]);

            if (pointer1 > 0)
            {
                s.Seek(pointer1, SeekOrigin.Begin);
                _section1 = s.ReadBytes(section1Size);
            }
            else
            {
                _section1 = null;
            }

            if (pointer2 > 0)
            {
                s.Seek(pointer2, SeekOrigin.Begin);

                for (var i = 0; i < section2Count; i++)
                {
                    var stringOffset = s.ReadValueS32(Endianness);

                    var pos = s.Position;
                    s.Seek(stringOffset, SeekOrigin.Begin);
                    
                    var name = GetString(s, "NAMES");

                    s.Seek(pos, SeekOrigin.Begin);

                    _namesList.Add(name);
                    _totalNamesLength += name.Original.GetLength(Encoding) + 1;
                }
            }

            if (pointer3 > 0)
            {
                s.Seek(pointer3, SeekOrigin.Begin);
                _section3 = s.ReadBytes((int) (s.Length - pointer3));
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

        public override void Save(string fileName, IList<TFString> strings, ExportOptions options)
        {
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                WriteHeader(fs, strings, options);
                WriteData(fs, strings, options);
                WriteRemainder(fs, strings, options);
            }
        }

        private void WriteHeader(Stream s, IList<TFString> strings, ExportOptions options)
        {
            var newTotalStringLength = 0;
            foreach (var item in _dataList)
            {
                var str = FindString(strings, string.Empty, item.Data.Offset);

                newTotalStringLength += str.GetLength(options.SelectedEncoding) + 1;
            }
            var dif = newTotalStringLength - _totalStringLength;

            var newTotalNamesLength = 0;
            foreach (var item in _namesList)
            {
                var str = FindString(strings, "NAMES", item.Offset);

                newTotalNamesLength += str.GetLength(options.SelectedEncoding) + 1;
            }
            
            var dif2 = newTotalNamesLength - _totalNamesLength;

            var pointer1 = (_header[8] << 24) + (_header[9] << 16) + (_header[10] << 8) + _header[11];
            var pointer2 = (_header[16] << 24) + (_header[17] << 16) + (_header[18] << 8) + _header[19];
            var pointer3 = (_header[20] << 24) + (_header[21] << 16) + (_header[22] << 8) + _header[23];
            
            var newPointer1 = 0;
            var newPointer2 = 0;
            var newPointer3 = 0;
            
            if (pointer1 > 0)
            {
                newPointer1 = pointer1 + dif;
            }
            if (pointer2 > 0)
            {
                newPointer2 = pointer2 + dif;
            }
            if (pointer3 > 0)
            {
                newPointer3 = pointer3 + dif + dif2;
            }

            s.Write(_header, 0, 8);
            s.WriteValueS32(newPointer1, Endianness);
            s.Write(_header, 12, 4);
            s.WriteValueS32(newPointer2, Endianness);
            s.WriteValueS32(newPointer3, Endianness);
            s.Write(_header, 24, _header.Length - 24);
        }

        private void WriteData(Stream s, IList<TFString> strings, ExportOptions options)
        {
            var stringOffset = _firstStringOffset;
            var pos = s.Position;

            s.Seek(_firstPropertyOffset, SeekOrigin.Begin);
            s.WriteBytes(_stringProperties);
            
            s.Seek(pos, SeekOrigin.Begin);
            foreach (var value in _dataList)
            {
                s.WriteValueS32(stringOffset, Endianness);
                s.WriteValueS32(value.PropertiesOffset, Endianness);
                s.WriteValueS32(value.unknown, Endianness);

                pos = s.Position;
                s.Seek(stringOffset, SeekOrigin.Begin);

                var str = FindString(strings, string.Empty, value.Data.Offset);

                var tuple = ParsePauses(str);
                WriteString(s, tuple.Item1, options);
                stringOffset = (int) s.Position;

                if (tuple.Item2.Count > 0)
                {
                    s.Seek(value.PropertiesOffset, SeekOrigin.Begin);
                    UpdatePauses(s, tuple.Item2);
                }

                s.Seek(pos, SeekOrigin.Begin);
            }
        }

        private void UpdatePauses(Stream s, IList<byte> pauses)
        {
            var temp = s.ReadBytes(16);

            int i = 0;
            while (temp[0] != 0x01 || temp[1] != 0x01)
            {
                if (temp[0] == 0x02 && temp[1] == 0x09)
                {
                    if (i < pauses.Count)
                    {
                        temp[7] = pauses[i];
                        i++;

                        s.Seek(-16, SeekOrigin.Current);
                        s.WriteBytes(temp);
                    }
                }

                temp = s.ReadBytes(16);
            }
        }

        private Tuple<string, IList<byte>> ParsePauses(string str)
        {
            var pauses = new List<byte>();

            var temp = RemoveTags(str);
            if (temp.Contains("^^"))
            {
                var i = temp.IndexOf("^^", 0, StringComparison.Ordinal);
                while (i > -1)
                {
                    var previousCount = pauses.Count;
                    pauses.Add((byte) (i - 2 * previousCount));
                    i = temp.IndexOf("^^", i + 2, StringComparison.Ordinal);
                }
            }

            var strResult = str.Replace("^^", string.Empty);

            return new Tuple<string, IList<byte>>(strResult, pauses);
        }

        private void WriteRemainder(Stream s, IList<TFString> strings, ExportOptions options)
        {
            var newTotalStringLength = 0;
            foreach (var item in _dataList)
            {
                var str = FindString(strings, string.Empty, item.Data.Offset);

                newTotalStringLength += str.GetLength(options.SelectedEncoding) + 1;
            }
            var dif = newTotalStringLength - _totalStringLength;

            var newTotalNamesLength = 0;
            foreach (var item in _namesList)
            {
                var str = FindString(strings, "NAMES", item.Offset);

                newTotalNamesLength += str.GetLength(options.SelectedEncoding) + 1;
            }
            
            var dif2 = newTotalNamesLength - _totalNamesLength;

            var pointer1 = (_header[8] << 24) + (_header[9] << 16) + (_header[10] << 8) + _header[11];
            var pointer2 = (_header[16] << 24) + (_header[17] << 16) + (_header[18] << 8) + _header[19];
            var pointer3 = (_header[20] << 24) + (_header[21] << 16) + (_header[22] << 8) + _header[23];
            
            var newPointer1 = 0;
            var newPointer2 = 0;
            var newPointer3 = 0;
            
            if (pointer1 > 0)
            {
                newPointer1 = pointer1 + dif;
            }
            if (pointer2 > 0)
            {
                newPointer2 = pointer2 + dif;
            }
            if (pointer3 > 0)
            {
                newPointer3 = pointer3 + dif + dif2;
            }

            var section1Size = ((_header[12] << 8) + _header[13]) * 16;
            var section2Count = ((_header[14] << 8) + _header[15]);

            if (newPointer1 > 0)
            {
                s.Seek(newPointer1, SeekOrigin.Begin);
                s.WriteBytes(_section1);
            }

            if (newPointer2 > 0)
            {
                s.Seek(newPointer2, SeekOrigin.Begin);
                var stringOffset = newPointer2 + section2Count * 4;
                for (var i = 0; i < section2Count; i++)
                {
                    s.WriteValueS32(stringOffset, Endianness);
                    
                    var pos = s.Position;
                    s.Seek(stringOffset, SeekOrigin.Begin);
                    
                    var str = FindString(strings, "NAMES", _namesList[i].Offset);
                    WriteString(s, str, options);

                    stringOffset = (int) s.Position;
                    
                    s.Seek(pos, SeekOrigin.Begin);
                }
            }

            if (newPointer3 > 0)
            {
                s.Seek(newPointer3, SeekOrigin.Begin);
                s.WriteBytes(_section3);
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
