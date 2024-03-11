using UnityEngine;
using HUD;
using MoreSlugcats;
using RWCustom;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using MonoMod;
using HunterExpansion.CustomOracle;
using Menu;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

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
            //这是在修区域菜单无法找到“NSH”区域的问题
            IL.Menu.FastTravelScreen.ctor += FastTravelScreen_ctorIL;
            IL.Menu.FastTravelScreen.FinalizeRegionSwitch += FastTravelScreen_FinalizeRegionSwitchIL;
            //这是在修离开NSH区域后，NSH房间的珍珠丢失的问题（回到NSH区域的下一个循环将重新生成）
            //这是在不开启重要物品追踪的情况下，让NSH的房间也能存东西
            IL.RegionState.AdaptWorldToRegionState += RegionState_AdaptWorldToRegionStateIL;
            //这是让读取避难所前两个字母作为区域名，变成在NSH区域读前三个字母
            IL.SaveState.SaveToString += SaveState_SaveToStringIL;
            //这是在修多人联机时，梦境结束会卡在雨眠界面，雨眠cg疯狂抖动的问题
            IL.RainWorldGame.CommunicateWithUpcomingProcess += RainWorldGame_CommunicateWithUpcomingProcessIL;
        }

        public static void Init()
        {
            //这是在修Emgtx的bug，解决与速通计时器的冲突
            On.MoreSlugcats.SpeedRunTimer.Update += SpeedRunTimer_Update;
            //这是在修NSH珍珠在避难所显示为方块的问题
            On.ItemSymbol.SpriteNameForItem += ItemSymbol_SpriteNameForItem;
            //这是在修NSH区域迭代器主板闪烁总为红光的问题
            On.SuperStructureFuses.ctor += SuperStructureFuses_ctor;
            //这是在修过业力门进结局卡住的问题
            On.WorldLoader.SpawnerStabilityCheck += WorldLoader_SpawnerStabilityCheck;
            //这是在修区域菜单无法找到“NSH”区域的问题
            On.PlayerProgression.MiscProgressionData.ConditionalShelterData.GetShelterRegion += ConditionalShelterData_GetShelterRegion;
            //这是预防读不到NSH区域的避难所
            On.PlayerProgression.MiscProgressionData.SaveDiscoveredShelter += MiscProgressionData_SaveDiscoveredShelter;

            //这是在修warp传送到sb_oe业力门时，先传送到oe区域，再想传送到sb区域，会传送不了
            //On.AbstractPhysicalObject.ChangeRooms += AbstractPhysicalObject_ChangeRooms;
            //On.PathFinder.Reset += PathFinder_Reset;

        }
        #region IL Hooks
        public static void DataPearl_UpdateIL(ILContext il)
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
                Debug.LogException(e);
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
                Debug.LogException(e);
            }
        }

        private static void FastTravelScreen_ctorIL(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);
                if (c.TryGotoNext(MoveType.After,
                    (i) => i.MatchCallvirt<String>("Substring"),
                    (i) => i.Match(OpCodes.Call)))
                {
                    Plugin.Log("FastTravelScreen_ctorIL MatchFind!");
                    c.Emit(OpCodes.Ldarg_0);
                    c.Emit(OpCodes.Ldloc_S, (byte)14);
                    c.EmitDelegate<Func<bool, FastTravelScreen, int, bool>>((flag, self, num3) =>
                    {
                        return flag || 
                               (Regex.Split(self.currentShelter, "_")[0] == "NSH" &&
                                self.allRegions[self.accessibleRegions[num3]].name == self.currentShelter.Substring(0, 3));
                    });
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static void FastTravelScreen_FinalizeRegionSwitchIL(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);
                if (c.TryGotoNext(MoveType.After,
                    (i) => i.MatchCallvirt<String>("Substring"),
                    (i) => i.Match(OpCodes.Ldarg_0),
                    (i) => i.Match(OpCodes.Ldfld),
                    (i) => i.Match(OpCodes.Ldarg_0),
                    (i) => i.Match(OpCodes.Ldfld),
                    (i) => i.Match(OpCodes.Ldarg_1),
                    (i) => i.Match(OpCodes.Callvirt),
                    (i) => i.Match(OpCodes.Ldelem_Ref),
                    (i) => i.Match(OpCodes.Ldfld),
                    (i) => i.Match(OpCodes.Call)))
                {
                    Plugin.Log("FastTravelScreen_FinalizeRegionSwitchIL 0 MatchFind!");
                    c.Emit(OpCodes.Ldarg_0);
                    c.Emit(OpCodes.Ldarg_1);
                    c.EmitDelegate<Func<bool, FastTravelScreen, int, bool>>((flag, self, newRegion) =>
                    {
                        return flag ||
                               (self.currentShelter != null &&
                                Regex.Split(self.currentShelter, "_")[0] == "NSH" &&
                                self.allRegions[self.accessibleRegions[newRegion]].name == self.currentShelter.Substring(0, 3));
                    });
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            try
            {
                ILCursor c = new ILCursor(il);
                if (c.TryGotoNext(MoveType.After,
                    (i) => i.MatchCallvirt<String>("Substring"),
                    (i) => i.Match(OpCodes.Ldarg_0),
                    (i) => i.Match(OpCodes.Ldfld),
                    (i) => i.Match(OpCodes.Ldarg_0),
                    (i) => i.Match(OpCodes.Ldfld),
                    (i) => i.Match(OpCodes.Ldarg_1),
                    (i) => i.Match(OpCodes.Callvirt),
                    (i) => i.Match(OpCodes.Ldelem_Ref),
                    (i) => i.Match(OpCodes.Ldfld),
                    (i) => i.Match(OpCodes.Call)))
                {
                    Plugin.Log("FastTravelScreen_FinalizeRegionSwitchIL 1 MatchFind!");
                    c.Emit(OpCodes.Ldarg_0);
                    c.Emit(OpCodes.Ldarg_1);
                    c.Emit(OpCodes.Ldloc_S, (byte)9);
                    c.EmitDelegate<Func<bool, FastTravelScreen, int, int, bool>>((flag, self, newRegion, k) =>
                    {
                        return flag ||
                               (self.playerShelters[k] != null &&
                                Regex.Split(self.playerShelters[k], "_")[0] == "NSH" &&
                                self.allRegions[self.accessibleRegions[newRegion]].name == self.playerShelters[k].Substring(0, 3));
                    });
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

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
                        if(Regex.Split(RainWorld.ShelterAfterPassage, "_")[0] == "NSH")
                            self.objectTrackers[num3].lastSeenRegion = RainWorld.ShelterAfterPassage.Substring(0, 3);
                    });
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static void RegionState_AdaptWorldToRegionStateIL(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);
                //对self.room.game.GetStorySession.playerSessionRecords[num]进行null检查
                if (c.TryGotoNext(MoveType.After,
                    (i) => i.MatchStloc(11)))//stloc.s 11
                {
                    Plugin.Log("RegionState_AdaptWorldToRegionStateIL MatchFind!");
                    c.Emit(OpCodes.Ldloc_S, (byte)11);//找到flag3的本地变量
                    c.Emit(OpCodes.Ldloc_S, (byte)10);//找到abstractRoom的本地变量
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
                    (i) => i.MatchStloc(8),
                    (i) => i.Match(OpCodes.Br_S),
                    (i) => i.Match(OpCodes.Ldarg_0)))
                {
                    Plugin.Log("RainWorldGame_CommunicateWithUpcomingProcessIL MatchFind!");
                    if (pos != null)
                    {
                        c.Emit(OpCodes.Ldloc_S, (byte)8);//找到i的本地变量
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
                Debug.LogException(e);
            }
        }
        #endregion
        public static string ItemSymbol_SpriteNameForItem(On.ItemSymbol.orig_SpriteNameForItem orig, AbstractPhysicalObject.AbstractObjectType itemType, int intData)
        {
            string result = orig(itemType, intData);
            if (itemType == NSHPearlRegistry.NSHPearl)
            {
                result = "Symbol_Pearl";
            }
            return result;
        }

        public static void SpeedRunTimer_Update(On.MoreSlugcats.SpeedRunTimer.orig_Update orig, SpeedRunTimer self)
        {
            if (self.ThePlayer().abstractCreature.world.game.GetStorySession.playerSessionRecords[0] == null)
            {
                return;
            }
            orig(self);
        }

        public static void SuperStructureFuses_ctor(On.SuperStructureFuses.orig_ctor orig, SuperStructureFuses self, PlacedObject placedObject, IntRect rect, Room room)
        {
            orig(self, placedObject, rect, room);
            if (room.world.region.name == "NSH" && !room.world.game.IsArenaSession)
            {
                self.broken = room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.CorruptionSpores);
                if (room.world.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
                {
                    self.broken = 1f;
                }
            }
        }
        
        public static void WorldLoader_SpawnerStabilityCheck(On.WorldLoader.orig_SpawnerStabilityCheck orig, WorldLoader self, World.CreatureSpawner spawner)
        {
            if (self.worldName == "NSH")
                return;
            orig(self, spawner);
        }

        public static string ConditionalShelterData_GetShelterRegion(On.PlayerProgression.MiscProgressionData.ConditionalShelterData.orig_GetShelterRegion orig, PlayerProgression.MiscProgressionData.ConditionalShelterData self)
        {
            string result = orig(self);
            if (Regex.Split(self.shelterName, "_")[0] == "NSH")
            {
                result = "NSH";
            }
            return result;
        }

        private static void MiscProgressionData_SaveDiscoveredShelter(On.PlayerProgression.MiscProgressionData.orig_SaveDiscoveredShelter orig, PlayerProgression.MiscProgressionData self, string roomName)
        {
            if (Regex.Split(roomName, "_")[0] == "NSH")
            {
                string key = "NSH";
                self.updateConditionalShelters(roomName, self.currentlySelectedSinglePlayerSlugcat);
                if (!self.discoveredShelters.ContainsKey(key) || self.discoveredShelters[key] == null)
                {
                    self.discoveredShelters[key] = new List<string>();
                }
                if (!self.discoveredShelters[key].Contains(roomName))
                {
                    self.discoveredShelters[key].Add(roomName);
                }
                return;
            }
            orig(self, roomName);
        }
        /*
        private static void Region_ctor(On.Region.orig_ctor orig, Region self, string name, int firstRoomIndex, int regionNumber, SlugcatStats.Name storyIndex)
        {
            orig(self, name, firstRoomIndex, regionNumber, storyIndex);
            if (self.name == "NSH")
            {
                self.regionParams.slugPupSpawnChance = 0f;
            }
        }*/
        /*
        public static void AbstractPhysicalObject_ChangeRooms(On.AbstractPhysicalObject.orig_ChangeRooms orig, AbstractPhysicalObject self, WorldCoordinate newCoord)
        {
            //问题：self.world.GetAbstractRoom(newCoord) == null
            Plugin.Log("newCoord：" + (newCoord));
            Plugin.Log("newCoord.room：" + (newCoord.room));
            Plugin.Log("self.world.GetAbstractRoom(self.pos) == null? " + (self.world.GetAbstractRoom(self.pos) == null));
            Plugin.Log("this.firstRoomIndex：" + self.world.firstRoomIndex);
            Plugin.Log("this.firstRoomIndex + this.NumberOfRooms：" + self.world.NumberOfRooms);
            orig(self, newCoord);
        }
        
        public static void PathFinder_Reset(On.PathFinder.orig_Reset orig, PathFinder self, Room newRealizedRoom)
        {
            self.currentlyFollowingDestination = self.creature.pos;
            self.realizedRoom = newRealizedRoom;
            self.room = self.realizedRoom.abstractRoom.index;
            self.checkNextList.Clear();
            int num = self.realizedRoom.TileWidth;
            int num2 = 0;
            int num3 = 0;
            int num4 = self.realizedRoom.TileHeight;
            bool flag = false;
            for (int i = 0; i < self.realizedRoom.TileWidth; i++)
            {
                for (int j = 0; j < self.realizedRoom.TileHeight; j++)
                {
                    if (self.realizedRoom == null)
                    {
                        Debug.Log("@@ REALIZED ROOM NULL !!");
                    }
                    else if (self.realizedRoom.aimap == null)
                    {
                        Debug.Log("@@ AIMAP NULL FOR ROOM " + self.realizedRoom.abstractRoom.name);
                    }
                    if (self.realizedRoom.aimap.TileAccessibleToCreature(i, j, self.creatureType))
                    {
                        flag = true;
                        if (i < num)
                        {
                            num = i;
                        }
                        if (j < num4)
                        {
                            num4 = j;
                        }
                        if (i > num3)
                        {
                            num3 = i;
                        }
                        if (j > num2)
                        {
                            num2 = j;
                        }
                    }
                }
            }
            if (!flag)
            {
                num = 0;
                num4 = 0;
                num3 = 1;
                num2 = 1;
            }
            self.coveredArea = new IntRect(num, num4, num3, num2);
            num3 = self.realizedRoom.TileWidth - num3 - 1;
            num2 = self.realizedRoom.TileHeight - num2 - 1;
            self.CurrentRoomCells = new PathFinder.PathingCell[self.realizedRoom.TileWidth - num3 - num, self.realizedRoom.TileHeight - num2 - num4];
            for (int k = 0; k < self.realizedRoom.TileWidth - num3 - num; k++)
            {
                for (int l = 0; l < self.realizedRoom.TileHeight - num2 - num4; l++)
                {
                    self.CurrentRoomCells[k, l] = new PathFinder.PathingCell(new WorldCoordinate(self.room, k + num, l + num4, -1));
                }
            }
            if (self.visualize && self.realizedRoom.TileWidth * self.realizedRoom.TileHeight < 16000)
            {
                self.debugDrawer = new PathfindingVisualizer(self.world, self.realizedRoom, self, self.CurrentRoomCells.GetLength(0), self.CurrentRoomCells.GetLength(1), new IntVector2(self.coveredArea.left, self.coveredArea.bottom));
            }
            if (self.accessibilityMapper != null)
            {
                self.accessibilityMapper.CullClients(self);
            }
            self.accessibilityMapper = null;
            if (self.nonShortcutRoomEntrancePos != null)
            {
                self.InitiatePath(self.nonShortcutRoomEntrancePos.Value.abstractNode);
            }
            else if (!self.creature.pos.TileDefined && self.creature.pos.NodeDefined)
            {
                self.InitiatePath(self.creature.pos.abstractNode);
            }
            else
            {
                self.InitiAccessibilityMapping(self.creature.pos, null);
            }
            self.reAssignDestinationOnceAccessibilityMappingIsDone = true;
        }
        */
    }
}
