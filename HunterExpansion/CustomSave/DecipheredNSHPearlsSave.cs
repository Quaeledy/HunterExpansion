using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HunterExpansion.CustomOracle;
using MoreSlugcats;
using System.Runtime;
using static System.Net.Mime.MediaTypeNames;

namespace HunterExpansion.CustomSave
{
    public class DecipheredNSHPearlsSave
    {
        public static string Header = "LORENSH";
        public static List<DataPearl.AbstractDataPearl.DataPearlType> decipheredNSHPearls;

        public static void Init()
        {
            On.PlayerProgression.MiscProgressionData.ctor += MiscProgressionData_ctor;
            On.PlayerProgression.MiscProgressionData.FromString += MiscProgressionData_FromString;
            On.PlayerProgression.MiscProgressionData.ToString += MiscProgressionData_ToString;
        }

        private static void MiscProgressionData_ctor(On.PlayerProgression.MiscProgressionData.orig_ctor orig, PlayerProgression.MiscProgressionData self, PlayerProgression owner)
        {
            orig(self, owner);
            decipheredNSHPearls = new List<DataPearl.AbstractDataPearl.DataPearlType>();
        }

        private static string MiscProgressionData_ToString(On.PlayerProgression.MiscProgressionData.orig_ToString orig, PlayerProgression.MiscProgressionData self)
        {
            string result = orig(self);
            if (decipheredNSHPearls.Count > 0 && !result.Contains("<mpdA>" + Header + "<mpdB>"))
            {
                result += "<mpdA>" + Header + "<mpdB>";
                for (int num4 = 0; num4 < decipheredNSHPearls.Count; num4++)
                {
                    string str7 = result;
                    DataPearl.AbstractDataPearl.DataPearlType dataPearlType3 = decipheredNSHPearls[num4];
                    result = str7 + ((dataPearlType3 != null) ? dataPearlType3.ToString() : null);
                    if (num4 < decipheredNSHPearls.Count - 1)
                    {
                        result += ",";
                    }
                }
                Plugin.Log("decipheredNSHPearls save!");
            }
            return result;
        }

        private static void MiscProgressionData_FromString(On.PlayerProgression.MiscProgressionData.orig_FromString orig, PlayerProgression.MiscProgressionData self, string s)
        {
            orig(self, s); 
            string[] array = Regex.Split(s, "<mpdA>");
            for (int i = 0; i < array.Length; i++)
            {
                string[] array2 = Regex.Split(array[i], "<mpdB>");
                string header = array2[0].ToUpper();
                if (header == Header)
                {
                    decipheredNSHPearls.Clear();
                    foreach (string value4 in array2[1].Split(new char[]
                    {
                         ','
                    }))
                    {
                        decipheredNSHPearls.Add(new DataPearl.AbstractDataPearl.DataPearlType(value4, false));
                    }
                    for (int j = self.unrecognizedSaveStrings.Count - 1; j >= 0; j--)
                    {
                        if (self.unrecognizedSaveStrings[j].Contains(array[i]))
                        {
                            self.unrecognizedSaveStrings.Remove(array[i]);
                        }
                    }
                    Plugin.Log(Header + " load from string : " + array2[1]);
                }
            }
        }

        #region 设置破译珍珠
        public static bool SetNSHPearlDeciphered(PlayerProgression.MiscProgressionData data, DataPearl.AbstractDataPearl.DataPearlType pearlType, bool forced = false)
        {
            if (pearlType != null && !forced)
            {
                int num = CollectionsMenu.DataPearlToFileID(pearlType);
                if (num != -1 && !Conversation.EventsFileExists(data.owner.rainWorld, num))//, MoreSlugcatsEnums.SlugcatStatsName.Artificer
                {
                    return SetPearlDeciphered(data, pearlType);
                }
            }
            if (pearlType == null || GetNSHPearlDeciphered(data, pearlType))
            {
                return false;
            }
            decipheredNSHPearls.Add(pearlType);
            data.owner.SaveProgression(false, true);
            return true;
        }

        public static bool GetNSHPearlDeciphered(PlayerProgression.MiscProgressionData data, DataPearl.AbstractDataPearl.DataPearlType pearlType)
        {
            return pearlType != null && decipheredNSHPearls.Contains(pearlType);
        }

        public static bool SetPearlDeciphered(PlayerProgression.MiscProgressionData data, DataPearl.AbstractDataPearl.DataPearlType pearlType)
        {
            if (pearlType == null || data.GetPearlDeciphered(pearlType))
            {
                return false;
            }
            data.decipheredPearls.Add(pearlType);
            data.owner.SaveProgression(false, true);
            return true;
        }
        #endregion
    }
}
