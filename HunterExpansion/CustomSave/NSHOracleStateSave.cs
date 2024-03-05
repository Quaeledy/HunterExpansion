using HunterExpansion.CustomOracle;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace HunterExpansion.CustomSave
{
    public class NSHOracleStateSave
    {
        public static string Header = "NSHOracleState";

        public static NSHOracleState NSHOracleState
        {
            get
            {
                if (privNSHOracleState == null)
                {
                    privNSHOracleState = new NSHOracleState(false, saveStateNumber);
                }
                return privNSHOracleState;
            }
        }

        public static SlugcatStats.Name saveStateNumber;
        public static NSHOracleState privNSHOracleState;

        public static void Init()
        {
            On.MiscWorldSaveData.ctor += MiscWorldSaveData_ctor;
            On.MiscWorldSaveData.FromString += MiscWorldSaveData_FromString;
            On.MiscWorldSaveData.ToString += MiscWorldSaveData_ToString;
            //On.SaveState.ctor += SaveState_ctor;
        }
        /*
        //重置存档时重置数据
        public static void SaveState_ctor(On.SaveState.orig_ctor orig, SaveState self, SlugcatStats.Name saveStateNumber, PlayerProgression progression)
        {
            orig(self, saveStateNumber, progression);
            privNSHOracleState = new NSHOracleState(false, saveStateNumber);
        }
        */
        private static void MiscWorldSaveData_ctor(On.MiscWorldSaveData.orig_ctor orig, MiscWorldSaveData self, SlugcatStats.Name saveStateNumber)
        {
            orig(self, saveStateNumber);
            privNSHOracleState = new NSHOracleState(false, saveStateNumber);
        }

        private static string MiscWorldSaveData_ToString(On.MiscWorldSaveData.orig_ToString orig, MiscWorldSaveData self)
        {
            string result = orig(self);
            if (privNSHOracleState != null && privNSHOracleState.playerEncounters > 0)
            {
                result += Header + "<mwB>" + privNSHOracleState.ToString() + "<mwA>";
            }
            return result;
        }

        private static void MiscWorldSaveData_FromString(On.MiscWorldSaveData.orig_FromString orig, MiscWorldSaveData self, string s)
        {
            orig(self, s);
            self.unrecognizedSaveStrings.Clear();
            string[] array = Regex.Split(s, "<mwA>");
            int i = 0;
            while (i < array.Length)
            {
                bool flag = false;
                string[] array2 = Regex.Split(array[i], "<mwB>");
                string header = array2[0];
                if (header == null)
                {
                    goto IL_557;
                }
                if (header == Header)
                {
                    privNSHOracleState = new NSHOracleState(false, self.saveStateNumber);
                    privNSHOracleState.FromString(array2[1]);

                    for (int j = self.unrecognizedSaveStrings.Count - 1; j >= 0; j--)
                    {
                        if (self.unrecognizedSaveStrings[j].Contains(array[i]))
                        {
                            self.unrecognizedSaveStrings.Remove(array[i]);
                        }
                    }

                }
                IL_559:
                if (flag && array[i].Trim().Length > 0 && array2.Length >= 1)
                {
                    self.unrecognizedSaveStrings.Add(array[i]);
                }
                i++;
                continue;
                IL_557:
                flag = true;
                goto IL_559;
            }
            /*
            string[] array = Regex.Split(s, "<mpdA>");
            for (int i = 0; i < array.Length; i++)
            {
                string[] array2 = Regex.Split(array[i], "<mpdB>");
                string header = array2[0].ToUpper();

                if (header == Header[3])
                {
                    privNSHOracleState = new NSHOracleState(false, saveStateNumber);
                    privNSHOracleState.FromString(array2[1]);
                    for (int j = self.unrecognizedSaveStrings.Count - 1; j >= 0; j--)
                    {
                        if (self.unrecognizedSaveStrings[j].Contains(array[i]))
                        {
                            self.unrecognizedSaveStrings.Remove(array[i]);
                        }
                    }
                }
            }*/
        }

        internal static uint ComputeStringHash(string s)
        {
            uint num = 0;
            if (s != null)
            {
                num = 2166136261U;
                for (int i = 0; i < s.Length; i++)
                {
                    num = ((uint)s[i] ^ num) * 16777619U;
                }
            }
            return num;
        }
    }
}
