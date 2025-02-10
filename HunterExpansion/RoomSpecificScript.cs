using CustomSaveTx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using MoreSlugcats;
using HunterExpansion.CustomEnding;
using HunterExpansion.CustomDream;
using HunterExpansion.CustomEffects;
using HunterExpansion.CustomSave;
using Random = UnityEngine.Random;
using System.Text.RegularExpressions;
using System.IO;
using HunterExpansion.CustomOracle;
using System.Globalization;

namespace HunterExpansion
{
    public static class RoomSpecificScript
    {
        public static void Init()
        {
            On.Room.ctor += Room_ctor;
            On.RoomSpecificScript.AddRoomSpecificScript += RoomSpecificScript_AddRoomSpecificScript;
        }
        
        private static void Room_ctor(On.Room.orig_ctor orig, Room self, RainWorldGame game, World world, AbstractRoom abstractRoom)
        {
            orig.Invoke(self, game, world, abstractRoom);

            if (self.abstractRoom.name == "GATE_SB_OE" ||
                self.abstractRoom.name == "SB_GOR02" ||
                self.abstractRoom.name == "SB_GOR02RIV" ||
                self.abstractRoom.name == "SB_GOR02SAINT")
            {
                self.roomSettings.roomSpecificScript = true;
            }
            if (world != null && world.region != null && world.region.name != "NSH" && TravelCompletedSave.travelCompleted)
            {
                TravelCompletedSave.travelCompleted = true;
            }
        }
        
        private static void RoomSpecificScript_AddRoomSpecificScript(On.RoomSpecificScript.orig_AddRoomSpecificScript orig, Room room)
        {
            orig.Invoke(room);
            AddRoomSpecificScript(room);
        }

        public static void AddRoomSpecificScript(Room room)
        {
            string name = room.abstractRoom.name;
            if (name == "NSH_ROOF03")
            {
                room.AddObject(new NSH_ROOF03GradientGravity(room));
            }
            if (name == "NSH_E01")
            {
                room.AddObject(new NSH_E01GradientGravity(room));
            }
            if (name == "NSH_E01")
            {
                room.AddObject(new NSH_E01GradientGravity(room));
            }
            if (room.game.IsStorySession)
            {
                if (name == "SB_GOR02")
                {
                    room.AddObject(new SB_GOR_AquamarinePearl(room));
                }
                if (name == "SB_GOR02RIV")
                {
                    room.AddObject(new SB_GOR_AquamarinePearl(room));
                    room.AddObject(new SB_GOR_Warp(room, "GATE_SB_NSH"));
                }
                if (name == "SB_GOR02SAINT")
                {
                    room.AddObject(new SB_GOR_AquamarinePearl(room));
                    room.AddObject(new SB_GOR_Warp(room, "GATE_SB_NSH"));
                }
                if (name == "GATE_SB_OE")
                {
                    room.AddObject(new SetPlayerPosAfterWarpTo_GATE_SB_OE(room));
                    room.AddObject(new GATE_SB_OE_Warp(room, null));
                }
                if (name == "GATE_SB_NSH")
                {
                    room.AddObject(new GATE_SB_NSH_Warp_Back(room, room.snow ? "SB_GOR02SAINT" : "SB_GOR02RIV"));
                    room.AddObject(new GATE_SB_NSH_Warp(room, "GATE_NSH_DGL"));
                }
                if (name == "GATE_NSH_DGL")
                {
                    room.AddObject(new SetPlayerPosAfterWarpTo_GATE_NSH_DGL(room));
                    room.AddObject(new GATE_NSH_DGL_Warp(room, "GATE_OE_SU"));
                }
                if (name == "GATE_OE_SU")
                {
                    room.AddObject(new SetPlayerPosAfterWarpTo_GATE_OE_SU(room));
                }
            }
        }

        #region 房间传送
        public class GATE_SB_OE_Warp : RoomSpecificScriptWarp
        {
            public GATE_SB_OE_Warp(Room room, string newRoomName) : base(room, newRoomName)
            {
                this.room = room;
                this.newRoom = newRoomName;
                this.newRegion = "NSH";
                this.newIntPos = new IntVector2(24, 34);
                this.upcomingProcess = ProcessManager.ProcessID.Dream;//不用考虑红猫，因为红猫没有使用GoToGameOver

                EndingSession.openGate = false;
                EndingSession.openGateName = "";
                EndingSession.openCount = 0;
                EndingSession.noGrabbedCount = 0;

                this.blackRect = null;
                this.isControled = false;
            }

            public override void Update(bool eu)
            {
                //确定传送房间
                if (this.newRoom == null)
                {
                    this.newRoom = this.room.game.session.characterStats.name == Plugin.SlugName ? "NSH_AI" : "GATE_NSH_DGL";
                    this.newPos = this.room.game.session.characterStats.name == Plugin.SlugName ? new Vector2(-1f, -1f) : new Vector2(661f, 200f);
                }

                //播放结局cg
                if (player != null && player == this.room.game.Players[0].realizedCreature as Player)
                {
                    if (isBufferZone)
                    {
                        if (!isControled)
                        {
                            isControled = true;
                            obj = new CutsceneHunter(this.room);
                            this.room.AddObject(obj);
                        }
                    }
                }

                base.Update(eu);
            }

            public override void Reset()
            {
                isControled = false;
                EndingSession.openGate = false;
                EndingSession.openCount = 0;
                if (obj != null)
                {
                    player.room.RemoveObject(obj);
                    obj = null;
                }
                base.Reset();
            }

