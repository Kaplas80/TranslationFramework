using System;
using System.IO;
using System.Linq;
using TF.Core.Entities;

namespace TF.Core.Projects.Disgaea.Files
{
    public static class FileFactory
    {
        public static ITFFile GetFile(string fileName)
        {
            var header = ReadHeader(fileName);
            
            if (fileName.ToLower().EndsWith("talk.dat"))
            {
                return new DatFile(fileName);
            }

            return null;
        }

        public static ITFFile GetFile(string fileName, byte[] fileContent)
        {
            var header = new byte[4];
            Array.Copy(fileContent, header, 4);
            
            if (fileName.ToLower().EndsWith("talk.dat"))
            {
                return new DatFile(fileName);
            }

            return null;
        }

        private static byte[] ReadHeader(string fileName)
        {
            using (var br = new BinaryReader(new FileStream(fileName, FileMode.Open)))
            {
                var bytes = br.ReadBytes(4);

                return bytes;
            }
        }
    }
}
