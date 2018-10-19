namespace TF.Core.Projects.Infliction
{
    public class ProjectInfo : ISupportedProject
    {
        public string SaveProjectFilter => "Traducción Infliction|*.tf_inf";

        public string OpenProjectFilter => "*.tf_inf";

        public override string ToString() => "Infliction";
    }
}
