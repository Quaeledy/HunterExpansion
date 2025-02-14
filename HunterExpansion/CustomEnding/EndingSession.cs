using HunterExpansion.CustomDream;
using HunterExpansion.CustomEffects;
using HunterExpansion.CustomOracle;
using HunterExpansion.CustomSave;
using MoreSlugcats;
using RWCustom;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

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
            if (self.room.game.devToolsActive && Input.GetKey(KeyCode.LeftControl))
            {
                Plugin.Log("Player Pos: " + self.DangerPos);
            }
            if (self.room.game.devToolsActive && Input.GetKey(KeyCode.LeftControl) && Input.GetKey("6"))
            {
                PearlFixedSave.pearlFixed = true;
            }
            if (self.room.game.devToolsActive && Input.GetKey(KeyCode.LeftControl) && Input.GetKey("7"))
            {
                self.room.game.rainWorld.progression.currentSaveState.miscWorldSaveData.SLOracleState.neuronsLeft = 5;
            }
            /*
            if (self.room.game.session.characterStats.name != Plugin.SlugName)
                return;

            //播放结局cg
            if (self.room.game.session.characterStats.name == Plugin.SlugName && 
                self.room.abstractRoom.name == "GATE_SB_OE" && PearlFixedSave.pearlFixed && openGate &&
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
                            if (self.room != null && self.room.game != null && self.room.game.manager != null && self.room.game.manager.musicPlayer != null)
                            //下面这一行在结局时NullReferenceException: Object reference not set to an instance of an object
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
                                        Plugin.Log(string.Format("Could not warp player {0} [{1}]", abstractCreature, arg), false);
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
                                Plugin.Log(string.Format("Could not warp player {0} [{1}]", self.abstractCreature, arg), false);
                            }
                        }
                        if (!self.room.game.rainWorld.progression.miscProgressionData.regionsVisited.ContainsKey(self.room.world.name))
                        {
                            self.room.game.rainWorld.progression.miscProgressionData.regionsVisited.Add(self.room.world.name, new List<string>());
                        }
                        if (!self.room.game.rainWorld.progression.miscProgressionData.regionsVisited[self.room.world.name].Contains(self.room.game.GetStorySession.saveStateNumber.value))
                        {
                            self.room.game.rainWorld.progression.miscProgressionData.regionsVisited[self.room.world.name].Add(self.room.game.GetStorySession.saveStateNumber.value);
                        }
                        Plugin.Log("Warp and End!");
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
            */
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
                    if (self.room.abstractRoom.name == "GATE_SB_NSH")
                    {
                        wantPos = (self.firstChunk.pos.x > 560f) ? new Vector2(650f, 640f) : new Vector2(470f, 640f);
                    }
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
                if ((openCount == 50 || openCount == 100) && self.room.game.session.characterStats.name == Plugin.SlugName)
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
                    if (Random.value > 0.75f && self.room.game.session.characterStats.name == Plugin.SlugName)
                    {
                        self.room.PlaySound(SoundID.Moon_Wake_Up_Green_Swarmer_Flash, fixedPearl.firstChunk.pos, 0.5f, 1f);
                        self.room.AddObject(new ElectricFullScreen.SparkFlash(fixedPearl.firstChunk.pos, 20f));
                        self.room.AddObject(new Spark(fixedPearl.firstChunk.pos, Custom.RNV() * Random.value * 40f, new Color(0f, 1f, 0f), null, 30, 120));
                    }
                    else
                    {
                        self.room.PlaySound(SoundID.Moon_Wake_Up_Green_Swarmer_Flash, fixedPearl.firstChunk.pos, 0.5f, 1f);
                        self.room.PlaySound(SoundID.Fire_Spear_Explode, fixedPearl.firstChunk.pos, 0.5f, 1f);
                        self.room.AddObject(new ElectricFullScreen.SparkFlash(fixedPearl.firstChunk.pos, 50f));
                        self.room.AddObject(new Spark(fixedPearl.firstChunk.pos, Custom.RNV() * Random.value * 40f, new Color(0f, 1f, 0f), null, 30, 120));
                        fixedPearl.Destroy();
                    }
                }
                if (openCount == 300)
                {
                    openCount = 0;
                    openGate = true;
                    if (fixedPearl.slatedForDeletetion)
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
            if (self.room.game.session.characterStats.name == Plugin.SlugName && goEnding)
            {
                if (!isControled)
                {
                    isControled = true;
                    self.room.AddObject(new EndingCutsceneHunter(self.room));
                    /*
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
                    }*/
                }
                if (self.dead)
                {
                    isControled = false;
                    goEnding = false;
                }
            }
        }

        /*
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
        */

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
                if (this.inActionCounter < 100)
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
                else if (!NSHOracleMeetHunter.isControled && this.Player.bodyMode != Player.BodyModeIndex.Crawl)
                {
                    x = 0;
                    y = -1;
                }
                //如果NSH说完话，玩家开始蜷缩身体
                else if (NSHOracleMeetHunter.isControled)
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
