using System.Collections.Generic;
using CustomOracleTx;
using UnityEngine;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using RWCustom;
using CustomDreamTx;
using HunterExpansion.CustomDream;
using CoralBrain;
using Music;
using HunterExpansion.CustomSave;
using MoreSlugcats;
using System.Text.RegularExpressions;
using BepInEx;
using IL;
using On;
using RewiredConsts;
using UnityEngine.Diagnostics;
using Kittehface.Framework20;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Networking.PlayerConnection;
using System.Data;
using System;
using CustomRegions.Mod;
using CustomRegions.Collectables;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Reflection;

namespace HunterExpansion.CustomOracle
{
    public class NSHOracleBehaviour : CustomOracleBehaviour
    {
        public NSHOracleState State
        {
            get
            {
                if (this.oracle.room.game.session is StoryGameSession)
                {
                    return NSHOracleStateSave.NSHOracleState;
                }
                if (this.DEBUGSTATE == null)
                {
                    this.DEBUGSTATE = new NSHOracleState(true, null);
                }
                return this.DEBUGSTATE;
            }
        }

        private NSHOracleState DEBUGSTATE;
        private CustomSubBehaviour subBehavior;
        public new NSHConversation conversation;

        //读珍珠、物品相关
        public NSHConversation itemConversation;
        public NSHConversation interruptConversation;
        public PhysicalObject inspectItem;
        public List<DataPearl.AbstractDataPearl> readDataPearlOrbits;
        public List<AbstractWorldEntity> readItemOrbits;
        public Dictionary<DataPearl.AbstractDataPearl, GlyphLabel> readPearlGlyphs;
        public List<EntityID> talkedAboutThisSession;
        public PhysicalObject holdingObject;
        public bool refuseRestartConversation;
        public new bool restartConversationAfterCurrentDialoge;//base.Update会在NSH重新开始解读时令this.restartConversationAfterCurrentDialoge = false，从而变成NSH无限循环播放重新解读的句子，所以要new一个

        //遇到其他迭代器人偶
        //public Oracle inspectOracle;

        //梦境相关
        public bool setDream = false;
        public bool setPutDown = false;

        //结局相关
        public Vector2 landPos = Vector2.zero;
        /*
        public bool tryGivePupMark = false;
        public bool hasGivePupMark = false;
        public bool tryTalkToPup = false;
        public bool hasTalkToPup = false;*/

        //好感度相关
        public static bool generateKillingIntent = false;

        //展示图像相关
        public ProjectedImage showImage;
        public Vector2 idealShowMediaPos;
        public Vector2 showMediaPos;
        public int consistentShowMediaPosCounter;
        public OracleChatLabel chatLabel;

        public override int NotWorkingPalette => 25;
        public override int GetWorkingPalette => 39119;//39119 //26

        public override Vector2 GetToDir => Vector2.up;

        public NSHOracleBehaviour(Oracle oracle) : base(oracle)
        {
            //this.movementBehavior = ((Random.value < 0.5f) ? NSHOracleMovementBehavior.Meditate : CustomMovementBehavior.Idle);//0.5f
            this.action = (Random.value < 0.5f) ? NSHOracleBehaviorAction.General_Meditate : CustomAction.General_Idle;
            this.talkedAboutThisSession = new List<EntityID>();
            this.oracle.health = (RipNSHSave.ripNSH && oracle.room.world.region != null && oracle.room.world.region.name != "HR") ? 0f : 1f;
            this.InitStoryPearlCollection();

            setDream = false;
            setPutDown = false; 
            generateKillingIntent = false;
            landPos = Vector2.zero;
            
            //清除旧梦境的影响
            if (NSHOracleMeetHunter.nshSwarmer != null)
            {
                NSHOracleMeetHunter.nshSwarmer.Destroy();
                NSHOracleMeetHunter.nshSwarmer = null;
            }
        }

        public override void Update(bool eu)
        {
            if(oracle.room.game.devToolsActive && Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.L))
                this.State.InfluenceLike(1f);
            if (player != null && generateKillingIntent)
            {
                KillPlayerUpdate();
            }
            if (player != null && player.room != null && player.room.world.region != null && player.room.world.region.name == "HR" && !(subBehavior is NSHOracleRubicon))
                NewAction(NSHOracleBehaviorAction.Rubicon_Init);
            if (player != null && player.room != null && player.room.abstractRoom.name == "NSH_AI" &&
                player.room.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel &&
                !(subBehavior is NSHOracleMeetSofanthiel))
                NewAction(NSHOracleBehaviorAction.MeetSofanthiel_Idle);
            if (player != null && player.room != null && player.room.abstractRoom.name == "NSH_AI" &&
                player.room.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Saint &&
                !(subBehavior is NSHOracleMeetSaint))
                NewAction(NSHOracleBehaviorAction.MeetSaint_Idle);
            if (CustomDreamRx.currentActivateNormalDream != null &&
                CustomDreamRx.currentActivateNormalDream.activateDreamID == DreamID.HunterDream_1 &&
                !setDream)
            {
                setDream = true;
                NewAction(NSHOracleBehaviorAction.MeetHunter_Init);
            }
            /*
            //这是假如把其他迭代器人偶拿进来
            if (this.player != null && this.player.room == this.oracle.room)
            {
                List<PhysicalObject>[] physicalObjects = this.oracle.room.physicalObjects;
                for (int i = 0; i < physicalObjects.Length; i++)
                {
                    for (int j = 0; j < physicalObjects[i].Count; j++)
                    {
                        PhysicalObject physicalObject = physicalObjects[i][j];
                        if (physicalObject is Oracle && inspectOracle == null)
                        {
                            if ((physicalObject as Oracle).ID == Oracle.OracleID.SL || (physicalObject as Oracle).ID == MoreSlugcatsEnums.OracleID.DM ||
                                (physicalObject as Oracle).ID == Oracle.OracleID.SS || (physicalObject as Oracle).ID == MoreSlugcatsEnums.OracleID.CL)
                            {
                                inspectOracle = (Oracle) physicalObject;
                                NewAction(MeetOracle_Init);
                            }
                        }
                    }
                }
            }
            if (inspectOracle != null)
            {
                inspectOracle.stun = Mathf.Max(oracle.stun, Random.Range(2, 5)); 
                Vector2 wantPos = oracle.firstChunk.pos;
                inspectOracle.firstChunk.vel *= Custom.LerpMap(inspectOracle.firstChunk.vel.magnitude, 1f, 6f, 0.999f, 0.9f);
                inspectOracle.firstChunk.vel += Vector2.ClampMagnitude(wantPos - inspectOracle.firstChunk.pos, 100f) / 100f * 0.4f;
            }
            */

            if (this.oracle.health == 0f)
            {
                this.getToWorking = 0f;
            }
            /*
            //防止NSH总是倒立
            if (oracle.room.abstractRoom.name == "HR_AI" && oracle != null && oracle.room != null)
            {
                Vector2 headPos = (this.oracle.graphicsModule as OracleGraphics).head.pos;
                Vector2 handsPos = 0.5f * ((this.oracle.graphicsModule as OracleGraphics).hands[0].pos + (this.oracle.graphicsModule as OracleGraphics).hands[1].pos);
                if (headPos.y < handsPos.y && GetToDir.y > 0)
                {
                    (this.oracle.graphicsModule as OracleGraphics).head.vel += new Vector2(headPos.x - handsPos.x > 0 ? 10f : 10f, 10f);
                }
            }*/

            //这是更新NSH房间收藏的珍珠
            this.UpdateStoryPearlCollection();
            //这个if判断是base.Update()的内容
            if (this.timeSinceSeenPlayer >= 0)
            {
                this.timeSinceSeenPlayer++;
            }
            //这是被拿房间里的演算珍珠
            /*
            if (this.pearlPickupReaction && this.timeSinceSeenPlayer > 300 && this.oracle.room.game.IsStorySession && this.oracle.room.game.GetStorySession.saveState.deathPersistentSaveData.theMark && (!(this.currSubBehavior is SSOracleBehavior.ThrowOutBehavior) || this.action == SSOracleBehavior.Action.ThrowOut_Polite_ThrowOut))
            {
                bool flag = false;
                if (this.player != null)
                {
                    for (int k = 0; k < this.player.grasps.Length; k++)
                    {
                        if (this.player.grasps[k] != null && this.player.grasps[k].grabbed is PebblesPearl)
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                if (ModManager.MSC && this.oracle.room.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Spear)
                {
                    flag = false;
                }
                if (flag && !this.lastPearlPickedUp && (this.conversation == null || (this.conversation.age > 300 && !this.conversation.paused)))
                {
                    if (this.conversation != null)
                    {
                        this.conversation.paused = true;
                        this.restartConversationAfterCurrentDialoge = true;
                    }
                    this.dialogBox.Interrupt(this.Translate("Yes, help yourself. They are not edible."), 10);
                    this.pearlPickupReaction = false;
                }
                this.lastPearlPickedUp = flag;
            }
            */
            //这是对话
            if (this.conversation != null)
            {
                if (this.restartConversationAfterCurrentDialoge && this.conversation.paused &&
                    this.action != CustomAction.General_GiveMark && this.dialogBox.messages.Count == 0 &&
                    (!ModManager.MSC || this.player.room == this.oracle.room))
                {
                    this.conversation.paused = false;
                    this.restartConversationAfterCurrentDialoge = false;
                    this.conversation.RestartCurrent();
                }
                //退出房间时this.player == null，所以base.Update(eu)要写在这一段下面？
                if ((this.player == null) ||
                    (this.player != null && this.player.room != null && this.player.room != this.oracle.room))//算了我还是加一个this.player == null检查吧
                {
                    if (!this.conversation.paused)
                    {
                        this.conversation.paused = true;
                        this.State.InfluenceLike(-0.1f);
                        State.totalInterruptions++;
                        if (CustomDreamRx.currentActivateNormalDream == null)
                        {
                            Plugin.Log("NSH Normal Conversation Interrupt!");
                            this.conversation.Interrupt(this.Translate("..."), 0);
                            //this.dialogBox.NewMessage(this.Translate("..."), 10);
                            this.getToWorking = 1f;
                            //this.action = (Random.value < 0.5f) ? NSHOracleBehaviorAction.General_Meditate : CustomAction.General_Idle;
                        }
                    }
                }
            }
            else if (ModManager.MSC && this.itemConversation != null)
            {
                if (this.itemConversation.slatedForDeletion)
                {
                    movementBehavior = CustomMovementBehavior.Idle;
                    this.itemConversation = null;
                    if (this.inspectItem != null)
                    {
                        this.inspectItem.firstChunk.vel = Custom.DirVec(this.inspectItem.firstChunk.pos, this.player.mainBodyChunk.pos) * 3f;
                        if (this.inspectItem is DataPearl)
                        {
                            this.readDataPearlOrbits.Add((this.inspectItem as DataPearl).AbstractPearl);
                        }
                        else if (this.inspectItem is FireEgg)
                        {
                            this.readItemOrbits.Add((this.inspectItem as FireEgg).abstractPhysicalObject as AbstractWorldEntity);
                        }
                        this.inspectItem = null;
                    }
                }
                else
                {
                    this.itemConversation.Update();
                    if (this.player != null)
                    {
                        this.lookPoint = this.player.firstChunk.pos;
                    }
                    //退出房间时this.player == null，所以base.Update(eu)要写在这一段下面？
                    if (this.player == null ||
                        (this.player.room != null && this.player.room != this.oracle.room) ||
                        this.inspectItem.grabbedBy.Count > 0)//算了我还是加一个this.player == null检查吧
                    {
                        if (!this.itemConversation.paused)
                        {
                            this.itemConversation.paused = true;
                            this.InterruptPearlMessagePlayerLeaving();
                            State.totalInterruptions++;
                        }
                    }
                    else if (this.itemConversation.paused && !this.restartConversationAfterCurrentDialoge && this.inspectItem.grabbedBy.Count == 0)
                    {
                        this.ResumePauseditemConversation();
                    }
                    //两种重新把珍珠给NSH的情况
                    if (this.itemConversation.paused && this.refuseRestartConversation && this.dialogBox.messages.Count == 0)//这句作为更严重的后果应该放前面
                    {
                        this.itemConversation.paused = false;
                        this.refuseRestartConversation = false;
                        this.restartConversationAfterCurrentDialoge = false;
                        this.itemConversation.Destroy();
                    }
                    else if (this.itemConversation.paused && this.restartConversationAfterCurrentDialoge && this.dialogBox.messages.Count == 0 && this.interruptConversation == null)
                    {
                        this.itemConversation.paused = false;
                        this.restartConversationAfterCurrentDialoge = false;
                        this.itemConversation.RestartCurrent();
                    }
                }
            }
            else
            {
                this.restartConversationAfterCurrentDialoge = false;
            }

