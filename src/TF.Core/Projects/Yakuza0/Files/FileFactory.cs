using System;
using System.IO;
using System.Linq;
using TF.Core.Entities;

namespace TF.Core.Projects.Yakuza0.Files
{
    public static class FileFactory
    {
        private static readonly byte[] EPMB_HEADER = {0x65, 0x50, 0x4d, 0x42};
        private static readonly byte[] DB_HEADER = {0x20, 0x07, 0x03, 0x19};
        private static readonly byte[] MSG_HEADER = {0x20, 0xF7};

        public static ITFFile GetFile(string fileName)
        {
            if (fileName.Contains("string_tbl.bin_"))
            {
                return new StringTblFile(fileName);
            }

            if (fileName.Contains("mail.bin_"))
            {
                return new MailFile(fileName);
            }

            if (fileName.Contains("cmn.bin"))
            {
                return new CmnFile(fileName);
            }

            var header = ReadHeader(fileName);

            if (header.SequenceEqual(EPMB_HEADER))
            {
                return new EpmbFile(fileName);
            }

            if (header.SequenceEqual(DB_HEADER))
            {
                return new DbFile(fileName);
            }

            var header2bytes = new byte[2];
            Array.Copy(header, header2bytes, 2);
            if (header2bytes.SequenceEqual(MSG_HEADER))
            {
                return new MsgFile(fileName);
            }

            if (fileName.EndsWith(".exe"))
            {
                return new ExeFile(fileName);
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
