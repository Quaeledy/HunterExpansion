using HunterExpansion.CustomOracle;
using HunterExpansion.CustomSave;
//using Debug = UnityEngine.Debug;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HunterExpansion.HRTalk
{
    public class HRTalkHooks
    {
        public static bool oracleHasSpawn = false;
        public static bool NSHHasSpawn = false;
        public static bool SRSHasSpawn = false;

        public static void InitIL()
        {
            IL.Player.ClassMechanicsSaint += Player_ClassMechanicsSaintIL;
            IL.SSOracleBehavior.SSOracleRubicon.Update += SSOracleRubicon_UpdateIL;
            IL.Oracle.ctor += Oracle_ctorIL;
        }

        public static void Init()
        {
            On.Oracle.ctor += Oracle_ctor;
            On.RoomCamera.ChangeBothPalettes += RoomCamera_ChangeBothPalettes;
            On.Room.ReadyForAI += Room_ReadyForAI;
            On.Player.ctor += Player_ctor;
        }
        #region IL输出修改后代码的示例
        /*
        public static ILCursor text;
        public static bool logged;
        //On.Player.ClassMechanicsSaint += Player_ClassMechanicsSaint;
        public static void Player_ClassMechanicsSaint(On.Player.orig_ClassMechanicsSaint orig, Player self)
        {
            orig(self);
            try
            {
                if (!logged)
                {
                    var instruction = text.Next;
                    while (instruction.Next != null)
                    {
                        Plugin.Log(instruction.ToString());
                        instruction = instruction.Next;
                    }
                    logged = true;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }*/
        #endregion

        #region IL Hooks
        private static void Oracle_ctorIL(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);
                //当NSH还没在魔方节点生成时，需要把房间设为HR_AI
                if (c.TryGotoNext(MoveType.After,
                    (i) => i.MatchCall<PhysicalObject>("set_buoyancy")))
                {

                    Plugin.Log("Oracle_ctorIL MatchFind!");
                    c.EmitDelegate<Action>(() =>
                    {
                        if (!NSHHasSpawn)
                            NSHOracleRegistry.currentLoadingRoom = "HR_AI";
                        if (!SRSHasSpawn)
                            SRSOracleRegistry.currentLoadingRoom = "HR_AI";
                    });
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static void Player_ClassMechanicsSaintIL(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);

                //在圣猫的飞升技能中加入NSH
                if (c.TryGotoNext(MoveType.After,
                    (i) => i.Match(OpCodes.Ldc_R4),
                    (i) => i.Match(OpCodes.Ldc_R4),
                    (i) => i.Match(OpCodes.Ldc_I4_5),
                    (i) => i.Match(OpCodes.Call),
                    (i) => i.Match(OpCodes.Newobj),
                    (i) => i.Match(OpCodes.Callvirt),
                    (i) => i.Match(OpCodes.Ldc_I4_1),
                    (i) => i.Match(OpCodes.Stloc_S),
                    (i) => i.Match(OpCodes.Ldloc_S)))//这里已经找到了physicalObject的本地变量
                {
                    Plugin.Log("Player_ClassMechanicsSaintIL MatchFind!");
                    c.Emit(OpCodes.Ldloc_S, (byte)15);//找到flag2的本地变量
                    c.Emit(OpCodes.Ldarg_0);//找到self
                    c.EmitDelegate<Func<PhysicalObject, bool, Player, bool>>((physicalObject, flag2, self) =>
                    {
                        if (self.abstractCreature.world.game.IsStorySession &&
                            self.abstractCreature.world.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Saint &&
                            self.room.game.session is StoryGameSession && physicalObject is Oracle &&
                            (physicalObject as Oracle).ID == NSHOracleRegistry.NSHOracle && !(RipNSHSave.ripNSH))
                        {
                            RipNSHSave.ripNSH = true;
                            switch (Random.Range(0, 4))
                            {
                                case 0:
                                    self.room.PlaySound(NSHOracleSoundID.NSH_AI_Break_1, 0f, 0.5f, 1.8f);
                                    break;
                                case 1:
                                    self.room.PlaySound(NSHOracleSoundID.NSH_AI_Break_2, 0f, 0.5f, 1.8f);
                                    break;
                                case 2:
                                    self.room.PlaySound(NSHOracleSoundID.NSH_AI_Break_3, 0f, 0.5f, 1.8f);
                                    break;
                                case 3:
                                    self.room.PlaySound(NSHOracleSoundID.NSH_AI_Break_4, 0f, 0.5f, 1.8f);
                                    break;
                            }
                            Vector2 pos = (physicalObject as Oracle).bodyChunks[0].pos;
                            self.room.AddObject(new ShockWave(pos, 500f, 0.75f, 18, false));
                            self.room.AddObject(new Explosion.ExplosionLight(pos, 320f, 1f, 5, Color.white));
                            Plugin.Log("Ascend saint NSH");
                            ((physicalObject as Oracle).oracleBehavior as NSHOracleBehaviour).dialogBox.Interrupt(((physicalObject as Oracle).oracleBehavior as NSHOracleBehaviour).Translate("..."), 1);
                            if (((physicalObject as Oracle).oracleBehavior as NSHOracleBehaviour).conversation != null)
                            {
                                ((physicalObject as Oracle).oracleBehavior as NSHOracleBehaviour).conversation.Destroy();
                                ((physicalObject as Oracle).oracleBehavior as NSHOracleBehaviour).conversation = null;
                            }
                            (physicalObject as Oracle).health = 0f;
                            flag2 = true;
                        }
                        return flag2;
                    });
                    c.Emit(OpCodes.Stloc_S, (byte)15);//找到flag2的本地变量
                    c.Emit(OpCodes.Ldloc_S, (byte)18);//这是给ST（茅草裂片）用的
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static void SSOracleRubicon_UpdateIL(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);
                ILCursor find = new ILCursor(il);
                ILLabel pos = null;
                //找到原方法结束的地方
                if (find.TryGotoNext(MoveType.After,
                    (i) => i.Match(OpCodes.Call),
                    (i) => i.Match(OpCodes.Ldarg_0),
                    (i) => i.Match(OpCodes.Ldc_I4_1),
                    (i) => i.Match(OpCodes.Stfld)))
                {
                    pos = find.MarkLabel();
                    Plugin.Log("SSOracleRubicon_UpdateIL Find Pos to MarkLabel!");
                }
                //当NSH被超度为true时，需要跳过原方法
                if (c.TryGotoNext(MoveType.After,
                    (i) => i.Match(OpCodes.Ldarg_0),
                    (i) => i.Match(OpCodes.Call),
                    (i) => i.Match(OpCodes.Ldc_I4_S),
                    (i) => i.Match(OpCodes.Ble)))
                {
                    Plugin.Log("SSOracleRubicon_UpdateIL MatchFind!");
                    if (pos != null)
                    {
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<SSOracleBehavior.SSOracleRubicon, bool>>((self) =>
                        {
                            //但是不要忘了，当NSH被超度为true时，其他迭的startedConversation依然需要为true;
                            if (RipNSHSave.ripNSH && !self.startedConversation)
                            {
                                self.startedConversation = true;
                                self.owner.conversation = null;
                            }
                            return RipNSHSave.ripNSH;
                        });
                        c.Emit(OpCodes.Brtrue, pos);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        #endregion

        private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);

            NSHHasSpawn = false;
            SRSHasSpawn = false;
        }

        private static void Oracle_ctor(On.Oracle.orig_ctor orig, Oracle self, AbstractPhysicalObject abstractPhysicalObject, Room room)
        {
            Plugin.Log("room.oracleWantToSpawn: " + (room.oracleWantToSpawn != null ? room.oracleWantToSpawn.ToString() : "NULL"));
            oracleHasSpawn = false;
            //如果在魔方节点已经生成了该生成的迭代器，则生成NSH
            if (ModManager.MSC && room.world.name == "HR" && room.oracleWantToSpawn != null)
            {
                List<PhysicalObject>[] physicalObjects = room.physicalObjects;
                for (int i = 0; i < physicalObjects.Length; i++)
                {
                    for (int j = 0; j < physicalObjects[i].Count; j++)
                    {
                        PhysicalObject physicalObject = physicalObjects[i][j];
                        if ((physicalObject is Oracle))
                        {
                            if ((physicalObject as Oracle).ID == room.oracleWantToSpawn)
                            {
                                oracleHasSpawn = true;
                                Plugin.Log(room.oracleWantToSpawn.ToString() + " oracle has spawn!");
                                break;
                            }
                        }
                    }
                }
                for (int i = 0; i < physicalObjects.Length; i++)
                {
                    for (int j = 0; j < physicalObjects[i].Count; j++)
                    {
                        PhysicalObject physicalObject = physicalObjects[i][j];
                        if ((physicalObject is Oracle))
                        {
                            if ((physicalObject as Oracle).ID == NSHOracleRegistry.NSHOracle)
                            {
                                NSHHasSpawn = true;
                                Plugin.Log("NSH oracle has spawn!");
                                break;
                            }
                            if ((physicalObject as Oracle).ID == SRSOracleRegistry.SRSOracle)
                            {
                                SRSHasSpawn = true;
                                Plugin.Log("SRS oracle has spawn!");
                                break;
                            }
                        }
                    }
                }
                if (oracleHasSpawn && !NSHHasSpawn)//如果之前的迭代器已经生成了，但NSH还没生成
                    NSHOracleRegistry.currentLoadingRoom = "HR_AI";
                else
                    NSHOracleRegistry.currentLoadingRoom = "NSH_AI";
                if (oracleHasSpawn && !SRSHasSpawn)//如果之前的迭代器已经生成了，但SRS还没生成
                    SRSOracleRegistry.currentLoadingRoom = "HR_AI";
                else
                    SRSOracleRegistry.currentLoadingRoom = "SRS_AI";
            }

            orig(self, abstractPhysicalObject, room);
        }

        private static void RoomCamera_ChangeBothPalettes(On.RoomCamera.orig_ChangeBothPalettes orig, RoomCamera self, int palA, int palB, float blend)
        {
            if ((self.room != null && self.room.abstractRoom.name == "HR_AI" &&
                RipNSHSave.ripNSH && self.room.game.IsStorySession &&
                self.room.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Saint) ||
                (self.room != null && self.room.abstractRoom.name == "NSH_AI" && self.room.game.IsStorySession &&
                self.room.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel))
            {
                return;
            }
            orig(self, palA, palB, blend);
        }

        private static void Room_ReadyForAI(On.Room.orig_ReadyForAI orig, Room self)
        {
            if (RipNSHSave.ripNSH && self.game != null && self.game.IsStorySession &&
                !self.game.GetStorySession.saveState.miscWorldSaveData.hrMelted)
            {
                NSHOracleRegistry.currentLoadingRoom = self.abstractRoom.name;
                SRSOracleRegistry.currentLoadingRoom = self.abstractRoom.name;
            }

            orig.Invoke(self);
        }
    }
}
