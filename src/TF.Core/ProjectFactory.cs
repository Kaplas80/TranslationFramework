using System;
using TF.Core.Entities;
using TF.Core.Projects.SAO_HF;
using TF.Core.Projects.Shenmue;
using TF.Core.Projects.SRR;
using TF.Core.Projects.Yakuza0;

namespace TF.Core
{
    public static class ProjectFactory
    {
        public static Project GetProject(string projectPath)
        {
            if (projectPath.EndsWith("tf_yak0"))
            {
                return Yakuza0Project.GetProject(projectPath);
            }

            if (projectPath.EndsWith("tf_shm"))
            {
                return ShenmueProject.GetProject(projectPath);
            }

            if (projectPath.EndsWith("tf_sao_hf"))
            {
                return SAOProject.GetProject(projectPath);
            }

            if (projectPath.EndsWith("tf_srr"))
            {
                return SRRProject.GetProject(projectPath);
            }

            throw new Exception();
        }
    }
}
