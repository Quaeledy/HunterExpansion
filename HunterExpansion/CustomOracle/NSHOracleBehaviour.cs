using System.Collections.Generic;
using CustomOracleTx;
using UnityEngine;
using Random = UnityEngine.Random;
using RWCustom;
using CustomDreamTx;
using HunterExpansion.CustomDream;
using HunterExpansion;
using HunterExpansion.CustomSave;
using MoreSlugcats;

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
        
        //读珍珠、物品相关
        public NSHConversation itemConversation;
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

        //好感度相关
        public static bool generateKillingIntent = false;

        public override int NotWorkingPalette => 25;
        public override int GetWorkingPalette => 39119;//39119 //26

        public override Vector2 GetToDir => Vector2.up;

        public NSHOracleBehaviour(Oracle oracle) : base(oracle)
        {
            //this.movementBehavior = ((Random.value < 0.5f) ? NSHOracleMovementBehavior.Meditate : CustomMovementBehavior.Idle);//0.5f
            this.action = (Random.value < 1f) ? NSHOracleBehaviorAction.General_Meditate : CustomAction.General_Idle;//0.5f
            this.talkedAboutThisSession = new List<EntityID>();
            this.oracle.health = (RipNSHSave.ripNSH && oracle.room.world.region.name != "HR") ? 0f : 1f;
            this.InitStoryPearlCollection();

            setDream = false;
            setPutDown = false;
            landPos = Vector2.zero;
        }

        public override void Update(bool eu)
        {
            if (player != null && generateKillingIntent)
            {
                KillPlayerUpdate();
            }
            if (player != null && player.room != null && player.room.world.region.name == "HR" && !(subBehavior is NSHOracleRubicon))
                NewAction(NSHOracleBehaviorAction.Rubicon_Init);
            if (player != null && player.room != null && player.room.abstractRoom.name == "NSH_AI" &&
                player.room.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel && 
                !(subBehavior is NSHOracleMeetSofanthiel))
                NewAction(NSHOracleBehaviorAction.MeetSofanthiel_Init);
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

            if (CustomDreamRx.currentActivateNormalDream == null)
            {
                //这是更新NSH房间收藏的珍珠
                this.UpdateStoryPearlCollection();
                
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
                        else if(this.itemConversation.paused && this.restartConversationAfterCurrentDialoge && this.dialogBox.messages.Count == 0)
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

                base.Update(eu);

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
                                    if(this.conversation != null)
                                    {
                                        this.conversation.Interrupt(this.Translate("..."), 0);
                                        this.conversation = null;
                                    }
                                    if (State.GetOpinion == NSHOracleState.PlayerOpinion.Likes)
                                    {
                                        this.dialogBox.NewMessage(this.Translate("What are you doing? Please don't attack me."), 20);
                                        this.oracle.room.PlaySound(NSHOracleSoundID.NSH_AI_Attack_1, 10f, 1f, 1f);
                                        State.annoyances++;
                                        State.InfluenceLike(-0.4f);
                                    }
                                    else if (State.GetOpinion == NSHOracleState.PlayerOpinion.Neutral)
                                    {
                                        this.dialogBox.NewMessage(this.Translate("Beast, stop your barbaric behavior."), 20);
                                        this.oracle.room.PlaySound(NSHOracleSoundID.NSH_AI_Attack_1, 10f, 1f, 1f);
                                        State.annoyances++;
                                        State.InfluenceLike(-0.5f);
                                    }
                                    else
                                    {
                                        if(State.annoyances == 0)
                                        {
                                            this.dialogBox.NewMessage(this.Translate("Remember to be polite in the next cycle."), 20);
                                        }
                                        else
                                        {
                                            this.dialogBox.NewMessage(this.Translate("I have already warned you."), 20);
                                        }
                                        this.oracle.room.PlaySound(NSHOracleSoundID.NSH_AI_Attack_2, 10f, 1f, 1f);
                                        State.annoyances++;
                                        State.likesPlayer = -1f;
                                        generateKillingIntent = true;
                                    }
                                }
                            }
                            bool flag3 = false;
                            bool flag4 = this.action == CustomAction.General_Idle;
                            // && this.currSubBehavior is SSOracleBehavior.SSSleepoverBehavior && (this.currSubBehavior as SSOracleBehavior.SSSleepoverBehavior).panicObject == null
                            if (this.oracle.ID == NSHOracleRegistry.NSHOracle)// && this.oracle.room.game.GetStorySession.saveStateNumber == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Artificer && this.currSubBehavior is SSOracleBehavior.ThrowOutBehavior
                            {
                                flag4 = true;
                                //flag3 = true;
                            }
                            if (this.inspectItem == null && (this.conversation == null || flag3) && flag4 
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
                                         !((physicalObject is SSOracleSwarmer) || (physicalObject is SLOracleSwarmer)) && //不读神经元
                                         !this.readItemOrbits.Contains(physicalObject.abstractPhysicalObject) &&
                                         !this.talkedAboutThisSession.Contains(physicalObject.abstractPhysicalObject.ID))
                                {
                                    this.inspectItem = physicalObject;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                base.Update(eu);
            }
            
            /*
                if (this.oracle.room.world.name == "HR")
                {
                    int num9 = 0;
                    if (this.oracle.ID == MoreSlugcats.MoreSlugcatsEnums.OracleID.DM)
                    {
                        num9 = 2;
                    }
                    float num10 = Custom.Dist(this.oracle.arm.cornerPositions[0], this.oracle.arm.cornerPositions[2]) * 0.4f;
                    if (Custom.Dist(this.baseIdeal, this.oracle.arm.cornerPositions[num9]) >= num10)
                    {
                        this.baseIdeal = this.oracle.arm.cornerPositions[num9] + (this.baseIdeal - this.oracle.arm.cornerPositions[num9]).normalized * num10;
                    }
                }
                if (this.currSubBehavior.LowGravity >= 0f)
                {
                    this.oracle.room.gravity = this.currSubBehavior.LowGravity;
                    return;
                }*/
            /*
            if (!this.currSubBehavior.Gravity)
            {
                this.oracle.room.gravity = Custom.LerpAndTick(this.oracle.room.gravity, 0f, 0.05f, 0.02f);
                return;
            }*/
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
                if (player.room.world.region.name != "HR")
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
            if (action == CustomAction.General_Idle)
            {
                if (movementBehavior != CustomMovementBehavior.Idle)
                {
                    movementBehavior = CustomMovementBehavior.Idle;
                }
                throwOutCounter = 0;
                if (player != null && player.room == oracle.room)
                {
                    discoverCounter++;
                    if (oracle.room.GetTilePosition(player.mainBodyChunk.pos).y < 32 && (Custom.DistLess(player.mainBodyChunk.pos, oracle.firstChunk.pos, 150f) || !Custom.DistLess(player.mainBodyChunk.pos, oracle.room.MiddleOfTile(oracle.room.ShortcutLeadingToNode(0).StartTile), 150f)))// discoverCounter > 220 ||
                    {
                        SeePlayer();
                    }
                }
            }
            else if (action == CustomAction.General_GiveMark)
            {
                //开始开光
                if (inActionCounter > 30 && inActionCounter < 300)
                {
                    movementBehavior = CustomMovementBehavior.Investigate;
                    player.Stun(20);
                    player.mainBodyChunk.vel += Vector2.ClampMagnitude(oracle.room.MiddleOfTile(24, 14) - player.mainBodyChunk.pos, 40f) / 40f * 3.2f * Mathf.InverseLerp(20f, 160f, (float)inActionCounter);
                }
                if (inActionCounter == 30)
                {
                    oracle.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Telekenisis, 0f, 1f, 1f);
                }
                //开光一瞬
                if (inActionCounter == 300)
                {
                    player.Stun(40);
                    (oracle.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.theMark = true;
                    for (int m = 0; m < 20; m++)
                    {
                        oracle.room.AddObject(new Spark(player.mainBodyChunk.pos, Custom.RNV() * Random.value * 40f, new Color(1f, 1f, 1f), null, 30, 120));
                    }
                    oracle.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, 0f, 1f, 1f);
                }
                //轻轻放下
                if (inActionCounter > 300 && inActionCounter < 480 && player != null && !setPutDown)
                {
                    player.mainBodyChunk.vel += 0.8f * Vector2.up + 0.05f * Vector2.up * Mathf.InverseLerp(300f, 380f, (float)inActionCounter);
                    player.bodyChunks[1].vel += 0.8f * Vector2.up;
                    if (player.mainBodyChunk.pos.y <= 95f)
                    {
                        setPutDown = true;
                    }
                }
                //开光之后
                if (inActionCounter > 300 && player.graphicsModule != null)
                {
                    movementBehavior = CustomMovementBehavior.Talk;
                    (player.graphicsModule as PlayerGraphics).markAlpha = Mathf.Max((player.graphicsModule as PlayerGraphics).markAlpha, Mathf.InverseLerp(500f, 300f, (float)inActionCounter));
                }
                if (inActionCounter >= 500)
                {
                    NewAction(NSHOracleBehaviorAction.MeetHunter_TalkAfterGiveMark);
                    if (conversation != null)
                    {
                        conversation.paused = false;
                    }
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
                    if (oracle.room.GetTilePosition(player.mainBodyChunk.pos).y < 32 && (Custom.DistLess(player.mainBodyChunk.pos, oracle.firstChunk.pos, 150f) || !Custom.DistLess(player.mainBodyChunk.pos, oracle.room.MiddleOfTile(oracle.room.ShortcutLeadingToNode(0).StartTile), 150f)))//discoverCounter > 220 || 
                    {
                        SeePlayer();
                    }
                }
            }
        }

        public override void AddConversationEvents(CustomOracleConversation conv, Conversation.ID id)
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
                if (this.currSubBehavior is NSHOracleMeetRivulet)
                {
                    (this.currSubBehavior as NSHOracleMeetRivulet).ShowMediaMovementBehavior();
                }
            }
            //不能转变，否则可能导致NSH提前开启下一次对话
            /*
            else if (this.movementBehavior == CustomMovementBehavior.Idle)
            {
                //主要内容在base里，这里写一下概率转变就行
                if (Random.value < 0.001f && this.conversation == null && this.itemConversation == null)
                {
                    NewAction(NSHOracleBehaviorAction.General_Meditate);
                    this.movementBehavior = NSHOracleMovementBehavior.Meditate;
                }
            }*/
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
                    NewAction(CustomAction.General_Idle);
                    this.movementBehavior = CustomMovementBehavior.Idle;
                }
            }
            else if (this.movementBehavior == NSHOracleMovementBehavior.Land)
            {
                if (this.nextPos != landPos)
                    SetNewDestination(landPos);
            }
            base.Move();
        }
        #endregion

        #region 解读珍珠和物品
        public void StartItemConversation(PhysicalObject item)
        {
            NSHOracleState nshOracleState = NSHOracleStateSave.NSHOracleState;
            this.isRepeatedDiscussion = false;
            //我怀疑这段没用，因为调用StartItemConversation的条件有itemConversation == null
            if (this.itemConversation != null)
            {
                this.itemConversation.Interrupt("...", 0);
                this.itemConversation.Destroy();
                this.itemConversation = null;
            }
            //
            if (!(item is DataPearl))
            {
                Conversation.ID id = Conversation.ID.Moon_Misc_Item;
                SLOracleBehaviorHasMark.MiscItemType itemType = NSHConversation.TypeOfMiscItem(item);
                //如果讨厌玩家，则拒绝解读
                if (State.likesPlayer < 0f || State.GetOpinion == NSHOracleState.PlayerOpinion.Dislikes || State.GetOpinion == NSHOracleState.PlayerOpinion.NotSpeaking)
                {
                    id = NSHConversationID.RefusingToInterpretItems;
                }
                this.itemConversation = new NSHConversation(this, this.currSubBehavior as NSHOracleMeetHunter, id, this.dialogBox, itemType);
            }
            else
            {
                this.State.InfluenceLike(0.1f);
                //有名字的珍珠
                Conversation.ID id = Conversation.DataPearlToConversation((item as DataPearl).AbstractPearl.dataPearlType);
                if (!nshOracleState.significantPearls.Contains((item as DataPearl).AbstractPearl.dataPearlType))
                {
                    nshOracleState.significantPearls.Add((item as DataPearl).AbstractPearl.dataPearlType);
                }
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
                }/*
                //赞美诗珍珠（二编：原来是RM）
                else if (item.abstractPhysicalObject.type == MoreSlugcatsEnums.AbstractObjectType.HalcyonPearl)
                {
                    id = NSHOracleBehaviour.NSH_Halcyon_Pearl;
                }*/
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

                if (!nshOracleState.significantPearls.Contains((item as DataPearl).AbstractPearl.dataPearlType))
                {
                    nshOracleState.significantPearls.Add((item as DataPearl).AbstractPearl.dataPearlType);
                }
                //应该是加入收藏的意思
                if (State.likesPlayer >= 0f && ModManager.MSC && this.oracle.ID == NSHOracleRegistry.NSHOracle)
                {
                    this.isRepeatedDiscussion = DecipheredNSHPearlsSave.GetNSHPearlDeciphered(this.rainWorld.progression.miscProgressionData, (item as DataPearl).AbstractPearl.dataPearlType);
                    DecipheredNSHPearlsSave.SetNSHPearlDeciphered(this.rainWorld.progression.miscProgressionData, (item as DataPearl).AbstractPearl.dataPearlType, false);
                }
                //如果讨厌玩家，则拒绝解读
                if (State.likesPlayer < 0f || State.GetOpinion == NSHOracleState.PlayerOpinion.Dislikes || State.GetOpinion == NSHOracleState.PlayerOpinion.NotSpeaking)
                {
                    id = NSHConversationID.RefusingToInterpretItems;
                }
                this.itemConversation = new NSHConversation(this, this.currSubBehavior as NSHOracleMeetHunter, id, this.dialogBox, SLOracleBehaviorHasMark.MiscItemType.NA);
                nshOracleState.totalPearlsBrought++;
                if (RainWorld.ShowLogs)
                {
                    Plugin.Log("pearls brought up: " + nshOracleState.totalPearlsBrought.ToString());
                }
            }
            if (!this.isRepeatedDiscussion)
            {
                nshOracleState.totalItemsBrought++;
                nshOracleState.AddItemToAlreadyTalkedAbout(item.abstractPhysicalObject.ID);
            }
            this.talkedAboutThisSession.Add(item.abstractPhysicalObject.ID);
        }

        //被打断读取珍珠后的发言
        public void InterruptPearlMessagePlayerLeaving()
        {
            int num = Random.Range(0, 5);
            string s;
            if (State.GetOpinion == NSHOracleState.PlayerOpinion.Likes)
            {
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
                }
            }
            else if (State.GetOpinion == NSHOracleState.PlayerOpinion.Neutral)
            {
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
                }
            }
            else
            {
                s = "...";
            }
            this.itemConversation.Interrupt("...", 0);
            this.dialogBox.NewMessage(this.Translate(s), 10);
            this.State.InfluenceLike(-0.2f);
        }

        //继续读珍珠前的发言
        public void ResumePauseditemConversation()
        {
            int num = Random.Range(0, 5);
            string s;
            if (State.GetOpinion == NSHOracleState.PlayerOpinion.Likes)
            {
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
                }
            }
            else if (State.GetOpinion == NSHOracleState.PlayerOpinion.Neutral)
            {
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
                }
            }
            else
            {
                s = "...";
            }
            this.itemConversation.Interrupt(this.Translate(s), 10);
            this.restartConversationAfterCurrentDialoge = true;
            //抽到前四条会接着读，抽到第五条则不会继续读；NSH不喜欢猫时必定不能继续
            if (num > 3 || State.GetOpinion != NSHOracleState.PlayerOpinion.Likes)
            {
                this.refuseRestartConversation = true;
            }
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
                else
                {
                    this.readItemOrbits.Add(abstractWorldEntity);
                }
            }
            int num = 0;
            foreach (DataPearl.AbstractDataPearl abstractDataPearl2 in this.readDataPearlOrbits)
            {
                Vector2 pos = this.storedPearlOrbitLocation(num);
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

        #region 对话相关
        public void PlayerEncountersAdd()
        {
            State.playerEncounters++;
            if (this.oracle.room.game.GetStorySession.saveState.deathPersistentSaveData.theMark)
            {
                State.playerEncountersWithMark++;
            }
        }

        public void PlayerEncountersWithoutMark()
        {
            if (State.playerEncounters == 1)
                this.oracle.room.PlaySound(NSHOracleSoundID.NSH_AI_NoDialogue_1, 0f, 1f, 1.25f);
            else
            {
                switch (Random.Range(0, 2))
                {
                    case 0:
                        this.oracle.room.PlaySound(NSHOracleSoundID.NSH_AI_NoDialogue_2, 0f, 1f, 1.25f);
                        break;
                    case 1:
                        this.oracle.room.PlaySound(NSHOracleSoundID.NSH_AI_NoDialogue_3, 0f, 1f, 1.25f);
                        break;
                }
            }
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
    }
}
