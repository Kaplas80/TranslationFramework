namespace TF.Core.Projects.JJMacfield
{
    public class ProjectInfo : ISupportedProject
    {
        public string SaveProjectFilter => "Traducción J.J. Macfield|*.tf_jjm";

        public string OpenProjectFilter => "*.tf_jjm";

        public override string ToString() => "J.J. Macfield";
    }
}
