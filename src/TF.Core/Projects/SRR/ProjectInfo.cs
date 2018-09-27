namespace TF.Core.Projects.SRR
{
    public class ProjectInfo : ISupportedProject
    {
        public string SaveProjectFilter => "Traducción Shining Resonance Refrain|*.tf_srr";

        public string OpenProjectFilter => "*.tf_srr";

        public override string ToString() => "Shining Resonance Refrain";
    }
}
