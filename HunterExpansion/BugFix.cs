using UnityEngine;
using HUD;
using MoreSlugcats;
using RWCustom;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using MonoMod;
using HunterExpansion.CustomOracle;

namespace HunterExpansion
{
    public class BugFix
    {
        public static void InitIL()
        {
            //这是在修梦境结束时扔出珍珠使游戏崩溃的问题
            IL.DataPearl.Update += DataPearl_UpdateIL;
            //这是在修离开NSH区域后，NSH房间的珍珠丢失的问题？
            //IL.RegionState.AdaptWorldToRegionState += RegionState_AdaptWorldToRegionStateIL;
        }

        public static void Init()
        {
            //这是在修emgtx的bug，解决与速通计时器的冲突
            On.MoreSlugcats.SpeedRunTimer.Update += SpeedRunTimer_Update;
            //这是在修NSH珍珠在避难所显示为方块的问题
            On.ItemSymbol.SpriteNameForItem += ItemSymbol_SpriteNameForItem;
            //这是在修NSH区域迭代器主板闪烁总为红光的问题
            On.SuperStructureFuses.ctor += SuperStructureFuses_ctor;

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

        public static void RegionState_AdaptWorldToRegionStateIL(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);
                //对self.room.game.GetStorySession.playerSessionRecords[num]进行null检查
                if (c.TryGotoNext(MoveType.After,
                    (i) => i.MatchStloc(11)))//stloc.s 11
                {
                    Plugin.Log("RegionState_AdaptWorldToRegionState MatchFind!");
                    c.Emit(OpCodes.Ldloc_S, (byte)11);//找到flag3的本地变量
                    c.Emit(OpCodes.Ldloc_S, (byte)10);//找到abstractRoom的本地变量
                    c.EmitDelegate<Func<bool, AbstractRoom, bool>>((flag3, abstractRoom) =>
                    {
                        Plugin.Log("RegionState_AdaptWorldToRegionStateIL： " + (flag3 || abstractRoom.name == "NSH_AI"));
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
        /*
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
