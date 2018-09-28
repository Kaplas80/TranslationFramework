namespace TF.Core.Projects.Disgaea
{
    public class ProjectInfo : ISupportedProject
    {
        public string SaveProjectFilter => "Traducción Disgaea|*.tf_dsg";

        public string OpenProjectFilter => "*.tf_dsg";

        public override string ToString() => "Disgaea PC";
    }
}
