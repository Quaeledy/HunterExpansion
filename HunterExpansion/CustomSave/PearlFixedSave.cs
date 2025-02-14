using CustomSaveTx;

namespace HunterExpansion.CustomSave
{
    public class PearlFixedSave : DeathPersistentSaveDataTx
    {
        public static bool pearlFixed;

        public override string header
        {
            get
            {
                return "NSHPEARLFIXED";
            }
        }

        public PearlFixedSave(SlugcatStats.Name name) : base(name)
        {
            this.slugName = name;
        }

        public override string SaveToString(bool saveAsIfPlayerDied, bool saveAsIfPlayerQuit)
        {
            string result;
            if (saveAsIfPlayerDied || saveAsIfPlayerQuit)
            {
                result = this.origSaveData;
            }
            else
            {
                result = pearlFixed.ToString();
            }

            return result;
        }

        public override void LoadDatas(string data)
        {
            base.LoadDatas(data);
            pearlFixed = bool.Parse(data);
        }

        public override void ClearDataForNewSaveState(SlugcatStats.Name newSlugName)
        {
            base.ClearDataForNewSaveState(newSlugName);
            if (pearlFixed)
            {
                pearlFixed = false;
            }
        }
    }
}
