using CustomSaveTx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HunterExpansion.CustomSave
{
    public class AquamarinePearlTokenSave : DeathPersistentSaveDataTx
    {
        public static bool aquamarinePearlToken;

        public override string header
        {
            get
            {
                return "AQUAMARINEPEARLTOKEN";
            }
        }

        public AquamarinePearlTokenSave(SlugcatStats.Name name) : base(name)
        {
            this.slugName = name;
        }

        public override string SaveToString(bool saveAsIfPlayerDied, bool saveAsIfPlayerQuit)
        {
            string result;
            if (saveAsIfPlayerDied || saveAsIfPlayerQuit)
            {
                result = aquamarinePearlToken.ToString(); //this.origSaveData;
            }
            else
            {
                result = aquamarinePearlToken.ToString();
            }

            return result;
        }

        public override void LoadDatas(string data)
        {
            base.LoadDatas(data);
            aquamarinePearlToken = bool.Parse(data);
        }

        public override void ClearDataForNewSaveState(SlugcatStats.Name newSlugName)
        {
            base.ClearDataForNewSaveState(newSlugName);
            if (aquamarinePearlToken)
            {
                aquamarinePearlToken = false;
            }
        }
    }
}
