using System.Collections.Generic;
using System.Text;

namespace TF.Core.Entities
{
    public interface ITFFile
    {
        Gibbed.IO.Endian Endianness { get; }
        Encoding Encoding { get; }

        string Hash { get; }
        string Path { get; }

        IList<TFString> Strings { get; }

        string FileType { get; }

        void Read();
        void Save(string fileName, IList<TFString> strings, ExportOptions options);
    }
}