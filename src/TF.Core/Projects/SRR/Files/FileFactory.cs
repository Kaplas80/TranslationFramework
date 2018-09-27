using System;
using System.IO;
using System.Linq;
using TF.Core.Entities;

namespace TF.Core.Projects.SRR.Files
{
    public static class FileFactory
    {
        private static readonly byte[] MTP_HEADER = {0x4D, 0x54, 0x50, 0x41};

        public static ITFFile GetFile(string fileName)
        {
            var header = ReadHeader(fileName);
            
            if (header.SequenceEqual(MTP_HEADER))
            {
                return new MtpFile(fileName);
            }

            return null;
        }

        public static ITFFile GetFile(string fileName, byte[] fileContent)
        {
            var header = new byte[4];
            Array.Copy(fileContent, header, 4);
            
            if (header.SequenceEqual(MTP_HEADER))
            {
                return new MtpFile(fileName);
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
