using System.Collections.Generic;
using System.IO;
using System.Text;
using Gibbed.IO;

namespace TF.Core.Entities
{
    public abstract class TFFile : ITFFile
    {
        public long Id { get; set; }
        public virtual string Hash { get; set; }
        public virtual string Path { get; set; }
        
        public virtual IList<TFString> Strings { get; }

        public abstract Encoding Encoding { get; }

        
        public abstract Endian Endianness { get; }
        public abstract string FileType { get; }
        public abstract void Read(Stream s);
        public abstract void Save(string fileName, byte[] originalContent, IList<TFString> strings, ExportOptions options);

        protected TFFile()
        {
            
        }

        protected TFFile(string path)
        {
            Path = path;
            Hash = Utils.CalculateHash(path);
        }

        public override string ToString()
        {
            return Path;
        }
    }
}