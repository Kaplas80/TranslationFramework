using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;


namespace Patcher
{
    class Program
    {
        private const string SHA1 = "d7eb899660de26c670c0c211675c00564d7c8f2e";

        private static Tuple<int, byte>[] PATCH = 
        {
            new Tuple<int, byte>(0x00D8DB38, 0xCD),
            new Tuple<int, byte>(0x00D8DB39, 0xCC),
            new Tuple<int, byte>(0x00D8DB3A, 0x0C),
            new Tuple<int, byte>(0x00D8DB3B, 0x3F),
            new Tuple<int, byte>(0x00D8DB3C, 0x9A),
            new Tuple<int, byte>(0x00D8DB3D, 0x99),
            new Tuple<int, byte>(0x00D8DB3E, 0x19),
            new Tuple<int, byte>(0x00D8DB3F, 0x3F),
            new Tuple<int, byte>(0x00D8DB40, 0xCD),
            new Tuple<int, byte>(0x00D8DB41, 0xCC),
            new Tuple<int, byte>(0x00D8DB42, 0x0C),
            new Tuple<int, byte>(0x00D8DB43, 0x3F),
            new Tuple<int, byte>(0x00D8DB44, 0x9A),
            new Tuple<int, byte>(0x00D8DB45, 0x99),
            new Tuple<int, byte>(0x00D8DB46, 0x19),
            new Tuple<int, byte>(0x00D8DB47, 0x3F),
            new Tuple<int, byte>(0x00D8DB48, 0xCD),
            new Tuple<int, byte>(0x00D8DB49, 0xCC),
            new Tuple<int, byte>(0x00D8DB4A, 0x0C),
            new Tuple<int, byte>(0x00D8DB4B, 0x3F),
            new Tuple<int, byte>(0x00D8DB4C, 0x9A),
            new Tuple<int, byte>(0x00D8DB4D, 0x99),
            new Tuple<int, byte>(0x00D8DB4E, 0x19),
            new Tuple<int, byte>(0x00D8DB4F, 0x3F),
            new Tuple<int, byte>(0x00D8DE08, 0x33),
            new Tuple<int, byte>(0x00D8DE09, 0x33),
            new Tuple<int, byte>(0x00D8DE0A, 0xB3),
            new Tuple<int, byte>(0x00D8DE0B, 0x3E),
            new Tuple<int, byte>(0x00D8DE0C, 0x9A),
            new Tuple<int, byte>(0x00D8DE0D, 0x99),
            new Tuple<int, byte>(0x00D8DE0E, 0x99),
            new Tuple<int, byte>(0x00D8DE0F, 0x3E),
            new Tuple<int, byte>(0x00D8DE10, 0x66),
            new Tuple<int, byte>(0x00D8DE11, 0x66),
            new Tuple<int, byte>(0x00D8DE12, 0xE6),
            new Tuple<int, byte>(0x00D8DE13, 0x3E),
            new Tuple<int, byte>(0x00D8DE14, 0x9A),
            new Tuple<int, byte>(0x00D8DE15, 0x99),
            new Tuple<int, byte>(0x00D8DE16, 0x99),
            new Tuple<int, byte>(0x00D8DE17, 0x3E),
            new Tuple<int, byte>(0x00D8DE1B, 0x3F),
            new Tuple<int, byte>(0x00D8DE1F, 0x3F),
            new Tuple<int, byte>(0x00D8DF5C, 0xCD),
            new Tuple<int, byte>(0x00D8DF5D, 0xCC),
            new Tuple<int, byte>(0x00D8DF5E, 0x8C),
            new Tuple<int, byte>(0x00D8DF5F, 0x3F),
            new Tuple<int, byte>(0x00D8DF64, 0xCD),
            new Tuple<int, byte>(0x00D8DF65, 0xCC),
            new Tuple<int, byte>(0x00D8DF66, 0x8C),
            new Tuple<int, byte>(0x00D8DF67, 0x3F),
            new Tuple<int, byte>(0x00D8DF6C, 0xCD),
            new Tuple<int, byte>(0x00D8DF6D, 0xCC),
            new Tuple<int, byte>(0x00D8DF6E, 0x8C),
            new Tuple<int, byte>(0x00D8DF6F, 0x3F),
            new Tuple<int, byte>(0x00D8E25C, 0x8F),
            new Tuple<int, byte>(0x00D8E25D, 0xC2),
            new Tuple<int, byte>(0x00D8E25E, 0x95),
            new Tuple<int, byte>(0x00D8E25F, 0x3F),
            new Tuple<int, byte>(0x00D8E264, 0x8F),
            new Tuple<int, byte>(0x00D8E265, 0xC2),
            new Tuple<int, byte>(0x00D8E266, 0x95),
            new Tuple<int, byte>(0x00D8E267, 0x3F),
            new Tuple<int, byte>(0x00D8E26C, 0x8F),
            new Tuple<int, byte>(0x00D8E26D, 0xC2),
            new Tuple<int, byte>(0x00D8E26E, 0x95),
            new Tuple<int, byte>(0x00D8E26F, 0x3F),
            new Tuple<int, byte>(0x045B106C, 0xEB),
            new Tuple<int, byte>(0x045B106D, 0x1C),
            new Tuple<int, byte>(0x045C4298, 0xEB),
            new Tuple<int, byte>(0x045C4299, 0x47),
            new Tuple<int, byte>(0x067AF6D6, 0x90),
            new Tuple<int, byte>(0x067AF6D7, 0x90),
        };

        static void Main(string[] args)
        {
            Console.WriteLine("Argumentos: {0}", args);
            if (args.Length != 1)
            {
                Console.WriteLine("Debes pasar el fichero Yakuza0.exe original como parametro");
                Console.WriteLine("Pulsa RETURN para salir...");
                Console.ReadLine();
                return;
            }

            var file = args[0];

            if (!File.Exists(file))
            {
                Console.WriteLine($"No se encuentra el fichero {file}");
                Console.WriteLine("Pulsa RETURN para salir...");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Comprobando SHA1...");
            var sha1 = CalculateHash(file);

            if (sha1 != SHA1)
            {
                Console.WriteLine($"El fichero {file} tiene un SHA1 distinto, es posible que se haya actualizado o que lo hayas modificado");
                Console.WriteLine("Pulsa RETURN para salir...");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("SHA1 correcto. Parcheando...");

            File.Copy(file, file + ".backup", true);

            using (var fs = new FileStream(file, FileMode.Open))
            {
                foreach (var patch in PATCH)
                {
                    fs.Seek(patch.Item1, SeekOrigin.Begin);
                    fs.WriteByte(patch.Item2);
                }
            }

            Console.WriteLine("Parcheo finalizado.");
            Console.WriteLine("Pulsa RETURN para salir...");
            Console.ReadLine();
        }

        private static string CalculateHash(string path)
        {
            using (var cp = new SHA1CryptoServiceProvider())
            {
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    var hash = cp.ComputeHash(fs);

                    var sBuilder = new StringBuilder();

                    foreach (var value in hash)
                    {
                        sBuilder.Append(value.ToString("x2"));
                    }

                    return sBuilder.ToString();
                }
            }
        }
    }
}
