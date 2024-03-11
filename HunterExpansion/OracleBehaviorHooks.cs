using HunterExpansion.CustomOracle;
using HunterExpansion.CustomSave;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using MoreSlugcats;
using UnityEngine;
using RWCustom;
using HunterExpansion.CustomEffects;

namespace HunterExpansion
{
    public class OracleBehaviorHooks
    {
        //Moon修珍珠
        public static bool startFix = false;
        public static bool hasTalk = false;
        public static int fixCount = 0;
        public static DataPearl pearl;
        public static Conversation.ID MoonBeforeFixNSHPearl = new Conversation.ID("MoonBeforeFixNSHPearl", true);
        public static Conversation.ID MoonAfterFixNSHPearl = new Conversation.ID("MoonAfterFixNSHPearl", true);


        public static void Init()
        {
            On.OracleBehavior.FindPlayer += OracleBehavior_FindPlayer;

            //Moon修珍珠
            On.SLOracleBehaviorHasMark.ctor += SLOracleBehaviorHasMark_ctor;
            On.SLOracleBehaviorHasMark.Update += SLOracleBehaviorHasMark_Update;
            On.SLOracleBehaviorHasMark.MoonConversation.ctor += MoonConversation_ctor;
        }

        public static void OracleBehavior_FindPlayer(On.OracleBehavior.orig_FindPlayer orig, OracleBehavior self)
        {
            Player player = null;
            if (self.oracle.ID == NSHOracleRegistry.NSHOracle && self.oracle.room.game.session.characterStats.name == Plugin.SlugName && NSHOracleMeetHunter.fadeCounter > 0)
            {
                player = self.player;
            }
            orig(self);
            if (self.oracle.ID == NSHOracleRegistry.NSHOracle && self.oracle.room.game.session.characterStats.name == Plugin.SlugName && NSHOracleMeetHunter.fadeCounter > 0)
            {
                self.player = player;
            }
        }



        #region Moon相关
        private static void SLOracleBehaviorHasMark_ctor(On.SLOracleBehaviorHasMark.orig_ctor orig, SLOracleBehaviorHasMark self, Oracle oracle)
        {
            orig(self, oracle);

            startFix = false;
            hasTalk = false;
            fixCount = 0;
            pearl = null;
        }

