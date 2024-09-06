using CustomSaveTx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HunterExpansion.CustomSave
{
    public class TravelCompletedSave : DeathPersistentSaveDataTx
    {
        public static bool travelCompleted;

        public override string header
        {
            get
            {
                return "TRAVELCOMPLETED";
            }
        }

        public TravelCompletedSave(SlugcatStats.Name name) : base(name)
        {
            this.slugName = name;
        }

        public override string SaveToString(bool saveAsIfPlayerDied, bool saveAsIfPlayerQuit)
        {
            string result;
            if (saveAsIfPlayerDied || saveAsIfPlayerQuit)
            {
                result = travelCompleted.ToString();
            }
            else
            {
                result = travelCompleted.ToString();
            }

            return result;
        }

        public override void LoadDatas(string data)
        {
            base.LoadDatas(data);
            travelCompleted = bool.Parse(data);
        }

        public override void ClearDataForNewSaveState(SlugcatStats.Name newSlugName)
        {
            base.ClearDataForNewSaveState(newSlugName);
            if (travelCompleted)
            {
                travelCompleted = true;
            }
        }
    }
}
