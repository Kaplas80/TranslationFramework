using System.Collections.Generic;
using TF.Core.Projects.Yakuza0;

namespace TF.Core
{
    public static class Main
    {
        private static readonly ISupportedProject Yakuza0 = new ProjectInfo();
        private static readonly ISupportedProject Shenmue = new Projects.Shenmue.ProjectInfo();
        private static readonly List<ISupportedProject> SupportedProjects;

        static Main()
        {
            SupportedProjects = new List<ISupportedProject>
            {
                Yakuza0,
                Shenmue
            };
        }

        public static IList<ISupportedProject> GetSupportedProjects()
        {
            return SupportedProjects.AsReadOnly();
        }
    }
}