            #region base.Update()
            if (this.voice != null)
            {
                this.voice.alive = true;
                if (this.voice.slatedForDeletetion)
                {
                    this.voice = null;
                }
            }
            if (ModManager.MSC && this.oracle.room != null && this.oracle.room.game.rainWorld.safariMode)
            {
                this.safariCreature = null;
                float num = float.MaxValue;
                for (int i = 0; i < this.oracle.room.abstractRoom.creatures.Count; i++)
                {
                    if (this.oracle.room.abstractRoom.creatures[i].realizedCreature != null)
                    {
                        Creature realizedCreature = this.oracle.room.abstractRoom.creatures[i].realizedCreature;
                        float num2 = Custom.Dist(this.oracle.firstChunk.pos, realizedCreature.mainBodyChunk.pos);
                        if (num2 < num)
                        {
                            num = num2;
                            this.safariCreature = realizedCreature;
                        }
                    }
                }
            }
            this.FindPlayer();

            if (!this.oracle.Consious)
            {
                return;
            }
            this.unconciousTick = 0f;
            this.currSubBehavior.Update();
            if (this.oracle.slatedForDeletetion)
            {
                return;
            }
            if (this.conversation != null)
            {
                this.conversation.Update();
            }
            if (this.interruptConversation != null)
            {
                this.interruptConversation.Update();
                if(this.interruptConversation.slatedForDeletion)
                    this.interruptConversation = null;
            }
            if (!this.currSubBehavior.CurrentlyCommunicating)
            {
                this.pathProgression = this.UpdatePathProgression();
            }
            this.currentGetTo = Custom.Bezier(this.lastPos, this.ClampVectorInRoom(this.lastPos + this.lastPosHandle), this.nextPos, this.ClampVectorInRoom(this.nextPos + this.nextPosHandle), this.pathProgression);
            this.floatyMovement = false;
            this.investigateAngle += this.invstAngSpeed;
            this.inActionCounter++;
            if (this.player != null && this.player.room == this.oracle.room)
            {
                this.PlayerStayInRoomUpdate();
                this.playerOutOfRoomCounter = 0;
            }
            else
            {
                this.PlayerOutOfRoomUpdate();
                this.killFac = 0f;
                this.playerOutOfRoomCounter++;
            }
            if (this.pathProgression >= 1f && this.consistentBasePosCounter > 100 && !this.oracle.arm.baseMoving)
            {
                this.allStillCounter++;
            }
            else
            {
                this.allStillCounter = 0;
            }
            this.lastKillFac = this.killFac;
            this.GeneralActionUpdate();
            this.Move();
            if (this.working != this.getToWorking)
            {
                this.working = Custom.LerpAndTick(this.working, this.getToWorking, 0.05f, 0.033333335f);
            }
            for (int i = 0; i < this.oracle.room.game.cameras.Length; i++)
            {
                if (this.oracle.room.game.cameras[i].room == this.oracle.room &&
                    !this.oracle.room.game.cameras[i].AboutToSwitchRoom &&
                    this.oracle.room.game.cameras[i].paletteBlend != this.working)
                {
                    this.oracle.room.game.cameras[i].ChangeBothPalettes(this.NotWorkingPalette, this.GetWorkingPalette, this.working);
                }
            }
            #endregion

            //这是开始阅读珍珠
            if (this.inspectItem != null)
            {
                Vector2 vector = this.oracle.firstChunk.pos - this.inspectItem.firstChunk.pos;
                float num = Custom.Dist(this.oracle.firstChunk.pos, this.inspectItem.firstChunk.pos);
                this.inspectItem.firstChunk.vel += Vector2.ClampMagnitude(vector, 40f) / 40f * Mathf.Clamp(2f - num / 200f * 2f, 0.5f, 2f);
                if (this.inspectItem.firstChunk.vel.magnitude < 1f && num < 16f)
                {
                    this.inspectItem.firstChunk.vel = Custom.RNV() * 8f;
                }
                if (this.inspectItem.firstChunk.vel.magnitude > 8f)
                {
                    this.inspectItem.firstChunk.vel /= 2f;
                }
                if (num < 100f && this.itemConversation == null && this.conversation == null)
                {
                    this.StartItemConversation(this.inspectItem);
                    movementBehavior = CustomMovementBehavior.Talk;
                    if (this.player != null)
                        lookPoint = player.DangerPos;
                }
            }