        public static void SLOracleBehaviorHasMark_Update(On.SLOracleBehaviorHasMark.orig_Update orig, SLOracleBehaviorHasMark self, bool eu)
        {
            orig(self, eu);
            //有点小问题：moon修了珍珠后退出再进，则不会触发修珍珠对话，需要退出整个游戏才能刷新
            /*
            Plugin.Log("PearlFixedSave.pearlFixed: " + PearlFixedSave.pearlFixed);
            Plugin.Log("startFix: " + startFix);
            Plugin.Log("fixCount: " + fixCount);*/
            if (startFix)
            {
                if ((self.currentConversation == null || self.currentConversation.events.Count == 0) && pearl.grabbedBy.Count == 0)
                {
                    fixCount++;
                }
                //找到珍珠
                if (self.player != null && self.player.room == self.oracle.room)
                {
                    List<PhysicalObject>[] physicalObjects = self.oracle.room.physicalObjects;
                    for (int i = 0; i < physicalObjects.Length; i++)
                    {
                        for (int j = 0; j < physicalObjects[i].Count; j++)
                        {
                            PhysicalObject physicalObject = physicalObjects[i][j];
                            if ((physicalObject is DataPearl) && (physicalObject as DataPearl).AbstractPearl.dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.Red_stomach)
                            {
                                pearl = physicalObject as DataPearl;
                            }
                        }
                    }
                }
                if (fixCount > 0 && pearl != null && pearl.grabbedBy.Count != 0)
                {
                    fixCount = 0;
                    if (self.currentConversation != null)
                    {
                        self.currentConversation.Interrupt("...", 0);
                    }
                    self.oracle.room.AddObject(new Explosion.ExplosionLight(pearl.firstChunk.pos, 100f, 1f, 5, Color.white));
                    self.oracle.room.AddObject(new ShockWave(pearl.firstChunk.pos, 30f, 0.1f, 5, false));
                    self.oracle.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, 0f, 0.1f, 1.5f + Random.value * 0.5f);
                    self.dialogBox.NewMessage(self.Translate("Oh... don't you want me to do this?"), 30);
                }
                if (fixCount == 1 && pearl != null && pearl.grabbedBy.Count == 0)
                {
                    if (!hasTalk)
                    {
                        self.currentConversation = new SLOracleBehaviorHasMark.MoonConversation(MoonBeforeFixNSHPearl, self, SLOracleBehaviorHasMark.MiscItemType.NA);
                        fixCount++;
                    }
                    else
                    {
                        self.dialogBox.NewMessage(self.Translate("Um... I will continue."), 30);
                    }
                }
                if (fixCount > 1 && fixCount < 200 && pearl != null)
                {
                    if (fixCount % 20 == 0)
                    {
                        self.oracle.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Data_Bit, pearl.firstChunk.pos, 1f, 1f + Random.value * 2f);
                        self.oracle.room.AddObject(new Explosion.ExplosionLight(pearl.firstChunk.pos, 150f, 1f, 15, Color.green));
                    }

                    Vector2 wantPos = self.oracle.firstChunk.pos + new Vector2(-30f, 5f);
                    pearl.firstChunk.vel *= Custom.LerpMap(pearl.firstChunk.vel.magnitude, 1f, 6f, 0.999f, 0.9f);
                    pearl.firstChunk.vel += Vector2.ClampMagnitude(wantPos - pearl.firstChunk.pos, 100f) / 100f * 0.4f;
                    //抵消重力
                    pearl.firstChunk.vel += 1f * Vector2.up;
                    //随机速度
                    pearl.firstChunk.vel += (Random.value - 0.5f) * 0.05f * Vector2.up;
                    //注视方向
                    self.lookPoint = pearl.firstChunk.pos + new Vector2(-30f, -10f);
                }
                if (fixCount == 200 && pearl != null)
                {
                    self.oracle.room.PlaySound(SoundID.Moon_Wake_Up_Green_Swarmer_Flash, pearl.firstChunk.pos, 0.5f, 1f);
                    self.oracle.room.AddObject(new ElectricFullScreen.SparkFlash(pearl.firstChunk.pos, 50f));
                }
                if (fixCount == 220)
                {
                    PearlFixedSave.pearlFixed = true;
                    startFix = false;
                    self.currentConversation = new SLOracleBehaviorHasMark.MoonConversation(MoonAfterFixNSHPearl, self, SLOracleBehaviorHasMark.MiscItemType.NA);
                    fixCount++;
                    self.oracle.room.AddObject(new FixedDataPearlEffect(pearl, self.oracle.room));
                }
            }
        }

        public static void MoonConversation_ctor(On.SLOracleBehaviorHasMark.MoonConversation.orig_ctor orig, SLOracleBehaviorHasMark.MoonConversation self, Conversation.ID id, OracleBehavior slOracleBehaviorHasMark, SLOracleBehaviorHasMark.MiscItemType describeItem)
        {
            orig(self, id, slOracleBehaviorHasMark, describeItem);

            SaveState saveState = Custom.rainWorld.progression.currentSaveState;
            if (slOracleBehaviorHasMark.oracle.room.game.session.characterStats.name == Plugin.SlugName &&
                slOracleBehaviorHasMark.oracle.room.abstractRoom.name == "SL_AI" &&
                saveState.miscWorldSaveData.SLOracleState.neuronsLeft >= 4 &&
                id == Conversation.ID.Moon_Pearl_Red_stomach && !PearlFixedSave.pearlFixed)
            {
                startFix = true;
            }
            if (id == MoonBeforeFixNSHPearl)
            {
                LoadEventsFromFile(self, 51, "Moon-BeforeFix");
                hasTalk = true;
            }
            if (id == MoonAfterFixNSHPearl)
            {
                LoadEventsFromFile(self, 51, "Moon-AfterFix");
            }
        }

        public static void LoadEventsFromFile(SLOracleBehaviorHasMark.MoonConversation self, int fileName, string suffix)
        {
            Debug.Log("~~~LOAD CONVO " + fileName.ToString());
            InGameTranslator.LanguageID languageID = self.interfaceOwner.rainWorld.inGameTranslator.currentLanguage;
            string text;
            for (; ; )
            {
                text = AssetManager.ResolveFilePath(self.interfaceOwner.rainWorld.inGameTranslator.SpecificTextFolderDirectory(languageID) + Path.DirectorySeparatorChar.ToString() + fileName.ToString() + ".txt");
                if (suffix != null)
                {
                    string text2 = text;
                    text = AssetManager.ResolveFilePath(string.Concat(new string[]
                    {
                    self.interfaceOwner.rainWorld.inGameTranslator.SpecificTextFolderDirectory(languageID),
                    Path.DirectorySeparatorChar.ToString(),
                    fileName.ToString(),
                    "-",
                    suffix,
                    ".txt"
                    }));
                    if (!File.Exists(text))
                    {
                        text = text2;
                    }
                }
                if (File.Exists(text))
                {
                    goto IL_117;
                }
                Debug.Log("NOT FOUND " + text);
                if (!(languageID != InGameTranslator.LanguageID.English))
                {
                    break;
                }
                Debug.Log("RETRY WITH ENGLISH");
                languageID = InGameTranslator.LanguageID.English;
            }
            return;
            IL_117:
            string text3 = File.ReadAllText(text, Encoding.UTF8);
            if (text3[0] != '0')
            {
                text3 = Custom.xorEncrypt(text3, 54 + fileName + (int)self.interfaceOwner.rainWorld.inGameTranslator.currentLanguage * 7);
            }
            string[] array = Regex.Split(text3, "\r\n");
            try
            {
                if (Regex.Split(array[0], "-")[1] == fileName.ToString())
                {
                    for (int j = 1; j < array.Length; j++)
                    {// j是行数
                        string[] array3 = LocalizationTranslator.ConsolidateLineInstructions(array[j]);
                        if (array3.Length == 3)
                        {
                            int num;
                            int num2;
                            if (ModManager.MSC && !int.TryParse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture, out num) && int.TryParse(array3[2], NumberStyles.Any, CultureInfo.InvariantCulture, out num2))
                            {
                                self.events.Add(new Conversation.TextEvent(self, int.Parse(array3[0], NumberStyles.Any, CultureInfo.InvariantCulture), array3[1], int.Parse(array3[2], NumberStyles.Any, CultureInfo.InvariantCulture)));
                            }
                            else
                            {
                                self.events.Add(new Conversation.TextEvent(self, int.Parse(array3[0], NumberStyles.Any, CultureInfo.InvariantCulture), array3[2], int.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
                            }
                        }
                        else if (array3.Length == 2)
                        {
                            if (array3[0] == "SPECEVENT")
                            {
                                self.events.Add(new Conversation.SpecialEvent(self, 0, array3[1]));
                            }
                            else if (array3[0] == "PEBBLESWAIT")
                            {
                                self.events.Add(new SSOracleBehavior.PebblesConversation.PauseAndWaitForStillEvent(self, null, int.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
                            }
                        }
                        else if (array3.Length == 1 && array3[0].Length > 0)
                        {
                            self.events.Add(new Conversation.TextEvent(self, 0, array3[0], 0));
                        }
                    }
                }
            }
            catch
            {
                Debug.Log("TEXT ERROR");
                self.events.Add(new Conversation.TextEvent(self, 0, "TEXT ERROR", 100));
            }
        }
        #endregion
    }
}
