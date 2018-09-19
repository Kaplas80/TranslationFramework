namespace TF.Core.Projects.Shenmue
{
    public class ProjectInfo : ISupportedProject
    {
        public string SaveProjectFilter => "Proyectos de traducción Shenmue|*.tf_shm";

        public string OpenProjectFilter => "*.tf_shm";

        public override string ToString() => "Shenmue";
    }
}
