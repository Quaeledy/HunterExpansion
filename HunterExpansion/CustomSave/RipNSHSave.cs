﻿using CustomSaveTx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HunterExpansion.CustomSave
{
    public class RipNSHSave : DeathPersistentSaveDataTx
    {
        public static bool ripNSH;

        public override string header
        {
            get
            {
                return "NOSAVEDHARASSMENT";
            }
        }

        public RipNSHSave(SlugcatStats.Name name) : base(name)
        {
            this.slugName = name;
        }

        public override string SaveToString(bool saveAsIfPlayerDied, bool saveAsIfPlayerQuit)
        {
            string result;
            if (saveAsIfPlayerDied || saveAsIfPlayerQuit)
            {
                //result = this.origSaveData;
                result = ripNSH.ToString();
            }
            else
            {
                result = ripNSH.ToString();
            }

            return result;
        }

        public override void LoadDatas(string data)
        {
            base.LoadDatas(data);
            ripNSH = bool.Parse(data);
        }

        public override void ClearDataForNewSaveState(SlugcatStats.Name newSlugName)
        {
            base.ClearDataForNewSaveState(newSlugName);
            if (ripNSH)
            {
                ripNSH = false;
            }
        }
    }
}
