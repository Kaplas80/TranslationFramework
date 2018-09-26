using System.Collections.Generic;

namespace TF.Core
{
    public static class Main
    {
        private static readonly ISupportedProject Yakuza0 = new Projects.Yakuza0.ProjectInfo();
        private static readonly ISupportedProject Shenmue = new Projects.Shenmue.ProjectInfo();
        private static readonly ISupportedProject SAO_HF = new Projects.SAO_HF.ProjectInfo();
        private static readonly List<ISupportedProject> SupportedProjects;

        static Main()
        {
            SupportedProjects = new List<ISupportedProject>
            {
                Yakuza0,
                Shenmue,
                SAO_HF
            };
        }

        public static IList<ISupportedProject> GetSupportedProjects()
        {
            return SupportedProjects.AsReadOnly();
        }
    }
}
