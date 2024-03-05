using CustomDreamTx;
using HunterExpansion.CustomOracle;
using HunterExpansion.CustomSave;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MoreSlugcats;
using static CustomOracleTx.CustomOracleBehaviour;
using RWCustom;
using IL;
using On;

namespace HunterExpansion.CustomDream
{
    public class PlayerHooks
    {
        public static void Init()
        {
            On.Player.ctor += Player_ctor;
            On.SaveState.ctor += SaveState_ctor;//让红猫有梦
            On.Player.UpdateMSC += Player_DreamUpdate;
            On.Player.Die += Player_Die;//NSH的免死保护
            //On.Player.UpdateMSC += Player_Test;
        }

        public static bool hasEnterDream = false;
        public static bool setRedsIllness = false;
        public static bool setNSHNarration = false;
        public static bool setNSHConclusion = false;
        public static int conclusionLevel = 0;
        public static int corruptionSpawn = 0;
        public static int lizardSpawn = 0;
        public static float power = 1f;
        public static NSHNarration nshNarration;
        public static NSHConclusion nshConclusion;
        public static AbstractCreature abstractCreature;

        public static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            if (self.room.game.session.characterStats.name != Plugin.SlugName)
                return;

            //第一场梦境：需要幼崽体型
            if (CustomDreamRx.currentActivateNormalDream != null && CustomDreamRx.currentActivateNormalDream.activateDreamID == DreamID.HunterDream_0)
            {
                self.playerState.isPup = true;
            }
        }


        //让红猫有梦
        public static void SaveState_ctor(On.SaveState.orig_ctor orig, SaveState self, SlugcatStats.Name saveStateNumber, PlayerProgression progression)
        {
            orig(self, saveStateNumber, progression);
            if (saveStateNumber == Plugin.SlugName && self.dreamsState == null)
            {
                self.dreamsState = new DreamsState();//让红猫有梦
            }
        }

        public static void Player_DreamUpdate(On.Player.orig_UpdateMSC orig, Player self)
        {
            orig(self);
            if (self.room.game.session.characterStats.name != Plugin.SlugName)
                return;

            if (CustomDreamRx.currentActivateNormalDream != null)
            {
                //如果在梦中，玩家离开房间，则梦境结束
                if (CustomDreamRx.currentActivateNormalDream.IsPerformDream)
                {
                    if (self.room.abstractRoom.name == CustomDreamRx.currentActivateNormalDream.GetBuildDreamWorldParams().firstRoom)
                    {
                        hasEnterDream = true;
                    }
                    if (self.room.abstractRoom.name != CustomDreamRx.currentActivateNormalDream.GetBuildDreamWorldParams().firstRoom &&
                        hasEnterDream)
                    {
                        CustomDreamRx.currentActivateNormalDream.EndDream(self.room.game);
                    }
                }

                //第三场梦境：对战蜥蜴
                if (CustomDreamRx.currentActivateNormalDream.activateDreamID == DreamID.HunterDream_2)
                {
                    if (self.room.abstractRoom.name == "NSH_A01")
                    {
                        //设置初始对话
                        if (!setNSHNarration)
                        {
                            setNSHNarration = true;
                            nshNarration = new NSHNarration(self.room);
                            self.room.AddObject(nshNarration);
                            //生成监视者
                            for (int i = 0; i < 3; i++)
                            {
                                SpawnOverseerInRoom(self.room, "NSH_A01", CreatureTemplate.Type.Overseer, 2);
                            }
                            //以防万一，给点矛
                            for (int i = 0; i < 2; i++)
                            {
                                AbstractSpear absSpaer = new AbstractSpear(self.room.world, null, self.abstractPhysicalObject.pos, self.room.game.GetNewID(), false);
                                Spear spear = new Spear(absSpaer, self.room.world);
                                spear.abstractPhysicalObject.RealizeInRoom();
                            }
                        }
                        //设置结束对话
                        if (abstractCreature != null && abstractCreature.state.alive == false && !setNSHConclusion)
                        {
                            setNSHConclusion = true;
                            nshConclusion = new NSHConclusion(self.room);
                            self.room.AddObject(nshConclusion);
                        }
                        //初始对话结束后
                        if (nshNarration != null && nshNarration.slatedForDeletetion)
                        {
                            //去掉重力核心的影响
                            if (lizardSpawn >= 50)
                            {
                                for (int k = self.room.updateList.Count - 1; k >= 0; k--)
                                {
                                    UpdatableAndDeletable item = self.room.updateList[k];
                                    if (item is GravityDisruptor)
                                    {
                                        //来点特效
                                        if (lizardSpawn == 50)
                                        {
                                            self.room.AddObject(new ShockWave((item as GravityDisruptor).pos, 200f, 0.5f, 80, false));
                                            self.room.PlaySound(SoundID.Broken_Anti_Gravity_Switch_On, 0f, 1f, 1f);
                                            power = (item as GravityDisruptor).power;
                                        }
                                        //重力核心逐渐关闭
                                        (item as GravityDisruptor).power = Mathf.Lerp(power, 0f, Mathf.Min(lizardSpawn / 50f - 1f, 1f));
                                        //屏幕震动
                                        self.room.ScreenMovement(new Vector2?((item as GravityDisruptor).pos), new Vector2(0f, 0f), Mathf.Min(Custom.LerpMap(lizardSpawn, 50f, 120f, 0f, 1.5f, 1.2f), Custom.LerpMap(lizardSpawn, 50f, 120f, 1.5f, 0f)));
                                    }
                                }
                                for (int i = 0; i < self.room.roomSettings.ambientSounds.Count - 1; i++)
                                {
                                    if (self.room.roomSettings.ambientSounds[i].sample == "SO_SFX-Escape.ogg")
                                    {
                                        self.room.roomSettings.ambientSounds[i].volume = Mathf.Lerp(power, 0f, Mathf.Min(lizardSpawn / 50f - 1f, 1f));
                                        break;
                                    }
                                }
                            }
                            //生成蜥蜴
                            if (lizardSpawn == 250)
                            {
                                SpawnCreatureInDen(self.room, "NSH_A01", 1, CreatureTemplate.Type.WhiteLizard, 20);
                            }
                            lizardSpawn++;
                        }
                        //防止被咬住后没死亡也游戏结束了
                        if (self.dangerGrasp != null)
                        {
                            ExemptingFromDeath(self);
                        }
                    }
                }

                //腐化的噩梦特效
                if (CustomDreamRx.currentActivateNormalDream.activateDreamID == DreamID.HunterDream_4)
                {
                    //生成特效
                    if (self.room.abstractRoom.name == "NSH_AITEST")
                    {
                        RedsIllnessEffect redsIllnessEffect = new RedsIllnessEffect(self, self.room);
                        if (!setRedsIllness)
                        {
                            setRedsIllness = true;
                            self.room.AddObject(redsIllnessEffect);
                        }
                        //生成香菇
                        if (corruptionSpawn == 400)
                        {
                            SpawnCreatureInDen(self.room, "NSH_AITEST", 1, MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs, 20);
                        }
                        corruptionSpawn++;
                    }
                }
            }
        }

