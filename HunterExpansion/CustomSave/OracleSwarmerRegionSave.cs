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
    public class OracleSwarmerRegionSave : DeathPersistentSaveDataTx
    {
        public static Dictionary<EntityID, string> oracleSwarmerRegion = new Dictionary<EntityID, string>();

        public override string header
        {
            get
            {
                return "ORACLESWARMERREGION";
            }
        }

        public OracleSwarmerRegionSave(SlugcatStats.Name name) : base(name)
        {
            this.slugName = name;
        }

        public override string SaveToString(bool saveAsIfPlayerDied, bool saveAsIfPlayerQuit)
        {
            string result;
            if (saveAsIfPlayerDied || saveAsIfPlayerQuit)
            {
                result = this.ToString(oracleSwarmerRegion);
            }
            else
            {
                result = this.ToString(oracleSwarmerRegion);
            }

            return result;
        }

        public override void LoadDatas(string data)
        {
            base.LoadDatas(data);
            oracleSwarmerRegion = FromString(data);
        }

        public override void ClearDataForNewSaveState(SlugcatStats.Name newSlugName)
        {
            base.ClearDataForNewSaveState(newSlugName);
            if (oracleSwarmerRegion.Count > 0)
            {
                oracleSwarmerRegion = new Dictionary<EntityID, string>();
            }
        }

        public string ToString(Dictionary<EntityID, string> d)
        {
            string result = "";
            foreach (KeyValuePair<EntityID, string> item in d)
            {
                result += "<mpdA>" + item.Key.ToString() + "<mpdB>" + item.Value;
            }
            return result;
        }

        public Dictionary<EntityID, string> FromString(string s)
        {
            Dictionary<EntityID, string> result = new Dictionary<EntityID, string>();
            string[] array = Regex.Split(s, "<mpdA>");
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] != "")
                {
                    string[] array2 = Regex.Split(array[i], "<mpdB>");
                    result.Add(EntityID.FromString(array2[0]), array2[1]);
                }
            }
            return result;
        }
    }
}