            public override void GoToGameOver(RainWorldGame self)
            {
                if (this.room.game.session.characterStats.name == Plugin.SlugName)
                {
                    if(self.manager.upcomingProcess == null)//不能和上面的if合并，因为下面的else
                    {/*
                        RainWorldGame game = this.room.game;
                        for (int l = 0; l < game.world.GetAbstractRoom(player.abstractCreature.pos).creatures.Count; l++)
                        {
                            if (ModManager.MSC && game.world.GetAbstractRoom(player.abstractCreature.pos).creatures[l].creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
                            {
                                PlayerNPCState slugpup = game.world.GetAbstractRoom(player.abstractCreature.pos).creatures[l].state as PlayerNPCState;
                                Plugin.Log("Slugpup foodInStomach (old): " + slugpup.foodInStomach);
                                Plugin.Log("Slugpup MaxFoodInStomach: " + (slugpup.player.realizedCreature as Player).MaxFoodInStomach);
                                slugpup.foodInStomach = (slugpup.player.realizedCreature as Player).MaxFoodInStomach;
                                Plugin.Log("Help to feed slugpup! creature index: " + l);
                                Plugin.Log("Slugpup foodInStomach (new): " + slugpup.foodInStomach);
                            }
                        }*/
                        this.room.game.GoToRedsGameOver();
                    }
                }
                else
                {
                    TravelCompletedSave.travelCompleted = false;
                    SaveState saveState = self.GetStorySession.saveState;
                    Plugin.Log("{0}: Go to NSH!", saveState.saveStateNumber.ToString());
                    saveState.cycleNumber += 15;
                    saveState.deathPersistentSaveData.karma = saveState.deathPersistentSaveData.karmaCap;
                    self.manager.statsAfterCredits = false;
                    base.GoToGameOver(self);
                }
            }

            public override bool isBufferZone
            {
                get
                {
                    if (player != null && player.firstChunk.pos.x > 150f && player.firstChunk.pos.x < 450f && PearlFixedSave.pearlFixed && EndingSession.openGate)
                    {
                        return true;
                    }
                    return false;
                }
                set
                {
                }
            }

            public override bool isWarpZone
            {
                get
                {
                    if (player != null && this.player.DangerPos.x <= 200f && 
                        this.blackRect != null && this.blackRect.alpha == 1f && 
                        PearlFixedSave.pearlFixed && EndingSession.openGate)
                    {
                        return true;
                    }
                    return false;
                }
                set
                {
                }
            }

            public override Player player
            {
                get
                {
                    AbstractCreature firstAlivePlayer = this.room.game.FirstAlivePlayer;
                    if (this.room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null)
                    {
                        return (firstAlivePlayer.realizedCreature as Player);
                    }
                    return null;
                }
                set
                {
                    player = value;
                }
            }

            private bool isControled = false;
            private CutsceneHunter obj = null;
        }

        public class GATE_SB_NSH_Warp : RoomSpecificScriptWarp
        {
            public GATE_SB_NSH_Warp(Room room, string newRoomName) : base(room, newRoomName)
            {
                this.newPos = new Vector2(661f, 200f);
                this.newRegion = "NSH";
                this.upcomingProcess = ProcessManager.ProcessID.Dream;

                this.fixedPearl = null;
                this.openGate = false;
                this.openGateName = "";
                this.openCount = 0;
                this.noGrabbedCount = 0;
                this.isControled = false;
                this.fixedPearl = null;
            }

            public override void Update(bool eu)
            {
                base.Update(eu);
                if (!this.room.regionGate.EnergyEnoughToOpen)// && openGate
                {
                    if (room.game.world.region.name == "NSH" && room.abstractRoom.name == "GATE_SB_NSH")
                    {
                        if (isBufferZone)
                        {
                            if (!isControled)
                            {
                                isControled = true;
                                obj = new CutsceneHunter(player.room);
                                player.room.AddObject(obj);
                            }
                        }
                    }
                    else
                    {
                        if ((player.firstChunk.pos.x <= 380f || player.firstChunk.pos.x >= 780f) && !this.room.regionGate.EnergyEnoughToOpen)
                        {
                            openGate = false;
                            openCount = 0;
                        }
                    }
                }
            }

            public override void Reset()
            {
                isControled = false;
                openGate = false;
                openCount = 0;
                if (obj != null)
                {
                    this.room.RemoveObject(obj);
                    obj.Destroy();
                    obj = null;
                }
                base.Reset();
            }

            public override void GoToGameOver(RainWorldGame self)
            {
                TravelCompletedSave.travelCompleted = false;
                SaveState saveState = self.GetStorySession.saveState;
                saveState.cycleNumber += 15;
                saveState.deathPersistentSaveData.karma = saveState.deathPersistentSaveData.karmaCap;
                self.manager.statsAfterCredits = false;
                base.GoToGameOver(self);
            }

            public DataPearl fixedPearl;
            public List<GreenSparks> greenSparks;
            private bool isControled = false;
            public string openGateName = "";
            public bool openGate = false;
            public int openCount = 0;
            public int noGrabbedCount = 0;
            private CutsceneHunter obj = null;

            public override bool isBufferZone
            {
                get
                {
                    if (player != null && player.firstChunk.pos.x > 200f && player.firstChunk.pos.x < 400f)
                    {
                        return true;
                    }
                    return false;
                }
                set
                {
                }
            }

            public override bool isWarpZone
            {
                get
                {
                    if (player != null && player.firstChunk.pos.x <= 200f)
                    {
                        return true;
                    }
                    return false;
                }
                set
                {
                }
            }

            public override Player player
            {
                get
                {
                    AbstractCreature firstAlivePlayer = this.room.game.FirstAlivePlayer;
                    if (this.room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null)
                    {
                        return (firstAlivePlayer.realizedCreature as Player);
                    }
                    return null;
                }
                set
                {
                }
            }
        }

        public class GATE_NSH_DGL_Warp : RoomSpecificScriptWarp
        {
            public GATE_NSH_DGL_Warp(Room room, string newRoomName) : base(room, newRoomName)
            {
                this.newPos = new Vector2(661f, 200f);
                this.upcomingProcess = ProcessManager.ProcessID.Dream;

                this.noGrabbedCount = 0;
                this.isControled = false;
                this.hasSetDestination = false;

                bool isImpossibleToReachOE = IsImpossibleToReachOE(room);
                this.newRoom = isImpossibleToReachOE ? "GATE_OE_SU" : "GATE_SB_OE";
                this.newRegion = isImpossibleToReachOE ? "SU" : "SB";
                RoomSpecificScript.shouldGoRegionName = isImpossibleToReachOE ? "SU" : "SB";
            }

