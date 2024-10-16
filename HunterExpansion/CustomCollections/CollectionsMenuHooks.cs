﻿using HunterExpansion.CustomSave;
using Menu;
using MoreSlugcats;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using UnityEngine;
using RWCustom;
using MonoMod.Cil;
using System;
using Mono.Cecil.Cil;
using System.Runtime.Remoting.Messaging;
using HunterExpansion.CustomOracle;
using Debug = UnityEngine.Debug;

namespace HunterExpansion.CustomCollections
{
    public class CollectionsMenuHooks
    {
        private static readonly CollectionsMenu.PearlReadContext PearlReadContext_NSH = new CollectionsMenu.PearlReadContext("NSH", true);
        private static readonly SlugcatStats.Name NSH = new SlugcatStats.Name("NSH", false);
        static bool flag_NSH_1 = false;
        static bool flag_NSH_2 = false;

        public static void InitIL()
        {
            IL.MoreSlugcats.CollectionsMenu.ctor += CollectionsMenu_ctorIL;
            IL.MoreSlugcats.CollectionsMenu.Singal += CollectionsMenu_SingalIL;
            IL.MoreSlugcats.CollectionsMenu.AddIteratorButtons += CollectionsMenu_AddIteratorButtonsIL;
        }

        public static void Init()
        {
            On.MoreSlugcats.CollectionsMenu.Singal += CollectionsMenu_Singal;
            On.MoreSlugcats.CollectionsMenu.InitLabelsFromPearlFile += CollectionsMenu_InitLabelsFromPearlFile;
            On.MoreSlugcats.CollectionsMenu.UpdateInfoText += CollectionsMenu_UpdateInfoText;
            On.RainWorldGame.ctor += RainWorldGame_ctor;
        }
        #region IL Hooks
        public static void CollectionsMenu_ctorIL(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);
                //在收藏中加入NSH
                if (c.TryGotoNext(MoveType.After,
                    (i) => i.Match(OpCodes.Ldarg_0),
                    (i) => i.MatchLdfld<CollectionsMenu>("usedPearlTypes"),
                    (i) => i.MatchLdloc(8),
                    (i) => i.Match(OpCodes.Ldelem_Ref),
                    (i) => i.MatchCallvirt<PlayerProgression.MiscProgressionData>("GetPebblesPearlDeciphered")))
                {
                    Plugin.Log("CollectionsMenu_ctorIL MatchFind");
                    c.Emit(OpCodes.Ldarg_1);//manager
                    c.Emit(OpCodes.Ldarg_0);//self
                    c.Emit(OpCodes.Ldloc_S, (byte)8);//j
                    c.EmitDelegate<Func<bool, ProcessManager, CollectionsMenu, int, bool>>((getPebblesPearlDeciphered, manager, self, j) =>
                    {
                            return getPebblesPearlDeciphered || DecipheredNSHPearlsSave.GetNSHPearlDeciphered(manager.rainWorld.progression.miscProgressionData, self.usedPearlTypes[j]);
                    });
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }

