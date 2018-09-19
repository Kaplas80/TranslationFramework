using System;

using TF.Core.Entities;

namespace TF.Core
{
    public static class ProjectFactory
    {
        public static Project GetProject(string projectPath)
        {
            if (projectPath.EndsWith("tf_yak0"))
            {
                return Projects.Yakuza0.Yakuza0Project.GetProject(projectPath);
            }

            if (projectPath.EndsWith("tf_shm"))
            {
                return Projects.Shenmue.ShenmueProject.GetProject(projectPath);
            }

            throw new Exception();
        }
    }
}
