using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        
        public override void Read(Stream s)
        {
            ReadHeader(s);
            ReadData(s);
            ReadRemainder(s);
        }

        private void ReadHeader(Stream s)
        {
            s.ReadBytes(3);
            var num1 = s.ReadValueS8();
            var headerLength = num1 * 16 + 24;

            s.Seek(0, SeekOrigin.Begin);

            _header = s.ReadBytes(headerLength);
        }

        private void ReadData(Stream s)
        {
            _dataList = new List<DataItem>();
            _totalStringLength = 0;

            var pointer1 = (_header[8] << 24) + (_header[9] << 16) + (_header[10] << 8) + _header[11];

            s.Seek(4, SeekOrigin.Current);
            _firstStringOffset = s.ReadValueS32(Endianness);
            _firstPropertyOffset = s.ReadValueS32(Endianness);
            _totalStringLength = pointer1 - _firstStringOffset;
            s.Seek(-12, SeekOrigin.Current);

            var start = s.Position;
            while (start < _firstPropertyOffset)
            {
                var item = new DataItem();
                item.unknown = s.ReadValueS32(Endianness);
                var stringOffset = s.ReadValueS32(Endianness);
                item.PropertiesOffset = s.ReadValueS32(Endianness);

                var pos = s.Position;
                
                s.Seek(item.PropertiesOffset, SeekOrigin.Begin);
                var pauses = GetPauses(s);

                s.Seek(stringOffset, SeekOrigin.Begin);
                var str = GetString(s, (start + 4).ToString("X8"));
                s.Seek(pos, SeekOrigin.Begin);

                InsertPauses(str, pauses);

                item.Data = str;
                _dataList.Add(item);
                
                //_totalStringLength += GetLength(str.Original, Encoding, false, false) + 1;

                start = s.Position;
            }

            s.Seek(_firstPropertyOffset, SeekOrigin.Begin);
            _stringProperties = s.ReadBytes(_firstStringOffset - _firstPropertyOffset);
        }

        private void InsertPauses(TFString str, IList<short> pauses)
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
                    var c = strOriginal[j + 1];
                    while (strOriginal[j] != '>')
                    {
                        sb.Append(strOriginal[j]);
                        j++;
                    }
                    sb.Append(strOriginal[j]);
                    j++;

                    if (c != 'C') // No es color
                    {
                        i++;
                    }
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

        private string AddPauses(string strClean, IList<short> pauses)
        {
            var result = strClean;
            for (var i = pauses.Count - 1; i >= 0; i--)
            {
                var pos = Math.Min(pauses[i], result.Length);
                result = result.Insert(pos, "^^");
            }

            return result;
        }

        private string RemoveTags(string strOriginal)
        {
            var temp = strOriginal.Replace("\\r\\n", " ");
            temp = temp.Replace("\\n", " ");
            temp = Regex.Replace(temp, @"<Color[^>]*>", string.Empty);
            return Regex.Replace(temp, @"<[^>]*>", " ");
        }

        private IList<short> GetPauses(Stream s)
        {
            var result = new List<short>();
            var temp = s.ReadBytes(16);

            while (temp[0] != 0x01 || temp[1] != 0x01)
            {
                if (temp[0] == 0x02 && temp[1] == 0x09)
                {
                    var pause =(short) (temp[6] * 256 + temp[7]);
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
                    _totalNamesLength += GetLength(name.Original, Encoding, false, false) + 1;
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
                Visible = !string.IsNullOrWhiteSpace(str),
            };

            return tfString;
        }

        public override void Save(string fileName, byte[] originalContent, IList<TFString> strings, ExportOptions options)
        {
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                WriteHeader(fs, strings, options);
                var pointer1 = WriteData(fs, strings, options);
                var newPointers = WriteRemainder(fs, strings, options);

                fs.Seek(8, SeekOrigin.Begin);
                fs.WriteValueS32(pointer1, Endianness);
                fs.Seek(4, SeekOrigin.Current);
                fs.WriteValueS32(newPointers.Item1, Endianness);
                fs.WriteValueS32(newPointers.Item2, Endianness);
            }
        }

        public void Save(Stream s, byte[] originalContent, IList<TFString> strings, ExportOptions options)
        {
            WriteHeader(s, strings, options);

            var pointer1 = WriteData(s, strings, options);
            var newPointers = WriteRemainder(s, strings, options);

            s.Seek(8, SeekOrigin.Begin);
            s.WriteValueS32(pointer1, Endianness);
            s.Seek(4, SeekOrigin.Current);
            s.WriteValueS32(newPointers.Item1, Endianness);
            s.WriteValueS32(newPointers.Item2, Endianness);
        }

        private void WriteHeader(Stream s, IList<TFString> strings, ExportOptions options)
        {
            s.WriteBytes(_header);
        }

        private int WriteData(Stream s, IList<TFString> strings, ExportOptions options)
        {
            var stringOffset = _firstStringOffset;
            var pos = s.Position;

            s.Seek(_firstPropertyOffset, SeekOrigin.Begin);
            s.WriteBytes(_stringProperties);
            
            s.Seek(pos, SeekOrigin.Begin);
            foreach (var value in _dataList)
            {
                s.WriteValueS32(value.unknown, Endianness);
                s.WriteValueS32(stringOffset, Endianness);
                s.WriteValueS32(value.PropertiesOffset, Endianness);

                pos = s.Position;
                s.Seek(stringOffset, SeekOrigin.Begin);

                var str = FindString(strings, value.Data.Offset);

                var isOriginal = str.Original == str.Translation;

                var translationTuple = ParsePauses(str.Translation);
                var originalTuple = ParsePauses(str.Original);

                WriteString(s, translationTuple.Item1, options, isOriginal);
                
                stringOffset = (int) s.Position;

                if (translationTuple.Item2.Count > 0)
                {
                    s.Seek(value.PropertiesOffset, SeekOrigin.Begin);
                    UpdatePauses(s, translationTuple.Item2);
                }

                var colorTags = ParseTags(str.Translation);
                
                if (colorTags.Count > 0)
                {
                    var originalColorTags = ParseTags(str.Original);
                    s.Seek(value.PropertiesOffset, SeekOrigin.Begin);
                    UpdateColorTags(s, colorTags, originalColorTags);
                }

                s.Seek(value.PropertiesOffset, SeekOrigin.Begin);
                if (!isOriginal)
                {
                    var newLength = GetLength(translationTuple.Item1, options.SelectedEncoding, true, true);
                    var originalLength = GetLength(originalTuple.Item1, Encoding, true, true);
                    UpdateLength(s, (short) originalLength, (short) newLength);

                    s.Seek(pos - 12, SeekOrigin.Begin);
                    s.WriteValueS16((short)newLength, Endianness);
                }

                s.Seek(pos, SeekOrigin.Begin);
            }

            s.Seek(0, SeekOrigin.End);

            while (s.Position % 8 != 0)
            {
                s.WriteByte(0);
            }

            return _section1 == null? 0:(int)s.Position;
        }

        private List<Tuple<short, short>> ParseTags(string input)
        {
            var temp = input.Replace("^^", string.Empty);
            temp = temp.Replace("\\r\\n", " ");
            temp = Regex.Replace(temp, @"<^C[^>]*>", " ");

            var result = new List<Tuple<short, short>>();
            var matches = Regex.Matches(temp, @"<Color:[^>]*>");
            var acum = 0;
            for (var i = 0; i < matches.Count; i = i+2)
            {
                var pos1 = (short)(matches[i].Index - acum);
                acum += matches[i].Length;
                var pos2 = (short)(matches[i + 1].Index - acum);
                acum += matches[i + 1].Length;

                var t = new Tuple<short, short>(pos1, pos2);
                result.Add(t);
            }
            return result;
        }

        private void UpdateColorTags(Stream s, List<Tuple<short, short>> newColorTags,
            List<Tuple<short, short>> originalColorTags)
        {
            var pos = s.Position;

            for (var i = 0; i < originalColorTags.Count; i++)
            {
                s.Seek(pos, SeekOrigin.Begin);
                
                var temp = s.ReadBytes(16);

                while (temp[0] != 0x01 || temp[1] != 0x01)
                {
                    var value = temp[6] * 256 + temp[7];
                    if (value == originalColorTags[i].Item1)
                    {
                        s.Seek(-10, SeekOrigin.Current);
                        s.WriteValueS16(newColorTags[i].Item1, Endianness);
                        s.Seek(8, SeekOrigin.Current);
                    }

                    if (value == originalColorTags[i].Item2)
                    {
                        s.Seek(-10, SeekOrigin.Current);
                        s.WriteValueS16(newColorTags[i].Item2, Endianness);
                        s.Seek(8, SeekOrigin.Current);
                    }

                    temp = s.ReadBytes(16);
                }
            }
        }

        private int GetLength(string str, Encoding enc, bool removeTags, bool removeBreaks)
        {
            var temp = str.Replace("^^", string.Empty);
            
            if (removeBreaks)
            {
                temp = temp.Replace("\\r\\n", " ");
            }
            else
            {
                temp = temp.Replace("\\r\\n", "\r\n");
            }

            if (removeTags)
            {
                temp = Regex.Replace(temp, @"<Color[^>]*>", string.Empty);
                temp = Regex.Replace(temp, @"<[^>]*>", " ");
            }

            return temp.GetLength(enc);
        }

        private void UpdatePauses(Stream s, IList<short> pauses)
        {
            var temp = s.ReadBytes(16);

            int i = 0;
            while (temp[0] != 0x01 || temp[1] != 0x01)
            {
                if (temp[0] == 0x02 && temp[1] == 0x09)
                {
                    if (i < pauses.Count)
                    {
                        s.Seek(-10, SeekOrigin.Current);
                        s.WriteValueS16(pauses[i], Endianness);
                        s.Seek(8, SeekOrigin.Current);
                        i++;
                    }
                }

                temp = s.ReadBytes(16);
            }
        }

        private void UpdateLength(Stream s, short originalLength, short newLength)
        {
            var temp = s.ReadBytes(16);

            while (temp[0] != 0x01 || temp[1] != 0x01)
            {
                var value = temp[6] * 256 + temp[7];
                if (value == originalLength || value > newLength)
                {
                    s.Seek(-10, SeekOrigin.Current);
                    s.WriteValueS16(newLength, Endianness);
                    s.Seek(8, SeekOrigin.Current);
                }

                temp = s.ReadBytes(16);
            }

            s.Seek(-10, SeekOrigin.Current);
            s.WriteValueS16(newLength, Endianness);
            s.Seek(8, SeekOrigin.Current);
        }

        private Tuple<string, IList<short>> ParsePauses(string str)
        {
            var pauses = new List<short>();

            var temp = RemoveTags(str);
            if (temp.Contains("^^"))
            {
                var i = temp.IndexOf("^^", 0, StringComparison.Ordinal);
                while (i > -1)
                {
                    var previousCount = pauses.Count;
                    pauses.Add((short) (i - 2 * previousCount));
                    i = temp.IndexOf("^^", i + 2, StringComparison.Ordinal);
                }
            }

            var strResult = str.Replace("^^", string.Empty);

            return new Tuple<string, IList<short>>(strResult, pauses);
        }

        private Tuple<int, int> WriteRemainder(Stream s, IList<TFString> strings, ExportOptions options)
        {
            var section2Count = ((_header[14] << 8) + _header[15]);

            if (_section1 != null)
            {
                s.WriteBytes(_section1);
            }

            var newPointer2 = (int) s.Position;

            var stringOffset = newPointer2 + section2Count * 4;
            for (var i = 0; i < section2Count; i++)
            {
                s.WriteValueS32(stringOffset, Endianness);
                
                var pos = s.Position;
                s.Seek(stringOffset, SeekOrigin.Begin);
                
                var str = FindString(strings, _namesList[i].Offset);
                WriteString(s, str, options);

                stringOffset = (int) s.Position;
                
                s.Seek(pos, SeekOrigin.Begin);
            }

            if (section2Count == 0)
            {
                newPointer2 = 0;
            }

            s.Seek(0, SeekOrigin.End);
            var newPointer3 = 0;
            
            if (_section3 != null)
            {
                newPointer3 = (int)s.Position;
                s.WriteBytes(_section3);
            }

            return new Tuple<int, int>(newPointer2, newPointer3);
        }

        private static TFString FindString(IEnumerable<TFString> strings, int offset)
        {
            return strings.FirstOrDefault(s => s.Offset == offset);
        }

        private void WriteString(Stream s, TFString str, ExportOptions options)
        {
            if (str.Original == str.Translation)
            {
                var aux = str.Original;

                aux = Yakuza0Project.WritingReplacements(aux);

                s.WriteStringZ(aux, Encoding);
            }
            else
            {
                var aux = str.Translation;
                if (options.CharReplacement != 0)
                {
                    aux = Utils.ReplaceChars(aux, options.CharReplacementList);
                }

                aux = Yakuza0Project.WritingReplacements(aux);

                s.WriteStringZ(aux, options.SelectedEncoding);
            }
        }

        private void WriteString(Stream s, string str, ExportOptions options, bool isOriginal)
        {
            if (isOriginal)
            {
                str = Yakuza0Project.WritingReplacements(str);

                s.WriteStringZ(str, Encoding);
            }
            else
            {
                var aux = str;
                if (options.CharReplacement != 0)
                {
                    aux = Utils.ReplaceChars(aux, options.CharReplacementList);
                }

                aux = Yakuza0Project.WritingReplacements(aux);

                s.WriteStringZ(aux, options.SelectedEncoding);
            }
        }
    }
}