            public override void Update(bool eu)
            {
                base.Update(eu);
                if (player != null && !hasSetDestination)
                {
                    bool isImpossibleToReachOE = IsImpossibleToReachOE(room);
                    this.newRoom = isImpossibleToReachOE ? "GATE_OE_SU" : "GATE_SB_OE";
                    this.newRegion = isImpossibleToReachOE ? "SU" : "SB";
                    this.hasSetDestination = true;
                }
                if (player != null && !this.room.regionGate.EnergyEnoughToOpen)// && openGate
                {
                    if (room.game.world.region.name == "SU" || room.game.world.region.name == "SB")
                    {
                        if (isBufferZone)
                        {
                            if (!isControled)
                            {
                                isControled = true;
                                obj = new CutsceneHunter(player.room);
                                player.room.AddObject(obj);
                            }
                        }
                    }
                }
            }

            public override void Reset()
            {
                isControled = false;
                if (obj != null)
                {
                    this.room.RemoveObject(obj);
                    obj.Destroy();
                    obj = null;
                }
                base.Reset();
            }

            public override void GoToGameOver(RainWorldGame self)
            {
                TravelCompletedSave.travelCompleted = false;
                SaveState saveState = self.GetStorySession.saveState;
                Plugin.Log("{0}: Leave NSH!", saveState.saveStateNumber.ToString());
                saveState.cycleNumber += 15;
                saveState.deathPersistentSaveData.karma = saveState.deathPersistentSaveData.karmaCap;
                self.manager.statsAfterCredits = false;
                base.GoToGameOver(self);
            }

            private bool isControled = false;
            private bool hasSetDestination = false;
            private int noGrabbedCount = 0;
            private CutsceneHunter obj = null;

            public override bool isBufferZone
            {
                get
                {
                    if (player != null && player.firstChunk.pos.x > 150f && player.firstChunk.pos.x < 350f)
                    {
                        return true;
                    }
                    return false;
                }
                set
                {
                }
            }

            public override bool isWarpZone
            {
                get
                {
                    if (player != null && player.firstChunk.pos.x <= 150f)
                    {
                        return true;
                    }
                    return false;
                }
                set
                {
                }
            }

            public override Player player
            {
                get
                {
                    AbstractCreature firstAlivePlayer = this.room.game.FirstAlivePlayer;
                    if (this.room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null)
                    {
                        return (firstAlivePlayer.realizedCreature as Player);
                    }
                    return null;
                }
                set
                {
                }
            }
        }

        public class SB_GOR_Warp : RoomSpecificScriptWarp
        {
            public SB_GOR_Warp(Room room, string newRoomName) : base(room, newRoomName)
            {
                this.newIntPos = new IntVector2(50, 80);
            }

            public override void Update(bool eu)
            {
                base.Update(eu);

                if (this.isBufferZone)// && !(this.room.game.session as StoryGameSession).saveState.miscWorldSaveData.pebblesEnergyTaken
                {
                    this.player.firstChunk.vel.y /= 2f;
                }
                else if (blackRect != null && this.player.DangerPos.y >= 0f)
                {
                    blackRect.alpha = Mathf.Max(0f, blackRect.alpha - 0.03f);
                }
                if (this.player.DangerPos.y < -300f)
                {
                    this.player.firstChunk.vel.y = 0f;
                }
            }

            public override bool isBufferZone
            {
                get
                {
                    if (player != null && this.player.DangerPos.y < 0f && (this.player.DangerPos.x < 800f || (blackRect != null && blackRect.alpha > 0f)))
                    {
                        return true;
                    }
                    return false;
                }
                set
                {
                }
            }

            public override bool isWarpZone
            {
                get
                {
                    if (player != null && this.player.DangerPos.y < -300f && blackRect != null && blackRect.alpha >= 0.99f)
                    {
                        return true;
                    }
                    return false;
                }
                set
                {
                }
            }

            public override Player player
            {
                get
                {
                    AbstractCreature firstAlivePlayer = this.room.game.FirstAlivePlayer;
                    if (this.room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null)
                    {
                        return (firstAlivePlayer.realizedCreature as Player);
                    }
                    return null;
                }
                set
                {
                }
            }
        }

        public class GATE_SB_NSH_Warp_Back : RoomSpecificScriptWarp
        {
            public GATE_SB_NSH_Warp_Back(Room room, string newRoomName) : base(room, newRoomName)
            {
                this.counter = 0;
                this.newPos = new Vector2(481f, 310f);
            }

            public override void Update(bool eu)
            {
                base.Update(eu);
                counter++;
                if (isBufferZone && blackRect != null)// && !(this.room.game.session as StoryGameSession).saveState.miscWorldSaveData.pebblesEnergyTaken
                {
                    if (counter <= 5)
                    {
                        blackRect.alpha = 1f;
                    }
                    if (this.player.firstChunk.vel.y > 1f && counter >= 200f)
                    {
                        blackRect.alpha = Mathf.Min(1f, blackRect.alpha + 0.03f);
                        this.player.firstChunk.vel.y += 5f;
                    }
                    else if (counter < 200f)
                    {
                        blackRect.alpha = Mathf.Max(0f, blackRect.alpha - 0.03f);
                        this.player.firstChunk.vel.y -= 2f;
                    }
                }
                else if (blackRect != null && this.player.DangerPos.y <= 1800f)
                {
                    blackRect.alpha = Mathf.Max(0f, blackRect.alpha - 0.03f);
                }

                if (this.player.DangerPos.y > 1250f && this.player.DangerPos.x > 800f)
                {
                    if (room.game.cameras[0].followAbstractCreature == player.abstractCreature)
                    {
                        room.game.cameras[0].MoveCamera(player.room, 2);
                        //room.game.cameras[0].GetCameraBestIndex();
                    }
                }
            }

            public override void Reset()
            {
                this.counter = 0;
                base.Reset();
            }

            int counter;

            public override bool isBufferZone
            {
                get
                {
                    if (player != null && this.player.DangerPos.y > 1250f && this.player.DangerPos.x > 1000f && this.player.DangerPos.y <= 1800f)
                    {
                        return true;
                    }
                    return false;
                }
                set
                {
                }
            }

            public override bool isWarpZone
            {
                get
                {
                    if (player != null && this.player.DangerPos.y > 1800f && blackRect != null && blackRect.alpha >= 0.99f)
                    {
                        return true;
                    }
                    return false;
                }
                set
                {
                }
            }

