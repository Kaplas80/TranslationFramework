namespace TF.Core.Projects.Spellforce2
{
    public class ProjectInfo : ISupportedProject
    {
        public string SaveProjectFilter => "Traducción Spellforce 2|*.tf_spf2";

        public string OpenProjectFilter => "*.tf_spf2";

        public override string ToString() => "Spellforce 2";
    }
}
