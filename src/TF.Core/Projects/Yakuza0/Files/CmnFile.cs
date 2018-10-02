using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public override void Read(Stream s)
        {
            _dataList = new List<DataItem>();

            var currentIndex = s.FindPattern(PATTERN);

            while (currentIndex != -1)
            {
                var subtitleType = s.ReadValueU64(Endianness);
                if (subtitleType == 0)
                {
                    // El subtitulo es normal
                    s.ReadBytes(40);

                    var subCount1 = s.ReadValueS32(Endianness);
                    var subCount2 = s.ReadValueS32(Endianness);

                    s.ReadBytes(16);

                    // Estoy al comienzo de los primeros subtitulos
                    s.ReadBytes(272 * subCount1);

                    // Ahora los segundos
                    for (var i = 0; i < subCount2; i++)
                    {
                        s.ReadBytes(16);

                        var pos = s.Position;

                        var item = GetDataItem(s, (int) pos, currentIndex.ToString("X8"), 160);
                        _dataList.Add(item);

                        s.Seek(pos + 256, SeekOrigin.Begin);
                    }
                }
                else
                {
                    // El subtitulo está desplazado
                    // Hacemos un poco de "magia" para colocarnos al principio del texto
                    s.ReadBytes(266);
                    var numJapaneseSubs = s.ReadValueS32(Endianness);
                    s.ReadBytes(12);
                    s.ReadBytes(16);

                    s.ReadBytes(numJapaneseSubs * 144);

                    // Ahora estoy al principio de los subtitulos en inglés
                    var numEnglishSubs = s.ReadValueS32(Endianness);
                    s.ReadBytes(12);
                    s.ReadBytes(16);

                    for (int i = 0; i < numEnglishSubs; i++)
                    {
                        s.ReadBytes(16);

                        var pos = s.Position;
                        var item = GetDataItem(s, (int) pos, currentIndex.ToString("X8"), 128);
                        _dataList.Add(item);

                        s.Seek(pos + 128, SeekOrigin.Begin);
                    }
                }

                currentIndex = s.FindPattern(PATTERN);
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
                    FileId = Id,
                    Offset = offset,
                    Section = section,
                    Original = str,
                    Translation = str,
                    Visible = !string.IsNullOrEmpty(str.TrimEnd('\0'))
                }
            };

            return item;
        }

        public override void Save(string fileName, byte[] originalContent, IList<TFString> strings, ExportOptions options)
        {
            File.WriteAllBytes(fileName, originalContent);

            using (var fs = new FileStream(fileName, FileMode.Open))
            {
                for (var i = 0; i < _dataList.Count; i++)
                {
                    var item = _dataList[i];

                    fs.Seek(item.Data.Offset, SeekOrigin.Begin);

                    var tfString = strings.FirstOrDefault(x => x.Offset == item.Data.Offset);

                    if (tfString != null)
                    {
                        var str = tfString.Translation;

                        if (tfString.Visible && !string.IsNullOrEmpty(str))
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
}