            public override Player player
            {
                get
                {
                    AbstractCreature firstAlivePlayer = this.room.game.FirstAlivePlayer;
                    if (this.room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null)
                    {
                        return (firstAlivePlayer.realizedCreature as Player);
                    }
                    return null;
                }
                set
                {
                }
            }
        }

        public abstract class RoomSpecificScriptWarp : UpdatableAndDeletable
        {
            public RoomSpecificScriptWarp(Room room, string newRoomName)
            {
                this.room = room;
                this.newRoom = newRoomName;
                this.oldRegion = room.world.region.name;
                this.newRegion = room.world.region.name;
                /*
                string[] regions = Regex.Split(newRoomName, "_");
                regions = regions.Where((element, index) => index != 0).ToArray();
                for (int i = 0; i < regions.Length; i++)
                {
                    if (regions[i] != this.room.world.region.name)
                    this.newRegion = regions[i];
                }
                if(this.newRegion == null)
                {
                    this.newRegion = "";
                    Plugin.Log("Error! RoomSpecificScript Warp Can Not Find New Region!");
                }*/

                this.newPos = new Vector2(-1f, -1f);
                this.upcomingProcess = ProcessManager.ProcessID.Game;

                this.blackRect = null;
            }

            public override void Update(bool eu)
            {
                base.Update(eu);
                if (player != null)// && openGate
                {
                    if ((oldRegion == newRegion && room.game.world.region.name == this.newRegion) ||
                        (oldRegion != newRegion && room.game.world.region.name == this.newRegion && !this.room.regionGate.EnergyEnoughToOpen))
                    {
                        if (isBufferZone)
                        {
                            if (blackRect == null)
                            {
                                Plugin.Log("In Buffer Zone!");
                                if (room != null && room.game != null && room.game.manager != null && room.game.manager.musicPlayer != null)
                                    //下面这一行在结局时NullReferenceException: Object reference not set to an instance of an object
                                    room.game.manager.musicPlayer.FadeOutAllSongs(40f);
                                //添加黑幕
                                blackRect = new FSprite("pixel");
                                blackRect.scaleX = Screen.width * 1.1f;
                                blackRect.scaleY = Screen.height * 1.1f;
                                blackRect.x = (Screen.width * 1.1f) / 2f;
                                blackRect.y = (Screen.height * 1.1f) / 2f;
                                blackRect.color = Color.black;
                                blackRect.alpha = 0f;
                                Futile.stage.AddChild(blackRect);
                                blackRect.MoveToFront();
                            }
                            blackRect.alpha = Mathf.Min(1f, blackRect.alpha + 0.03f);
                            Plugin.Log("blackRect.alpha: " + blackRect.alpha);
                        }
                        if (isWarpZone && player.room != null && player.room == this.room && this.room.game.manager.upcomingProcess == null)
                        {
                            Plugin.Log("Try to Warp!");
                            this.TryWarp();
                            this.GoToGameOver(this.room.game);
                            return;
                        }/*
                        if (player.room != null && player.room != this.room)
                        {
                            blackRect.alpha = Mathf.Max(0f, blackRect.alpha - 0.03f);
                        }*/
                    }
                }
            }

            public override void Destroy()
            {
                this.Reset();
                base.Destroy();
            }

            public virtual void Reset()
            {
                if (blackRect != null)
                {
                    blackRect.alpha = 0f;
                    blackRect.RemoveFromContainer();
                    blackRect = null;
                }
            }

            public virtual void GoToGameOver(RainWorldGame self)
            {
                SaveState saveState = self.GetStorySession.saveState;
                if (self.world.region != null &&
                    self.manager.upcomingProcess == null &&
                    this.upcomingProcess != ProcessManager.ProcessID.Game)
                {
                    //帮忙喂一下猫崽
                    RainWorldGame game = this.room.game;
                    for (int l = 0; l < game.world.GetAbstractRoom(player.abstractCreature.pos).creatures.Count; l++)
                    {
                        if (ModManager.MSC && game.world.GetAbstractRoom(player.abstractCreature.pos).creatures[l].creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
                        {
                            PlayerNPCState slugpup = game.world.GetAbstractRoom(player.abstractCreature.pos).creatures[l].state as PlayerNPCState;
                            slugpup.foodInStomach = (slugpup.player.realizedCreature as Player).MaxFoodInStomach;
                        }
                    }
                    //IL_167:
                    if (self.manager.musicPlayer != null)
                    {
                        self.manager.musicPlayer.FadeOutAllSongs(20f);
                    }
                    Plugin.Log("{0}: Warp to {1}!", saveState.saveStateNumber.ToString(), newRoom);
                    AbstractCreature abstractCreature = self.FirstAlivePlayer;
                    if (abstractCreature == null)
                    {
                        abstractCreature = self.FirstAnyPlayer;
                    }
                    if (this.upcomingProcess == ProcessManager.ProcessID.Dream && this.room.game.IsStorySession)
                    {
                        if (saveState.dreamsState == null)
                        {
                            saveState.dreamsState = new DreamsState();//让没有梦的蛞蝓猫有梦
                        }
                        saveState.dreamsState.EndOfCycleProgress(self.GetStorySession.saveState, self.world.region.name, self.world.GetAbstractRoom(abstractCreature.pos).name);
                    }
                    SaveState.forcedEndRoomToAllowwSave = abstractCreature.Room.name;
                    saveState.BringUpToDate(self);
                    SaveState.forcedEndRoomToAllowwSave = "";
                    RainWorldGame.ForceSaveNewDenLocation(self, newRoom, true);
                    self.manager.rainWorld.progression.SaveWorldStateAndProgression(false);
                    self.manager.rainWorld.progression.SaveProgressionAndDeathPersistentDataOfCurrentState(false, false);
                    if (this.upcomingProcess == ProcessManager.ProcessID.SlideShow)
                    {
                        self.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SlideShow);
                    }
                    else if (this.upcomingProcess == ProcessManager.ProcessID.Dream)
                    {
                        self.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Dream);
                    }
                    else if (this.upcomingProcess == ProcessManager.ProcessID.MainMenu)
                    {
                        self.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
                    }
                    else
                    {
                        self.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
                    }
                }
            }

