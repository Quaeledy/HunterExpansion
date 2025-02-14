using CustomSaveTx;
using System.Text.RegularExpressions;

namespace HunterExpansion.CustomSave
{
    internal class IntroTextSave : DeathPersistentSaveDataTx
    {
        public static bool[] introText = new bool[1];

        public override string header
        {
            get
            {
                return "NSHINTROTEXT";
            }
        }

        public IntroTextSave(SlugcatStats.Name name) : base(name)
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
                result = "<mpdA>" + header;
                for (int i = 0; i < introText.Length; i++)
                {
                    result += "<mpdB>" + i.ToString() + "<introText>" + introText[i].ToString();
                }
            }

            return result;
        }

        public override void LoadDatas(string data)
        {
            base.LoadDatas(data);

            string[] array = Regex.Split(data, "<mpdA>");
            for (int i = 0; i < array.Length; i++)
            {
                string[] array2 = Regex.Split(array[i], "<mpdB>");
                if (array2[0] == header)
                {
                    for (int j = 1; j < array2.Length; j++)
                    {
                        string[] array3 = Regex.Split(array2[j], "<introText>");
                        if (array3.Length > 1 && array3[0] != "" && array3[1] != "")
                        {
                            int header = int.Parse(array3[0]);
                            introText[header] = bool.Parse(array3[1]);
                        }
                    }
                }
            }
        }

        public override void ClearDataForNewSaveState(SlugcatStats.Name newSlugName)
        {
            base.ClearDataForNewSaveState(newSlugName);
            for (int i = 0; i < introText.Length; i++)
            {
                introText[i] = false;
            }
        }
    }
}
