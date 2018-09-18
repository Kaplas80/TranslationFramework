using System.Collections.Generic;
using System.IO;
using System.Text;
using Gibbed.IO;
using TF.Core.Entities;

namespace TF.Core.Projects.Yakuza0.Files
{
    public class CmnFile : TFFile
    {
        private class DataItem
        {
            public TFString Data;
            public int MaxLength;
        }

        private static readonly byte[] PATTERN = {0x8E, 0x9A, 0x96, 0x8B, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};

        private List<DataItem> _dataList;

        public CmnFile(string fileName) : base(fileName)
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
                     result.Add(item.Data);
                }
            
                return result;
            }
        }

        public override string FileType => "cmn.bin";

        public override void Read()
        {
            using (var fs = new FileStream(Path, FileMode.Open))
            {
                _dataList = new List<DataItem>();

                var currentIndex = fs.FindPattern(PATTERN);

                while (currentIndex != -1)
                {
                    var subtitleType = fs.ReadValueU64(Endianness);
                    if (subtitleType == 0)
                    {
                        // El subtitulo es normal
                        fs.ReadBytes(40);

                        var subCount1 = fs.ReadValueS32(Endianness);
                        var subCount2 = fs.ReadValueS32(Endianness);

                        fs.ReadBytes(16);

                        // Estoy al comienzo de los primeros subtitulos
                        fs.ReadBytes(272 * subCount1);

                        // Ahora los segundos
                        for (var i = 0; i < subCount2; i++)
                        {
                            fs.ReadBytes(16);

                            var pos = fs.Position;

                            var item = GetDataItem(fs, (int)pos, currentIndex.ToString("X8"), 160);
                            _dataList.Add(item);

                            fs.Seek(pos + 256, SeekOrigin.Begin);
                        }
                    }
                    else
                    {
                        // El subtitulo está desplazado
                        // Hacemos un poco de "magia" para colocarnos al principio del texto
                        fs.ReadBytes(266);
                        var type = fs.ReadValueS32(Endianness);
                        int step = 0;
                        int numSubs = 0;
                        switch (type)
                        {
                            case 1:
                                step = 32;
                                numSubs = 1;
                                break;
                            case 2:
                                step = 176;
                                numSubs = 2;
                                break;
                            case 3:
                                step = 320;
                                numSubs = 1;
                                break;
                            case 4:
                                step = 464;
                                numSubs = 2;
                                break;
                        }
                        
                        fs.ReadBytes(172 + step);

                        for (int i = 0; i < numSubs; i++)
                        {
                            fs.ReadBytes(16);

                            var pos = fs.Position;
                            var item = GetDataItem(fs, (int) pos, currentIndex.ToString("X8"), 128);
                            _dataList.Add(item);

                            fs.Seek(pos + 128, SeekOrigin.Begin);
                        }
                    }

                    currentIndex = fs.FindPattern(PATTERN);
                }
            }
        }

        private DataItem GetDataItem(Stream s, int offset, string section, int length)
        {
            var str = s.ReadStringZ(Encoding);
            if (!string.IsNullOrEmpty(str.TrimEnd('\0')))
            {
                str = Yakuza0Project.ReadingReplacements(str);
            }

            var item = new DataItem
            {
                MaxLength = length,
                Data = new TFString
                {
                    Offset = offset,
                    Section = section,
                    Original = str,
                    Translation = str,
                    Visible = !string.IsNullOrEmpty(str.TrimEnd('\0'))
                }
            };

            return item;
        }

        public override void Save(string fileName, IList<TFString> strings, ExportOptions options)
        {
            File.Copy(Path, fileName, true);
            using (var fs = new FileStream(fileName, FileMode.Open))
            {
                for (var i = 0; i < _dataList.Count; i++)
                {
                    var item = _dataList[i];

                    fs.Seek(item.Data.Offset, SeekOrigin.Begin);

                    var str = strings[i].Translation;

                    if (strings[i].Visible && !string.IsNullOrEmpty(str))
                    {
                        var zeros = new byte[item.MaxLength];

                        fs.WriteBytes(zeros);

                        if (options.CharReplacement != 0)
                        {
                            str = Utils.ReplaceChars(str, options.CharReplacementList);
                        }

                        str = Yakuza0Project.WritingReplacements(str);

                        if (str.GetLength(options.SelectedEncoding) >= item.MaxLength)
                        {
                            str = str.Substring(0, item.MaxLength - 1);
                        }
                        
                        fs.Seek(item.Data.Offset, SeekOrigin.Begin);
                        fs.WriteStringZ(str, options.SelectedEncoding);
                    }
                }
            }
        }
    }
}