            public virtual void TryWarp()
            {
                //传送
                AbstractRoom room = player.abstractCreature.world.GetAbstractRoom(newRoom);
                if (ModManager.CoopAvailable)
                {
                    foreach (AbstractCreature abstractCreature in this.room.game.Players)
                    {
                        if (abstractCreature.Room != room)
                        {
                            try
                            {
                                WarpWithinTheSameRegion(abstractCreature.realizedCreature as Player, newRoom, newIntPos.x, newIntPos.y);
                            }
                            catch (Exception arg)
                            {
                                Plugin.Log(string.Format("Could not warp player {0} [{1}]", abstractCreature, arg), false);
                            }
                        }
                    }
                }
                else
                {
                    try
                    {
                        WarpWithinTheSameRegion(player, newRoom, newIntPos.x, newIntPos.y);
                    }
                    catch (Exception arg)
                    {
                        Plugin.Log(string.Format("Could not warp player {0} [{1}]", player.abstractCreature, arg), false);
                    }
                }
                Plugin.Log("Warp and End!");
            }

            public string newRoom;
            public string oldRegion;
            public string newRegion;
            public Vector2 newPos;
            public IntVector2 intPos;
            public FSprite blackRect;
            public ProcessManager.ProcessID upcomingProcess;

            public virtual bool isBufferZone
            {
                get
                {
                    if (player != null && player.firstChunk.pos.x > 200f && player.firstChunk.pos.x < 400f)
                    {
                        return true;
                    }
                    return false;
                }
                set
                {
                    isBufferZone = value;
                }
            }

            public virtual bool isWarpZone
            {
                get
                {
                    if (player != null && player.firstChunk.pos.x <= 200f)
                    {
                        return true;
                    }
                    return false;
                }
                set
                {
                    isWarpZone = value;
                }
            }

            public virtual Player player
            {
                get
                {
                    return this.room.game.Players[0].realizedCreature as Player;
                }
                set
                {
                    player = value;
                }
            }

            public IntVector2 newIntPos
            {
                get
                {
                    if (newPos != new Vector2(-1f, -1f))
                        return Room.StaticGetTilePosition(newPos);
                    else if (intPos != null)
                        return intPos;
                    else
                        return new IntVector2(20, 20);
                }
                set
                {
                    intPos = value;
                }
            }
        }

