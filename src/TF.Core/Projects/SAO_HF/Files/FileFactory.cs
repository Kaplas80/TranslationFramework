using System;
using System.IO;
using System.Linq;
using TF.Core.Entities;


namespace TF.Core.Projects.SAO_HF.Files
{
    public static class FileFactory
    {
        private static readonly byte[] OFS3_HEADER = {0x4F, 0x46, 0x53, 0x33};
        private static readonly byte[] SCRIPT_HEADER = {0x20, 0x00, 0x00, 0x00};

        public static ITFFile GetFile(string fileName)
        {
            var header = ReadHeader(fileName);
            if (fileName.Contains("localize_msg.dat") || header.SequenceEqual(OFS3_HEADER))
            {
                return new Ofs3File(fileName);
            }

            if (header.SequenceEqual(SCRIPT_HEADER))
            {
                return new ScriptFile(fileName);
            }

            return null;
        }

        public static ITFFile GetFile(string fileName, byte[] fileContent)
        {
            var header = new byte[4];
            Array.Copy(fileContent, header, 4);
            
            if (fileName.Contains("localize_msg.dat") || header.SequenceEqual(OFS3_HEADER))
            {
                return new Ofs3File(fileName);
            }

            if (header.SequenceEqual(SCRIPT_HEADER))
            {
                return new ScriptFile(fileName);
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
