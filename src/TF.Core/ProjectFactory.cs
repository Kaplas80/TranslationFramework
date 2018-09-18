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

            throw new Exception();
        }
    }
}
