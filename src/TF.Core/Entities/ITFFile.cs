using System.Collections.Generic;
using System.IO;
using System.Text;
using Gibbed.IO;

namespace TF.Core.Entities
{
    public interface ITFFile
    {
        long Id { get; set; }

        Endian Endianness { get; }
        Encoding Encoding { get; }

        string Hash { get; }
        string Path { get; }

        IList<TFString> Strings { get; }

        string FileType { get; }

        void Read(Stream s);
        void Save(string fileName, byte[] originalContent, IList<TFString> strings, ExportOptions options);
    }
}