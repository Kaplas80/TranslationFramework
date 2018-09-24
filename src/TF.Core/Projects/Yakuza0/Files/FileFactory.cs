using System;
using System.IO;
using System.Linq;
using TF.Core.Entities;

namespace TF.Core.Projects.Yakuza0.Files
{
    public static class FileFactory
    {
        private static readonly byte[] EPMB_HEADER = {0x65, 0x50, 0x4D, 0x42};
        private static readonly byte[] DB_HEADER = {0x20, 0x07, 0x03, 0x19};
        private static readonly byte[] MSG_HEADER = {0x20, 0xF7};
        private static readonly byte[] MFP_HEADER = {0x6D, 0x66, 0x70, 0x62};

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

            if (fileName.Contains("Yakuza0.exe"))
            {
                return new ExeFile(fileName);
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

            if (header.SequenceEqual(MFP_HEADER))
            {
                return new MfpFile(fileName);
            }

            var header2bytes = new byte[2];
            Array.Copy(header, header2bytes, 2);
            if (header2bytes.SequenceEqual(MSG_HEADER))
            {
                return new MsgFile(fileName);
            }

            return null;
        }

        public static ITFFile GetFile(string fileName, byte[] content)
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

            if (fileName.Contains("Yakuza0.exe"))
            {
                return new ExeFile(fileName);
            }

            var header = new byte[4];
            Array.Copy(content, header, 4);

            if (header.SequenceEqual(EPMB_HEADER))
            {
                return new EpmbFile(fileName);
            }

            if (header.SequenceEqual(DB_HEADER))
            {
                return new DbFile(fileName);
            }

            if (header.SequenceEqual(MFP_HEADER))
            {
                return new MfpFile(fileName);
            }

            var header2bytes = new byte[2];
            Array.Copy(header, header2bytes, 2);
            if (header2bytes.SequenceEqual(MSG_HEADER))
            {
                return new MsgFile(fileName);
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
