
namespace TF.Core.Entities
{
    public class DbFile
    {
        public long Id { get; set; }
        public string Path { get; set; }
        public string Hash { get; set; }
        public byte[] Content { get; set; }
    }
}
