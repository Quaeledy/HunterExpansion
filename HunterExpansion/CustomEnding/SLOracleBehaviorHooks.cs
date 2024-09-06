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
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using Random = UnityEngine.Random;

namespace HunterExpansion.CustomEnding
{
    public class SLOracleBehaviorHooks
    {
        //Moon修珍珠
        public static bool startFix = false;
        public static bool hasTalk = false;
        public static int fixCount = 0;
        public static DataPearl pearl;
        public static Conversation.ID MoonBeforeFixNSHPearl = new Conversation.ID("MoonBeforeFixNSHPearl", true);
        public static Conversation.ID MoonAfterFixNSHPearl = new Conversation.ID("MoonAfterFixNSHPearl", true);

        //Moon读NSH神经元
        public static bool isNSHOracleSwarmer = false;


        public static void InitIL()
        {
            IL.SLOracleBehavior.Update += SLOracleBehavior_UpdateIL;
        }

        public static void Init()
        {
            //On.OracleBehavior.FindPlayer += OracleBehavior_FindPlayer;

            //Moon修珍珠和解读NSH神经元、绿珍珠差分
            On.SLOracleBehaviorHasMark.ctor += SLOracleBehaviorHasMark_ctor;
            On.SLOracleBehaviorHasMark.Update += SLOracleBehaviorHasMark_Update;
            On.SLOracleBehaviorHasMark.MoonConversation.ctor += MoonConversation_ctor;
        }