        public static void WarpWithinTheSameRegion(Player player, string newRoomName, int x, int y)
        {
            RainWorldGame game = player.abstractCreature.world.game;
            /*//这是跨区域传送
            if (data.name.Split(new char[]{'_'})[0] != player.abstractCreature.world.region.name)
            {
                for (int i = 0; i < game.AlivePlayers.Count; i++)
                {
                    AbstractCreature absPly = game.AlivePlayers[i];
                    if (absPly != null)
                    {
                        AbstractRoom oldRoom = absPly.Room;
                        try
                        {
                            World oldWorld = game.overWorld.activeWorld;
                            game.overWorld.LoadWorld(data.name.Split(new char[]
                            {
                                '_'
                            })[0], game.overWorld.PlayerCharacterNumber, false);
                            this.WorldLoaded(game, oldRoom, oldWorld, data.name, new IntVector2(data.x, data.y));
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }
                    }
                    TimeBackCreatureModule module;
                    bool flag3 = TimeBackHooks.creatureModules.TryGetValue(absPly, out module);
                    if (flag3)
                    {
                        module.Clear();
                    }
                }
            }
            else
            {*/
            AbstractRoom oldRoom = player.room.abstractRoom;
            AbstractRoom room = player.abstractCreature.world.GetAbstractRoom(newRoomName);
            AbstractPhysicalObject stomachObject = null;
            if (player != null && player.objectInStomach != null)
            {
                stomachObject = player.objectInStomach;
            }/*
            //放下矛
            if (player.spearOnBack != null && player.spearOnBack.spear != null)
            {
                player.spearOnBack.DropSpear();
            }
            //放下猫崽
            if ((ModManager.MSC || ModManager.CoopAvailable) && player.slugOnBack != null && player.slugOnBack.slugcat != null)
            {
                player.slugOnBack.DropSlug();
            }*/
            //更换房间
            if (room.realizedRoom == null)
            {
                room.RealizeRoom(game.world, game);

                if (game.world.loadingRooms.Count > 0)
                {
                    for (int n = 0; n < 1; n++)
                    {
                        for (int num = game.world.loadingRooms.Count - 1; num >= 0; num--)
                        {
                            if (game.world.loadingRooms[num].done)
                            {
                                game.world.loadingRooms.RemoveAt(num);
                            }
                            else
                            {
                                while (!game.world.loadingRooms[num].done)
                                {
                                    game.world.loadingRooms[num].Update();
                                }
                            }
                        }
                    }
                }
            }
            if (player.room.abstractRoom != room)//  && !player.isSlugpup
            {
                if (player.grasps != null)
                {
                    for (int g = 0; g < player.grasps.Length; g++)
                    {
                        if (player.grasps[g] != null && player.grasps[g].grabbed != null && !player.grasps[g].discontinued &&
                            player.grasps[g].grabbed is Creature && (!(player.grasps[g].grabbed is Player) || !(player.grasps[g].grabbed as Player).isSlugpup))// 
                        {
                            player.ReleaseGrasp(g);
                        }
                    }
                }
                room.realizedRoom.aimap.NewWorld(room.index);
                if (player.abstractCreature.realizedCreature != null)
                {
                    oldRoom.realizedRoom.RemoveObject(player.abstractCreature.realizedCreature);
                }
                Plugin.Log("Old Room Before Warp: " + player.abstractCreature.world.GetAbstractRoom(player.abstractCreature.pos).name);
                Plugin.Log("New Room After Warp: " + player.abstractCreature.world.GetAbstractRoom(new WorldCoordinate(room.index, x, y, 0)).name);
                player.abstractCreature.Move(new WorldCoordinate(room.index, x, y, 0));
                /*
                if (player.abstractCreature.creatureTemplate.AI && player.abstractCreature.abstractAI.RealAI != null && player.abstractCreature.abstractAI.RealAI.pathFinder != null)
                {
                    player.abstractCreature.abstractAI.SetDestination(QuickConnectivity.DefineNodeOfLocalCoordinate(player.abstractCreature.abstractAI.destination, player.abstractCreature.world, player.abstractCreature.creatureTemplate));
                    player.abstractCreature.abstractAI.timeBuffer = 0;
                    if (player.abstractCreature.abstractAI.destination.room == player.abstractCreature.pos.room && player.abstractCreature.abstractAI.destination.abstractNode == player.abstractCreature.pos.abstractNode)
                    {
                        player.abstractCreature.abstractAI.path.Clear();
                    }
                    else
                    {
                        List<WorldCoordinate> list = player.abstractCreature.abstractAI.RealAI.pathFinder.CreatePathForAbstractreature(player.abstractCreature.abstractAI.destination);
                        if (list != null)
                        {
                            player.abstractCreature.abstractAI.path = list;
                        }
                        else
                        {
                            player.abstractCreature.abstractAI.FindPath(player.abstractCreature.abstractAI.destination);
                        }
                    }
                    player.abstractCreature.abstractAI.RealAI = null;
                }
                */
                /*
                player.PlaceInRoom(room.realizedRoom);
                player.abstractCreature.ChangeRooms(new WorldCoordinate(room.index, x, y, 0));
                room.AddEntity(player.abstractCreature);*/
                player.abstractCreature.RealizeInRoom();

                //矛
                Player.SpearOnBack spearOnBack = player.spearOnBack;
                if (((spearOnBack != null) ? spearOnBack.spear : null) != null)
                {
                    player.spearOnBack.spear.PlaceInRoom(room.realizedRoom);
                    player.spearOnBack.spear.room = player.room;
                }
                //猫崽
                Player.SlugOnBack slugOnBack = player.slugOnBack;
                if (((slugOnBack != null) ? slugOnBack.slugcat : null) != null)
                {
                    AbstractCreature slug = player.slugOnBack.slugcat.abstractCreature;
                    if (slug.abstractAI != null)
                    {
                        if (player.abstractCreature.world != slug.world)
                        {
                            player.slugOnBack.DropSlug();
                            slug.realizedCreature = null;
                            slug.world = player.abstractCreature.world;
                            slug.abstractAI.NewWorld(player.room.world);
                            slug.ChangeRooms(new WorldCoordinate(room.index, x, y, 0));
                            player.slugOnBack.SlugToBack(slug.realizedCreature as Player);
                        }
                        player.slugOnBack.slugcat.AI.NewRoom(room.realizedRoom);
                    }
                    player.slugOnBack.slugcat.PlaceInRoom(room.realizedRoom);
                    player.slugOnBack.slugcat.room = player.room;
                }
                if (stomachObject != null && player.objectInStomach == null)
                {
                    player.objectInStomach = stomachObject;
                }/*
                for (int i3 = game.shortcuts.transportVessels.Count - 1; i3 >= 0; i3--)
                {
                    if (!game.overWorld.activeWorld.region.IsRoomInRegion(game.shortcuts.transportVessels[i3].room.index))
                    {
                        game.shortcuts.transportVessels.RemoveAt(i3);
                    }
                }
                for (int i4 = game.shortcuts.betweenRoomsWaitingLobby.Count - 1; i4 >= 0; i4--)
                {
                    if (!game.overWorld.activeWorld.region.IsRoomInRegion(game.shortcuts.betweenRoomsWaitingLobby[i4].room.index))
                    {
                        game.shortcuts.betweenRoomsWaitingLobby.RemoveAt(i4);
                    }
                }
                for (int i5 = game.shortcuts.borderTravelVessels.Count - 1; i5 >= 0; i5--)
                {
                    if (!game.overWorld.activeWorld.region.IsRoomInRegion(game.shortcuts.borderTravelVessels[i5].room.index))
                    {
                        game.shortcuts.borderTravelVessels.RemoveAt(i5);
                    }
                }*/
                /*
                if (player.abstractCreature != null && player.abstractCreature.creatureTemplate.AI)
                {
                    player.abstractCreature.abstractAI.NewWorld(newWorld);
                    player.abstractCreature.InitiateAI();
                    player.abstractCreature.abstractAI.RealAI.NewRoom(newRoom.realizedRoom);
                    if (player.abstractCreature.creatureTemplate.type == CreatureTemplate.Type.Overseer && (player.abstractCreature.abstractAI as OverseerAbstractAI).playerGuide)
                    {
                        MethodInfo kpginw = typeof(OverWorld).GetMethod("KillPlayerGuideInNewWorld", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        kpginw.Invoke(game.overWorld, new object[]
                        {
                            newWorld,
                            player.abstractCreature
                        });
                    }
                }*/
                if (game.cameras[0].followAbstractCreature == player.abstractCreature)
                {
                    game.cameras[0].virtualMicrophone.AllQuiet();
                    game.cameras[0].MoveCamera(player.room, 0);
                    game.cameras[0].GetCameraBestIndex();
                }
            }
            else
            {
                player.SuperHardSetPosition(new Vector2((float)(x * 20), (float)(y * 20)) + new Vector2(10f, 10f));
            }
            //}
        }
        #endregion

        #region NSH内部重力设置
        public class NSH_ROOF03GradientGravity : UpdatableAndDeletable
        {
            public NSH_ROOF03GradientGravity(Room room)
            {
                this.room = room;
            }

            public override void Update(bool eu)
            {
                base.Update(eu);
                AbstractCreature firstAlivePlayer = this.room.game.FirstAlivePlayer;
                if (this.room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null && firstAlivePlayer.realizedCreature.room == this.room)
                {
                    float value = Vector2.Distance((firstAlivePlayer.realizedCreature as Player).firstChunk.pos, new Vector2(4160f, 200f));
                    this.room.gravity = Mathf.InverseLerp(0f, 3000f, value);
                }
            }
        }

        public class NSH_E01GradientGravity : UpdatableAndDeletable
        {
            public NSH_E01GradientGravity(Room room)
            {
                this.room = room;
            }

