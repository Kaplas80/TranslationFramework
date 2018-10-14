namespace TF.Core.Projects.MegaManLegends
{
    public class ProjectInfo : ISupportedProject
    {
        public string SaveProjectFilter => "Traducción MegaMan Legends|*.tf_mml";

        public string OpenProjectFilter => "*.tf_mml";

        public override string ToString() => "MegaMan Legends";
    }
}
