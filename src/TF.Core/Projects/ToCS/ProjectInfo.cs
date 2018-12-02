namespace TF.Core.Projects.ToCS
{
    public class ProjectInfo : ISupportedProject
    {
        public string SaveProjectFilter => "Traducción ToCS|*.tf_tocs";

        public string OpenProjectFilter => "*.tf_tocs";

        public override string ToString() => "ToCS";
    }
}