            public override void Update(bool eu)
            {
                base.Update(eu);
                AbstractCreature firstAlivePlayer = this.room.game.FirstAlivePlayer;
                if (this.room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null && firstAlivePlayer.realizedCreature.room == this.room)
                {
                    //房间中轴：x = 2529
                    if ((firstAlivePlayer.realizedCreature as Player).firstChunk.pos.x > 2529)
                    {
                        this.room.gravity = 0f;
                    }
                    else
                    {

                        float value = Vector2.Distance((firstAlivePlayer.realizedCreature as Player).firstChunk.pos, new Vector2(2529f, 329f));
                        this.room.gravity = 0.65f * Mathf.InverseLerp(0f, 2000f, value);
                    }
                }
            }
        }
        #endregion

        #region 设置玩家在传送后的位置
        public class SetPlayerPosAfterWarpTo_GATE_SB_OE : UpdatableAndDeletable
        {
            public SetPlayerPosAfterWarpTo_GATE_SB_OE(Room room)
            {
                this.room = room;
                this.player = this.room.game.Players[0].realizedCreature as Player;
            }

            public override void Update(bool eu)
            {
                base.Update(eu);
                //强行设置玩家位置
                Vector2 vector = new Vector2(661f, 200f);
                IntVector2 pos = Room.StaticGetTilePosition(vector);
                if (player == null)
                    this.player = this.room.game.Players[0].realizedCreature as Player;
                if (this.room.game.GetStorySession.saveState.denPosition == this.room.abstractRoom.name &&
                    this.room.world.rainCycle.timer < 400 &&
                    this.player != null)
                {
                    if (ModManager.CoopAvailable)
                    {
                        List<PhysicalObject> list = (from x in this.room.physicalObjects.SelectMany((List<PhysicalObject> x) => x)
                                                     where x is Player
                                                     select x).ToList<PhysicalObject>();
                        int num = list.Count<PhysicalObject>();
                        foreach (AbstractCreature abstractCreature in this.room.game.Players)
                        {
                            (abstractCreature.realizedCreature as Player).SuperHardSetPosition(vector);
                            abstractCreature.pos = this.room.ToWorldCoordinate(vector);
                        }
                    }
                    else
                    {
                        player.SuperHardSetPosition(vector);
                        player.abstractCreature.pos = this.room.ToWorldCoordinate(vector);
                    }
                    //让业力门没能量
                    if (this.room.regionGate is WaterGate)
                        (this.room.regionGate as WaterGate).waterLeft = 0.5f;
                    if (this.room.regionGate is ElectricGate)
                        (this.room.regionGate as ElectricGate).batteryLeft = 0.5f;
                    this.Destroy();
                }

            }

            public override void Destroy()
            {
                base.Destroy();
            }

            public Player player;
        }

        public class SetPlayerPosAfterWarpTo_GATE_OE_SU : UpdatableAndDeletable
        {
            public SetPlayerPosAfterWarpTo_GATE_OE_SU(Room room)
            {
                this.room = room;
                this.player = this.room.game.Players[0].realizedCreature as Player;
            }

            public override void Update(bool eu)
            {
                base.Update(eu);
                //强行设置玩家位置
                Vector2 vector = this.room.world.region.name == "SU" ? new Vector2(668f, 200f) : new Vector2(293f, 200f);
                IntVector2 pos = Room.StaticGetTilePosition(vector);
                if (player == null)
                    this.player = this.room.game.Players[0].realizedCreature as Player;
                if (this.room.game.GetStorySession.saveState.denPosition == this.room.abstractRoom.name &&
                    this.room.world.rainCycle.timer < 400 &&
                    this.player != null)
                {
                    if (ModManager.CoopAvailable)
                    {
                        List<PhysicalObject> list = (from x in this.room.physicalObjects.SelectMany((List<PhysicalObject> x) => x)
                                                     where x is Player
                                                     select x).ToList<PhysicalObject>();
                        int num = list.Count<PhysicalObject>();
                        foreach (AbstractCreature abstractCreature in this.room.game.Players)
                        {
                            (abstractCreature.realizedCreature as Player).SuperHardSetPosition(vector);
                            abstractCreature.pos = this.room.ToWorldCoordinate(vector);
                        }
                        this.Destroy();
                    }
                    else
                    {
                        player.SuperHardSetPosition(vector);
                        player.abstractCreature.pos = this.room.ToWorldCoordinate(vector);
                        this.Destroy();
                    }
                }

            }

            public override void Destroy()
            {
                base.Destroy();
            }

            public Player player;
        }

        public class SetPlayerPosAfterWarpTo_GATE_NSH_DGL : UpdatableAndDeletable
        {
            public SetPlayerPosAfterWarpTo_GATE_NSH_DGL(Room room)
            {
                this.room = room;
                this.player = this.room.game.Players[0].realizedCreature as Player;
            }

            public override void Update(bool eu)
            {
                base.Update(eu);
                //强行设置玩家位置
                Vector2 vector = new Vector2(661f, 200f);
                IntVector2 pos = Room.StaticGetTilePosition(vector);
                if (player == null)
                    this.player = this.room.game.Players[0].realizedCreature as Player;
                if (this.room.game.GetStorySession.saveState.denPosition == this.room.abstractRoom.name &&
                    this.room.world.rainCycle.timer < 400 &&
                    this.player != null)
                {
                    if (ModManager.CoopAvailable)
                    {
                        List<PhysicalObject> list = (from x in this.room.physicalObjects.SelectMany((List<PhysicalObject> x) => x)
                                                     where x is Player
                                                     select x).ToList<PhysicalObject>();
                        int num = list.Count<PhysicalObject>();
                        foreach (AbstractCreature abstractCreature in this.room.game.Players)
                        {
                            (abstractCreature.realizedCreature as Player).SuperHardSetPosition(vector);
                            abstractCreature.pos = this.room.ToWorldCoordinate(vector);
                        }
                    }
                    else
                    {
                        player.SuperHardSetPosition(vector);
                        player.abstractCreature.pos = this.room.ToWorldCoordinate(vector);
                    }
                    //让业力门没能量
                    if (this.room.regionGate is WaterGate)
                        (this.room.regionGate as WaterGate).waterLeft = 0.5f;
                    if (this.room.regionGate is ElectricGate)
                        (this.room.regionGate as ElectricGate).batteryLeft = 0.5f;
                    this.Destroy();
                }

            }