        //防止对战蜥蜴梦境中死亡
        public static void Player_Die(On.Player.orig_Die orig, Player self)
        {
            if (CustomDreamRx.currentActivateNormalDream != null && CustomDreamRx.currentActivateNormalDream.activateDreamID == DreamID.HunterDream_2)
            {
                ExemptingFromDeath(self);
                return;
            }
            orig(self);
        }

        public static void SpawnOverseerInRoom(Room room, string roomName, CreatureTemplate.Type type, int ownerIterator)
        {
            AbstractCreature result;
            int roomID = room.game.world.GetAbstractRoom(roomName).index;
            if (room.game.world.GetAbstractRoom(roomID).realizedRoom == null)
            {
                Plugin.Log("Exception : Can't spawn creature in room.");
                result = null;
            }
            else
            {
                AbstractCreature abstractCreature = new AbstractCreature(room.game.world, StaticWorld.GetCreatureTemplate(type), null, new WorldCoordinate(roomID, -1, -1, -1), room.game.GetNewID());
                (abstractCreature.abstractAI as OverseerAbstractAI).ownerIterator = ownerIterator;
                room.game.world.GetAbstractRoom(roomID).AddEntity(abstractCreature);
                abstractCreature.RealizeInRoom();
                result = abstractCreature;
            }
        }

        public static void SpawnCreatureInDen(Room room, string roomName, int denID, CreatureTemplate.Type creatureType, int remainTime)
        {
            int roomID = room.game.world.GetAbstractRoom(roomName).index;
            AbstractRoom absroom = room.game.world.GetAbstractRoom(roomID);
            int dens = absroom.dens;
            int suggestDen = Mathf.Min(denID, dens);
            int denNodeIndex = -1;
            for (int i = 0; i < absroom.nodes.Length; i++)
            {
                bool flag = absroom.nodes[i].type == AbstractRoomNode.Type.Den;
                if (flag)
                {
                    denNodeIndex = i;
                    bool flag2 = suggestDen == 0;
                    if (flag2)
                    {
                        break;
                    }
                    suggestDen--;
                }
            }
            abstractCreature = new AbstractCreature(room.game.world, StaticWorld.GetCreatureTemplate(creatureType), null, new WorldCoordinate(roomID, -1, -1, denNodeIndex), room.game.GetNewID());
            abstractCreature.remainInDenCounter = remainTime;
            absroom.MoveEntityToDen(abstractCreature);
        }

