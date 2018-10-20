using System;
using System.IO;
using TF.Core.Entities;

namespace TF.Core.Projects.Spellforce2.Files
{
    public static class FileFactory
    {
        public static ITFFile GetFile(string fileName)
        {
            var header = ReadHeader(fileName);
            return CommonGetFile(fileName, header);
        }

        public static ITFFile GetFile(string fileName, byte[] content)
        {
            var header = new byte[4];
            Array.Copy(content, header, 4);

            return CommonGetFile(fileName, header);
        }

        private static byte[] ReadHeader(string fileName)
        {
            using (var br = new BinaryReader(new FileStream(fileName, FileMode.Open)))
            {
                var bytes = br.ReadBytes(4);

                return bytes;
            }
        }

        private static ITFFile CommonGetFile(string fileName, byte[] header)
        {
            if (fileName.EndsWith("9018.dat") || fileName.EndsWith("9035.dat") ||
                fileName.EndsWith("9036.dat") || fileName.EndsWith("9037.dat") ||
                fileName.EndsWith("9039.dat") || fileName.EndsWith("9040.dat") ||
                fileName.EndsWith("9041.dat") || fileName.EndsWith("9042.dat") ||
                fileName.EndsWith("9044.dat") || fileName.EndsWith("9045.dat") ||
                fileName.EndsWith("9046.dat") || fileName.EndsWith("9047.dat") ||
                fileName.EndsWith("9053.dat") || 
                fileName.EndsWith("9055.dat") || fileName.EndsWith("9057.dat"))
            {
                return new Type1aFile(fileName);
            }

            if (fileName.EndsWith("9043.dat") || fileName.EndsWith("9048.dat"))
            {
                return new Type1bFile(fileName);
            }

            if (fileName.EndsWith("9051.dat"))
            {
                return new Type2File(fileName);
            }

            if (fileName.EndsWith("9052.dat"))
            {
                return new Type3File(fileName);
            }

            if (fileName.EndsWith("9003.dat") || fileName.EndsWith("9050.dat"))
            {
                return new Type4File(fileName);
            }

            return null;
        }
    }
}
