namespace TF.Core.Projects.BattleRealms
{
    public class ProjectInfo : ISupportedProject
    {
        public string SaveProjectFilter => "Traducción Battle Realms|*.tf_btr";

        public string OpenProjectFilter => "*.tf_btr";

        public override string ToString() => "Battle Realms";
    }
}
