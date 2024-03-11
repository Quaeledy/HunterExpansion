using System.Collections.Generic;
using UnityEngine;
using RWCustom;
using MoreSlugcats;
using System.Linq;
using HunterExpansion.CustomOracle;
using HunterExpansion.CustomSave;
using HunterExpansion.CustomDream;
using HunterExpansion.CustomEffects;
using System;
using JollyCoop;
using Menu;
using static MonoMod.InlineRT.MonoModRule;
using Random = UnityEngine.Random;
using Expedition;

namespace HunterExpansion.CustomEnding
{
    public class EndingSession
    {
        //珍珠开业力门
        public static bool openGate = false;
        public static string openGateName = "";
        public static int openCount = 0;
        public static int noGrabbedCount = 0;
        public static DataPearl fixedPearl;
        public static List<GreenSparks> greenSparks;

        //进入结局cg前的准备
        private static bool isControled = false;
        public static FSprite blackRect = new FSprite("pixel");
        private static CutsceneHunter obj = null;

        //结局后的结局
        public static bool goEnding = false;

        public static void Init()
        {
            On.Player.ctor += Player_ctor;
            On.Player.UpdateMSC += Player_EndUpdate;
            //On.RainWorldGame.CommunicateWithUpcomingProcess += RainWorldGame_CommunicateWithUpcomingProcess;
            //On.SaveState.SessionEnded += SaveState_SessionEnded;
            //On.RegionState.AdaptRegionStateToWorld += RegionState_AdaptRegionStateToWorld;
        }
        private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);

