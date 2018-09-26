namespace TF.Core.Projects.Shenmue
{
    public class ProjectInfo : ISupportedProject
    {
        public string SaveProjectFilter => "Traducción Shenmue|*.tf_shm";

        public string OpenProjectFilter => "*.tf_shm";

        public override string ToString() => "Shenmue";
    }
}