        #region IL Hooks
        public static void SLOracleBehavior_UpdateIL(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);
                if (c.TryGotoNext(MoveType.After,
                    (i) => i.MatchLdfld<BodyChunk>("pos"),
                    (i) => i.MatchStloc(4)))
                {
                    Plugin.Log("SLOracleBehavior_UpdateIL MatchFind!");
                    c.Emit(OpCodes.Ldarg_0);
                    c.EmitDelegate<Action<SLOracleBehavior>>((self) =>
                    {
                        if (self.holdingObject is OracleSwarmer &&
                            OracleSwarmerHooks.OracleSwarmerData.TryGetValue(self.holdingObject as SSOracleSwarmer, out var nshOracleSwarmer) &&
                            nshOracleSwarmer.spawnRegion == "NSH")
                        {
                            Plugin.Log("This swarmer is from NSH.");
                            isNSHOracleSwarmer = true;
                        }
                    });
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }
        #endregion

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
                            if (physicalObject is DataPearl && (physicalObject as DataPearl).AbstractPearl.dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.Red_stomach)
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
                        self.currentConversation.Interrupt(self.Translate("..."), 0);
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
            self.interfaceOwner = slOracleBehaviorHasMark;
            self.id = id;
            self.dialogBox = slOracleBehaviorHasMark.dialogBox;
            self.events = new List<Conversation.DialogueEvent>();
            self.myBehavior = slOracleBehaviorHasMark;
            self.currentSaveFile = slOracleBehaviorHasMark.oracle.room.game.GetStorySession.saveStateNumber;
            self.describeItem = describeItem;
            if (self.id == Conversation.ID.MoonRecieveSwarmer && isNSHOracleSwarmer)
            {
                isNSHOracleSwarmer = false;
                if (self.myBehavior is SLOracleBehaviorHasMark)
                {
                    if (self.State.neuronsLeft - 1 > 2 && (self.myBehavior as SLOracleBehaviorHasMark).respondToNeuronFromNoSpeakMode)
                    {
                        self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("You... Strange thing. Now self?"), 10));
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("I will accept your gift..."), 10));
                    }
                    switch (self.State.neuronsLeft - 1)
                    {
                        case -1:
                        case 0:
                            break;
                        case 1:
                            self.events.Add(new Conversation.TextEvent(self, 40, "...", 10));
                            self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("You!"), 10));
                            self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("...you...killed..."), 10));
                            self.events.Add(new Conversation.TextEvent(self, 0, "...", 10));
                            self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("...me"), 10));
                            break;
                        case 2:
                            self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("...thank you... better..."), 10));
                            self.events.Add(new Conversation.TextEvent(self, 20, self.Translate("still, very... bad."), 10));
                            break;
                        case 3:
                            self.events.Add(new Conversation.TextEvent(self, 20, self.Translate("Thank you... That is a little better. Thank you, creature."), 10));
                            if (!(self.myBehavior as SLOracleBehaviorHasMark).respondToNeuronFromNoSpeakMode)
                            {
                                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Maybe self is asking too much... But, would you bring me another one?"), 0));
                            }
                            break;
                        default:
                            if ((self.myBehavior as SLOracleBehaviorHasMark).respondToNeuronFromNoSpeakMode)
                            {
                                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Thank you. I do wonder what you want."), 10));
                            }
                            else
                            {
                                if (SLOracleReceiveNSHNeuronCounterSave.sLOracleReceiveNSHNeuronCounter == 0)
                                {
                                    Custom.Log(new string[]
                                    {
                                        "moon recieve first neuron. Has neurons:",
                                        self.State.neuronsLeft.ToString()
                                    });
                                    if (self.State.neuronsLeft == 5)
                                    {
                                        NSHConversation.LoadEventsFromFile(self, 45);
                                    }
                                    else
                                    {
                                        NSHConversation.LoadEventsFromFile(self, 19);
                                    }
                                }
                                else if (SLOracleReceiveNSHNeuronCounterSave.sLOracleReceiveNSHNeuronCounter == 1)
                                {
                                    NSHConversation.LoadEventsFromFile(self, 159);
                                }
                                else
                                {
                                    switch (UnityEngine.Random.Range(0, 4))
                                    {
                                        case 0:
                                            self.events.Add(new Conversation.TextEvent(self, 30, self.Translate("Thank you, again. I feel wonderful."), 10));
                                            break;
                                        case 1:
                                            self.events.Add(new Conversation.TextEvent(self, 30, self.Translate("Thank you so very much!"), 10));
                                            break;
                                        case 2:
                                            self.events.Add(new Conversation.TextEvent(self, 30, self.Translate("It is strange... I'm remembering myself, but also... him."), 10));
                                            break;
                                        default:
                                            self.events.Add(new Conversation.TextEvent(self, 30, self.Translate("Thank you... Sincerely."), 10));
                                            break;
                                    }
                                }/*
                                SLOrcacleState state = self.State;
                                int neuronGiveConversationCounter = state.neuronGiveConversationCounter;
                                state.neuronGiveConversationCounter = neuronGiveConversationCounter + 1;*/
                                SLOracleReceiveNSHNeuronCounterSave.sLOracleReceiveNSHNeuronCounter++;
                            }
                            break;
                    }
                    (self.myBehavior as SLOracleBehaviorHasMark).respondToNeuronFromNoSpeakMode = false;
                    return;
                }
            }
            else if (self.id == Conversation.ID.Moon_Pearl_Red_stomach)//红猫珍珠的差分
            {
                if (self.currentSaveFile != SlugcatStats.Name.Red)
                {
                    self.PearlIntro();
                    if (PearlFixedSave.pearlFixed)
                        NSHConversation.LoadEventsFromFile(self, 51, "Moon");
                    else
                        NSHConversation.LoadEventsFromFile(self, 51, "Moon");
                    return;
                }
            }

            orig(self, id, slOracleBehaviorHasMark, describeItem);

            SaveState saveState = Custom.rainWorld.progression.currentSaveState;
            if (slOracleBehaviorHasMark.oracle.room.game.session.characterStats.name == Plugin.SlugName &&
                slOracleBehaviorHasMark.oracle.room.abstractRoom.name == "SL_AI" &&
                saveState.miscWorldSaveData.SLOracleState.neuronsLeft >= 4 &&
                saveState.miscWorldSaveData.SLOracleState.GetOpinion == SLOrcacleState.PlayerOpinion.Likes &&
                id == Conversation.ID.Moon_Pearl_Red_stomach && !PearlFixedSave.pearlFixed)
            {
                startFix = true;
            }
            if (id == MoonBeforeFixNSHPearl)
            {
                NSHConversation.LoadEventsFromFile(self, 51, "Moon-BeforeFix");
                hasTalk = true;
            }
            if (id == MoonAfterFixNSHPearl)
            {
                NSHConversation.LoadEventsFromFile(self, 51, "Moon-AfterFix");
            }
        }
        #endregion
    }
}
