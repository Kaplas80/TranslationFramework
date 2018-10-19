using System;
using System.IO;
using TF.Core.Entities;

namespace TF.Core.Projects.Infliction.Files
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
            var file = Path.GetFileName(fileName);
            if (file == "InflictionText.uexp")
            {
                return new TextFile(fileName);
            }

            if (file == "BP_Difficulty.uexp" || file == "BP_MainMenu.uexp")
            {
                return new MenuFile(fileName);
            }

            return null;
        }
    }
}
