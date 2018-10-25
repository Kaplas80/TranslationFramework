namespace TF.Core.Entities
{
    public class TFString
    {
        public long Id { get; set; }

        public long FileId { get; set; }

        public int Offset { get; set; }

        public string Section { get; set; }

        public string Original { get; set; }

        public string Translation { get; set; }

        public bool Visible { get; set; }
    }
}
