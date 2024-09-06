using CustomSaveTx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HunterExpansion.CustomSave
{
    public class SLOracleReceiveNSHNeuronCounterSave : DeathPersistentSaveDataTx
    {
        public static int sLOracleReceiveNSHNeuronCounter = 0;

        public override string header
        {
            get
            {
                return "SLORACLERECEIVENSHNEURONCOUNTER";
            }
        }

        public SLOracleReceiveNSHNeuronCounterSave(SlugcatStats.Name name) : base(name)
        {
            this.slugName = name;
        }

        public override string SaveToString(bool saveAsIfPlayerDied, bool saveAsIfPlayerQuit)
        {
            string result;
            if (saveAsIfPlayerDied || saveAsIfPlayerQuit)
            {
                result = origSaveData;
            }
            else
            {
                result = sLOracleReceiveNSHNeuronCounter.ToString();
            }

            return result;
        }

        public override void LoadDatas(string data)
        {
            base.LoadDatas(data);
            sLOracleReceiveNSHNeuronCounter = int.Parse(data);
        }

        public override void ClearDataForNewSaveState(SlugcatStats.Name newSlugName)
        {
            base.ClearDataForNewSaveState(newSlugName);
            sLOracleReceiveNSHNeuronCounter = 0;
        }
    }
}
