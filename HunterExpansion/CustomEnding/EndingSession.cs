using CustomDreamTx;
using HunterExpansion.CustomOracle;
using HunterExpansion.CustomSave;
using System.Collections.Generic;
using UnityEngine;
using RWCustom;
using MoreSlugcats;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using Harmony;
using SlugBase.Features;
using System.Linq;
using HunterExpansion.CustomDream;
using HunterExpansion.CustomEffects;

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

        //结局后的结局
        public static bool goEnding = false;

        public static void Init()
        {
            On.Player.ctor += Player_ctor;
            On.Player.UpdateMSC += Player_EndUpdate;
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
            if (self.room.IsGateRoom() && PearlFixedSave.pearlFixed && openGate &&
                self == self.room.game.Players[0].realizedCreature as Player)
            {
                if (self.room.game.world.region.name == "NSH")
                {
                    if (self.firstChunk.pos.x > 100f && self.firstChunk.pos.x < 300f)
                    {
                        if (!isControled)
                        {
                            isControled = true;
                            self.room.AddObject(new CutsceneHunter(self.room));
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
                        WarpWithinTheSameRegion(self, "NSH_AI", 24, 34);
                        self.room.game.GoToRedsGameOver();
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
