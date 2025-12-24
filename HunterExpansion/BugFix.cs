using CustomDreamTx;
using HunterExpansion.CustomDream;
using HunterExpansion.CustomOracle;
using HunterExpansion.CustomSave;
using Menu;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using RWCustom;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HunterExpansion
{
    public class BugFix
    {
        public static void InitIL()
        {
            //这是在修梦境结束时扔出珍珠使游戏崩溃的问题
            IL.DataPearl.Update += DataPearl_UpdateIL;
            //这是在修进入NSH区域就报错的问题
            IL.OverseersWorldAI.DirectionFinder.ctor += DirectionFinder_ctorIL;
            //这是在修离开NSH区域后，NSH房间的珍珠丢失的问题（回到NSH区域的下一个循环将重新生成）
            //这是在不开启重要物品追踪的情况下，让NSH的房间也能存东西
            IL.RegionState.AdaptWorldToRegionState += RegionState_AdaptWorldToRegionStateIL;
            //这是在修多人联机时，梦境结束会卡在雨眠界面，雨眠cg疯狂抖动的问题
            IL.RainWorldGame.CommunicateWithUpcomingProcess += RainWorldGame_CommunicateWithUpcomingProcessIL;
            //这是在修NSH外缘业力门传送去郊区会卡住的问题
            IL.FliesWorldAI.ctor += FliesWorldAI_ctorIL;
            //这是修复胖猫不能正常触发去NSH过场cg的问题
            IL.ProcessManager.PostSwitchMainProcess += ProcessManager_PostSwitchMainProcessIL; 
        }

        public static void Init()
        {
            //这是在使部分游戏日志可以输出
            //On.RWCustom.Custom.Log += Custom_Log;

            //这是在修NSH珍珠在避难所显示为方块的问题
            On.ItemSymbol.SpriteNameForItem += ItemSymbol_SpriteNameForItem;
            //这是在修NSH区域迭代器主板闪烁总为红光的问题
            On.SuperStructureFuses.ctor += SuperStructureFuses_ctor;
            //这是在修过业力门进结局卡住的问题
            //On.WorldLoader.SpawnerStabilityCheck += WorldLoader_SpawnerStabilityCheck;
            //这是在修区域菜单无法找到“NSH”区域的问题
            //On.PlayerProgression.MiscProgressionData.ConditionalShelterData.GetShelterRegion += ConditionalShelterData_GetShelterRegion;
            //这是预防读不到NSH区域的避难所
            //On.PlayerProgression.MiscProgressionData.SaveDiscoveredShelter += MiscProgressionData_SaveDiscoveredShelter;
            //这是在修Emgtx的bug，解决与速通计时器的冲突
            //On.MoreSlugcats.SpeedRunTimer.Update += SpeedRunTimer_Update;
            //这是在修warp传送到sb_oe业力门时，先传送到oe区域，再想传送到sb区域，会传送不了
            //On.AbstractPhysicalObject.ChangeRooms += AbstractPhysicalObject_ChangeRooms;
            //On.PathFinder.Reset += PathFinder_Reset;
            //这是修复多人雨眠卡死bug
            //On.SaveState.BringUpToDate += SaveState_BringUpToDate;
            //这是修复传送报错bug？修不了，可能是Warp的问题
            //On.OverWorld.WorldLoaded += OverWorld_WorldLoaded;*/
            //这是在解决读取文件中特殊事件不能生效的问题
            On.Conversation.SpecialEvent.Activate += SpecialEvent_Activate;
            //这是在修传送梦境会卡在雨眠界面，雨眠cg疯狂抖动的问题
            On.DreamsState.StaticEndOfCycleProgress += DreamsState_StaticEndOfCycleProgress;
            //这是在修带猫崽过门遇到监视者导致卡死的问题
            On.OverseerAbstractAI.HowInterestingIsCreature += OverseerAbstractAI_HowInterestingIsCreature;
            //这是修复圣猫想去NSH，跳崖后卡死的问题
            On.MoreSlugcats.BlizzardGraphics.InitiateSprites += BlizzardGraphics_InitiateSprites;
            //这是修复warp到其他区域导致卡死的问题
            On.RegionState.AdaptRegionStateToWorld += RegionState_AdaptRegionStateToWorld;
        }

        #region IL Hooks
        private static void DataPearl_UpdateIL(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);
                //对self.room.game.GetStorySession.playerSessionRecords[num]进行null检查
                if (c.TryGotoNext(MoveType.After,
                    (i) => i.Match(OpCodes.Br_S),
                    (i) => i.Match(OpCodes.Ldc_I4_0),
                    (i) => i.Match(OpCodes.Stloc_2),
                    (i) => i.Match(OpCodes.Ldloc_2)))
                {
                    Plugin.Log("DataPearl_UpdateIL MatchFind!");
                    c.Emit(OpCodes.Ldarg_0);
                    c.EmitDelegate<Func<bool, DataPearl, bool>>((flag, self) =>
                    {
                        return flag && (self.room.game.GetStorySession.playerSessionRecords[(self.grabbedBy[0].grabber as Player).playerState.playerNumber] != null);
                    });
                    c.Emit(OpCodes.Stloc_2);
                    c.Emit(OpCodes.Ldloc_2);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }

        private static void DirectionFinder_ctorIL(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);
                //让监视者不要指示NSH区域的业力门符号（因为它们不是数字）
                if (c.TryGotoNext(MoveType.After,
                    (i) => i.MatchLdloc(10),
                    (i) => i.MatchLdloc(12),
                    (i) => i.Match(OpCodes.Ldelem_Ref),
                    (i) => i.Match(OpCodes.Ldarg_1),
                    (i) => i.MatchLdfld<World>("region"),
                    (i) => i.MatchLdfld<Region>("name"),
                    (i) => i.Match(OpCodes.Call)))
                {
                    Plugin.Log("DirectionFinder_ctorIL MatchFind!");
                    c.Emit(OpCodes.Ldarg_1);
                    c.EmitDelegate<Func<bool, World, bool>>((flag, world) =>
                    {
                        return flag && (world.region.name != "NSH");
                    });
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }

        private static void FliesWorldAI_ctorIL(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);
                if (c.TryGotoNext(MoveType.After,
                    (i) => i.Match(OpCodes.Ldarg_1),
                    (i) => i.MatchLdfld<World>("region"),
                    (i) => i.MatchLdfld<Region>("name"),
                    (i) => i.MatchLdstr("SU"),
                    (i) => i.Match(OpCodes.Call)))//这里已经找到了world.region.name == "SU"
                {
                    Plugin.Log("FliesWorldAI_ctorIL MatchFind!");
                    c.Emit(OpCodes.Ldarg_0);//找到FliesWorldAI
                    c.EmitDelegate<Func<bool, FliesWorldAI, bool>>((flag, self) =>
                    {
                        return flag && self.fliesToSpawn.Length >= 3;
                    });
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static void ProcessManager_PostSwitchMainProcessIL(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);
                if (c.TryGotoNext(MoveType.After,
                    (i) => i.MatchNewobj<MoreSlugcats.ScribbleDreamScreen>(),
                    (i) => i.MatchStfld<ProcessManager>("currentMainLoop")))
                {
                    Plugin.Log("ProcessManager_PostSwitchMainProcessIL MatchFind!");
                    c.Emit(OpCodes.Ldarg_0);//找到ProcessManager
                    c.EmitDelegate<Action<ProcessManager>>((self) =>
                    {
                        if (TravelDreamRegistry.modifyScribbleDreamScreen)
                        {
                            TravelDreamRegistry.modifyScribbleDreamScreen = false;
                            self.currentMainLoop = new DreamScreen(self);
                        }
                    });
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        /*
        public static void SaveState_SaveToStringIL(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);
                if (c.TryGotoNext(MoveType.After,
                    (i) => i.MatchCallvirt<String>("Substring"),
                    (i) => i.Match(OpCodes.Stfld)))
                {
                    Plugin.Log("SaveState_SaveToStringIL MatchFind!");
                    c.Emit(OpCodes.Ldarg_0);
                    c.Emit(OpCodes.Ldloc_S, (byte)11);
                    c.EmitDelegate<Action<SaveState, int>>((self, num3) =>
                    {
                        if (Regex.Split(RainWorld.ShelterAfterPassage, "_")[0] == "NSH")
                            self.objectTrackers[num3].lastSeenRegion = RainWorld.ShelterAfterPassage.Substring(0, 3);
                    });
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        */
        public static void RegionState_AdaptWorldToRegionStateIL(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);
                if (c.TryGotoNext(MoveType.After,
                    (i) => i.MatchStloc(11)))//stloc.s 11
                {
                    Plugin.Log("RegionState_AdaptWorldToRegionStateIL MatchFind!");
                    c.Emit(OpCodes.Ldloc_S, (byte)10);//找到flag3的本地变量
                    c.Emit(OpCodes.Ldloc_S, (byte)9);//找到abstractRoom的本地变量
                    c.EmitDelegate<Func<bool, AbstractRoom, bool>>((flag3, abstractRoom) =>
                    {
                        return flag3 || abstractRoom.name == "NSH_AI";
                    });
                    c.Emit(OpCodes.Stloc_S, (byte)11);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static void RainWorldGame_CommunicateWithUpcomingProcessIL(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);
                ILCursor find = new ILCursor(il);
                ILLabel pos = null;
                //找到循环结束的地方
                if (find.TryGotoNext(MoveType.After,
                    (i) => i.MatchCallvirt(out var method) && method.Name == "AddRange"))//(i) => i.MatchCallvirt<List<PlayerSessionRecord.KillRecord>>("AddRange")
                {
                    pos = find.MarkLabel();
                    Plugin.Log("RainWorldGame_CommunicateWithUpcomingProcessIL Find Pos to MarkLabel!");
                }
                //当self.GetStorySession.playerSessionRecords[i] == null时，需要跳过这一循环

                //对self.GetStorySession.playerSessionRecords[i]进行null检查
                if (c.TryGotoNext(MoveType.After,
                    (i) => i.MatchLdsfld<ModManager>("CoopAvailable"),
                    (i) => i.Match(OpCodes.Brfalse_S),
                    (i) => i.Match(OpCodes.Ldc_I4_1),
                    (i) => i.MatchStloc(10),
                    (i) => i.Match(OpCodes.Br_S),
                    (i) => i.Match(OpCodes.Ldarg_0)))
                {
                    Plugin.Log("RainWorldGame_CommunicateWithUpcomingProcessIL MatchFind!");
                    if (pos != null)
                    {
                        c.Emit(OpCodes.Ldloc_S, (byte)10);//找到i的本地变量
                        c.EmitDelegate<Func<RainWorldGame, int, bool>>((self, i) =>
                        {
                            return (self.GetStorySession.playerSessionRecords[i] != null);
                        });
                        c.Emit(OpCodes.Brfalse_S, pos);
                        c.Emit(OpCodes.Ldarg_0);
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }
        #endregion
        private static void Custom_Log(On.RWCustom.Custom.orig_Log orig, params string[] values)
        {
            orig(values);
            for (int i = 0; i < values.Length; i++)
                Plugin.Log("Custom_Log: " + values[i]);
        }

        private static void RegionState_AdaptRegionStateToWorld(On.RegionState.orig_AdaptRegionStateToWorld orig, RegionState self, int playerShelter, int activeGate)
        {
            if (self.world == null && self.saveState.saveStateNumber == Plugin.SlugName)
            {
                return;
            }
            orig(self, playerShelter, activeGate);
        }

        private static string ItemSymbol_SpriteNameForItem(On.ItemSymbol.orig_SpriteNameForItem orig, AbstractPhysicalObject.AbstractObjectType itemType, int intData)
        {
            string result = orig(itemType, intData);
            if (itemType == NSHPearlRegistry.NSHPearl)
            {
                result = "Symbol_Pearl";
            }
            return result;
        }

        private static void SuperStructureFuses_ctor(On.SuperStructureFuses.orig_ctor orig, SuperStructureFuses self, PlacedObject placedObject, IntRect rect, Room room)
        {
            orig(self, placedObject, rect, room);
            if (room != null && room.world.region != null && room.world.region.name == "NSH" && !room.world.game.IsArenaSession)
            {
                self.broken = room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.CorruptionSpores);
                if (room.world.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
                {
                    self.broken = 1f;
                }
            }
        }

        private static void WorldLoader_SpawnerStabilityCheck(On.WorldLoader.orig_SpawnerStabilityCheck orig, WorldLoader self, World.CreatureSpawner spawner)
        {
            if (self.worldName == "NSH")
                return;
            orig(self, spawner);
        }

        private static void SpecialEvent_Activate(On.Conversation.SpecialEvent.orig_Activate orig, Conversation.SpecialEvent self)
        {
            orig(self);
            if (self.owner.interfaceOwner is NSHOracleBehaviour)
                (self.owner.interfaceOwner as NSHOracleBehaviour).SpecialEvent(self.eventName);
        }

        private static void DreamsState_StaticEndOfCycleProgress(On.DreamsState.orig_StaticEndOfCycleProgress orig, SaveState saveState, string currentRegion, string denPosition, ref int cyclesSinceLastDream, ref int cyclesSinceLastFamilyDream, ref int cyclesSinceLastGuideDream, ref int inGWOrSHCounter, ref DreamsState.DreamID upcomingDream, ref DreamsState.DreamID eventDream, ref bool everSleptInSB, ref bool everSleptInSB_S01, ref bool guideHasShownHimselfToPlayer, ref int guideThread, ref bool guideHasShownMoonThisRound, ref int familyThread)
        {
            List<WeakReference<CustomNormalDreamTx>> allSlugDreams = new List<WeakReference<CustomNormalDreamTx>>();
            foreach (CustomNormalDreamTx dream in CustomDreamRx.normalDreamTreatments)
            {
                if (dream.focusSlugcat == Plugin.AllSlugcats)
                {
                    dream.focusSlugcat = saveState.saveStateNumber;
                }
            }
            if ((denPosition == "GATE_NSH_DGL" || denPosition == "GATE_SB_OE" || denPosition == "GATE_OE_SU") &&
                !TravelCompletedSave.travelCompleted)
            {
                cyclesSinceLastFamilyDream = 0;//屏蔽FamilyDream计数，防止被原本的方法干扰
            }
            orig(saveState, currentRegion, denPosition, ref cyclesSinceLastDream, ref cyclesSinceLastFamilyDream, ref cyclesSinceLastGuideDream, ref inGWOrSHCounter, ref upcomingDream, ref eventDream, ref everSleptInSB, ref everSleptInSB_S01, ref guideHasShownHimselfToPlayer, ref guideThread, ref guideHasShownMoonThisRound, ref familyThread);
        }

        private static float OverseerAbstractAI_HowInterestingIsCreature(On.OverseerAbstractAI.orig_HowInterestingIsCreature orig, OverseerAbstractAI self, AbstractCreature testCrit)
        {
            if (testCrit == null ||
                testCrit.realizedCreature == null ||
                testCrit.realizedCreature.room == null ||
                testCrit.realizedCreature.room.IsGateRoom())
                return 0f;
            return orig(self, testCrit);
        }

        private static void BlizzardGraphics_InitiateSprites(On.MoreSlugcats.BlizzardGraphics.orig_InitiateSprites orig, MoreSlugcats.BlizzardGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            if (self.room == null)
                self.room = self.rCam.room;
            orig(self, sLeaser, rCam);
        }
        /*
        private static void SaveState_ctor(On.SaveState.orig_ctor orig, SaveState self, SlugcatStats.Name saveStateNumber, PlayerProgression progression)
        {
            orig.Invoke(self, saveStateNumber, progression);
        }

        private static string DeathPersistentSaveData_SaveToString(On.DeathPersistentSaveData.orig_SaveToString orig, DeathPersistentSaveData self, bool saveAsIfPlayerDied, bool saveAsIfPlayerQuit)
        {
            string result = orig.Invoke(self, saveAsIfPlayerDied, saveAsIfPlayerQuit);
            return result;
        }

        private static void DeathPersistentSaveData_FromString(On.DeathPersistentSaveData.orig_FromString orig, DeathPersistentSaveData self, string s)
        {
            orig.Invoke(self, s);
        }*/
    }
}
