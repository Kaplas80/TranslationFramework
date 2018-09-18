using System.IO;

namespace TF.Core.Projects.Yakuza0
{
    public class ProjectInfo : ISupportedProject
    {
        public string SaveProjectFilter => "Proyectos de traducción Yakuza 0|*.tf_yak0";

        public bool ValidateInstallPath(string path)
        {
            return File.Exists($"{path}\\Yakuza0.exe");
        }

        public override string ToString() => "Yakuza 0";
    }
}
