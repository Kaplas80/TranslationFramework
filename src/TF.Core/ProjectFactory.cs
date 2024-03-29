﻿using System;
using TF.Core.Entities;
using TF.Core.Projects.BattleRealms;
using TF.Core.Projects.Disgaea;
using TF.Core.Projects.JJMacfield;
using TF.Core.Projects.NightCry;
using TF.Core.Projects.SAO_HF;
using TF.Core.Projects.Shenmue;
using TF.Core.Projects.Spellforce2;
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

            if (projectPath.EndsWith("tf_dsg"))
            {
                return DisgaeaProject.GetProject(projectPath);
            }

            if (projectPath.EndsWith("tf_btr"))
            {
                return BattleRealmsProject.GetProject(projectPath);
            }

            if (projectPath.EndsWith("tf_spf2"))
            {
                return Spellforce2Project.GetProject(projectPath);
            }

            if (projectPath.EndsWith("tf_jjm"))
            {
                return JJMacfieldProject.GetProject(projectPath);
            }

            if (projectPath.EndsWith("tf_nightcry"))
            {
                return NightCryProject.GetProject(projectPath);
            }

            throw new Exception();
        }
    }
}
