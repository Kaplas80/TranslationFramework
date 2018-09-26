namespace TF.Core.Projects.SAO_HF
{
    public class ProjectInfo : ISupportedProject
    {
        public string SaveProjectFilter => "Traducción Sword Art Online - HF|*.tf_sao_hf";

        public string OpenProjectFilter => "*.tf_sao_hf";

        public override string ToString() => "Sword Art Online - HF";
    }
}
