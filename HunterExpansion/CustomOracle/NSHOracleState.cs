using MoreSlugcats;
using RWCustom;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace HunterExpansion.CustomOracle
{
    public class NSHOracleState
    {
        private int[] integers;
        //public bool[] miscBools;
        public bool[] unrecognizedMiscBools;
        public List<DataPearl.AbstractDataPearl.DataPearlType> significantPearls;
        public List<SLOracleBehaviorHasMark.MiscItemType> miscItemsDescribed;
        public List<EntityID> alreadyTalkedAboutItems;
        public List<string> unrecognizedSaveStrings;
        public int[] unrecognizedIntegers;
        public bool shownEnergyCell;
        public float likesPlayer;
        public bool isDebugState;
        public bool increaseLikeOnSave = true;
        public bool talkedAboutPebblesDeath;

        #region 属性
        public int playerEncounters
        {
            get
            {
                return this.integers[0];
            }
            set
            {
                this.integers[0] = value;
            }
        }

        public int playerEncountersWithMark
        {
            get
            {
                return this.integers[1];
            }
            set
            {
                this.integers[1] = value;
            }
        }
        /*
        public int neuronsLeft
        {
            get
            {
                return this.integers[2];
            }
            set
            {
                this.integers[2] = value;
            }
        }
        
        public int neuronGiveConversationCounter
        {
            get
            {
                return this.integers[3];
            }
            set
            {
                this.integers[3] = value;
            }
        }

        public int totNeuronsGiven
        {
            get
            {
                return this.integers[4];
            }
            set
            {
                this.integers[4] = value;
            }
        }
        */
        public int leaves
        {
            get
            {
                return this.integers[2];
            }
            set
            {
                this.integers[2] = value;
            }
        }

        public int annoyances
        {
            get
            {
                return this.integers[3];
            }
            set
            {
                this.integers[3] = value;
            }
        }

        public int totalInterruptions
        {
            get
            {
                return this.integers[4];
            }
            set
            {
                this.integers[4] = value;
            }
        }

        public int totalItemsBrought
        {
            get
            {
                return this.integers[5];
            }
            set
            {
                this.integers[5] = value;
            }
        }

        public int totalPearlsBrought
        {
            get
            {
                return this.integers[6];
            }
            set
            {
                this.integers[6] = value;
            }
        }
        public int miscPearlCounter
        {
            get
            {
                return this.integers[7];
            }
            set
            {
                this.integers[7] = value;
            }
        }

        public int playerEncountersState
        {
            get
            {
                return this.integers[8];
            }
            set
            {
                this.integers[8] = value;
            }
        }
        /*
        public int chatLogA
        {
            get
            {
                return this.integers[8];
            }
            set
            {
                this.integers[8] = value;
            }
        }
        public int chatLogB
        {
            get
            {
                return this.integers[9];
            }
            set
            {
                this.integers[9] = value;
            }
        }*/
        /*
        public bool hasToldPlayerNotToEatNeurons
        {
            get
            {
                return this.miscBools[0];
            }
            set
            {
                this.miscBools[0] = value;
            }
        }
        */
        #endregion

        public NSHOracleState.PlayerOpinion GetOpinion
        {
            get
            {
                return new NSHOracleState.PlayerOpinion(ExtEnum<NSHOracleState.PlayerOpinion>.values.GetEntry((int)Mathf.Clamp(Custom.LerpMap(this.likesPlayer, -1f, 1f, 0f, (float)ExtEnum<NSHOracleState.PlayerOpinion>.values.Count), 0f, (float)(ExtEnum<NSHOracleState.PlayerOpinion>.values.Count - 1))), false);
            }
        }

        public bool SpeakingTerms
        {
            get
            {
                return this.GetOpinion != NSHOracleState.PlayerOpinion.NotSpeaking;
            }
        }

        public void InfluenceLike(float influence)
        {
            this.likesPlayer = Mathf.Clamp(this.likesPlayer + influence, -1f, 1f);
        }

        public NSHOracleState(bool isDebugState, SlugcatStats.Name saveStateNumber)
        {
            this.isDebugState = isDebugState;
            this.ForceResetState(saveStateNumber);
        }

        public void AddItemToAlreadyTalkedAbout(EntityID ID)
        {
            for (int i = 0; i < this.alreadyTalkedAboutItems.Count; i++)
            {
                if (this.alreadyTalkedAboutItems[i] == ID)
                {
                    return;
                }
            }
            this.alreadyTalkedAboutItems.Add(ID);
        }

        public bool HaveIAlreadyDescribedThisItem(EntityID ID)
        {
            for (int i = 0; i < this.alreadyTalkedAboutItems.Count; i++)
            {
                if (this.alreadyTalkedAboutItems[i] == ID)
                {
                    return true;
                }
            }
            return false;
        }

        public void ForceResetState(SlugcatStats.Name saveStateNumber)
        {
            this.increaseLikeOnSave = true;
            this.integers = new int[14];
            this.unrecognizedIntegers = null;
            //this.miscBools = new bool[1];
            this.unrecognizedMiscBools = null;
            this.significantPearls = new List<DataPearl.AbstractDataPearl.DataPearlType>();
            this.miscItemsDescribed = new List<SLOracleBehaviorHasMark.MiscItemType>();
            this.unrecognizedSaveStrings = new List<string>();
            this.likesPlayer = 0.1f;
            if (saveStateNumber == SlugcatStats.Name.Red || (ModManager.MSC && saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Spear))
            {
                this.likesPlayer = 0.6f;
            }
            /*
            if (saveStateNumber == SlugcatStats.Name.Red || (ModManager.MSC && saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel))
            {
                this.neuronsLeft = 0;
            }
            else if ((ModManager.MSC && saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet) || (ModManager.MSC && saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint))
            {
                this.neuronsLeft = 7;
            }
            else
            {
                this.neuronsLeft = 5;
            }*/
            this.playerEncountersWithMark = 0;
            this.alreadyTalkedAboutItems = new List<EntityID>();
            this.shownEnergyCell = false;
            Plugin.Log("Reset NSH oracle state for {0}", saveStateNumber.value);
            /*
            this.chatLogA = -1;
            this.chatLogB = -1;
            */
        }

        public override string ToString()
        {
            string text = "";
            text += "integersArray<slosB>";
            text += SaveUtils.SaveIntegerArray('.', this.integers, this.unrecognizedIntegers);
            text += "<slosA>";
            /*
            text += "miscBools<slosB>";
            text += SaveUtils.SaveBooleanArray(this.miscBools, this.unrecognizedMiscBools);*/
            text += "<slosA>";
            text += "significantPearls<slosB>";
            for (int i = 0; i < this.significantPearls.Count; i++)
            {
                string str = text;
                DataPearl.AbstractDataPearl.DataPearlType dataPearlType = this.significantPearls[i];
                text = str + ((dataPearlType != null) ? dataPearlType.ToString() : null);
                if (i < this.significantPearls.Count - 1)
                {
                    text += ",";
                }
            }
            text += "<slosA>";
            text += "miscItemsDescribed<slosB>";
            for (int j = 0; j < this.miscItemsDescribed.Count; j++)
            {
                string str2 = text;
                SLOracleBehaviorHasMark.MiscItemType miscItemType = this.miscItemsDescribed[j];
                text = str2 + ((miscItemType != null) ? miscItemType.ToString() : null);
                if (j < this.miscItemsDescribed.Count - 1)
                {
                    text += ",";
                }
            }
            text += "<slosA>";
            if (this.increaseLikeOnSave)
            {
                this.InfluenceLike(0.15f);
            }
            text += string.Format(CultureInfo.InvariantCulture, "likesPlayer<slosB>{0}<slosA>", this.likesPlayer);
            if (this.alreadyTalkedAboutItems.Count > 0)
            {
                text += "itemsAlreadyTalkedAbout<slosB>";
                for (int k = 0; k < this.alreadyTalkedAboutItems.Count; k++)
                {
                    text = text + this.alreadyTalkedAboutItems[k].ToString() + ((k < this.alreadyTalkedAboutItems.Count - 1) ? "<slosC>" : "");
                }
                text += "<slosA>";
            }
            if (ModManager.MSC && this.talkedAboutPebblesDeath)
            {
                text += "talkedPebblesDeath<slosA>";
            }
            if (ModManager.MSC && this.shownEnergyCell)
            {
                text += "shownEnergyCell<slosA>";
            }
            foreach (string str3 in this.unrecognizedSaveStrings)
            {
                text = text + str3 + "<slosA>";
            }
            return text;
        }

        public void FromString(string s)
        {
            this.unrecognizedSaveStrings.Clear();
            string[] array = Regex.Split(s, "<slosA>");
            int i = 0;
            while (i < array.Length)
            {
                string[] array2 = Regex.Split(array[i], "<slosB>");
                string text = array2[0];
                if (text == null)
                {
                    goto IL_344;
                }
                uint num = ComputeStringHash(text);
                if (num <= 1220639398U)
                {
                    if (num <= 824950339U)
                    {
                        if (num != 534114507U)
                        {
                            if (num != 824950339U)
                            {
                                goto IL_344;
                            }
                            if (!(text == "integersArray"))
                            {
                                goto IL_344;
                            }
                            this.unrecognizedIntegers = SaveUtils.LoadIntegersArray(array2[1], '.', this.integers);
                        }
                        else
                        {
                            if (!(text == "itemsAlreadyTalkedAbout"))
                            {
                                goto IL_344;
                            }
                            string[] array3 = Regex.Split(array2[1], "<slosC>");
                            for (int j = 0; j < array3.Length; j++)
                            {
                                if (array3[j].Length > 0)
                                {
                                    this.alreadyTalkedAboutItems.Add(EntityID.FromString(array3[j]));
                                }
                            }
                        }
                    }
                    else if (num != 957569486U)
                    {
                        if (num != 1220639398U)
                        {
                            goto IL_344;
                        }
                        if (!(text == "shownEnergyCell"))
                        {
                            goto IL_344;
                        }
                        if (ModManager.MSC)
                        {
                            this.shownEnergyCell = true;
                        }
                        else
                        {
                            this.unrecognizedSaveStrings.Add(array[i]);
                        }
                    }
                    else
                    {
                        if (!(text == "miscItemsDescribed"))
                        {
                            goto IL_344;
                        }
                        if (Custom.IsDigitString(array2[1]))
                        {
                            BackwardsCompatibilityRemix.ParseMiscItems(array2[1], this.miscItemsDescribed);
                        }
                        else
                        {
                            this.miscItemsDescribed.Clear();
                            foreach (string text2 in array2[1].Split(new char[]
                            {
                            ','
                            }))
                            {
                                if (!(text2 == string.Empty))
                                {
                                    this.miscItemsDescribed.Add(new SLOracleBehaviorHasMark.MiscItemType(text2, false));
                                }
                            }
                        }
                    }
                }
                else if (num <= 1813340087U)
                {
                    if (num != 1393539262U)
                    {
                        if (num != 1813340087U)
                        {
                            goto IL_344;
                        }
                        if (!(text == "talkedPebblesDeath"))
                        {
                            goto IL_344;
                        }
                        if (ModManager.MSC)
                        {
                            this.talkedAboutPebblesDeath = true;
                        }
                        else
                        {
                            this.unrecognizedSaveStrings.Add(array[i]);
                        }
                    }
                    else
                    {
                        if (!(text == "likesPlayer"))
                        {
                            goto IL_344;
                        }
                        this.likesPlayer = float.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                    }
                }
                else if (num != 1861699070U)
                {
                    if (num != 4159351693U)
                    {
                        goto IL_344;
                    }
                    if (!(text == "significantPearls"))
                    {
                        goto IL_344;
                    }
                    if (Custom.IsDigitString(array2[1]))
                    {
                        BackwardsCompatibilityRemix.ParseSignificantPearls(array2[1], this.significantPearls);
                    }
                    else
                    {
                        this.significantPearls.Clear();
                        foreach (string text3 in array2[1].Split(new char[]
                        {
                        ','
                        }))
                        {
                            if (!(text3 == string.Empty))
                            {
                                this.significantPearls.Add(new DataPearl.AbstractDataPearl.DataPearlType(text3, false));
                            }
                        }
                    }
                }
            /*
            else
            {
                if (!(text == "miscBools"))
                {
                    goto IL_344;
                }
                this.unrecognizedMiscBools = SaveUtils.LoadBooleanArray(array2[1], this.miscBools);
            }*/
            IL_368:
                i++;
                continue;
            IL_344:
                if (array[i].Trim().Length > 0 && array2.Length >= 1)
                {
                    this.unrecognizedSaveStrings.Add(array[i]);
                    goto IL_368;
                }
                goto IL_368;
            }
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

        public class PlayerOpinion : ExtEnum<NSHOracleState.PlayerOpinion>
        {
            public PlayerOpinion(string value, bool register = false) : base(value, register)
            {
            }

            public static readonly NSHOracleState.PlayerOpinion NotSpeaking = new NSHOracleState.PlayerOpinion("NotSpeaking", true);
            public static readonly NSHOracleState.PlayerOpinion Dislikes = new NSHOracleState.PlayerOpinion("Dislikes", true);
            public static readonly NSHOracleState.PlayerOpinion Neutral = new NSHOracleState.PlayerOpinion("Neutral", true);
            public static readonly NSHOracleState.PlayerOpinion Likes = new NSHOracleState.PlayerOpinion("Likes", true);
        }
    }
}