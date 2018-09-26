namespace TF.Core.Projects.Yakuza0
{
    public class ProjectInfo : ISupportedProject
    {
        public string SaveProjectFilter => "Traducción Yakuza 0|*.tf_yak0";

        public string OpenProjectFilter => "*.tf_yak0";

        public override string ToString() => "Yakuza 0";
    }
}