        //NSH给猎手提供的免死
        public static void ExemptingFromDeath(Player self)
        {
            self.room.AddObject(new Explosion.ExplosionLight(self.mainBodyChunk.pos, 150f, 1f, 8, new Color(0f, 1f, 0f)));
            self.room.AddObject(new ShockWave(self.mainBodyChunk.pos, 60f, 0.1f, 8, false));
            if (self.grabbedBy.Count > 0)
            {
                for (int i = 0; i < self.grabbedBy.Count; i++)
                {
                    Creature grabber = self.grabbedBy[i].grabber;
                    if (grabber != null && grabber.grasps != null)
                    {
                        for (int j = 0; j < grabber.grasps.Length; j++)
                        {
                            if (grabber.grasps[j] != null &&
                                grabber.grasps[j].grabbed != null &&
                                grabber.grasps[j].grabbed == self)
                            {
                                grabber.ReleaseGrasp(j);
                            }
                        }
                    }
                    grabber.Stun(10);
                }
            }
            conclusionLevel++;
            Plugin.Log("The number of exempting from death : " + conclusionLevel);
        }
        /*
        public static void Player_Test(On.Player.orig_UpdateMSC orig, Player self)
        {
            orig(self);
            if (Input.GetKey(KeyCode.X) && Input.GetKey(KeyCode.F1))
            {
                self.room.PlaySound(SoundID.SL_AI_Talk_1, 0f, 1f, 1f);
            }
            if (Input.GetKey(KeyCode.X) && Input.GetKey(KeyCode.F2))
            {
                self.room.PlaySound(SoundID.SL_AI_Talk_2, 0f, 1f, 1f);
            }
            if (Input.GetKey(KeyCode.X) && Input.GetKey(KeyCode.F3))
            {
                self.room.PlaySound(SoundID.SL_AI_Talk_3, 0f, 1f, 1f);
            }
            if (Input.GetKey(KeyCode.X) && Input.GetKey(KeyCode.F4))
            {
                self.room.PlaySound(SoundID.SL_AI_Talk_4, 0f, 1f, 1f);
            }
            if (Input.GetKey(KeyCode.X) && Input.GetKey(KeyCode.F5))
            {
                self.room.PlaySound(SoundID.SL_AI_Talk_5, 0f, 1f, 1f);
            }
            if (Input.GetKey(KeyCode.X) && Input.GetKey(KeyCode.F6))
            {
                if (self.room.game.manager.musicPlayer == null)
                {
                    return;
                }
                if (self.room.game.manager.musicPlayer.song == null || !(self.room.game.manager.musicPlayer.song is HalcyonSong))
                {
                    if (self.room.game.StoryCharacter != MoreSlugcatsEnums.SlugcatStatsName.Saint)
                    {
                        self.room.game.manager.musicPlayer.RequestHalcyonSong("NA_19 - Halcyon Memories");
                        return;
                    }
                    self.room.game.manager.musicPlayer.RequestHalcyonSong("NA_19x - Halcyon Memories");
                    return;
                }
                else
                {
                    float[] array = new float[1024];
                    float num = 0f;
                    self.room.game.manager.musicPlayer.song.subTracks[0].source.GetSpectrumData(array, 0, FFTWindow.Hamming);
                    for (int i = 0; i < 1024; i++)
                    {
                        num += array[i];
                    }
                    self.beatScale = Mathf.Clamp(num / 0.25f, 0f, 1f);
                    self.volumeMultiplier = Mathf.Lerp(self.volumeMultiplier, 0.6f, 0.1f);
                    float num2 = 1f;
                    float num3 = 99999f;
                    AbstractCreature firstAlivePlayer = self.room.game.FirstAlivePlayer;
                    if (self.room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null)
                    {
                        if (firstAlivePlayer.realizedCreature.room != null && (firstAlivePlayer.realizedCreature.room.abstractRoom.name == "CL_B04" || firstAlivePlayer.realizedCreature.room.abstractRoom.name == "CL_D08"))
                        {
                            if (firstAlivePlayer.realizedCreature.room.abstractRoom.name == "CL_D08")
                            {
                                num3 = Custom.Dist((firstAlivePlayer.realizedCreature as Player).mainBodyChunk.pos, new Vector2(3649f, 227f));
                            }
                            else
                            {
                                num3 = Custom.Dist((firstAlivePlayer.realizedCreature as Player).mainBodyChunk.pos, new Vector2(256f, 1230f));
                            }
                            num2 = 0.23f;
                        }
                        else if (firstAlivePlayer.realizedCreature.room == null || firstAlivePlayer.realizedCreature.room != self.room)
                        {
                            num3 = 99999f;
                        }
                        else
                        {
                            num3 = Custom.Dist((firstAlivePlayer.realizedCreature as Player).mainBodyChunk.pos, self.firstChunk.pos);
                            if (self.room.CameraViewingPoint(self.firstChunk.pos) != self.room.CameraViewingPoint((firstAlivePlayer.realizedCreature as Player).mainBodyChunk.pos))
                            {
                                num2 = 0.5f;
                            }
                        }
                    }
                    
                    float num4 = Mathf.Max(0f, 1f - num3 / 1700f) * self.volumeMultiplier * num2;
                    if ((self.room.game.manager.musicPlayer.song as HalcyonSong).setVolume != null)
                    {
                        (self.room.game.manager.musicPlayer.song as HalcyonSong).setVolume = new float?(Mathf.Max((self.room.game.manager.musicPlayer.song as HalcyonSong).setVolume.Value, num4));
                        return;
                    }
                    (self.room.game.manager.musicPlayer.song as HalcyonSong).setVolume = new float?(num4);
                    return;
                }
            }
        }*/
    }
}