            public override void Destroy()
            {
                base.Destroy();
            }

            public Player player;
        }
        #endregion

        public class SB_GOR_AquamarinePearl : UpdatableAndDeletable
        {
            public SB_GOR_AquamarinePearl(Room room)
            {
                this.room = room;

                if (this.room.snow == true ||
                    this.room.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Spear ||
                    this.room.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
                {
                    PearlFixedSave.pearlFixed = false;
                }
                else if (this.room.game != null && this.room.game.IsStorySession &&
                         this.room.game.session.characterStats.name != Plugin.SlugName &&
                         this.room.game.rainWorld.progression.currentSaveState.miscWorldSaveData.SLOracleState.neuronsLeft >= 0)
                {
                    PearlFixedSave.pearlFixed = true;
                }
            }

            public override void Update(bool eu)
            {
                base.Update(eu);
                if (this.room.game.session.characterStats.name == Plugin.SlugName)
                {
                    Plugin.Log("Don't Spawn Aquamarine Pearl in Hunter Campaign.");
                    this.Destroy();
                    return;
                }
                if (this.room.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Spear ||
                    this.room.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Artificer || 
                    this.room.game.rainWorld.progression.currentSaveState.miscWorldSaveData.SLOracleState.neuronsLeft <= 0)
                {
                    Plugin.Log("Don't Spawn Aquamarine Pearl before Hunter Campaign.");
                    this.Destroy();
                    return;
                }
                if (!this.room.game.IsStorySession || AquamarinePearlTokenSave.aquamarinePearlToken)
                {
                    Plugin.Log("Has Spawn Aquamarine Pearl, SB_GOR_AquamarinePearl Destroy.");
                    this.Destroy();
                    return;
                }/*
                for (int i = 0; i < this.room.roomSettings.placedObjects.Count; i++)
                {
                    if (this.room.roomSettings.placedObjects[i].type == PlacedObject.Type.DataPearl &&
                        (this.room.roomSettings.placedObjects[i] as DataPearl).AbstractPearl.type == DataPearl.AbstractDataPearl.DataPearlType.Red_stomach)
                    {
                        this.Destroy();
                    }
                }*/

                Vector2 vector = new Vector2(531f, 310f);
                AbstractCreature firstAlivePlayer = this.room.game.FirstAlivePlayer;
                if (this.room.game.session.characterStats.name != Plugin.SlugName &&
                    this.room.game.rainWorld.progression.currentSaveState.miscWorldSaveData.SLOracleState.neuronsLeft >= 0 &&
                    this.aquamarinePearl == null)// && !(this.room.game.session as StoryGameSession).saveState.miscWorldSaveData.pebblesEnergyTaken
                {
                    //生成珍珠
                    int num = this.room.roomSettings.placedObjects.Count;
                    this.room.roomSettings.placedObjects.Add(new PlacedObject(PlacedObject.Type.DataPearl, null));
                    DataPearl.AbstractDataPearl absAquamarinePearl = new DataPearl.AbstractDataPearl(this.room.world,
                                                                                                  AbstractPhysicalObject.AbstractObjectType.DataPearl,
                                                                                                  null, this.room.GetWorldCoordinate(vector),
                                                                                                  this.room.game.GetNewID(),
                                                                                                  this.room.abstractRoom.index,
                                                                                                  num,
                                                                                                  this.room.roomSettings.placedObjects[num].data as PlacedObject.ConsumableObjectData,
                                                                                                  DataPearl.AbstractDataPearl.DataPearlType.Red_stomach);
                    aquamarinePearl = new DataPearl(absAquamarinePearl, this.room.world);
                    aquamarinePearl.abstractPhysicalObject.RealizeInRoom();
                    Plugin.Log("Spawn Aquamarine Pearl, ID: " + aquamarinePearl.abstractPhysicalObject.ID.ToString());
                    AquamarinePearlTokenSave.aquamarinePearlToken = true;
                    this.Destroy();
                }
            }

            public void ReloadRooms()
            {
                for (int i = this.room.world.activeRooms.Count - 1; i >= 0; i--)
                {
                    if (this.room.world.activeRooms[i] != this.room.game.cameras[0].room)
                    {
                        if (this.room.game.roomRealizer != null)
                        {
                            this.room.game.roomRealizer.KillRoom(this.room.world.activeRooms[i].abstractRoom);
                        }
                        else
                        {
                            this.room.world.activeRooms[i].abstractRoom.Abstractize();
                        }
                    }
                }
            }

            private DataPearl aquamarinePearl;
        }

        public static bool IsImpossibleToReachOE(Room room)
        {
            string text = AssetManager.ResolveFilePath("Modify" + Path.DirectorySeparatorChar.ToString() + "IsImpossibleToReachOE" + "-" + room.game.session.characterStats.name.value + ".txt");
            if (!File.Exists(text))
            {
                text = AssetManager.ResolveFilePath("Modify" + Path.DirectorySeparatorChar.ToString() + "IsImpossibleToReachOE" + ".txt");
            }
            if (File.Exists(text))
            {
                string text2 = File.ReadAllText(text, Encoding.UTF8);
                string[] array = Regex.Split(text2, "\r\n");
                for (int j = 0; j < array.Length; j++)
                {
                    string[] array2 = Regex.Split(array[j], " : ");
                    if (array2.Length > 1 && array2[0] == room.game.session.characterStats.name.value)
                    {
                        Plugin.Log("{0} can reach OE by file? {1}", array2[0], array2[1]);
                        string isImpossibleToReachOE = array2[1];
                        bool result = bool.TryParse(isImpossibleToReachOE, out _);
                        return result;
                    }
                }
                Plugin.Log("Can't Found {0} in IsImpossibleToReachOE.txt", room.game.session.characterStats.name.value);
            }
            else
            {
                Plugin.Log("Can't Found IsImpossibleToReachOE.txt");
            }
            if (room.game.IsMoonHeartActive())
            {
                return true;
            }
            return false;
        }

        public static string shouldGoRegionName = "SB";
    }
}
