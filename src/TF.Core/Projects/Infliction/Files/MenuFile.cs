using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gibbed.IO;
using TF.Core.Entities;

namespace TF.Core.Projects.Infliction.Files
{
    public class MenuFile : TextFile
    {
        public MenuFile(string fileName) : base(fileName)
        {
            
        }

        protected override byte[] PATTERN => new byte[] { 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x21, 0x00, 0x00, 0x00 };
    }
}
