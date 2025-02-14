using CustomSaveTx;

namespace HunterExpansion.CustomSave
{
    public class FondDreamCompletedSave : DeathPersistentSaveDataTx
    {
        public static bool fondDreamCompleted;

        public override string header
        {
            get
            {
                return "FONDDREAMCOMPLETED";
            }
        }

        public FondDreamCompletedSave(SlugcatStats.Name name) : base(name)
        {
            this.slugName = name;
        }

        public override string SaveToString(bool saveAsIfPlayerDied, bool saveAsIfPlayerQuit)
        {
            string result;
            if (saveAsIfPlayerDied || saveAsIfPlayerQuit)
            {
                result = fondDreamCompleted.ToString();
            }
            else
            {
                result = fondDreamCompleted.ToString();
            }

            return result;
        }

        public override void LoadDatas(string data)
        {
            base.LoadDatas(data);
            fondDreamCompleted = bool.Parse(data);
        }

        public override void ClearDataForNewSaveState(SlugcatStats.Name newSlugName)
        {
            base.ClearDataForNewSaveState(newSlugName);
            if (fondDreamCompleted)
            {
                fondDreamCompleted = false;
            }
        }
    }
}
