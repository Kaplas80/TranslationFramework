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
        //private static readonly byte[] MSG_HEADER = {0x20, 0xF7};
        //private static readonly byte[] MSG_HEADER2 = {0x20, 0x67};
        private static readonly byte[] MFP_HEADER = {0x6D, 0x66, 0x70, 0x62};
        
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
                        
            if (fileName.EndsWith(".msg"))
            {
                return new MsgFile(fileName);
            }

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

            if (fileName.EndsWith("arms_repair.bin"))
            {
                return new ArmsRepairFile(fileName);
            }

            if (fileName.EndsWith("blacksmith.bin"))
            {
                return new BlacksmithFile(fileName);
            }
            
            if (fileName.EndsWith("present.bin") || 
                fileName.EndsWith("send.bin") ||
                fileName.EndsWith("throw.bin"))
            {
                return new CommonShopBinFile(fileName);
            }

            if (fileName.EndsWith("sale0000.bin") ||
                fileName.EndsWith("sale0001.bin") ||
                fileName.EndsWith("sale0002.bin"))
            {
                return new SaleBinFile(fileName);
            }
            
            if (fileName.EndsWith("ai_popup.bin"))
            {
                return new AiPopupFile(fileName);
            }
            
            if (fileName.EndsWith("snitch.bin"))
            {
                return new SnitchFile(fileName);
            }

            if ((fileName.Contains("restaurant00") ||
                 fileName.Contains("ex_shop00") ||
                 fileName.Contains("shop00")) && fileName.EndsWith(".bin"))
            {
                return new RestaurantFile(fileName);
            }

            if (fileName.Contains("bar00") && fileName.EndsWith(".bin"))
            {
                return new BarFile(fileName);
            }

            return null;
        }
    }
}
