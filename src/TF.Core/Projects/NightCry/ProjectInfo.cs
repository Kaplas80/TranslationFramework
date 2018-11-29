namespace TF.Core.Projects.NightCry
{
    public class ProjectInfo : ISupportedProject
    {
        public string SaveProjectFilter => "Traducción NightCry|*.tf_nightcry";

        public string OpenProjectFilter => "*.tf_nightcry";

        public override string ToString() => "NightCry";
    }
}
