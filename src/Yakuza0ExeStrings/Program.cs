using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gibbed.IO;

namespace Yakuza0ExeStrings
{
    class Program
    {
        private static readonly long FILE_BASE = 0x0140001600;

        static void Main(string[] args)
        {
            var usedStrings = new Dictionary<long, bool>();
            using (var outputStream = new FileStream("result.txt", FileMode.Create))
            using (var output = new StreamWriter(outputStream, Encoding.UTF8))
            using (var input = new FileStream(@"H:\Steam\steamapps\common\Yakuza 0\media\Yakuza0.exe.backup", FileMode.Open))
            {
                input.Seek(0x01043A00, SeekOrigin.Begin);

                while (input.Position < 0x0115B000)
                {
                    var currentOffset = input.Position;

                    var read = input.ReadValueS64(Endian.Little);

                    var possibleOffset = read - FILE_BASE;

                    if (possibleOffset > 0 && possibleOffset < input.Length)
                    {
                        var inDictionary = usedStrings.ContainsKey(possibleOffset);

                        var pos = input.Position;
                        input.Seek(possibleOffset, SeekOrigin.Begin);

                        var str = input.ReadStringZ(Encoding.UTF8);

                        if (!string.IsNullOrWhiteSpace(str))
                        {
                            str = str.Replace("\r\n", "\\r\\n").Replace("\r", "\\r").Replace("\n", "\\n");

                            var line = $"{currentOffset:X16}\t{possibleOffset:X16}\t{(inDictionary ? 1 : 0)}\t{str}";
                            output.WriteLine(line);
                            

                            if (!inDictionary)
                            {
                                usedStrings[possibleOffset] = true;
                            }
                        }
                        
                        input.Seek(pos, SeekOrigin.Begin);
                    }
                    else
                    {
                        output.WriteLine();
                    }
                }
            }
        }
    }
}