            openGate = false; 
            openGateName = "";
            openCount = 0; 
            noGrabbedCount = 0;
            isControled = false;
            goEnding = false;
            fixedPearl = null;
        }

        public static void Player_EndUpdate(On.Player.orig_UpdateMSC orig, Player self)
        {
            orig(self);
            if (self.room.game.session.characterStats.name != Plugin.SlugName)
                return;

            //播放结局cg
            if (self.room.abstractRoom.name == "GATE_SB_OE" && PearlFixedSave.pearlFixed && openGate &&
                self == self.room.game.Players[0].realizedCreature as Player)
            {
                if (self.room.game.world.region.name == "NSH")
                {
                    if (self.firstChunk.pos.x > 100f && self.firstChunk.pos.x < 300f)
                    {
                        if (!isControled)
                        {
                            isControled = true;
                            obj = new CutsceneHunter(self.room);
                            self.room.AddObject(obj);
                            self.room.game.manager.musicPlayer.FadeOutAllSongs(40f);
                            //添加黑幕
                            if (blackRect == null)
                            {
                                blackRect = new FSprite("pixel");
                            }
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
                    }
                    if (self.firstChunk.pos.x <= 100f)
                    {
                        isControled = false;
                        openGate = false;
                        openCount = 0;
                        if (obj != null)
                        {
                            self.room.RemoveObject(obj);
                            obj = null;
                        }
                        //传送
                        AbstractRoom room = self.abstractCreature.world.GetAbstractRoom("NSH_AI");
                        if (ModManager.CoopAvailable)
                        {
                            List<PhysicalObject> list = (from x in self.room.physicalObjects.SelectMany((List<PhysicalObject> x) => x)
                                                         where x is Player
                                                         select x).ToList<PhysicalObject>();
                            int num = list.Count<PhysicalObject>();
                            foreach (AbstractCreature abstractCreature in self.room.game.Players)
                            {
                                if (abstractCreature.Room != room)
                                {
                                    try
                                    {
                                        WarpWithinTheSameRegion(abstractCreature.realizedCreature as Player, "NSH_AI", 24, 34);
                                        //JollyCustom.WarpAndRevivePlayer(abstractCreature, room, self.room.LocalCoordinateOfNode(0));
                                    }
                                    catch (Exception arg)
                                    {
                                        Plugin.Log(string.Format("Could not warp and revive player {0} [{1}]", abstractCreature, arg), false);
                                    }
                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                WarpWithinTheSameRegion(self, "NSH_AI", 24, 34);
                                //WarpPlayer(self.room.game.Players[0], room, self.room.LocalCoordinateOfNode(0));
                            }
                            catch (Exception arg)
                            {
                                Plugin.Log(string.Format("Could not warp and revive player {0} [{1}]", self.abstractCreature, arg), false);
                            }
                        }
                        self.room.game.GoToRedsGameOver();
                        return;
                    }
                }
                else
                {
                    if ((self.firstChunk.pos.x <= 280f || self.firstChunk.pos.x >= 680f) && !self.room.regionGate.EnergyEnoughToOpen)
                    {
                        openGate = false;
                        openCount = 0;
                    }
                }
            }

            //开门动画
            if (self.room.IsGateRoom() && PearlFixedSave.pearlFixed && !openGate && self.room.regionGate.EnergyEnoughToOpen &&
                self == self.room.game.Players[0].realizedCreature as Player)//这一条是确保只对一个玩家更新
            {
                GreenSparks greenSpark = new GreenSparks(self.room, 1f);
                if (openCount == 0 && fixedPearl != null && fixedPearl.grabbedBy.Count == 0)//防止玩家吞吐珍珠的一瞬间也能开启业力门
                {
                    noGrabbedCount++;
                }
                if (fixedPearl != null && fixedPearl.abstractPhysicalObject.realizedObject == null)//防止玩家吞吐珍珠的一瞬间也能开启业力门
                {
                    openCount = (int)Mathf.Max(openCount - 5, 0);
                    noGrabbedCount = 0;
                }
                if (fixedPearl != null && fixedPearl.abstractPhysicalObject.realizedObject != null && noGrabbedCount > 5 &&
                    ((fixedPearl.firstChunk.pos.x > 250f && fixedPearl.firstChunk.pos.x < 650f) || openCount > 0))
                {
                    openCount++;

                    //屏幕震动
                    self.room.ScreenMovement(new Vector2?(fixedPearl.firstChunk.pos), new Vector2(0f, 0f), Mathf.Min(Custom.LerpMap((float)openCount, 40f, 300f, 0f, 1.5f, 1.2f), Custom.LerpMap((float)openCount, 40f, 300f, 1.5f, 0f)));
                    //珍珠移动
                    Vector2 wantPos = (self.firstChunk.pos.x > 480f) ? new Vector2(570f, 295f) : new Vector2(390f, 295f);
                    fixedPearl.firstChunk.vel *= Custom.LerpMap(fixedPearl.firstChunk.vel.magnitude, 1f, 6f, 0.999f, 0.9f);
                    fixedPearl.firstChunk.vel += Vector2.ClampMagnitude(wantPos - fixedPearl.firstChunk.pos, 100f) / 100f * 0.4f;
                    //抵消重力
                    fixedPearl.firstChunk.vel += 1f * Vector2.up;
                }
                //找到珍珠
                List<PhysicalObject>[] physicalObjects = self.room.physicalObjects;
                for (int i = 0; i < physicalObjects.Length; i++)
                {
                    for (int j = 0; j < physicalObjects[i].Count; j++)
                    {
                        PhysicalObject physicalObject = physicalObjects[i][j];
                        if ((physicalObject is DataPearl) && physicalObject.abstractPhysicalObject.realizedObject != null &&
                            (physicalObject as DataPearl).AbstractPearl.dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.Red_stomach)
                        {
                            fixedPearl = physicalObject as DataPearl;
                        }
                    }
                }
                //在业力门范围，珍珠启动后不能再被抓住
                if (openCount > 0 && fixedPearl != null && fixedPearl.grabbedBy.Count != 0)
                {
                    for (int i = 0; i < fixedPearl.grabbedBy.Count; i++)
                    {
                        if (self.grasps != null)
                        {
                            for (int j = 0; j < self.grasps.Length; j++)
                            {
                                if (self.grasps[j] != null &&
                                    self.grasps[j].grabbed != null &&
                                    self.grasps[j].grabbed == fixedPearl)
                                {
                                    self.ReleaseGrasp(j);
                                }
                            }
                        }
                    }
                }
                if (openCount == 1)
                {
                    greenSparks = new List<GreenSparks>();
                    openGateName = self.room.abstractRoom.name;
                }
                //生成监视者
                if (openCount == 50 || openCount == 100)
                {
                    PlayerHooks.SpawnOverseerInRoom(self.room, "GATE_SB_OE", CreatureTemplate.Type.Overseer, 2);
                }
                //加绿色闪电
                if (openCount == 21)
                {
                    self.room.AddObject(new ElectricFullScreen(self.room, 0.8f, 280, 180, 40));
                }
                //珍珠开门动画
                if (openCount > 21 && openCount < 220 && fixedPearl != null)
                {
                    if (openCount % 20 == 0)
                    {
                        self.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Data_Bit, fixedPearl.firstChunk.pos, 1f, 1f + Random.value * 2f);
                        self.room.AddObject(new Explosion.ExplosionLight(fixedPearl.firstChunk.pos, 150f, 1f, 15, Color.green));
                    }
                    //加绿色电火花
                    if (greenSpark != null)
                    {
                        self.room.AddObject(greenSpark);
                        if (greenSparks != null && greenSpark != null)
                            greenSparks.Add(greenSpark);
                    }
                }
                if (openCount == 220 && fixedPearl != null)
                {
                    self.room.PlaySound(SoundID.Moon_Wake_Up_Green_Swarmer_Flash, fixedPearl.firstChunk.pos, 0.5f, 1f);
                    self.room.PlaySound(SoundID.Fire_Spear_Explode, fixedPearl.firstChunk.pos, 0.5f, 1f);
                    self.room.AddObject(new ElectricFullScreen.SparkFlash(fixedPearl.firstChunk.pos, 50f));
                    self.room.AddObject(new Spark(fixedPearl.firstChunk.pos, Custom.RNV() * Random.value * 40f, new Color(0f, 1f, 0f), null, 30, 120));
                    fixedPearl.Destroy();
                }
                if (openCount == 300)
                {
                    openCount = 0;
                    openGate = true;
                    fixedPearl = null;
                    for (int k = greenSparks.Count - 1; k >= 0; k--)
                    {
                        GreenSparks spark = greenSparks[k];
                        greenSparks.Remove(spark);
                        spark.Destroy();
                    }
                    greenSparks.Clear();
                }
            }

            //结局后的结局
            if (goEnding)
            {
                if (!isControled)
                {
                    isControled = true;
                    self.room.AddObject(new EndingCutsceneHunter(self.room));
                    //将玩家图层置于迭代器上方
                    List<PhysicalObject>[] physicalObjects = self.room.physicalObjects;
                    for (int i = 0; i < physicalObjects.Length; i++)
                    {
                        for (int j = 0; j < physicalObjects[i].Count; j++)
                        {
                            PhysicalObject physicalObject = physicalObjects[i][j];
                            if (physicalObject is Oracle && (physicalObject as Oracle).ID == NSHOracleRegistry.NSHOracle)
                            {
                                if (physicalObject.graphicsModule != null)
                                {
                                    if (self is IDrawable)
                                        physicalObject.graphicsModule.AddObjectToInternalContainer(self as IDrawable, 0);
                                    else if (self.graphicsModule != null)
                                        physicalObject.graphicsModule.AddObjectToInternalContainer(self.graphicsModule, 0);
                                }
                                //self.graphicsModule.AddObjectToInternalContainer(physicalObject as IDrawable, 0);
                            }
                        }
                    }
                }
                if (self.dead)
                {
                    isControled = false;
                    goEnding = false;
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
            AbstractRoom room = player.abstractCreature.world.GetAbstractRoom(newRoomName);
            
            //放下矛
            if (player.spearOnBack != null && player.spearOnBack.spear != null)
            {
                player.spearOnBack.DropSpear();
            }
            //放下猫崽
            if ((ModManager.MSC || ModManager.CoopAvailable) && player.slugOnBack != null && player.slugOnBack.slugcat != null)
            {
                player.slugOnBack.DropSlug();
            }
            //更换房间
            if (room.realizedRoom == null)
            {
                room.RealizeRoom(game.world, game);
            }
            if (player.room.abstractRoom != room)// && !player.isSlugpup
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
                if (player.abstractCreature.realizedCreature != null)
                {
                    player.abstractCreature.realizedCreature.Destroy();
                    player.abstractCreature.realizedCreature = null;
                }
                player.abstractCreature.Move(new WorldCoordinate(room.index, x, y, 0));
                player.PlaceInRoom(room.realizedRoom);
                player.abstractCreature.ChangeRooms(new WorldCoordinate(room.index, x, y, 0));
                /*
                Player.SpearOnBack spearOnBack = player.spearOnBack;
                if (((spearOnBack != null) ? spearOnBack.spear : null) != null)
                {
                    player.spearOnBack.spear.PlaceInRoom(room.realizedRoom);
                    player.spearOnBack.spear.room = player.room;
                }
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
                            slug.ChangeRooms(new WorldCoordinate(room.index, x, y, -1));
                            player.slugOnBack.SlugToBack(slug.realizedCreature as Player);
                        }
                        player.slugOnBack.slugcat.AI.NewRoom(room.realizedRoom);
                    }
                    player.slugOnBack.slugcat.PlaceInRoom(room.realizedRoom);
                    player.slugOnBack.slugcat.room = player.room;
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

        private static void RainWorldGame_CommunicateWithUpcomingProcess(On.RainWorldGame.orig_CommunicateWithUpcomingProcess orig, RainWorldGame self, MainLoopProcess nextProcess)
        {
            Debug.Log("NEXT PROCESS COMMUNICATION");
            //base.CommunicateWithUpcomingProcess(nextProcess);
            if (nextProcess is StoryGameStatisticsScreen)
            {
                (nextProcess as StoryGameStatisticsScreen).forceWatch = true;
                if ((nextProcess as StoryGameStatisticsScreen).scene != null && (nextProcess as StoryGameStatisticsScreen).scene.sceneID == MenuScene.SceneID.RedsDeathStatisticsBkg)
                {
                    ((nextProcess as StoryGameStatisticsScreen).scene as InteractiveMenuScene).timer = 0;
                }
            }
            if (nextProcess is KarmaLadderScreen || nextProcess is DreamScreen || (self.StoryCharacter == SlugcatStats.Name.Red && nextProcess is SlideShow) || (ModManager.MSC && (nextProcess is ScribbleDreamScreen || nextProcess is ScribbleDreamScreenOld || nextProcess is SlideShow || nextProcess is EndCredits)))
            {
                int karma = self.GetStorySession.saveState.deathPersistentSaveData.karma;
                Debug.Log("savKarma: " + karma.ToString());
                if (self.sawAGhost != null)
                {
                    Debug.Log("Ghost end of process stuff");
                    self.manager.CueAchievement(GhostWorldPresence.PassageAchievementID(self.sawAGhost), 2f);
                    if (self.GetStorySession.saveState.deathPersistentSaveData.karmaCap == 8)
                    {
                        self.manager.CueAchievement(RainWorld.AchievementID.AllGhostsEncountered, 10f);
                    }
                    self.GetStorySession.saveState.GhostEncounter(self.sawAGhost, self.rainWorld);
                }
                int num = karma;
                if (nextProcess.ID == ProcessManager.ProcessID.DeathScreen && !self.GetStorySession.saveState.deathPersistentSaveData.reinforcedKarma)
                {
                    num = Custom.IntClamp(num - 1, 0, self.GetStorySession.saveState.deathPersistentSaveData.karmaCap);
                }
                Debug.Log("next screen MAP KARMA: " + num.ToString());
                if (self.cameras[0].hud != null)
                {
                    self.cameras[0].hud.map.mapData.UpdateData(self.world, 1 + self.GetStorySession.saveState.deathPersistentSaveData.foodReplenishBonus, num, self.GetStorySession.saveState.deathPersistentSaveData.karmaFlowerPosition, true);
                }
                AbstractCreature abstractCreature = self.FirstAlivePlayer;
                if (abstractCreature == null)
                {
                    abstractCreature = self.FirstAnyPlayer;
                }
                int num2 = -1;
                Vector2 vector = Vector2.zero;
                if (abstractCreature != null)
                {
                    num2 = abstractCreature.pos.room;
                    vector = abstractCreature.pos.Tile.ToVector2() * 20f;
                    if (nextProcess.ID == ProcessManager.ProcessID.DeathScreen && self.cameras[0].hud != null && self.cameras[0].hud.textPrompt != null)
                    {
                        num2 = self.cameras[0].hud.textPrompt.deathRoom;
                        vector = self.cameras[0].hud.textPrompt.deathPos;
                    }
                    else if (abstractCreature.realizedCreature != null)
                    {
                        vector = abstractCreature.realizedCreature.mainBodyChunk.pos;
                    }
                    if (abstractCreature.realizedCreature != null && abstractCreature.realizedCreature.room != null && num2 == abstractCreature.realizedCreature.room.abstractRoom.index)
                    {
                        vector = Custom.RestrictInRect(vector, abstractCreature.realizedCreature.room.RoomRect.Grow(50f));
                    }
                }
                KarmaLadderScreen.SleepDeathScreenDataPackage sleepDeathScreenDataPackage;
                if (ModManager.MSC && self.wasAnArtificerDream)
                {
                    sleepDeathScreenDataPackage = self.manager.dataBeforeArtificerDream;
                    self.manager.dataBeforeArtificerDream = null;
                }
                else
                {
                    sleepDeathScreenDataPackage = new KarmaLadderScreen.SleepDeathScreenDataPackage((nextProcess.ID == ProcessManager.ProcessID.SleepScreen || nextProcess.ID == ProcessManager.ProcessID.Dream) ? self.GetStorySession.saveState.food : self.cameras[0].hud.textPrompt.foodInStomach, new IntVector2(karma, self.GetStorySession.saveState.deathPersistentSaveData.karmaCap), self.GetStorySession.saveState.deathPersistentSaveData.reinforcedKarma, num2, vector, self.cameras[0].hud.map.mapData, self.GetStorySession.saveState, self.GetStorySession.characterStats, self.GetStorySession.playerSessionRecords[0], self.GetStorySession.saveState.lastMalnourished, self.GetStorySession.saveState.malnourished);
                    if (ModManager.MSC && self.GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
                    {
                        self.manager.dataBeforeArtificerDream = sleepDeathScreenDataPackage;
                    }
                    if (ModManager.CoopAvailable)
                    {
                        for (int i = 1; i < self.GetStorySession.playerSessionRecords.Length; i++)
                        {/*
                            Plugin.Log("self.GetStorySession.playerSessionRecords[{0}] == null? " + (self.GetStorySession.playerSessionRecords[i] == null));
                            Plugin.Log("self.GetStorySession.playerSessionRecords[{0}].kills == null? {1}", i, (self.GetStorySession.playerSessionRecords[i].kills == null));
                            if (self.GetStorySession.playerSessionRecords[i].kills != null)
                            {
                                Plugin.Log("self.GetStorySession.playerSessionRecords[{0}].kills.Count == null? {1}", i, (self.GetStorySession.playerSessionRecords[i].kills.Count));
                            }*/
                            if (self.GetStorySession.playerSessionRecords[i] != null && self.GetStorySession.playerSessionRecords[i].kills != null && self.GetStorySession.playerSessionRecords[i].kills.Count > 0)
                            {
                                sleepDeathScreenDataPackage.sessionRecord.kills.AddRange(self.GetStorySession.playerSessionRecords[i].kills);
                            }
                        }
                    }
                }
                if (nextProcess is KarmaLadderScreen)
                {
                    (nextProcess as KarmaLadderScreen).GetDataFromGame(sleepDeathScreenDataPackage);
                }
                else if (nextProcess is DreamScreen)
                {
                    (nextProcess as DreamScreen).GetDataFromGame(self.GetStorySession.saveState.dreamsState.UpcomingDreamID, sleepDeathScreenDataPackage);
                }
                else if (self.StoryCharacter == SlugcatStats.Name.Red && nextProcess is SlideShow)
                {
                    (nextProcess as SlideShow).endGameStatsPackage = sleepDeathScreenDataPackage;
                    (nextProcess as SlideShow).processAfterSlideShow = ProcessManager.ProcessID.Statistics;
                }
                if (nextProcess is ScribbleDreamScreen)
                {
                    (nextProcess as ScribbleDreamScreen).GetDataFromGame(self.GetStorySession.saveState.dreamsState.UpcomingDreamID, sleepDeathScreenDataPackage);
                }
                if (nextProcess is ScribbleDreamScreenOld)
                {
                    (nextProcess as ScribbleDreamScreenOld).GetDataFromGame(self.GetStorySession.saveState.dreamsState.UpcomingDreamID, sleepDeathScreenDataPackage);
                }
                if (nextProcess is EndCredits)
                {
                    (nextProcess as EndCredits).passthroughPackage = sleepDeathScreenDataPackage;
                }
                if (nextProcess is SlideShow)
                {
                    (nextProcess as SlideShow).passthroughPackage = sleepDeathScreenDataPackage;
                }
            }
        }

        public static void SaveState_SessionEnded(On.SaveState.orig_SessionEnded orig, SaveState self, RainWorldGame game, bool survived, bool newMalnourished)
        {
            self.lastMalnourished = self.malnourished;
            self.malnourished = newMalnourished;
            self.deathPersistentSaveData.sessionTrackRecord.Add(new DeathPersistentSaveData.SessionRecord(survived, game.GetStorySession.playerSessionRecords[0].wokeUpInRegion != game.world.region.name));
            if (self.deathPersistentSaveData.sessionTrackRecord.Count > 20)
            {
                self.deathPersistentSaveData.sessionTrackRecord.RemoveAt(0);
            }
            for (int i = self.deathPersistentSaveData.deathPositions.Count - 1; i >= 0; i--)
            {
                if (self.deathPersistentSaveData.deathPositions[i].Valid)
                {
                    self.deathPersistentSaveData.deathPositions[i] = new WorldCoordinate(self.deathPersistentSaveData.deathPositions[i].room, self.deathPersistentSaveData.deathPositions[i].x, self.deathPersistentSaveData.deathPositions[i].y, self.deathPersistentSaveData.deathPositions[i].abstractNode + 1);
                }
                else
                {
                    self.deathPersistentSaveData.deathPositions[i] = new WorldCoordinate(self.deathPersistentSaveData.deathPositions[i].unknownName, self.deathPersistentSaveData.deathPositions[i].x, self.deathPersistentSaveData.deathPositions[i].y, self.deathPersistentSaveData.deathPositions[i].abstractNode + 1);
                }
                if (self.deathPersistentSaveData.deathPositions[i].abstractNode >= 7)
                {
                    self.deathPersistentSaveData.deathPositions.RemoveAt(i);
                }
            }
            if (survived)
            {
                self.deathPersistentSaveData.foodReplenishBonus = 0;
                if (RainWorld.ShowLogs)
                {
                    Debug.Log("resetting food rep bonus");
                }
                self.RainCycleTick(game, true);
                self.cyclesInCurrentWorldVersion++;
                if (ModManager.MMF && self.progression.miscProgressionData.returnExplorationTutorialCounter > 0)
                {
                    self.progression.miscProgressionData.returnExplorationTutorialCounter = 3;
                }
                self.food = 0;
                if (ModManager.CoopAvailable)
                {
                    if (!(game.session.Players[0].state as PlayerState).permaDead && game.session.Players[0].realizedCreature != null && game.session.Players[0].realizedCreature.room != null)
                    {
                        self.food = (game.session.Players[0].realizedCreature as Player).FoodInRoom(true);
                    }
                    else if (game.AlivePlayers.Count > 0 && game.FirstAlivePlayer != null)
                    {
                        self.food = (game.FirstAlivePlayer.realizedCreature as Player).FoodInRoom(true);
                    }
                }
                else
                {
                    for (int j = 0; j < game.session.Players.Count; j++)
                    {
                        self.food += (game.session.Players[j].realizedCreature as Player).FoodInRoom(true);
                    }
                }
                self.food = Custom.IntClamp(self.food, 0, game.GetStorySession.characterStats.maxFood);
                if (self.malnourished)
                {
                    self.food -= game.GetStorySession.characterStats.foodToHibernate;
                }
                else if (self.lastMalnourished)
                {
                    if (game.devToolsActive && self.food < game.GetStorySession.characterStats.maxFood)
                    {
                        Debug.Log("FOOD COUNT ISSUE! " + self.food.ToString() + " " + game.GetStorySession.characterStats.maxFood.ToString());
                    }
                    self.food = 0;
                }
                else
                {
                    self.food -= game.GetStorySession.characterStats.foodToHibernate;
                }
                self.BringUpToDate(game);
                for (int k = 0; k < game.GetStorySession.playerSessionRecords.Length; k++)
                {
                    if (game.GetStorySession.playerSessionRecords[k] != null && (!ModManager.CoopAvailable || game.world.GetAbstractRoom(game.Players[k].pos) != null))
                    {
                        game.GetStorySession.playerSessionRecords[k].pupCountInDen = 0;
                        bool flag = false;
                        game.GetStorySession.playerSessionRecords[k].wentToSleepInRegion = game.world.region.name;
                        for (int l = 0; l < game.world.GetAbstractRoom(game.Players[k].pos).creatures.Count; l++)
                        {
                            if (game.world.GetAbstractRoom(game.Players[k].pos).creatures[l].state.alive && game.world.GetAbstractRoom(game.Players[k].pos).creatures[l].state.socialMemory != null && game.world.GetAbstractRoom(game.Players[k].pos).creatures[l].realizedCreature != null && game.world.GetAbstractRoom(game.Players[k].pos).creatures[l].abstractAI != null && game.world.GetAbstractRoom(game.Players[k].pos).creatures[l].abstractAI.RealAI != null && game.world.GetAbstractRoom(game.Players[k].pos).creatures[l].abstractAI.RealAI.friendTracker != null && game.world.GetAbstractRoom(game.Players[k].pos).creatures[l].abstractAI.RealAI.friendTracker.friend != null && game.world.GetAbstractRoom(game.Players[k].pos).creatures[l].abstractAI.RealAI.friendTracker.friend == game.Players[k].realizedCreature && game.world.GetAbstractRoom(game.Players[k].pos).creatures[l].state.socialMemory.GetLike(game.Players[k].ID) > 0f)
                            {
                                if (ModManager.MSC && game.world.GetAbstractRoom(game.Players[k].pos).creatures[l].creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
                                {
                                    if ((game.world.GetAbstractRoom(game.Players[k].pos).creatures[l].state as PlayerNPCState).foodInStomach - ((game.world.GetAbstractRoom(game.Players[k].pos).creatures[l].state as PlayerNPCState).Malnourished ? SlugcatStats.SlugcatFoodMeter(MoreSlugcatsEnums.SlugcatStatsName.Slugpup).x : SlugcatStats.SlugcatFoodMeter(MoreSlugcatsEnums.SlugcatStatsName.Slugpup).y) >= 0)
                                    {
                                        game.GetStorySession.playerSessionRecords[k].pupCountInDen++;
                                    }
                                }
                                else if (!flag)
                                {
                                    flag = true;
                                    game.GetStorySession.playerSessionRecords[k].friendInDen = game.world.GetAbstractRoom(game.Players[k].pos).creatures[l];
                                    SocialMemory.Relationship orInitiateRelationship = game.world.GetAbstractRoom(game.Players[k].pos).creatures[l].state.socialMemory.GetOrInitiateRelationship(game.Players[k].ID);
                                    orInitiateRelationship.like = Mathf.Lerp(orInitiateRelationship.like, 1f, 0.5f);
                                }
                            }
                        }
                    }
                }
                self.AppendKills(game.GetStorySession.playerSessionRecords[0].kills);
                if (ModManager.CoopAvailable)
                {
                    for (int m = 1; m < game.GetStorySession.playerSessionRecords.Length; m++)
                    {
                        self.AppendKills(game.GetStorySession.playerSessionRecords[m].kills);
                    }
                }
                game.GetStorySession.AppendTimeOnCycleEnd(false);
                self.deathPersistentSaveData.survives++;
                self.deathPersistentSaveData.winState.CycleCompleted(game);
                if (!ModManager.CoopAvailable)
                {
                    self.deathPersistentSaveData.friendsSaved += ((game.GetStorySession.playerSessionRecords[0].friendInDen != null) ? 1 : 0);
                }
                else
                {
                    List<AbstractCreature> list = new List<AbstractCreature>();
                    foreach (PlayerSessionRecord playerSessionRecord in game.GetStorySession.playerSessionRecords)
                    {
                        if (!list.Contains(playerSessionRecord.friendInDen))
                        {
                            list.Add(playerSessionRecord.friendInDen);
                        }
                    }
                    self.deathPersistentSaveData.friendsSaved += list.Count;
                }
                self.deathPersistentSaveData.karma++;
                if (self.malnourished)
                {
                    self.deathPersistentSaveData.reinforcedKarma = false;
                }
                game.rainWorld.progression.SaveWorldStateAndProgression(self.malnourished);
                return;
            }
            game.GetStorySession.AppendTimeOnCycleEnd(true);
            self.deathPersistentSaveData.AddDeathPosition(game.cameras[0].hud.textPrompt.deathRoom, game.cameras[0].hud.textPrompt.deathPos);
            self.deathPersistentSaveData.deaths++;
            if (self.deathPersistentSaveData.karma == 0 || (self.saveStateNumber == SlugcatStats.Name.White && Random.value < 0.5f) || self.saveStateNumber == SlugcatStats.Name.Yellow)
            {
                self.deathPersistentSaveData.foodReplenishBonus++;
                if (RainWorld.ShowLogs)
                {
                    Debug.Log("Ticking up food rep bonus to: " + self.deathPersistentSaveData.foodReplenishBonus.ToString());
                }
            }
            else if (RainWorld.ShowLogs)
            {
                Debug.Log("death screen, no food bonus");
            }
            self.deathPersistentSaveData.TickFlowerDepletion(1);
            if (ModManager.MMF && MMF.cfgExtraTutorials.Value)
            {
                if (RainWorld.ShowLogs)
                {
                    Debug.Log("Exploration tutorial counter : " + self.progression.miscProgressionData.returnExplorationTutorialCounter.ToString());
                }
                if (game.IsStorySession && (game.world.region.name == "SB" || game.world.region.name == "SL" || game.world.region.name == "UW" || self.deathPersistentSaveData.karmaCap > 8 || self.miscWorldSaveData.SSaiConversationsHad > 0))
                {
                    self.progression.miscProgressionData.returnExplorationTutorialCounter = -1;
                    if (RainWorld.ShowLogs)
                    {
                        Debug.Log("CANCEL exploration counter");
                    }
                }
                else if (game.IsStorySession && (game.world.region.name == "SH" || (ModManager.MSC && game.world.region.name == "VS") || game.world.region.name == "DS" || game.world.region.name == "CC" || game.world.region.name == "LF" || game.world.region.name == "SI"))
                {
                    if (RainWorld.ShowLogs)
                    {
                        Debug.Log("Exploration counter ticked to " + self.progression.miscProgressionData.returnExplorationTutorialCounter.ToString());
                    }
                    if (self.progression.miscProgressionData.returnExplorationTutorialCounter > 0)
                    {
                        PlayerProgression.MiscProgressionData miscProgressionData = self.progression.miscProgressionData;
                        int n = miscProgressionData.returnExplorationTutorialCounter;
                        miscProgressionData.returnExplorationTutorialCounter = n - 1;
                    }
                }
                else if (self.progression.miscProgressionData.returnExplorationTutorialCounter > 0)
                {
                    self.progression.miscProgressionData.returnExplorationTutorialCounter = 3;
                    if (RainWorld.ShowLogs)
                    {
                        Debug.Log("Reset exploration counter");
                    }
                }
            }
            if (ModManager.Expedition && game.rainWorld.ExpeditionMode)
            {
                ExpLog.Log("Loading previous cycle challenge progression");
                Expedition.Expedition.coreFile.Load();
                if (ExpeditionGame.expeditionComplete)
                {
                    ExpeditionGame.expeditionComplete = false;
                }
            }
            game.rainWorld.progression.SaveProgressionAndDeathPersistentDataOfCurrentState(true, false);
        }

        public static void RegionState_AdaptRegionStateToWorld(On.RegionState.orig_AdaptRegionStateToWorld orig, RegionState self, int playerShelter, int activeGate)
        {
            if (RainWorld.ShowLogs)
            {
                Debug.Log("Adapt region state to world " + self.regionName);
            }
            self.savedObjects.Clear();
            for (int i = 0; i < self.unrecognizedSavedObjects.Count; i++)
            {
                self.savedObjects.Add(self.unrecognizedSavedObjects[i]);
            }
            self.unrecognizedSavedObjects.Clear();
            self.savedPopulation.Clear();
            for (int j = 0; j < self.unrecognizedPopulation.Count; j++)
            {
                self.savedPopulation.Add(self.unrecognizedPopulation[j]);
            }
            self.unrecognizedPopulation.Clear();
            self.saveState.pendingObjects.Clear();
            //出错原因大概是 self.world == null
            for (int k = 0; k < self.world.NumberOfRooms; k++)
            {
                AbstractRoom abstractRoom = self.world.GetAbstractRoom(self.world.firstRoomIndex + k);
                for (int l = 0; l < abstractRoom.entities.Count; l++)
                {
                    if (abstractRoom.entities[l] is AbstractPhysicalObject && (abstractRoom.entities[l] as AbstractPhysicalObject).type != AbstractPhysicalObject.AbstractObjectType.KarmaFlower)
                    {
                        if (abstractRoom.entities[l] is AbstractSpear && (abstractRoom.entities[l] as AbstractSpear).stuckInWall)
                        {
                            self.savedObjects.Add(abstractRoom.entities[l].ToString());
                        }
                        else if (ModManager.MMF && ((abstractRoom.shelter && abstractRoom.index == playerShelter) || abstractRoom.name == SaveState.forcedEndRoomToAllowwSave) && (abstractRoom.entities[l] as AbstractPhysicalObject).type == AbstractPhysicalObject.AbstractObjectType.Creature)
                        {
                            AbstractCreature abstractCreature = (abstractRoom.entities[l] as AbstractPhysicalObject) as AbstractCreature;
                            float num = -1f;
                            foreach (AbstractCreature abstractCreature2 in self.world.game.Players)
                            {
                                if (abstractCreature != null && abstractCreature.state != null && abstractCreature.state.socialMemory != null)
                                {
                                    num = Mathf.Max(num, abstractCreature.state.socialMemory.GetLike(abstractCreature2.ID));
                                }
                            }
                            if (ModManager.MSC && abstractCreature.creatureTemplate.TopAncestor().type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
                            {
                                if (RainWorld.ShowLogs)
                                {
                                    Debug.Log("Add pup to pendingFriendSpawns " + abstractCreature.ToString());
                                }
                                self.saveState.pendingFriendCreatures.Add(SaveState.AbstractCreatureToStringStoryWorld(abstractCreature));
                                abstractCreature.LoseAllStuckObjects();
                                abstractCreature.saveCreature = false;
                            }
                            else if (abstractCreature.creatureTemplate.TopAncestor().type == CreatureTemplate.Type.LizardTemplate && abstractCreature.state.alive && num > 0.5f)
                            {
                                if (RainWorld.ShowLogs)
                                {
                                    Debug.Log("Add lizard to pendingFriendSpawns " + abstractCreature.ToString());
                                }
                                self.saveState.pendingFriendCreatures.Add(SaveState.AbstractCreatureToStringStoryWorld(abstractCreature));
                                abstractCreature.LoseAllStuckObjects();
                                abstractCreature.saveCreature = false;
                            }
                            else if (RainWorld.ShowLogs)
                            {
                                Debug.Log("Ignoring unfriendable creature " + abstractCreature.ToString());
                            }
                        }
                        else if (abstractRoom.shelter && (abstractRoom.entities[l] as AbstractPhysicalObject).type == AbstractPhysicalObject.AbstractObjectType.NeedleEgg)
                        {
                            self.savedPopulation.Add(self.AddHatchedNeedleFly((abstractRoom.entities[l] as AbstractPhysicalObject).pos));
                            self.savedPopulation.Add(self.AddHatchedNeedleFly((abstractRoom.entities[l] as AbstractPhysicalObject).pos));
                        }
                        else if (abstractRoom.shelter && !(abstractRoom.entities[l] is AbstractCreature) && (abstractRoom.entities[l] as AbstractPhysicalObject).type != AbstractPhysicalObject.AbstractObjectType.Creature)
                        {
                            if (RainWorld.ShowLogs)
                            {
                                Debug.Log("Attempting to add " + abstractRoom.entities[l].GetType().ToString() + " object to pendingObjects");
                            }
                            if (ModManager.MMF && MMF.cfgKeyItemPassaging.Value && abstractRoom.index == playerShelter)
                            {
                                if ((abstractRoom.entities[l] as AbstractPhysicalObject).tracker != null)
                                {
                                    if (RainWorld.ShowLogs)
                                    {
                                        Debug.Log("Adding " + abstractRoom.entities[l].GetType().ToString() + " object to pendingObjects");
                                    }
                                    self.saveState.pendingObjects.Add(abstractRoom.entities[l].ToString());
                                }
                                else
                                {
                                    if (RainWorld.ShowLogs)
                                    {
                                        Debug.Log("Adding " + abstractRoom.entities[l].GetType().ToString() + " object to savedObjects instead of pending");
                                    }
                                    self.savedObjects.Add(abstractRoom.entities[l].ToString());
                                }
                            }
                            else
                            {
                                if (RainWorld.ShowLogs)
                                {
                                    Debug.Log("Adding " + abstractRoom.entities[l].GetType().ToString() + " object to savedObjects");
                                }
                                self.savedObjects.Add(abstractRoom.entities[l].ToString());
                            }
                        }
                    }
                }
            }
            for (int m = 0; m < self.world.NumberOfRooms; m++)
            {
                AbstractRoom abstractRoom2 = self.world.GetAbstractRoom(self.world.firstRoomIndex + m);
                for (int n = 0; n < 2; n++)
                {
                    int num2 = (n == 0) ? abstractRoom2.creatures.Count : abstractRoom2.entitiesInDens.Count;
                    for (int num3 = 0; num3 < num2; num3++)
                    {
                        AbstractWorldEntity abstractWorldEntity = (n == 0) ? abstractRoom2.creatures[num3] : abstractRoom2.entitiesInDens[num3];
                        if (abstractWorldEntity is AbstractCreature && !(abstractWorldEntity as AbstractCreature).creatureTemplate.quantified && (abstractWorldEntity as AbstractCreature).creatureTemplate.saveCreature && (abstractWorldEntity as AbstractCreature).saveCreature)
                        {
                            string text = self.CreatureToStringInDenPos(abstractWorldEntity as AbstractCreature, playerShelter, activeGate);
                            if (text != "")
                            {
                                self.savedPopulation.Add(text);
                                for (int num4 = 0; num4 < self.loadedCreatures.Count; num4++)
                                {
                                    if (self.loadedCreatures[num4] == abstractWorldEntity as AbstractCreature)
                                    {
                                        self.loadedCreatures.RemoveAt(num4);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            for (int num5 = 0; num5 < self.loadedCreatures.Count; num5++)
            {
                if (RainWorld.ShowLogs)
                {
                    Debug.Log("Creature which was loaded but not saved");
                    Debug.Log("-- " + self.loadedCreatures[num5].creatureTemplate.name + " " + self.loadedCreatures[num5].ID.ToString());
                    Debug.Log("-- dead: " + self.loadedCreatures[num5].state.dead.ToString());
                }
                if (self.loadedCreatures[num5].state.dead)
                {
                    bool flag = false;
                    int num6 = 0;
                    while (num6 < self.saveState.respawnCreatures.Count && !flag)
                    {
                        if (self.loadedCreatures[num5].ID.spawner == self.saveState.respawnCreatures[num6])
                        {
                            flag = true;
                        }
                        num6++;
                    }
                    int num7 = 0;
                    while (num7 < self.saveState.waitRespawnCreatures.Count && !flag)
                    {
                        if (self.loadedCreatures[num5].ID.spawner == self.saveState.waitRespawnCreatures[num7])
                        {
                            flag = true;
                        }
                        num7++;
                    }
                    if (RainWorld.ShowLogs)
                    {
                        Debug.Log(flag ? "-- is put up for respawn." : "-- is NOT put up for respawn!");
                    }
                    if (!flag)
                    {
                        if (RainWorld.ShowLogs)
                        {
                            Debug.Log("Added for respawn");
                        }
                        self.saveState.respawnCreatures.Add(self.loadedCreatures[num5].ID.spawner);
                    }
                }
            }
            self.savedSticks.Clear();
            for (int num8 = 0; num8 < self.world.shelters.Length; num8++)
            {
                AbstractRoom abstractRoom3 = self.world.GetAbstractRoom(self.world.shelters[num8]);
                for (int num9 = 0; num9 < abstractRoom3.entities.Count; num9++)
                {
                    if (abstractRoom3.entities[num9] is AbstractPhysicalObject)
                    {
                        for (int num10 = 0; num10 < (abstractRoom3.entities[num9] as AbstractPhysicalObject).stuckObjects.Count; num10++)
                        {
                            if ((abstractRoom3.entities[num9] as AbstractPhysicalObject).stuckObjects[num10].A == abstractRoom3.entities[num9])
                            {
                                self.savedSticks.Add((abstractRoom3.entities[num9] as AbstractPhysicalObject).stuckObjects[num10].SaveToString(abstractRoom3.index));
                            }
                        }
                    }
                }
            }
        }


        /*
        public static void WarpPlayer(AbstractCreature absPlayer, AbstractRoom newRoom, WorldCoordinate position)
        {
            if ((absPlayer.state as PlayerState).permaDead && absPlayer.realizedCreature != null && absPlayer.realizedCreature.room != null)
            {
                Player player = absPlayer.realizedCreature as Player;
                player.room.RemoveObject(absPlayer.realizedCreature);
                if (player.grasps[0] != null)
                {
                    player.ReleaseGrasp(0);
                }
                if (player.grasps[1] != null)
                {
                    player.ReleaseGrasp(1);
                }
                List<AbstractPhysicalObject> allConnectedObjects = player.abstractCreature.GetAllConnectedObjects();
                if (player.room != null)
                {
                    for (int i = 0; i < allConnectedObjects.Count; i++)
                    {
                        if (allConnectedObjects[i].realizedObject != null)
                        {
                            player.room.RemoveObject(allConnectedObjects[i].realizedObject);
                        }
                    }
                }
            }
            if (absPlayer.realizedCreature == null || absPlayer.Room.realizedRoom == null || (absPlayer.state as PlayerState).permaDead || absPlayer.world != newRoom.world)
            {
                JollyCustom.Log("Reviving null player to " + newRoom.name, false);
                if (absPlayer.world != newRoom.world)
                {
                    absPlayer.world = newRoom.world;
                    absPlayer.pos = position;
                    absPlayer.Room.RemoveEntity(absPlayer);
                }
                newRoom.AddEntity(absPlayer);
                absPlayer.Move(position);
                absPlayer.realizedCreature.PlaceInRoom(newRoom.realizedRoom);
            }
            else if (absPlayer.Room.name != newRoom.name)
            {
                JollyCustom.Log(string.Format("Moving dead player to {0}, reason: null [{1}], roomNull [{2}]", newRoom, absPlayer.realizedCreature == null, absPlayer.Room.realizedRoom == null), false);
                JollyCustom.MovePlayerWithItems(absPlayer.realizedCreature as Player, absPlayer.Room.realizedRoom, newRoom.name, position);
            }
            (absPlayer.state as PlayerState).permaDead = false;
            if (absPlayer.realizedCreature != null)
            {
                absPlayer.realizedCreature.Stun(100);
            }
        }*/
    }

    public class CutsceneHunter : UpdatableAndDeletable
    {
        public CutsceneHunter(Room room)
        {
            this.room = room;
            this.phase = CutsceneHunter.Phase.Init;
            this.playerList = (from x in room.game.Players
                               select (Player)x.realizedCreature).ToList<Player>();
            if (ModManager.CoopAvailable)
            {
                this.hardmodePlayers = new List<CutsceneHunter.HardmodePlayer>();
                for (int i = 0; i < this.playerList.Count; i++)
                {
                    this.hardmodePlayers.Add(new CutsceneHunter.HardmodePlayer(this, i));
                }
            }
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (this.room.game.manager.fadeToBlack < 1f)
            {
                if (ModManager.CoopAvailable)
                {
                    this.CoopModeUpdate();
                    return;
                }
                this.SinglePlayerUpdate();
            }
        }

        private void SinglePlayerUpdate()
        {
            Player player = this.room.game.Players[0].realizedCreature as Player;
            if (this.phase == CutsceneHunter.Phase.Init)
            {
                if (player != null)
                {
                    if (!this.playerPosCorrect)
                    {
                        this.playerPosCorrect = true;
                        this.startController = new CutsceneHunter.StartController(new CutsceneHunter.HardmodePlayer(this, 0));
                        player.controller = this.startController;
                        player.standing = true;
                    }
                }
                if (this.playerPosCorrect)
                {
                    this.phase = CutsceneHunter.Phase.PlayerRun;
                    return;
                }
            }
            else if (this.phase == CutsceneHunter.Phase.PlayerRun)
            {
                this.startController.owner.playerMovGiveUpCounter++;
                if (this.startController.owner.playerAction > 8 || this.startController.owner.playerMovGiveUpCounter > 400)
                {
                    this.phase = CutsceneHunter.Phase.End;
                    return;
                }
            }
            else if (this.phase == CutsceneHunter.Phase.End)
            {
                if (player != null)
                {
                    player.controller = null;
                    this.room.game.cameras[0].followAbstractCreature = player.abstractCreature;
                }
                this.Destroy();
            }
        }

        private void CoopModeUpdate()
        {
            bool flag = true;
            foreach (CutsceneHunter.HardmodePlayer hardmodePlayer in this.hardmodePlayers)
            {
                hardmodePlayer.Update();
                if (hardmodePlayer.phase != CutsceneHunter.Phase.ResumedControl)
                {
                    flag = false;
                }
            }
            if (flag)
            {
                this.Destroy();
            }
        }


        public bool camPosCorrect;
        public bool playerPosCorrect;
        public CutsceneHunter.StartController startController;
        public List<Player> playerList;
        public List<CutsceneHunter.HardmodePlayer> hardmodePlayers;
        private CutsceneHunter.Phase phase;

        public class Phase : ExtEnum<CutsceneHunter.Phase>
        {
            public Phase(string value, bool register = false) : base(value, register)
            {
            }

            public static readonly CutsceneHunter.Phase Init = new CutsceneHunter.Phase("Init", true);
            public static readonly CutsceneHunter.Phase Wait = new CutsceneHunter.Phase("Wait", true);
            public static readonly CutsceneHunter.Phase PlayerRun = new CutsceneHunter.Phase("PlayerRun", true);
            public static readonly CutsceneHunter.Phase CleanUp = new CutsceneHunter.Phase("CleanUp", true);
            public static readonly CutsceneHunter.Phase End = new CutsceneHunter.Phase("End", true);
            public static readonly CutsceneHunter.Phase ResumedControl = new CutsceneHunter.Phase("ResumedControl", true);
        }

        public class StartController : Player.PlayerController
        {
            public StartController(CutsceneHunter.HardmodePlayer owner)
            {
                this.owner = owner;
            }

            public override Player.InputPackage GetInput()
            {
                return this.owner.GetInput();
            }

            public CutsceneHunter.HardmodePlayer owner;
        }

        public class HardmodePlayer
        {
            public bool MainPlayer
            {
                get
                {
                    Player player = this.Player;
                    return player != null && player.playerState.playerNumber == 0;
                }
            }

            public Player Player
            {
                get
                {
                    Room room = this.owner.room;
                    return (Player)((room != null) ? room.game.Players[this.playerNumber].realizedCreature : null);
                }
            }

            public HardmodePlayer(CutsceneHunter owner, int playerNumber)
            {
                this.phase = CutsceneHunter.Phase.Init;
                this.owner = owner;
                this.playerNumber = playerNumber;
            }

            public void Update()
            {
                if (this.phase == CutsceneHunter.Phase.Init)
                {
                    if (this.MainPlayer && !this.owner.room.game.cameras[0].InCutscene && this.Player != null)
                    {
                        this.owner.room.game.cameras[0].EnterCutsceneMode(this.Player.abstractCreature, RoomCamera.CameraCutsceneType.HunterStart);
                    }
                    if (this.Player != null)
                    {
                        if (!this.playerPosCorrect)
                        {
                            this.backUpRunSpeed = this.Player.slugcatStats.runspeedFac;
                            this.Player.slugcatStats.runspeedFac = 1.2f;
                            this.playerPosCorrect = true;
                            this.Player.controller = new CutsceneHunter.StartController(this);
                            this.Player.standing = true;
                        }
                    }
                    if (this.playerPosCorrect)
                    {
                        this.phase = CutsceneHunter.Phase.Wait;
                        return;
                    }
                }
                else if (this.phase == CutsceneHunter.Phase.Wait)
                {
                    int num = 3;
                    if (this.Player.slugcatStats.runspeedFac > 1.2f)
                    {
                        num = 5;
                    }
                    else if (this.Player.slugcatStats.runspeedFac < 0.7f)
                    {
                        num = 2;
                    }
                    if (this.MainPlayer || this.owner.hardmodePlayers[this.playerNumber - 1].playerAction == num)
                    {
                        this.phase = CutsceneHunter.Phase.PlayerRun;
                        return;
                    }
                    this.playerAction = 0;
                    return;
                }
                else if (this.phase == CutsceneHunter.Phase.PlayerRun)
                {
                    this.playerMovGiveUpCounter++;
                    if (this.playerAction > 8 || this.playerMovGiveUpCounter > 400)
                    {
                        this.phase = CutsceneHunter.Phase.CleanUp;
                        return;
                    }
                }
                else if (this.phase == CutsceneHunter.Phase.CleanUp)
                {
                    this.owner.room.game.cameras[0].ExitCutsceneMode();
                    if (this.Player != null)
                    {
                        this.Player.controller = null;
                        this.Player.slugcatStats.runspeedFac = this.backUpRunSpeed;
                        if (this.MainPlayer)
                        {
                            this.owner.room.game.cameras[0].followAbstractCreature = this.Player.abstractCreature;
                        }
                        this.phase = CutsceneHunter.Phase.ResumedControl;
                    }
                }
            }

            public Player.InputPackage GetInput()
            {
                if (this.Player == null)
                {
                    return new Player.InputPackage(false, Options.ControlSetup.Preset.None, 0, 0, false, false, false, false, false);
                }
                int x = 0;
                int y = 0;
                this.inActionCounter++;
                if(this.inActionCounter < 100)
                {
                    x = -1;
                    y = 1;
                }
                return new Player.InputPackage(false, Options.ControlSetup.Preset.KeyboardSinglePlayer, x, y, false, false, false, false, false);
            }

            private CutsceneHunter owner;
            public CutsceneHunter.Phase phase;
            public int jumpCounter = -1;
            public int playerMovGiveUpCounter;
            public int playerAction;
            public int inActionCounter;
            public int delayBetweenPlayers = 55;
            public bool playerPosCorrect;
            public int playerNumber;
            public float backUpRunSpeed;
        }
    }

    public class EndingCutsceneHunter : UpdatableAndDeletable
    {
        public EndingCutsceneHunter(Room room)
        {
            this.room = room;
            this.phase = EndingCutsceneHunter.Phase.Init;
            this.playerList = (from x in room.game.Players
                               select (Player)x.realizedCreature).ToList<Player>();
            if (ModManager.CoopAvailable)
            {
                this.hardmodePlayers = new List<EndingCutsceneHunter.HardmodePlayer>();
                for (int i = 0; i < this.playerList.Count; i++)
                {
                    this.hardmodePlayers.Add(new EndingCutsceneHunter.HardmodePlayer(this, i));
                }
            }
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (this.room.game.manager.fadeToBlack < 1f)
            {
                if (ModManager.CoopAvailable)
                {
                    this.CoopModeUpdate();
                    return;
                }
                this.SinglePlayerUpdate();
            }
        }

        private void SinglePlayerUpdate()
        {
            Player player = this.room.game.Players[0].realizedCreature as Player;
            if (this.phase == Phase.Init)
            {
                if (player != null)
                {
                    if (!this.playerPosCorrect)
                    {
                        this.playerPosCorrect = true;
                        this.startController = new EndingCutsceneHunter.StartController(new EndingCutsceneHunter.HardmodePlayer(this, 0));
                        player.controller = this.startController;
                        player.standing = true;
                    }
                }
                if (this.playerPosCorrect)
                {
                    this.phase = Phase.PlayerRun;
                    return;
                }
            }
            else if (this.phase == Phase.PlayerRun)
            {
                this.startController.owner.playerMovGiveUpCounter++;
                if (NSHOracleMeetHunter.playerWaitDeath)
                {
                    this.phase = Phase.WaitDeath;
                    return;
                }
            }
            else if (this.phase == Phase.WaitDeath)
            {
                if (player.dead)
                {
                    this.phase = Phase.End;
                    return;
                }
            }
            else if (this.phase == Phase.End)
            {
                if (player != null)
                {
                    player.controller = null;
                    this.room.game.cameras[0].followAbstractCreature = player.abstractCreature;
                }
                this.Destroy();
            }
        }

        private void CoopModeUpdate()
        {
            bool flag = true;
            foreach (EndingCutsceneHunter.HardmodePlayer hardmodePlayer in this.hardmodePlayers)
            {
                hardmodePlayer.Update();
                if (hardmodePlayer.phase != Phase.ResumedControl)
                {
                    flag = false;
                }
            }
            if (flag)
            {
                this.Destroy();
            }
        }


        public bool camPosCorrect;
        public bool playerPosCorrect;
        public EndingCutsceneHunter.StartController startController;
        public List<Player> playerList;
        public List<EndingCutsceneHunter.HardmodePlayer> hardmodePlayers;
        private EndingCutsceneHunter.Phase phase;

        public class Phase : ExtEnum<EndingCutsceneHunter.Phase>
        {
            public Phase(string value, bool register = false) : base(value, register)
            {
            }

            public static readonly EndingCutsceneHunter.Phase Init = new EndingCutsceneHunter.Phase("Init", true);
            public static readonly EndingCutsceneHunter.Phase Wait = new EndingCutsceneHunter.Phase("Wait", true);
            public static readonly EndingCutsceneHunter.Phase PlayerRun = new EndingCutsceneHunter.Phase("PlayerRun", true);
            public static readonly EndingCutsceneHunter.Phase WaitDeath = new EndingCutsceneHunter.Phase("WaitDeath", true);
            public static readonly EndingCutsceneHunter.Phase CleanUp = new EndingCutsceneHunter.Phase("CleanUp", true);
            public static readonly EndingCutsceneHunter.Phase End = new EndingCutsceneHunter.Phase("End", true);
            public static readonly EndingCutsceneHunter.Phase ResumedControl = new EndingCutsceneHunter.Phase("ResumedControl", true);
        }

        public class StartController : Player.PlayerController
        {
            public StartController(EndingCutsceneHunter.HardmodePlayer owner)
            {
                this.owner = owner;
            }

            public override Player.InputPackage GetInput()
            {
                return this.owner.GetInput();
            }

            public EndingCutsceneHunter.HardmodePlayer owner;
        }

        public class HardmodePlayer
        {
            public bool MainPlayer
            {
                get
                {
                    Player player = this.Player;
                    return player != null && player.playerState.playerNumber == 0;
                }
            }

            public Player Player
            {
                get
                {
                    Room room = this.owner.room;
                    return (Player)((room != null) ? room.game.Players[this.playerNumber].realizedCreature : null);
                }
            }

            public HardmodePlayer(EndingCutsceneHunter owner, int playerNumber)
            {
                this.phase = Phase.Init;
                this.owner = owner;
                this.playerNumber = playerNumber;
            }

            public void Update()
            {
                if (this.phase == Phase.Init)
                {
                    if (this.MainPlayer && !this.owner.room.game.cameras[0].InCutscene && this.Player != null)
                    {
                        this.owner.room.game.cameras[0].EnterCutsceneMode(this.Player.abstractCreature, RoomCamera.CameraCutsceneType.HunterStart);
                    }
                    if (this.Player != null)
                    {
                        if (!this.playerPosCorrect)
                        {
                            this.backUpRunSpeed = this.Player.slugcatStats.runspeedFac;
                            this.Player.slugcatStats.runspeedFac = 1.2f;
                            this.playerPosCorrect = true;
                            this.Player.controller = new EndingCutsceneHunter.StartController(this);
                            this.Player.standing = true;
                        }
                    }
                    if (this.playerPosCorrect)
                    {
                        this.phase = Phase.Wait;
                        return;
                    }
                    }
                    else if (this.phase == Phase.Wait)
                {
                    int num = 3;
                    if (this.Player.slugcatStats.runspeedFac > 1.2f)
                    {
                        num = 5;
                    }
                    else if (this.Player.slugcatStats.runspeedFac < 0.7f)
                    {
                        num = 2;
                    }
                    if (this.MainPlayer || this.owner.hardmodePlayers[this.playerNumber - 1].playerAction == num)
                    {
                        this.phase = Phase.PlayerRun;
                        return;
                    }
                    this.playerAction = 0;
                    return;
                }
                else if (this.phase == Phase.PlayerRun)
                {
                    this.playerMovGiveUpCounter++;
                    if (NSHOracleMeetHunter.playerWaitDeath)
                    {
                        this.phase = Phase.WaitDeath;
                        return;
                    }/*
                    if (this.playerAction > 8 || this.playerMovGiveUpCounter > 400)
                    {
                        this.phase = Phase.CleanUp;
                        return;
                    }*/
                }
                else if (this.phase == Phase.WaitDeath)
                {
                    if (this.MainPlayer && this.Player.dead)
                    {
                        this.phase = Phase.CleanUp;
                        return;
                    }
                }
                else if (this.phase == Phase.CleanUp)
                {
                    this.owner.room.game.cameras[0].ExitCutsceneMode();
                    if (this.Player != null)
                    {
                        this.Player.controller = null;
                        this.Player.slugcatStats.runspeedFac = this.backUpRunSpeed;
                        if (this.MainPlayer)
                        {
                            this.owner.room.game.cameras[0].followAbstractCreature = this.Player.abstractCreature;
                        }
                        this.phase = Phase.ResumedControl;
                    }
                }
            }

            public Player.InputPackage GetInput()
            {
                if (this.Player == null)
                {
                    return new Player.InputPackage(false, Options.ControlSetup.Preset.None, 0, 0, false, false, false, false, false);
                }
                int x = 0;
                int y = 0;
                this.inActionCounter++;
                //如果玩家还没有靠近NSH，则靠近NSH
                if (!NSHOracleMeetHunter.playerWaitDeath)
                {
                    x = (this.Player.DangerPos.x < NSHOracleMeetHunter.oraclePos.x) ? 1 : -1;
                    y = 1;
                }
                //如果玩家足够靠近NSH，并且还没有趴下，则趴下
                else if(!NSHOracleMeetHunter.isControled && this.Player.bodyMode != Player.BodyModeIndex.Crawl)
                {
                    x = 0;
                    y = -1;
                }
                //如果NSH说完话，玩家开始蜷缩身体
                else if(NSHOracleMeetHunter.isControled)
                {
                    x = 0;
                    y = -1;
                }
                else
                {
                    x = 0;
                    y = 0;
                }
                return new Player.InputPackage(false, Options.ControlSetup.Preset.KeyboardSinglePlayer, x, y, false, false, false, false, false);
            }

            private EndingCutsceneHunter owner;
            public EndingCutsceneHunter.Phase phase;
            public int jumpCounter = -1;
            public int playerMovGiveUpCounter;
            public int playerAction;
            public int inActionCounter;
            public int delayBetweenPlayers = 55;
            public bool playerPosCorrect;
            public int playerNumber;
            public float backUpRunSpeed;
        }
    }
}