        public static void CollectionsMenu_SingalIL(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);
                //在收藏中加入NSH
                if (c.TryGotoNext(MoveType.After,
                    (i) => i.Match(OpCodes.Ldarg_0),
                    (i) => i.MatchLdfld<CollectionsMenu>("iteratorButtons"),
                    (i) => i.MatchLdloc(11),
                    (i) => i.Match(OpCodes.Ldelem_Ref),
                    (i) => i.Match(OpCodes.Ldc_I4_0),
                    (i) => i.Match(OpCodes.Stfld),
                    (i) => i.Match(OpCodes.Ldarg_2)))
                {
                    Plugin.Log("CollectionsMenu_SingalIL MatchFind");
                    c.Emit(OpCodes.Ldloc_S, (byte)7);
                    c.EmitDelegate<Func<string, CollectionsMenu.PearlReadContext, CollectionsMenu.PearlReadContext>>((message, a) =>
                    {
                        if (message.Contains("NSH"))
                            return PearlReadContext_NSH;
                        else
                            return a;
                    });
                    c.Emit(OpCodes.Stloc_S, (byte)7);
                    c.Emit(OpCodes.Ldarg_2);//找到message
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
            try
            {
                ILCursor c = new ILCursor(il);

                //在收藏中加入NSH
                if (c.TryGotoNext(MoveType.After,
                    (i) => i.Match(OpCodes.Call),
                    (i) => i.Match(OpCodes.Brfalse_S),
                    (i) => i.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>("Saint"),
                    (i) => i.MatchStloc(8),
                    (i) => i.Match(OpCodes.Ldarg_0)))
                {
                    c.Emit(OpCodes.Ldloc_S, (byte)7);//找到a的本地变量
                    c.Emit(OpCodes.Ldloc_S, (byte)8);//找到saveFile的本地变量
                    c.EmitDelegate<Func<CollectionsMenu, CollectionsMenu.PearlReadContext, SlugcatStats.Name, SlugcatStats.Name>>((self, a, saveFile) =>
                    {
                        if (a == PearlReadContext_NSH)
                        {
                            return NSH;
                        }
                        else
                        {
                            return saveFile;
                        }
                    });
                    c.Emit(OpCodes.Stloc_S, (byte)8);//找到saveFile的本地变量
                    c.Emit(OpCodes.Ldarg_0);//还一个self
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }

        public static void CollectionsMenu_AddIteratorButtonsIL(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);
                //在收藏中加入NSH
                if (c.TryGotoNext(MoveType.After,
                    (i) => i.Match(OpCodes.Ldarg_0),
                    (i) => i.MatchCall<CollectionsMenu>("ClearIteratorButtons"),
                    (i) => i.Match(OpCodes.Ldc_I4_0),
                    (i) => i.MatchStloc(10)))
                {
                    Plugin.Log("CollectionsMenu_AddIteratorButtonsIL MatchFind");
                    c.Emit(OpCodes.Ldarg_0);
                    c.Emit(OpCodes.Ldarg_1);
                    c.Emit(OpCodes.Ldloc_1);
                    c.Emit(OpCodes.Ldloc_S, (byte)6);
                    c.EmitDelegate<Func<CollectionsMenu, int, bool, bool, bool>>((self, pearlIndex, flag2, flag7) =>
                    {
                        flag_NSH_1 = Conversation.EventsFileExists(self.rainWorld, pearlIndex, NSH);
                        flag_NSH_2 = flag_NSH_1 && (self.debug_enableAllButtons || DecipheredNSHPearlsSave.GetNSHPearlDeciphered(self.rainWorld.progression.miscProgressionData, self.usedPearlTypes[self.selectedPearlInd]));// || (flag && self.manager.rainWorld.progression.miscProgressionData.GetPearlDeciphered(self.usedPearlTypes[self.selectedPearlInd]))); ;
                        if (flag2 && !flag_NSH_1 && DecipheredNSHPearlsSave.GetNSHPearlDeciphered(self.rainWorld.progression.miscProgressionData, self.usedPearlTypes[self.selectedPearlInd]))
                        {
                            return true;
                        }
                        else
                        {
                            return flag7;
                        }
                    });
                    c.Emit(OpCodes.Stloc_S, (byte)6);//flag7
                    c.Emit(OpCodes.Ldloc_S, (byte)10);//num
                    c.EmitDelegate<Func<int, int>>((num) =>
                    {
                        if (flag_NSH_1)
                        {
                            num++;
                        }
                        return num;
                    });
                    c.Emit(OpCodes.Stloc_S, (byte)10);//num
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
            try
            {
                ILCursor c = new ILCursor(il);/*
                ILCursor find = new ILCursor(il);
                ILLabel pos = null;
                //text = new ILCursor(il);
                if (find.TryGotoNext(MoveType.After,
                    (i) => i.MatchLdsfld<CollectionsMenu.PearlReadContext>("FutureMoon"),
                    (i) => i.MatchStloc(15),
                    (i) => i.MatchLdloc(11),
                    (i) => i.Match(OpCodes.Ldc_I4_1),
                    (i) => i.Match(OpCodes.Add),
                    (i) => i.MatchStloc(11),
                    (i) => i.Match(OpCodes.Ldarg_2)))
                {
                    pos = find.MarkLabel();
                }
                if (pos == null)
                    Plugin.Log("CollectionsMenu_AddIteratorButtonsIL Can't find skip label!");
                else
                    Plugin.Log("CollectionsMenu_AddIteratorButtonsIL Find the pos to jump! ");
                */
                //在收藏中加入NSH
                if (c.TryGotoNext(MoveType.After,
                    (i) => i.MatchLdsfld<CollectionsMenu.PearlReadContext>("FutureMoon"),
                    (i) => i.MatchStloc(15),
                    (i) => i.MatchLdloc(11),
                    (i) => i.Match(OpCodes.Ldc_I4_1),
                    (i) => i.Match(OpCodes.Add),
                    (i) => i.MatchStloc(11),
                    (i) => i.Match(OpCodes.Ldarg_2)))
                {
                    //需要消耗掉arg.2
                    c.Emit(OpCodes.Ldarg_0);//找到self
                    c.Emit(OpCodes.Ldloc_S, (byte)11);//num2
                    c.Emit(OpCodes.Ldloc_S, (byte)12);//num3
                    c.Emit(OpCodes.Ldloc_S, (byte)13);//num4
                    c.Emit(OpCodes.Ldloc_S, (byte)14);//vector
                    c.Emit(OpCodes.Ldloc_S, (byte)15);//pearlReadContext
                    c.EmitDelegate<Func<int, CollectionsMenu, int, float, float, Vector2, CollectionsMenu.PearlReadContext, CollectionsMenu.PearlReadContext>>((pebAltPearlIndex, self, num2, num3, num4, vector, pearlReadContext) =>
                    {
                        if (flag_NSH_1)
                        {
                            self.iteratorButtons[num2] = new SimpleButton(self, self.pages[0], "", "TYPE_NSH", new Vector2((float)((int)(vector.x - (num3 + num4) * (float)num2)), vector.y), new Vector2(num4, num4));
                            self.iteratorButtons[num2].buttonBehav.greyedOut = !flag_NSH_2;
                            self.iteratorSprites[num2] = new FSprite(flag_NSH_2 ? "GuidanceNSH" : "Sandbox_SmallQuestionmark", true);
                            self.iteratorSprites[num2].color = new Color(0.75f, 1f, 0.75f);
                            self.iteratorSprites[num2].x = (float)((int)(self.iteratorButtons[num2].pos.x + self.iteratorButtons[num2].size.x / 2f - (1366f - self.manager.rainWorld.options.ScreenSize.x) / 2f));
                            self.iteratorSprites[num2].y = (float)((int)(self.iteratorButtons[num2].pos.y + self.iteratorButtons[num2].size.y / 2f));
                            self.pages[0].subObjects.Add(self.iteratorButtons[num2]);
                            self.pages[0].Container.AddChild(self.iteratorSprites[num2]);
                            if (pearlReadContext == CollectionsMenu.PearlReadContext.UnreadMoon && flag_NSH_2)
                            {
                                self.iteratorButtons[num2].toggled = true;
                                pearlReadContext = PearlReadContext_NSH;
                            }
                        }
                        return pearlReadContext;
                    });
                    c.Emit(OpCodes.Stloc_S, (byte)15);//num2
                    c.Emit(OpCodes.Ldloc_S, (byte)11);
                    c.EmitDelegate<Func<int, int>>((num2) =>
                    {
                        num2++;
                        return num2;
                    });
                    c.Emit(OpCodes.Stloc_S, (byte)11);//num2
                    //把消耗的arg.2还回去
                    c.Emit(OpCodes.Ldarg_2);//num2
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }
        #endregion

        public static void CollectionsMenu_Singal(On.MoreSlugcats.CollectionsMenu.orig_Singal orig, CollectionsMenu self, MenuObject sender, string message)
        {
            orig(self, sender, message);
            DataPearl.AbstractDataPearl.DataPearlType dataPearlType = self.usedPearlTypes[self.selectedPearlInd];
            CollectionsMenu.PearlReadContext a = CollectionsMenu.PearlReadContext.UnreadMoon;
            SlugcatStats.Name saveFile = null;
            if (a == PearlReadContext_NSH)
            {
                saveFile = NSH;
                self.InitLabelsFromPearlFile(CollectionsMenu.DataPearlToFileID(dataPearlType), saveFile);
            }
        }

        public static void CollectionsMenu_InitLabelsFromPearlFile(On.MoreSlugcats.CollectionsMenu.orig_InitLabelsFromPearlFile orig, CollectionsMenu self, int id, SlugcatStats.Name saveFile)
        {
            if (saveFile == NSH)
            {
                CollectionsMenu.ConversationLoader conversationLoader = new CollectionsMenu.ConversationLoader(self);
                NSHConversation.LoadEventsFromFile(conversationLoader, id, "NSH", saveFile, "Red");
                List<string> list = new List<string>();
                for (int i = 0; i < conversationLoader.events.Count; i++)
                {
                    if (conversationLoader.events[i] is Conversation.TextEvent)
                    {
                        list.Add((conversationLoader.events[i] as Conversation.TextEvent).text);
                    }
                }
                self.InitLabelsFromChatlog(list.ToArray());
                return;
            }
            orig(self, id, saveFile);
        }

        public static string CollectionsMenu_UpdateInfoText(On.MoreSlugcats.CollectionsMenu.orig_UpdateInfoText orig, CollectionsMenu self)
        {
            if (self.selectedObject is SimpleButton)
            {
                if ((self.selectedObject as SimpleButton).signalText == "TYPE_NSH")
                {
                    return self.Translate("No Significant Harassment's Transcription");
                }
            }
            return orig(self);
        }

        public static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
        {
            orig(self, manager);
            if (self.IsStorySession && ModManager.MSC && NSHOracleStateSave.NSHOracleState != null)
            {
                bool flag = false;
                foreach (DataPearl.AbstractDataPearl.DataPearlType dataPearlType in NSHOracleStateSave.NSHOracleState.significantPearls)
                {
                    if (self.GetStorySession.saveState.saveStateNumber == Plugin.SlugName)
                    {
                        if (!DecipheredNSHPearlsSave.GetNSHPearlDeciphered(self.rainWorld.progression.miscProgressionData, dataPearlType))
                        {
                            DecipheredNSHPearlsSave.SetNSHPearlDeciphered(self.rainWorld.progression.miscProgressionData, dataPearlType, false);
                            flag = true;
                        }
                    }
                }
                if (flag)
                {
                    self.rainWorld.progression.SaveProgression(false, true);
                }
            }
        }
    }
}