            //这是找到珍珠
            if (this.player != null && this.player.room == this.oracle.room)
            {
                List<PhysicalObject>[] physicalObjects = this.oracle.room.physicalObjects;
                for (int i = 0; i < physicalObjects.Length; i++)
                {
                    for (int j = 0; j < physicalObjects[i].Count; j++)
                    {
                        PhysicalObject physicalObject = physicalObjects[i][j];
                        if (physicalObject is Weapon)
                        {
                            Weapon weapon = physicalObject as Weapon;
                            if (weapon.mode == Weapon.Mode.Thrown && Custom.Dist(weapon.firstChunk.pos, this.oracle.firstChunk.pos) < 100f)
                            {
                                this.talkedAboutThisSession.Add(physicalObject.abstractPhysicalObject.ID);
                                weapon.ChangeMode(Weapon.Mode.Free);
                                weapon.SetRandomSpin();
                                weapon.firstChunk.vel *= -0.2f;
                                for (int num8 = 0; num8 < 5; num8++)
                                {
                                    this.oracle.room.AddObject(new Spark(weapon.firstChunk.pos, Custom.RNV(), Color.white, null, 16, 24));
                                }
                                this.oracle.room.AddObject(new Explosion.ExplosionLight(weapon.firstChunk.pos, 150f, 1f, 8, Color.white));
                                this.oracle.room.AddObject(new ShockWave(weapon.firstChunk.pos, 60f, 0.1f, 8, false));
                                this.oracle.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, weapon.firstChunk, false, 1f, 1.5f + Random.value * 0.5f);
                                if (this.conversation != null)
                                {
                                    this.conversation.Interrupt(this.Translate("..."), 0);
                                    this.conversation = null;
                                }
                                if (State.GetOpinion == NSHOracleState.PlayerOpinion.Likes)
                                {
                                    if (canSlugUnderstandlanguage())
                                        this.dialogBox.NewMessage(this.Translate("What are you doing? Please don't attack me."), 20);
                                    else
                                        this.oracle.room.PlaySound(NSHOracleSoundID.NSH_AI_Attack_1, 10f, 1f, 1f);
                                    State.annoyances++;
                                    State.InfluenceLike(-0.4f);
                                }
                                else if (State.GetOpinion == NSHOracleState.PlayerOpinion.Neutral)
                                {
                                    if (canSlugUnderstandlanguage())
                                        this.dialogBox.NewMessage(this.Translate("Stop this barbaric behavior, animal."), 20);
                                    else
                                        this.oracle.room.PlaySound(NSHOracleSoundID.NSH_AI_Attack_2, 10f, 1f, 1f);
                                    State.annoyances++;
                                    State.InfluenceLike(-0.5f);
                                }
                                else
                                {
                                    if (State.annoyances == 0)
                                    {
                                        if (canSlugUnderstandlanguage())
                                            this.dialogBox.NewMessage(this.Translate("Remember to behave in the next cycle."), 20);
                                        else
                                            this.oracle.room.PlaySound(NSHOracleSoundID.NSH_AI_Attack_3, 10f, 1f, 1f);
                                    }
                                    else
                                    {
                                        if (canSlugUnderstandlanguage())
                                            this.dialogBox.NewMessage(this.Translate("I have already warned you."), 20);
                                        else
                                            this.oracle.room.PlaySound(NSHOracleSoundID.NSH_AI_Attack_4, 10f, 1f, 1f);
                                    }
                                    State.annoyances++;
                                    State.likesPlayer = -1f;
                                    generateKillingIntent = true;
                                }
                            }
                        }
                        bool flag3 = false;
                        //flag4表示了几种NSH不做解读的情况
                        bool flag4 = (this.currSubBehavior is NSHOracleMeetSofanthiel) ||
                                     ((this.currSubBehavior is NSHOracleMeetSaint) && (this.currSubBehavior as NSHOracleMeetSaint).panicObject != null) ||
                                     ((this.currSubBehavior is NSHOracleMeetSpear) && this.rainWorld.progression.miscProgressionData.decipheredPebblesPearls.Contains(MoreSlugcatsEnums.DataPearlType.Spearmasterpearl)) ||
                                     ((this.currSubBehavior is NSHOracleMeetHunter) && oracle.room.game.rainWorld.progression.currentSaveState.miscWorldSaveData.SLOracleState.neuronsLeft <= 0 && !oracle.room.game.rainWorld.progression.currentSaveState.deathPersistentSaveData.altEnding) ||
                                     CustomDreamRx.currentActivateNormalDream != null;
                        if (((this.currSubBehavior is NSHOracleMeetSpear) && (physicalObject is SpearMasterPearl)) ||
                            ((this.currSubBehavior is NSHOracleMeetHunter) && (physicalObject is NSHSwarmer) && CustomDreamRx.currentActivateNormalDream == null))
                        {
                            flag4 = false;
                        }
                        // && this.currSubBehavior is SSOracleBehavior.SSSleepoverBehavior && (this.currSubBehavior as SSOracleBehavior.SSSleepoverBehavior).panicObject == null
                        //if (this.oracle.ID == NSHOracleRegistry.NSHOracle)// && this.oracle.room.game.GetStorySession.saveStateNumber == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Artificer && this.currSubBehavior is SSOracleBehavior.ThrowOutBehavior
                        //{
                        //flag4 = true;
                        //flag3 = true;
                        //}
                        if (this.inspectItem == null && (this.conversation == null || flag3) && !flag4
                            && !(physicalObject is NSHPearl) && !(physicalObject is Oracle) && !(physicalObject is Creature))
                        {
                            //读珍珠
                            if (physicalObject is DataPearl &&
                                (physicalObject as DataPearl).grabbedBy.Count == 0 &&
                                !this.readDataPearlOrbits.Contains((physicalObject as DataPearl).AbstractPearl) &&
                                !this.talkedAboutThisSession.Contains(physicalObject.abstractPhysicalObject.ID))
                            {
                                this.inspectItem = (physicalObject as DataPearl);
                                /*
                                if (!(this.inspectPearl is MoreSlugcats.SpearMasterPearl) ||
                                    !(this.inspectPearl.AbstractPearl as MoreSlugcats.SpearMasterPearl.AbstractSpearMasterPearl).broadcastTagged)
                                {
                                    if (RainWorld.ShowLogs)
                                    {
                                        string str = "---------- INSPECT PEARL TRIGGERED: ";
                                        DataPearl.AbstractDataPearl.DataPearlType dataPearlType = this.inspectPearl.AbstractPearl.dataPearlType;
                                        Debug.Log(str + ((dataPearlType != null) ? dataPearlType.ToString() : null));
                                    }
                                    if (this.inspectPearl is SpearMasterPearl)
                                    {
                                        this.LockShortcuts();
                                        if (this.oracle.room.game.cameras[0].followAbstractCreature.realizedCreature.firstChunk.pos.y > 600f)
                                        {
                                            this.oracle.room.game.cameras[0].followAbstractCreature.realizedCreature.Stun(40);
                                            this.oracle.room.game.cameras[0].followAbstractCreature.realizedCreature.firstChunk.vel = new Vector2(0f, -4f);
                                        }
                                        this.getToWorking = 0.5f;
                                        this.SetNewDestination(new Vector2(600f, 450f));
                                        break;
                                    }
                                    break;
                                }
                                else
                                {
                                    this.inspectPearl = null;
                                }*/
                            }
                            //读物品
                            else if (!(physicalObject is DataPearl) &&
                                     physicalObject.grabbedBy.Count == 0 &&
                                     !((physicalObject is Spear) && player.spearOnBack != null && player.spearOnBack.spear != null && physicalObject == player.spearOnBack.spear) && //不读玩家背上的矛
                                     !this.readItemOrbits.Contains(physicalObject.abstractPhysicalObject) &&
                                     !this.talkedAboutThisSession.Contains(physicalObject.abstractPhysicalObject.ID))
                            {
                                this.inspectItem = physicalObject;
                                if (physicalObject is FireEgg && (physicalObject as FireEgg).activeCounter > 0)
                                {
                                    (physicalObject as FireEgg).activeCounter = 0;
                                }
                            }
                        }
                    }
                }
            }
            if (this.currSubBehavior.LowGravity >= 0f)
            {
                this.oracle.room.gravity = this.currSubBehavior.LowGravity;
                return;
            }
            //这个if else判断是base.Update里的内容
            if (!this.currSubBehavior.Gravity)
            {
                this.oracle.room.gravity = Custom.LerpAndTick(this.oracle.room.gravity, 0f, 0.05f, 0.02f);
            }
            else
            {
                if (!ModManager.MSC || this.oracle.room.world.name != "HR" ||
                    !this.oracle.room.game.IsStorySession || !this.oracle.room.game.GetStorySession.saveState.deathPersistentSaveData.ripMoon ||
                    this.oracle.ID != Oracle.OracleID.SS)
                {
                    this.oracle.room.gravity = 1f - this.working;
                }
            }
        }

        #region 继承方法
        public override void SeePlayer()
        {
            base.SeePlayer();
            Plugin.Log("Oracle see player");

            if (player.room.world.game.StoryCharacter == Plugin.SlugName)
            {
                NewAction(NSHOracleBehaviorAction.MeetHunter_Init);
            }
            else if (player.room.world.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Spear)
            {
                NewAction(NSHOracleBehaviorAction.MeetSpear_Init);
            }
            else if (player.room.world.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
            {
                NewAction(NSHOracleBehaviorAction.MeetArtificer_Init);
            }
            else if (player.room.world.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
            {
                NewAction(NSHOracleBehaviorAction.MeetGourmand_Init);
            }
            else if (player.room.world.game.StoryCharacter == SlugcatStats.Name.White)
            {
                NewAction(NSHOracleBehaviorAction.MeetWhite_Init);
            }
            else if (player.room.world.game.StoryCharacter == SlugcatStats.Name.Yellow)
            {
                NewAction(NSHOracleBehaviorAction.MeetYellow_Init);
            }
            else if (player.room.world.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
            {
                NewAction(NSHOracleBehaviorAction.MeetRivulet_Init);
            }
            else if (player.room.world.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                if (player.room.world.region != null && player.room.world.region.name != "HR")
                    NewAction(NSHOracleBehaviorAction.MeetSaint_Init);
            }/*
            else if (player.room.world.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
            {
                NewAction(MeetSofanthiel_Init);
            }*/
            else
            {
                NewAction(NSHOracleBehaviorAction.MeetOtherSlugcat_Init);
            }
        }

        public override void NewAction(CustomAction nextAction)
        {
            Plugin.Log(string.Concat(new string[]
            {
                "new action: ",
                nextAction.ToString(),
                " (from ",
                action.ToString(),
                ")"
            }));

            if (nextAction == action) return;

            CustomSubBehaviour.CustomSubBehaviourID customSubBehaviourID = null;

            if (NSHOracleMeetHunter.SubBehaviourIsMeetHunter(nextAction))
            {
                customSubBehaviourID = NSHOracleBehaviorSubBehavID.MeetHunter;
            }
            else if (NSHOracleMeetSpear.SubBehaviourIsMeetSpear(nextAction))
            {
                customSubBehaviourID = NSHOracleBehaviorSubBehavID.MeetSpear;
            }
            else if (NSHOracleMeetArtificer.SubBehaviourIsMeetArtificer(nextAction))
            {
                customSubBehaviourID = NSHOracleBehaviorSubBehavID.MeetArtificer;
            }
            else if (NSHOracleMeetGourmand.SubBehaviourIsMeetGourmand(nextAction))
            {
                customSubBehaviourID = NSHOracleBehaviorSubBehavID.MeetGourmand;
            }
            else if (NSHOracleMeetWhite.SubBehaviourIsMeetWhite(nextAction))
            {
                customSubBehaviourID = NSHOracleBehaviorSubBehavID.MeetWhite;
            }
            else if (NSHOracleMeetYellow.SubBehaviourIsMeetYellow(nextAction))
            {
                customSubBehaviourID = NSHOracleBehaviorSubBehavID.MeetYellow;
            }
            else if (NSHOracleMeetRivulet.SubBehaviourIsMeetRivulet(nextAction))
            {
                customSubBehaviourID = NSHOracleBehaviorSubBehavID.MeetRivulet;
            }
            else if (NSHOracleMeetSaint.SubBehaviourIsMeetSaint(nextAction))
            {
                customSubBehaviourID = NSHOracleBehaviorSubBehavID.MeetSaint;
            }
            else if (NSHOracleRubicon.SubBehaviourIsRubicon(nextAction))
            {
                customSubBehaviourID = NSHOracleBehaviorSubBehavID.Rubicon;
            }
            else if (NSHOracleMeetSofanthiel.SubBehaviourIsMeetSofanthiel(nextAction))
            {
                customSubBehaviourID = NSHOracleBehaviorSubBehavID.MeetSofanthiel;
            }
            else if (NSHOracleMeetOtherSlugcat.SubBehaviourIsMeetOtherSlugcat(nextAction))
            {
                customSubBehaviourID = NSHOracleBehaviorSubBehavID.MeetOtherSlugcat;
            }
            else
                customSubBehaviourID = CustomSubBehaviour.CustomSubBehaviourID.General;

            currSubBehavior.NewAction(action, nextAction);

            if (customSubBehaviourID != CustomSubBehaviour.CustomSubBehaviourID.General && customSubBehaviourID != currSubBehavior.ID)
            {
                //CustomSubBehaviour subBehavior = null;
                for (int i = 0; i < allSubBehaviors.Count; i++)
                {
                    if (allSubBehaviors[i].ID == customSubBehaviourID)
                    {
                        subBehavior = allSubBehaviors[i];
                        break;
                    }
                }
                if (subBehavior == null)
                {
                    if (customSubBehaviourID == NSHOracleBehaviorSubBehavID.MeetHunter)
                    {
                        subBehavior = new NSHOracleMeetHunter(this);
                    }
                    else if(customSubBehaviourID == NSHOracleBehaviorSubBehavID.MeetSpear)
                    {
                        subBehavior = new NSHOracleMeetSpear(this);
                    }
                    else if (customSubBehaviourID == NSHOracleBehaviorSubBehavID.MeetArtificer)
                    {
                        subBehavior = new NSHOracleMeetArtificer(this);
                    }
                    else if (customSubBehaviourID == NSHOracleBehaviorSubBehavID.MeetGourmand)
                    {
                        subBehavior = new NSHOracleMeetGourmand(this);
                    }
                    else if (customSubBehaviourID == NSHOracleBehaviorSubBehavID.MeetWhite)
                    {
                        subBehavior = new NSHOracleMeetWhite(this);
                    }
                    else if (customSubBehaviourID == NSHOracleBehaviorSubBehavID.MeetYellow)
                    {
                        subBehavior = new NSHOracleMeetYellow(this);
                    }
                    else if (customSubBehaviourID == NSHOracleBehaviorSubBehavID.MeetRivulet)
                    {
                        subBehavior = new NSHOracleMeetRivulet(this);
                    }
                    else if (customSubBehaviourID == NSHOracleBehaviorSubBehavID.MeetSaint)
                    {
                        subBehavior = new NSHOracleMeetSaint(this);
                    }
                    else if (customSubBehaviourID == NSHOracleBehaviorSubBehavID.Rubicon)
                    {
                        subBehavior = new NSHOracleRubicon(this);
                    }
                    else if (customSubBehaviourID == NSHOracleBehaviorSubBehavID.MeetSofanthiel)
                    {
                        subBehavior = new NSHOracleMeetSofanthiel(this);
                    }
                    else if (customSubBehaviourID == NSHOracleBehaviorSubBehavID.MeetOtherSlugcat)
                    {
                        subBehavior = new NSHOracleMeetOtherSlugcat(this);
                    }
                    allSubBehaviors.Add(subBehavior);
                }
                subBehavior.Activate(action, nextAction);
                currSubBehavior.Deactivate();
                Plugin.Log("Switching subbehavior to: " + subBehavior.ID.ToString() + " from: " + this.currSubBehavior.ID.ToString());
                currSubBehavior = subBehavior;
            }
            inActionCounter = 0;
            action = nextAction;
        }

        public override void GeneralActionUpdate()
        {
            if (action == CustomAction.General_Idle)// || action == NSHOracleBehaviorAction.MeetSofanthiel_Idle || action == NSHOracleBehaviorAction.MeetSaint_Idle
            {
                if (movementBehavior != CustomMovementBehavior.Idle && movementBehavior != NSHOracleMovementBehavior.Meditate)
                {
                    movementBehavior = CustomMovementBehavior.Idle;
                }
                throwOutCounter = 0;
                if (player != null && player.room == oracle.room)
                {
                    discoverCounter++;
                    if (oracle.room.GetTilePosition(player.mainBodyChunk.pos).y < 32 && 
                        (Custom.DistLess(player.mainBodyChunk.pos, oracle.firstChunk.pos, 150f) || 
                         !Custom.DistLess(player.mainBodyChunk.pos, oracle.room.MiddleOfTile(oracle.room.ShortcutLeadingToNode(0).StartTile), 150f)))// discoverCounter > 220 ||
                    {
                        SeePlayer();
                    }
                }
            }
            else if (action == CustomAction.General_GiveMark)
            {
                List<Player> playerList = new List<Player>();
                playerList.Add(this.player);
                //开始开光
                if (inActionCounter > 30 && inActionCounter < 300)
                {
                    movementBehavior = CustomMovementBehavior.Investigate;
                    foreach (var player in playerList)
                    {
                        player.Stun(20);
                        player.mainBodyChunk.vel += Vector2.ClampMagnitude(oracle.room.MiddleOfTile(24, 14) - player.mainBodyChunk.pos, 40f) / 40f * 3.2f * Mathf.InverseLerp(20f, 160f, (float)inActionCounter);
                    }
                }
                if (inActionCounter == 30)
                {
                    oracle.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Telekenisis, 0f, 1f, 1f);
                }
                //开光一瞬
                if (inActionCounter == 300)
                {
                    foreach (var player in playerList)
                    {
                        player.Stun(40);
                        for (int m = 0; m < 20; m++)
                        {
                            oracle.room.AddObject(new Spark(player.mainBodyChunk.pos, Custom.RNV() * Random.value * 40f, new Color(1f, 1f, 1f), null, 30, 120));
                        }
                    }
                    (oracle.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.theMark = true;
                    oracle.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, 0f, 1f, 1f);
                }
                //轻轻放下
                if (inActionCounter > 300 && inActionCounter < 480 && playerList.Count != 0 && !setPutDown)
                {
                    foreach (var player in playerList)
                    {
                        player.mainBodyChunk.vel += 0.8f * Vector2.up + 0.05f * Vector2.up * Mathf.InverseLerp(300f, 380f, (float)inActionCounter);
                        player.bodyChunks[1].vel += 0.8f * Vector2.up;
                        if (player.mainBodyChunk.pos.y <= 95f)
                        {
                            setPutDown = true;
                        }
                    }
                }
                //开光之后
                if (inActionCounter > 300 && playerList.Count != 0)
                {
                    movementBehavior = CustomMovementBehavior.Talk;
                    foreach (var player in playerList)
                    {
                        if (player.graphicsModule != null)
                            (player.graphicsModule as PlayerGraphics).markAlpha = Mathf.Max((player.graphicsModule as PlayerGraphics).markAlpha, Mathf.InverseLerp(500f, 300f, (float)inActionCounter));
                    }
                }
                if (inActionCounter >= 500)
                {
                    if (conversation != null)
                        conversation.paused = false;
                    NewAction(NSHOracleBehaviorAction.MeetHunter_TalkAfterGiveMark);
                }
            }
            else if (action == NSHOracleBehaviorAction.General_Meditate)
            {
                if (movementBehavior != NSHOracleMovementBehavior.Meditate)
                {
                    movementBehavior = NSHOracleMovementBehavior.Meditate;
                }
                throwOutCounter = 0;
                if (player != null && player.room == oracle.room)
                {
                    discoverCounter++;
                    if (oracle.room.GetTilePosition(player.mainBodyChunk.pos).y < 32 && 
                        (Custom.DistLess(player.mainBodyChunk.pos, oracle.firstChunk.pos, 150f) || 
                         !Custom.DistLess(player.mainBodyChunk.pos, oracle.room.MiddleOfTile(oracle.room.ShortcutLeadingToNode(0).StartTile), 150f)))//discoverCounter > 220 || 
                    {
                        SeePlayer();
                    }
                }
            }
        }

        public void AddConversationEvents(NSHConversation conv, Conversation.ID id)
        {
            if (State.GetOpinion == NSHOracleState.PlayerOpinion.NotSpeaking)
            {
                return;
            }
            if (State.GetOpinion == NSHOracleState.PlayerOpinion.Dislikes)
            {
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("..."), 0));
            }
            if (subBehavior != null)
            {
                if (subBehavior is NSHOracleMeetHunter)
                {
                    (subBehavior as NSHOracleMeetHunter).AddConversationEvents(conv, id);
                }
                else if (subBehavior is NSHOracleMeetSpear)
                {
                    (subBehavior as NSHOracleMeetSpear).AddConversationEvents(conv, id);
                }
                else if (subBehavior is NSHOracleMeetArtificer)
                {
                    (subBehavior as NSHOracleMeetArtificer).AddConversationEvents(conv, id);
                }
                else if (subBehavior is NSHOracleMeetGourmand)
                {
                    (subBehavior as NSHOracleMeetGourmand).AddConversationEvents(conv, id);
                }
                else if (subBehavior is NSHOracleMeetWhite)
                {
                    (subBehavior as NSHOracleMeetWhite).AddConversationEvents(conv, id);
                }
                else if (subBehavior is NSHOracleMeetYellow)
                {
                    (subBehavior as NSHOracleMeetYellow).AddConversationEvents(conv, id);
                }
                else if (subBehavior is NSHOracleMeetRivulet)
                {
                    (subBehavior as NSHOracleMeetRivulet).AddConversationEvents(conv, id);
                }
                else if (subBehavior is NSHOracleMeetSaint)
                {
                    (subBehavior as NSHOracleMeetSaint).AddConversationEvents(conv, id);
                }
                else if (subBehavior is NSHOracleRubicon)
                {
                    (subBehavior as NSHOracleRubicon).AddConversationEvents(conv, id);
                }
                else if (subBehavior is NSHOracleMeetSofanthiel)
                {
                    (subBehavior as NSHOracleMeetSofanthiel).AddConversationEvents(conv, id);
                }
                else if (subBehavior is NSHOracleMeetOtherSlugcat)
                {
                    (subBehavior as NSHOracleMeetOtherSlugcat).AddConversationEvents(conv, id);
                }
            }
        }

        public override void Move()
        {
            if (this.movementBehavior == CustomMovementBehavior.ShowMedia)
            {
                if (this.currSubBehavior is NSHOracleMeetRivulet || this.currSubBehavior is NSHOracleMeetSpear)
                {
                    this.ShowMediaMovementBehavior();
                }
            }
            else if (this.movementBehavior == CustomMovementBehavior.Idle)
            {
                //主要内容在base里，这里写一下概率转变就行
                if (Random.value < 0.001f && this.conversation == null && this.itemConversation == null)
                {
                    this.movementBehavior = NSHOracleMovementBehavior.Meditate;
                }
            }
            else if (this.movementBehavior == NSHOracleMovementBehavior.Meditate)
            {
                if (this.nextPos != this.oracle.room.MiddleOfTile(24, 17))
                {
                    this.SetNewDestination(this.oracle.room.MiddleOfTile(24, 17));
                }
                this.investigateAngle = 0f;
                this.lookPoint = this.oracle.firstChunk.pos + new Vector2(0f, -40f);
                if (Random.value < 0.001f && this.conversation == null && this.itemConversation == null)
                {
                    this.movementBehavior = CustomMovementBehavior.Idle;
                }
            }
            else if (this.movementBehavior == NSHOracleMovementBehavior.Land)
            {
                if (this.nextPos != landPos)
                    SetNewDestination(landPos);
            }
            //base.Move();
            
            bool flag = this.movementBehavior == CustomOracleBehaviour.CustomMovementBehavior.Idle;
            if (flag)
            {
                this.invstAngSpeed = 1f;
                CustomOracleTx.CustomOracleTx.CustomOralceEX customOralceEX;
                bool flag2 = CustomOracleTx.CustomOracleTx.oracleEx.TryGetValue(this.oracle, out customOralceEX);
                if (flag2)
                {
                    bool flag3 = this.investigateCustomMarble == null && customOralceEX.customMarbles.Count > 0;
                    if (flag3)
                    {
                        this.investigateCustomMarble = customOralceEX.customMarbles[UnityEngine.Random.Range(0, customOralceEX.customMarbles.Count)];
                    }
                    bool flag4 = this.investigateCustomMarble != null && (this.investigateCustomMarble.orbitObj == this.oracle || Custom.DistLess(new Vector2(250f, 150f), this.investigateCustomMarble.firstChunk.pos, 100f));
                    if (flag4)
                    {
                        this.investigateCustomMarble = null;
                    }
                    bool flag5 = this.investigateCustomMarble != null;
                    if (flag5)
                    {
                        this.lookPoint = this.investigateCustomMarble.firstChunk.pos;
                        bool flag6 = Custom.DistLess(this.nextPos, this.investigateCustomMarble.firstChunk.pos, 100f);
                        if (flag6)
                        {
                            this.floatyMovement = true;
                            this.nextPos = this.investigateCustomMarble.firstChunk.pos - Custom.DegToVec(this.investigateAngle) * 50f;
                        }
                        else
                        {
                            this.SetNewDestination(this.investigateCustomMarble.firstChunk.pos - Custom.DegToVec(this.investigateAngle) * 50f);
                        }
                        bool flag7 = this.pathProgression == 1f && UnityEngine.Random.value < 0.005f;
                        if (flag7)
                        {
                            this.investigateCustomMarble = null;
                        }
                    }
                }
            }
            else
            {
                bool flag8 = this.movementBehavior == CustomOracleBehaviour.CustomMovementBehavior.KeepDistance;
                if (flag8)
                {
                    bool flag9 = this.player == null;
                    if (flag9)
                    {
                        this.movementBehavior = CustomOracleBehaviour.CustomMovementBehavior.Idle;
                    }
                    else
                    {
                        this.lookPoint = this.player.DangerPos;
                        Vector2 vector = new Vector2(UnityEngine.Random.value * this.oracle.room.PixelWidth, UnityEngine.Random.value * this.oracle.room.PixelHeight);
                        bool flag10 = !this.oracle.room.GetTile(vector).Solid && this.oracle.room.aimap.getTerrainProximity(vector) > 2 && Vector2.Distance(vector, this.player.DangerPos) > Vector2.Distance(this.nextPos, this.player.DangerPos) + 100f;
                        if (flag10)
                        {
                            this.SetNewDestination(vector);
                        }
                    }
                }
                else
                {
                    bool flag11 = this.movementBehavior == CustomOracleBehaviour.CustomMovementBehavior.Investigate;
                    if (flag11)
                    {
                        bool flag12 = this.player == null;
                        if (flag12)
                        {
                            this.movementBehavior = CustomOracleBehaviour.CustomMovementBehavior.Idle;
                        }
                        else
                        {
                            this.lookPoint = this.player.DangerPos;
                            bool flag13 = this.investigateAngle < -90f || this.investigateAngle > 90f || (float)this.oracle.room.aimap.getTerrainProximity(this.nextPos) < 2f;
                            if (flag13)
                            {
                                this.investigateAngle = Mathf.Lerp(-70f, 70f, UnityEngine.Random.value);
                                this.invstAngSpeed = Mathf.Lerp(0.4f, 0.8f, UnityEngine.Random.value) * ((UnityEngine.Random.value < 0.5f) ? -1f : 1f);
                            }
                            Vector2 vector2 = this.player.DangerPos + Custom.DegToVec(this.investigateAngle) * 150f;
                            bool flag14 = (float)this.oracle.room.aimap.getTerrainProximity(vector2) >= 2f;
                            if (flag14)
                            {
                                bool flag15 = this.pathProgression > 0.9f;
                                if (flag15)
                                {
                                    bool flag16 = Custom.DistLess(this.oracle.firstChunk.pos, vector2, 30f);
                                    if (flag16)
                                    {
                                        this.floatyMovement = true;
                                    }
                                    else
                                    {
                                        bool flag17 = !Custom.DistLess(this.nextPos, vector2, 30f);
                                        if (flag17)
                                        {
                                            this.SetNewDestination(vector2);
                                        }
                                    }
                                }
                                this.nextPos = vector2;
                            }
                        }
                    }
                    else
                    {
                        bool flag18 = this.movementBehavior == CustomOracleBehaviour.CustomMovementBehavior.Talk;
                        if (flag18)
                        {
                            bool flag19 = this.player == null;
                            if (flag19)
                            {
                                this.movementBehavior = CustomOracleBehaviour.CustomMovementBehavior.Idle;
                            }
                            else
                            {
                                this.lookPoint = this.player.DangerPos;
                                Vector2 vector3 = new Vector2(UnityEngine.Random.value * this.oracle.room.PixelWidth, UnityEngine.Random.value * this.oracle.room.PixelHeight);
                                bool flag20 = this.CommunicatePosScore(vector3) + 40f < this.CommunicatePosScore(this.nextPos) && !Custom.DistLess(vector3, this.nextPos, 30f);
                                if (flag20)
                                {
                                    this.SetNewDestination(vector3);
                                }
                            }
                        }
                        else
                        {
                            bool flag21 = this.movementBehavior == CustomOracleBehaviour.CustomMovementBehavior.ShowMedia;
                            if (flag21)
                            {
                            }
                        }
                    }
                }
            }
            bool flag22 = this.currSubBehavior != null && this.currSubBehavior.LookPoint != null;
            if (flag22)
            {
                this.lookPoint = this.currSubBehavior.LookPoint.Value;
            }
            this.consistentBasePosCounter++;
            bool readyForAI = this.oracle.room.readyForAI;
            if (readyForAI)
            {
                Vector2 vector4 = new Vector2(UnityEngine.Random.value * this.oracle.room.PixelWidth, UnityEngine.Random.value * this.oracle.room.PixelHeight);
                bool flag23 = !this.oracle.room.GetTile(vector4).Solid && this.BasePosScore(vector4) + 40f < this.BasePosScore(this.baseIdeal);
                if (flag23)
                {
                    this.baseIdeal = vector4;
                    this.consistentBasePosCounter = 0;
                }
            }
            else
            {
                this.baseIdeal = this.nextPos;
            }
        }

        //被迫抄一遍
        public override float CommunicatePosScore(Vector2 tryPos)
        {
            bool flag = this.oracle.room.GetTile(tryPos).Solid || this.player == null;
            float result;
            if (flag)
            {
                result = float.MaxValue;
            }
            else
            {
                float num = Mathf.Abs(Vector2.Distance(tryPos, this.player.DangerPos) - ((this.movementBehavior == CustomOracleBehaviour.CustomMovementBehavior.Talk) ? 250f : 400f));
                num -= (float)Custom.IntClamp(this.oracle.room.aimap.getTerrainProximity(tryPos), 0, 8) * 10f;
                bool flag2 = this.movementBehavior == CustomOracleBehaviour.CustomMovementBehavior.ShowMedia;
                if (flag2)
                {
                    num += (float)(Custom.IntClamp(this.oracle.room.aimap.getTerrainProximity(tryPos), 8, 16) - 8) * 10f;
                }
                result = num;
            }
            return result;
        }
        
        public override void UnconciousUpdate()
        {
            base.UnconciousUpdate();
            this.oracle.room.gravity = 1f;
            this.oracle.setGravity(0.9f);
            if (this.oracle.ID == Oracle.OracleID.SS)
            {
                for (int i = 0; i < this.oracle.room.game.cameras.Length; i++)
                {
                    if (this.oracle.room.game.cameras[i].room == this.oracle.room && !this.oracle.room.game.cameras[i].AboutToSwitchRoom)
                    {
                        this.oracle.room.game.cameras[i].ChangeBothPalettes(10, 26, 0.51f + Mathf.Sin(this.unconciousTick * 0.25707963f) * 0.35f);
                    }
                }
                this.unconciousTick += 1f;
            }
            if (ModManager.MSC && this.oracle.ID == MoreSlugcatsEnums.OracleID.DM)
            {
                this.oracle.dazed = 320f;
                float num = Mathf.Min(1f, this.unconciousTick / 320f);
                float num2 = num * 2f;
                if (num > 0.5f)
                {
                    num2 = 1f - (num - 0.5f) / 0.5f;
                }
                if (Random.value < 0.5f)
                {
                    for (int j = 0; j < this.oracle.room.game.cameras.Length; j++)
                    {
                        if (this.oracle.room.game.cameras[j].room == this.oracle.room && !this.oracle.room.game.cameras[j].AboutToSwitchRoom)
                        {
                            this.oracle.room.game.cameras[j].ChangeBothPalettes(10, 26, 1f - Mathf.Abs(Mathf.Sin(Random.value * 3.1415927f * 2f)) * num2 * 0.75f);
                        }
                    }
                }
                this.unconciousTick += 1f;
            }
        }

        public new void FindPlayer()
        {
            if (!ModManager.CoopAvailable || oracle.room == null || !oracle.room.game.IsStorySession || oracle.room.game.rainWorld.safariMode)
            {
                return;
            }

            bool flag = false;
            if (oracle.ID == Oracle.OracleID.SS && oracle.room.game.StoryCharacter == SlugcatStats.Name.Red && !oracle.room.game.GetStorySession.saveState.miscWorldSaveData.pebblesSeenGreenNeuron)
            {
                Player playerWithNeuronInStomach = PlayerWithNeuronInStomach;
                if (playerWithNeuronInStomach != null)
                {
                    flag = true;
                    player = playerWithNeuronInStomach;
                }
            }
            else
            {
                player = oracle.room.game.Players[0]?.realizedCreature as Player;
            }

            if (player == null || player.room != oracle.room || player.inShortcut)
            {
                player = ((PlayersInRoom.Count > 0) ? PlayersInRoom[0] : null);
                if (player != null)
                {
                    int num = 1;
                    while (!flag && player.inShortcut && num < PlayersInRoom.Count)
                    {
                        player = PlayersInRoom[num];
                        num++;
                    }
                }
            }
            /*
            if (PlayersInRoom.Count > 0 && PlayersInRoom[0].dead && player == PlayersInRoom[0])
            {
                player = null;
            }
            */
            if (player != null)
            {
                oracle.room.game.cameras[0].EnterCutsceneMode(player.abstractCreature, RoomCamera.CameraCutsceneType.Oracle);
            }
        }

        public new void InitateConversation(Conversation.ID convoId, CustomOracleBehaviour.CustomConversationBehaviour convBehav)
        {
            if (this.conversation != null)
            {
                this.conversation.Interrupt("...", 0);
                this.conversation.Destroy();
            }
            this.conversation = new NSHConversation(this, convBehav as NSHOracleMeetHunter, convoId, this.dialogBox, SLOracleBehaviorHasMark.MiscItemType.NA);
        }

        //读取文件的特殊事件
        public new void SpecialEvent(string eventName)
        {
            Plugin.Log("Active! SPECEVENT : " + eventName);
            if (eventName == "giveSwarmerToHunter")
            {
                if (this.currSubBehavior is NSHOracleMeetHunter)
                {
                    NSHOracleMeetHunter.giveSwarmerToHunter = true;
                }
            }
            else if(eventName == "refuseRestartConversation")
            {
                this.refuseRestartConversation = true;
            }/*
            if (ModManager.MSC)
            {
                if (eventName == "panic")
                {
                    OraclePanicDisplay oraclePanicDisplay = new OraclePanicDisplay(this.oracle);
                    this.owner.oracle.room.AddObject(oraclePanicDisplay);
                    if (this.owner.currSubBehavior is SSOracleBehavior.SSSleepoverBehavior)
                    {
                        (this.owner.currSubBehavior as SSOracleBehavior.SSSleepoverBehavior).panicObject = oraclePanicDisplay;
                    }
                }
                if (eventName == "resync")
                {
                    OracleBotResync oracleBotResync = new OracleBotResync(this.oracle);
                    this.owner.oracle.room.AddObject(oracleBotResync);
                    if (this.owner.currSubBehavior is SSOracleBehavior.SSOracleMeetArty)
                    {
                        (this.owner.currSubBehavior as SSOracleBehavior.SSOracleMeetArty).resyncObject = oracleBotResync;
                    }
                }
                if (eventName == "tag" && this.owner.currSubBehavior is SSOracleBehavior.SSSleepoverBehavior)
                {
                    (this.owner.currSubBehavior as SSOracleBehavior.SSSleepoverBehavior).tagTimer = 120f;
                }
                if (eventName == "unlock")
                {
                    if (this.owner.conversation != null)
                    {
                        this.owner.conversation.paused = true;
                    }
                }
            }
            */
        }

        //闭上眼睛
        public override bool EyesClosed
        {
            get
            {
                return this.oracle.health == 0f || this.movementBehavior == NSHOracleMovementBehavior.Meditate || !this.oracle.Consious;
            }
        }
        #endregion

        #region 解读珍珠和物品
        public void StartItemConversation(PhysicalObject item)
        {
            Plugin.Log("oracle look at item: " + item.abstractPhysicalObject.type.ToString());
            this.isRepeatedDiscussion = false;
            //我怀疑这段没用，因为调用StartItemConversation的条件有itemConversation == null
            //改成让conversation = null了
            if (this.conversation != null)
            {
                this.conversation.Interrupt(this.Translate("..."), 0);
                this.conversation.Destroy();
                this.conversation = null;
            }
            //找到物品对应的对话id
            Conversation.ID id = ItemToConversation(item);
            this.State.InfluenceLike(this.ItemInfluenceLike(item));
            SLOracleBehaviorHasMark.MiscItemType itemType = NSHConversation.TypeOfMiscItem(item);
            if (!(item is DataPearl))
            {
                this.itemConversation = new NSHConversation(this, this.currSubBehavior as NSHOracleMeetHunter, id, this.dialogBox, itemType);
            }
            else
            {
                if (!State.significantPearls.Contains((item as DataPearl).AbstractPearl.dataPearlType))
                {
                    State.significantPearls.Add((item as DataPearl).AbstractPearl.dataPearlType);
                }
                //应该是加入收藏的意思
                if (State.likesPlayer >= 0f && ModManager.MSC && this.oracle.ID == NSHOracleRegistry.NSHOracle)
                {
                    this.isRepeatedDiscussion = DecipheredNSHPearlsSave.GetNSHPearlDeciphered(this.rainWorld.progression.miscProgressionData, (item as DataPearl).AbstractPearl.dataPearlType);
                    if (canSlugUnderstandlanguage())
                        DecipheredNSHPearlsSave.SetNSHPearlDeciphered(this.rainWorld.progression.miscProgressionData, (item as DataPearl).AbstractPearl.dataPearlType, false);
                }
                this.itemConversation = new NSHConversation(this, this.currSubBehavior as NSHOracleMeetHunter, id, this.dialogBox, itemType);
                State.totalPearlsBrought++;
                if (RainWorld.ShowLogs)
                {
                    Plugin.Log("pearls brought up: " + State.totalPearlsBrought.ToString());
                }
            }
            this.State.InfluenceLike(this.ItemInfluenceLikeAfterTalk(item)); 
            if (!this.isRepeatedDiscussion)
            {
                State.totalItemsBrought++;
                State.AddItemToAlreadyTalkedAbout(item.abstractPhysicalObject.ID);
            }
            this.talkedAboutThisSession.Add(item.abstractPhysicalObject.ID);
        }

        //被打断读取珍珠后的发言
        public void InterruptPearlMessagePlayerLeaving()
        {
            Plugin.Log("NSH Item Conversation Interrupt!");
            Plugin.Log("NSH State GetOpinion: " + State.GetOpinion.ToString());
            int i = (this is NSHOracleBehaviour && (this as NSHOracleBehaviour).holdingObject != null) ? (this as NSHOracleBehaviour).holdingObject.abstractPhysicalObject.ID.RandomSeed : Random.Range(0, 100000);
            SlugcatStats.Name currentSaveFile = oracle.room.game.GetStorySession.saveStateNumber;/*
            int num = Random.Range(0, 5);
            string s;*/
            this.itemConversation.Interrupt(this.Translate("..."), 0);
            this.interruptConversation = new NSHConversation(this, this.currSubBehavior as NSHOracleMeetHunter, Conversation.ID.None, this.dialogBox, SLOracleBehaviorHasMark.MiscItemType.NA);
            if (State.GetOpinion == NSHOracleState.PlayerOpinion.Likes)
            {
                NSHConversation.LoadEventsFromFile(this.interruptConversation, 207, null, true, i);/*
                if (num == 0)
                {
                    s = "Lost patience, <PlayerName>?";
                }
                else if (num == 1)
                {
                    s = "Want to leave? In our creators, leaving early is a point deduction~";
                }
                else if (num == 2)
                {
                    s = "Go play, children.";
                }
                else if (num == 3)
                {
                    s = "Are you leaving? Did anything scare you?";
                }
                else
                {
                    s = "Are you leaving now? See you later.";
                }*/
            }
            else if (State.GetOpinion == NSHOracleState.PlayerOpinion.Neutral)
            {
                NSHConversation.LoadEventsFromFile(this.interruptConversation, 208, null, true, i);/*
                if (num == 0)
                {
                    s = "Lost patience?";
                }
                else if (num == 1)
                {
                    s = "Do you want to leave? Go quickly.";
                }
                else if (num == 2)
                {
                    s = "...";
                }
                else if (num == 3)
                {
                    s = "Are you leaving?";
                }
                else
                {
                    s = "Are you leaving now?";
                }*/
            }
            else
            {
                this.dialogBox.NewMessage(this.ReplaceParts(this.Translate("...")), 10);
                //s = "...";
            }
            //this.dialogBox.NewMessage(this.ReplaceParts(this.Translate(s)), 10);
            this.State.InfluenceLike(-0.2f);
        }

        //继续读珍珠前的发言
        public void ResumePauseditemConversation()
        {
            Plugin.Log("NSH Conversation Try Resume!");
            Plugin.Log("NSH State GetOpinion: " + State.GetOpinion.ToString());
            int i = (this is NSHOracleBehaviour && (this as NSHOracleBehaviour).holdingObject != null) ? (this as NSHOracleBehaviour).holdingObject.abstractPhysicalObject.ID.RandomSeed : Random.Range(0, 100000);
            SlugcatStats.Name currentSaveFile = oracle.room.game.GetStorySession.saveStateNumber;/*
            int num = Random.Range(0, 5);
            string s;*/
            this.interruptConversation = new NSHConversation(this, this.currSubBehavior as NSHOracleMeetHunter, Conversation.ID.None, this.dialogBox, SLOracleBehaviorHasMark.MiscItemType.NA);
            if (State.GetOpinion == NSHOracleState.PlayerOpinion.Likes)
            {
                NSHConversation.LoadEventsFromFile(this.interruptConversation, 209, null, true, i);/*
                if (num == 0)
                {
                    s = "What are you reluctant to part ways with? Okay, let's continue.";
                }
                else if (num == 1)
                {
                    s = "Is this a game exclusively for small animals?";
                }
                else if (num == 2)
                {
                    s = "You're really not cute! But okay, it's hard for me to get serious with you.";
                }
                else if (num == 3)
                {
                    s = "Come on, little beast. Where have we read?";
                }
                else
                {
                    s = "Have you changed your mind? But I won't continue~";
                }*/
            }
            else if (State.GetOpinion == NSHOracleState.PlayerOpinion.Neutral)
            {
                NSHConversation.LoadEventsFromFile(this.interruptConversation, 210, null, true, i);/*
                if (num == 0)
                {
                    s = "What are you reluctant to part with? I'm not interested in reading it to you.";
                }
                else if (num == 1)
                {
                    s = "I don't have time to play games with you, beast.";
                }
                else if (num == 2)
                {
                    s = "...";
                }
                else if (num == 3)
                {
                    s = "Little beast, I won't read it to impolite creatures.";
                }
                else
                {
                    s = "Have you changed your mind? But I won't continue.";
                }*/
            }
            else
            {
                this.dialogBox.NewMessage(this.ReplaceParts(this.Translate("...")), 10);
                //s = "...";
            }
            //this.itemConversation.Interrupt(this.Translate(s), 10);
            this.restartConversationAfterCurrentDialoge = true;
            //抽到前四条会接着读，抽到第五条则不会继续读（写于文件中）；NSH不喜欢猫时必定不能继续
            if (State.GetOpinion != NSHOracleState.PlayerOpinion.Likes)
            {
                this.refuseRestartConversation = true;
            }
            if (this.refuseRestartConversation)
            {
                Plugin.Log("NSH Refuse Restart Conversation!");
            }
        }

        //解读物品对好感的影响
        public float ItemInfluenceLike(PhysicalObject item)
        {
            float add = 0f;
            if (item is FireEgg)
            {
                add = 0.1f;
            }
            if (item is DataPearl)
            {
                add = 0.1f;
            }
            return add;
        }

        //解读物品对好感的影响
        public float ItemInfluenceLikeAfterTalk(PhysicalObject item)
        {
            float add = 0f;
            if (item is SLOracleSwarmer)
            {
                add = -0.4f;
            }
            if (item is SSOracleSwarmer && //注意，NSH的神经元需要比FP的神经元先判断
                 OracleSwarmerHooks.OracleSwarmerData.TryGetValue(item as SSOracleSwarmer, out var nshOracleSwarmer) &&
                 nshOracleSwarmer.spawnRegion == "NSH")
            {
                add = -0.1f;
            }
            return add;
        }

        //物品对应对话id
        public Conversation.ID ItemToConversation(PhysicalObject item)
        {
            Conversation.ID id = Conversation.ID.None;
            if (!(item is DataPearl))
            {
                id = Conversation.ID.Moon_Misc_Item;
            }
            else
            {
                //有名字的珍珠
                id = Conversation.DataPearlToConversation((item as DataPearl).AbstractPearl.dataPearlType);
                //NSH区域独有珍珠
                /*
                if ((item as DataPearl).AbstractPearl.dataPearlType.value == "NSH_Top_Pearl")
                {
                    id = NSHConversationID.NSH_Pearl_NSH_Top;
                }
                else if ((item as DataPearl).AbstractPearl.dataPearlType.value == "NSH_Box_Pearl")
                {
                    id = NSHConversationID.NSH_Pearl_NSH_Box;
                }*/
                //
                //矛大师珍珠
                if (item.abstractPhysicalObject.type == MoreSlugcatsEnums.AbstractObjectType.Spearmasterpearl)
                {
                    if (!(((item as DataPearl).AbstractPearl as SpearMasterPearl.AbstractSpearMasterPearl).broadcastTagged))
                    {
                        id = MoreSlugcatsEnums.ConversationID.Pebbles_Spearmaster_Read_Pearl;
                    }
                    else
                    {
                        id = MoreSlugcatsEnums.ConversationID.Moon_Spearmaster_Pearl;
                    }
                }
                //普通珍珠？
                else if ((item as DataPearl).AbstractPearl.dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.Misc || (item as DataPearl).AbstractPearl.dataPearlType.Index == -1)
                {
                    id = Conversation.ID.Moon_Pearl_Misc;
                }
                else if ((item as DataPearl).AbstractPearl.dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.Misc2)
                {
                    id = Conversation.ID.Moon_Pearl_Misc2;
                }
                //广播珍珠
                else if (ModManager.MSC && (item as DataPearl).AbstractPearl.dataPearlType == MoreSlugcatsEnums.DataPearlType.BroadcastMisc)
                {
                    id = MoreSlugcatsEnums.ConversationID.Moon_Pearl_BroadcastMisc;
                }
                //FP演算室珍珠
                else if (ModManager.MSC && (item as DataPearl).AbstractPearl.dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.PebblesPearl && ((item as DataPearl).AbstractPearl as PebblesPearl.AbstractPebblesPearl).color >= 0)
                {
                    id = Conversation.ID.Moon_Pebbles_Pearl;
                }
                //其他珍珠
                else
                {
                    var PearlData = Type.GetType("CustomRegions.Collectables.PearlData,CustomRegionsSupport", true);
                    var CustomDataPearlsListInfo = PearlData.GetField("CustomDataPearlsList", BindingFlags.Static | BindingFlags.Public);
                    var CustomDataPearlsList = (Dictionary<DataPearl.AbstractDataPearl.DataPearlType, Structs.CustomPearl>)CustomDataPearlsListInfo.GetValue(null);
                    bool isCRSPearl = false;
                    //使用crs的珍珠
                    foreach (KeyValuePair<DataPearl.AbstractDataPearl.DataPearlType, Structs.CustomPearl> keyValuePair in CustomDataPearlsList)
                    {
                        if (id == keyValuePair.Value.conversationID)
                        {
                            isCRSPearl = true;
                            id = new Conversation.ID(keyValuePair.Value.filePath, false);
                            Plugin.Log("NSH Read A CRS Pearl, file id :" + id.ToString());
                            break;
                        }
                    }
                    //其他mod的珍珠
                    if (!isCRSPearl)
                    {
                        Plugin.Log("NSH Read A Mod (not CRS) Pearl, file id :" + id.ToString());
                    }
                } 
            }
            //如果讨厌玩家，则拒绝解读
            if (State.likesPlayer < 0f || State.GetOpinion == NSHOracleState.PlayerOpinion.Dislikes || State.GetOpinion == NSHOracleState.PlayerOpinion.NotSpeaking)
            {
                id = NSHConversationID.RefusingToInterpretItems;
            }
            return id;
        }
        #endregion

        #region 储存珍珠和物品
        public void InitStoryPearlCollection()
        {
            this.readItemOrbits = new List<AbstractWorldEntity>();
            this.readDataPearlOrbits = new List<DataPearl.AbstractDataPearl>();
            this.readPearlGlyphs = new Dictionary<DataPearl.AbstractDataPearl, GlyphLabel>();
            foreach (AbstractWorldEntity abstractWorldEntity in this.oracle.room.abstractRoom.entities)
            {
                if (abstractWorldEntity is DataPearl.AbstractDataPearl)
                {
                    DataPearl.AbstractDataPearl abstractDataPearl = abstractWorldEntity as DataPearl.AbstractDataPearl;
                    if (abstractDataPearl.type != NSHPearlRegistry.NSHPearl)
                    {
                        this.readDataPearlOrbits.Add(abstractDataPearl);
                    }
                }
                else if((abstractWorldEntity as AbstractPhysicalObject).type == MoreSlugcatsEnums.AbstractObjectType.FireEgg)
                {
                    this.readItemOrbits.Add(abstractWorldEntity);
                }
                AbstractPhysicalObject obj = abstractWorldEntity as AbstractPhysicalObject;
                Plugin.Log("obj is {0}, obj.tracker == null? {1}", obj.type.ToString(), (obj.tracker == null));
            }
            int num = 0;
            foreach (DataPearl.AbstractDataPearl abstractDataPearl2 in this.readDataPearlOrbits)
            {
                Vector2 pos = this.storedPearlOrbitLocation(num);
                Plugin.Log("abstractDataPearl2.realizedObject != null? {0}", (abstractDataPearl2.realizedObject != null));
                if (abstractDataPearl2.realizedObject != null)
                {
                    abstractDataPearl2.realizedObject.firstChunk.pos = pos;
                }
                else
                {
                    abstractDataPearl2.pos.Tile = this.oracle.room.GetTilePosition(pos);
                }
                num++;
            }
            foreach (AbstractWorldEntity abstractitem in this.readItemOrbits)
            {
                Vector2 pos = this.storedPearlOrbitLocation(num);
                Plugin.Log("abstractitem.realizedObject != null? {0}", ((abstractitem as AbstractPhysicalObject).realizedObject != null));
                if ((abstractitem as AbstractPhysicalObject).realizedObject != null)
                {
                    (abstractitem as AbstractPhysicalObject).realizedObject.firstChunk.pos = pos;
                }
                else
                {
                    (abstractitem as AbstractPhysicalObject).pos.Tile = this.oracle.room.GetTilePosition(pos);
                }
                num++;
            }
            this.inspectItem = null;
        }

        public void UpdateStoryPearlCollection()
        {
            List<DataPearl.AbstractDataPearl> list = new List<DataPearl.AbstractDataPearl>(); 
            List<AbstractWorldEntity> list2 = new List<AbstractWorldEntity>();
            int num = 0;
            //判断珍珠
            foreach (DataPearl.AbstractDataPearl abstractDataPearl in this.readDataPearlOrbits)
            {
                if (abstractDataPearl.realizedObject != null)
                {
                    if (abstractDataPearl.realizedObject.grabbedBy.Count > 0)
                    {
                        list.Add(abstractDataPearl);
                    }
                    else
                    {
                        if (!this.readPearlGlyphs.ContainsKey(abstractDataPearl))
                        {
                            this.readPearlGlyphs.Add(abstractDataPearl, new GlyphLabel(abstractDataPearl.realizedObject.firstChunk.pos, GlyphLabel.RandomString(1, 1, 12842 + abstractDataPearl.dataPearlType.Index, false)));
                            this.oracle.room.AddObject(this.readPearlGlyphs[abstractDataPearl]);
                        }
                        else
                        {
                            this.readPearlGlyphs[abstractDataPearl].setPos = new Vector2?(abstractDataPearl.realizedObject.firstChunk.pos);
                        }
                        if (getToWorking > 0f)
                        {
                            abstractDataPearl.realizedObject.firstChunk.pos = Custom.MoveTowards(abstractDataPearl.realizedObject.firstChunk.pos, this.storedPearlOrbitLocation(num), 2.5f);
                            abstractDataPearl.realizedObject.firstChunk.vel *= 0.99f;
                        }
                        num++;
                    }
                }
            }
            foreach (DataPearl.AbstractDataPearl abstractDataPearl2 in list)
            {
                if (RainWorld.ShowLogs)
                {
                    string str = "stored pearl grabbed, releasing from storage ";
                    DataPearl.AbstractDataPearl abstractDataPearl3 = abstractDataPearl2;
                    Plugin.Log(str + ((abstractDataPearl3 != null) ? abstractDataPearl3.ToString() : null));
                }
                if(this.readPearlGlyphs.ContainsKey(abstractDataPearl2))
                    this.readPearlGlyphs[abstractDataPearl2].Destroy();//不进行if判断的话，这一行将导致带珍珠用Warp传送到NSH房间会引起游戏崩溃
                this.readPearlGlyphs.Remove(abstractDataPearl2);
                this.readDataPearlOrbits.Remove(abstractDataPearl2);
            }
            //判断物品
            foreach (AbstractWorldEntity abstractWorldEntity in this.readItemOrbits)
            {
                if ((abstractWorldEntity as AbstractPhysicalObject).realizedObject != null)
                {
                    if ((abstractWorldEntity as AbstractPhysicalObject).realizedObject.grabbedBy.Count > 0)
                    {
                        list2.Add(abstractWorldEntity);
                    }
                    //会保存火虫卵
                    else if ((abstractWorldEntity as AbstractPhysicalObject).type == MoreSlugcatsEnums.AbstractObjectType.FireEgg)
                    {
                        if (getToWorking > 0f)
                        {
                            (abstractWorldEntity as AbstractPhysicalObject).realizedObject.firstChunk.pos = Custom.MoveTowards((abstractWorldEntity as AbstractPhysicalObject).realizedObject.firstChunk.pos, this.storedPearlOrbitLocation(num), 2.5f);
                            (abstractWorldEntity as AbstractPhysicalObject).realizedObject.firstChunk.vel *= 0.99f;
                        }
                        num++;
                    }
                }
            }
            foreach (AbstractWorldEntity abstractWorldEntity2 in list2)
            {
                if (RainWorld.ShowLogs)
                {
                    string str = "stored item grabbed, releasing from storage ";
                    AbstractWorldEntity abstractWorldEntity3 = abstractWorldEntity2;
                    Plugin.Log(str + ((abstractWorldEntity3 != null) ? abstractWorldEntity3.ToString() : null));
                }
                this.readItemOrbits.Remove(abstractWorldEntity2);
                if(this.talkedAboutThisSession.Contains((abstractWorldEntity2 as AbstractPhysicalObject).ID))
                    this.dialogBox.NewMessage(this.Translate("Oh, of course, you can take it away anytime!"), 10);
            }
        }

        public Vector2 storedPearlOrbitLocation(int index)
        {
            float num = 5f;
            float num2 = (float)index % num;
            float num3 = Mathf.Floor((float)index / num);
            float num4 = num2 * 0.5f;
            return new Vector2(615f, 100f) + new Vector2(num2 * 26f, (num3 + num4) * 18f);
        }
        #endregion

        #region 普通对话相关
        public bool IsSeePlayer()
        {
            return player != null && player.room == oracle.room &&
                   oracle.room.GetTilePosition(player.mainBodyChunk.pos).y < 32 &&
                   (Custom.DistLess(player.mainBodyChunk.pos, oracle.firstChunk.pos, 150f) ||
                   !Custom.DistLess(player.mainBodyChunk.pos, oracle.room.MiddleOfTile(oracle.room.ShortcutLeadingToNode(0).StartTile), 150f));
        }

        public bool canSlugUnderstandlanguage()
        {
            return oracle.room.game.GetStorySession.saveState.deathPersistentSaveData.theMark ||
                   player.room.world.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Saint;
        }

        public void PlayerEncountersAdd()
        {
            State.playerEncounters++;
            if (canSlugUnderstandlanguage())
            {
                State.playerEncountersWithMark++;
            }
        }

        public void PlayerEncountersWithoutMark()
        {
            if (State.playerEncounters == 1)
                switch (Random.Range(0, 3))
                {
                    case 0:
                        this.oracle.room.PlaySound(NSHOracleSoundID.NSH_AI_LongDialogue_1, 0f, 1f, 1.25f);
                        break;
                    case 1:
                        this.oracle.room.PlaySound(NSHOracleSoundID.NSH_AI_LongDialogue_2, 0f, 1f, 1.25f);
                        break;
                    case 2:
                        this.oracle.room.PlaySound(NSHOracleSoundID.NSH_AI_LongDialogue_3, 0f, 1f, 1.25f);
                        break;
                }
            else
            {
                switch (Random.Range(0, 2))
                {
                    case 0:
                        this.oracle.room.PlaySound(NSHOracleSoundID.NSH_AI_ShortDialogue_1, 0f, 1f, 1.25f);
                        break;
                    case 1:
                        this.oracle.room.PlaySound(NSHOracleSoundID.NSH_AI_ShortDialogue_2, 0f, 1f, 1.25f);
                        break;
                }
            }
        }
        #endregion

        # region 展示图像
        public void ShowMediaMovementBehavior()
        {/*
            if (base.player != null)
            {
                this.lookPoint = base.player.DangerPos;
            }*/
            Vector2 vector = new Vector2(Random.value * base.oracle.room.PixelWidth, Random.value * base.oracle.room.PixelHeight);
            if (this.CommunicatePosScore(vector) + 40f < this.CommunicatePosScore(this.nextPos) && !Custom.DistLess(vector, this.nextPos, 30f))
            {
                this.SetNewDestination(vector);
            }
            this.consistentShowMediaPosCounter += (int)Custom.LerpMap(Vector2.Distance(this.showMediaPos, this.idealShowMediaPos), 0f, 200f, 1f, 10f);
            vector = new Vector2(Random.value * base.oracle.room.PixelWidth, Random.value * base.oracle.room.PixelHeight);
            if (this.ShowMediaScore(vector) + 40f < this.ShowMediaScore(this.idealShowMediaPos))
            {
                this.idealShowMediaPos = vector;
                this.consistentShowMediaPosCounter = 0;
            }
            vector = this.idealShowMediaPos + Custom.RNV() * Random.value * 40f;
            if (this.ShowMediaScore(vector) + 20f < this.ShowMediaScore(this.idealShowMediaPos))
            {
                this.idealShowMediaPos = vector;
                this.consistentShowMediaPosCounter = 0;
            }
            if (this.consistentShowMediaPosCounter > 300)
            {
                this.showMediaPos = Vector2.Lerp(this.showMediaPos, this.idealShowMediaPos, 0.1f);
                this.showMediaPos = Custom.MoveTowards(this.showMediaPos, this.idealShowMediaPos, 10f);
            }
        }

        private float ShowMediaScore(Vector2 tryPos)
        {
            if (base.oracle.room.GetTile(tryPos).Solid || base.player == null)
            {
                return float.MaxValue;
            }
            float num = Mathf.Abs(Vector2.Distance(tryPos, base.player.DangerPos) - 250f);
            num -= Mathf.Min((float)base.oracle.room.aimap.getTerrainProximity(tryPos), 9f) * 30f;
            num -= Vector2.Distance(tryPos, this.nextPos) * 0.5f;
            for (int i = 0; i < base.oracle.arm.joints.Length; i++)
            {
                num -= Mathf.Min(Vector2.Distance(tryPos, base.oracle.arm.joints[i].pos), 100f) * 10f;
            }
            if (base.oracle.graphicsModule != null)
            {
                for (int j = 0; j < (base.oracle.graphicsModule as OracleGraphics).umbCord.coord.GetLength(0); j += 3)
                {
                    num -= Mathf.Min(Vector2.Distance(tryPos, (base.oracle.graphicsModule as OracleGraphics).umbCord.coord[j, 0]), 100f);
                }
            }
            return num;
        }
        #endregion

        public void KillPlayerUpdate()
        {
            if ((!player.dead || killFac > 0.5f) && player.room == oracle.room)
            {
                this.killFac += 0.025f;
                if (this.killFac >= 1f)
                {
                    player.mainBodyChunk.vel += Custom.RNV() * 12f;
                    for (int k = 0; k < 20; k++)
                    {
                        oracle.room.AddObject(new Spark(player.mainBodyChunk.pos, Custom.RNV() * Random.value * 40f, new Color(1f, 1f, 1f), null, 30, 120));
                    }
                    player.Die();
                    this.killFac = 0f;
                    return;
                }
            }
            else
            {
                this.killFac *= 0.8f;
                this.getToWorking = 1f;
                base.movementBehavior = CustomMovementBehavior.KeepDistance;
                if (player.room != base.oracle.room && base.oracle.oracleBehavior.PlayersInRoom.Count <= 0)
                {
                    this.NewAction(CustomAction.General_Idle);
                    return;
                }
                if (!ModManager.CoopAvailable)
                {
                    player.mainBodyChunk.vel += Custom.DirVec(player.mainBodyChunk.pos, base.oracle.room.MiddleOfTile(24, 32)) * 0.6f * (1f - base.oracle.room.gravity);
                    if (base.oracle.room.GetTilePosition(player.mainBodyChunk.pos) == new IntVector2(24, 32) && player.enteringShortCut == null)
                    {
                        player.enteringShortCut = new IntVector2?(base.oracle.room.ShortcutLeadingToNode(1).StartTile);
                        return;
                    }
                }
                else
                {
                    using (List<Player>.Enumerator enumerator2 = base.oracle.oracleBehavior.PlayersInRoom.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            Player player = enumerator2.Current;
                            player.mainBodyChunk.vel += Custom.DirVec(player.mainBodyChunk.pos, base.oracle.room.MiddleOfTile(24, 32)) * 0.6f * (1f - base.oracle.room.gravity);
                            if (base.oracle.room.GetTilePosition(player.mainBodyChunk.pos) == new IntVector2(24, 32) && player.enteringShortCut == null)
                            {
                                player.enteringShortCut = new IntVector2?(base.oracle.room.ShortcutLeadingToNode(1).StartTile);
                            }
                        }
                        return;
                    }
                }
            }
        }

        public void LockShortcuts()
        {
            if (this.oracle.room.lockedShortcuts.Count == 0)
            {
                for (int i = 0; i < this.oracle.room.shortcutsIndex.Length; i++)
                {
                    this.oracle.room.lockedShortcuts.Add(this.oracle.room.shortcutsIndex[i]);
                }
            }
        }

        public void TurnOffSSMusic(bool abruptEnd)
        {
            Plugin.Log("Fading out SS music " + abruptEnd.ToString());
            for (int i = 0; i < this.oracle.room.updateList.Count; i++)
            {
                if (this.oracle.room.updateList[i] is SSMusicTrigger)
                {
                    this.oracle.room.updateList[i].Destroy();
                    break;
                }
            }
            if (abruptEnd && this.oracle.room.game.manager.musicPlayer != null && this.oracle.room.game.manager.musicPlayer.song != null && this.oracle.room.game.manager.musicPlayer.song is SSSong)
            {
                this.oracle.room.game.manager.musicPlayer.song.FadeOut(2f);
            }
        }

        public bool IsThereHasSlugPup()
        {
            foreach (var creature in oracle.room.abstractRoom.creatures)
            {
                if (creature.realizedCreature != null && creature.realizedCreature is Player)
                {
                    Player crit = creature.realizedCreature as Player;
                    if (crit.isSlugpup)//crit.isNPC || 
                        return true;
                }
            }
            return false;
        }

        public List<Player> FindSlugPup()
        {
            List<Player> pup = new List<Player>();
            foreach (var creature in oracle.room.abstractRoom.creatures)
            {
                if (creature.realizedCreature != null && creature.realizedCreature is Player)
                {
                    Player crit = creature.realizedCreature as Player;
                    if (crit.isSlugpup)//crit.isNPC || 
                        pup.Add(crit);
                }
            }
            return pup;
        }
    }
}
