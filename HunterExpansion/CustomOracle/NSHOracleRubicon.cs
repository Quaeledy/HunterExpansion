﻿using CustomOracleTx;
using HunterExpansion.CustomSave;
using MoreSlugcats;
using UnityEngine;
using static CustomOracleTx.CustomOracleBehaviour;
using Random = UnityEngine.Random;

namespace HunterExpansion.CustomOracle
{
    public class NSHOracleRubicon : NSHConversationBehaviour
    {
        public bool noticedPlayer;
        public bool startedConversation;
        public int dissappearedTimer;
        public float ghostOut;
        public HRKarmaShrine shrineControl;
        public int finalGhostFade;

        public NSHOracleRubicon(NSHOracleBehaviour owner) : base(owner)
        {
            Plugin.Log("NSH Oracle load Rubicon Behaviour!");
            noticedPlayer = false;
            startedConversation = false;
            dissappearedTimer = 0;
            ghostOut = 0;
            finalGhostFade = 0;
        }

        public static bool SubBehaviourIsRubicon(CustomAction nextAction)
        {
            return nextAction == NSHOracleBehaviorAction.Rubicon_Init;
        }

        public override void Update()
        {
            base.Update();
            if (this.owner.oracle == null || this.owner.oracle.room == null)
            {
                return;
            }
            if (!this.noticedPlayer)
            {
                this.owner.inActionCounter = 0;
                if (base.player != null && this.owner.oracle.room.GetTilePosition(base.player.mainBodyChunk.pos).y < 35)
                {
                    this.owner.getToWorking = 0f;
                    this.noticedPlayer = true;
                }
            }
            DeathPersistentSaveData deathPersistentSaveData = null;
            if (this.owner.oracle.room.game.IsStorySession)
            {
                deathPersistentSaveData = (this.owner.oracle.room.game.session as StoryGameSession).saveState.deathPersistentSaveData;
            }
            if (deathPersistentSaveData == null)
            {
                return;
            }
            if (this.owner.conversation != null)
            {
                if (base.player != null && base.player.room == this.owner.oracle.room)
                {
                    base.movementBehavior = CustomMovementBehavior.Talk;
                }
                else
                {
                    base.movementBehavior = CustomMovementBehavior.Idle;
                }
            }
            if (base.inActionCounter > 15 && !this.startedConversation && this.owner.conversation == null)
            {
                if (Plugin.ripSRS && RipNSHSave.ripNSH && deathPersistentSaveData.ripMoon && deathPersistentSaveData.ripPebbles)
                {
                    owner.InitateConversation(NSHConversationID.NSH_Moon_Pebbles_SRS_HR, this);
                    //this.Interrupt(base.Translate("..."), 200);
                    this.startedConversation = true;
                    return;
                }
                if (RipNSHSave.ripNSH && deathPersistentSaveData.ripMoon && deathPersistentSaveData.ripPebbles)
                {
                    owner.InitateConversation(MoreSlugcatsEnums.ConversationID.Moon_Pebbles_HR, this);
                    //this.Interrupt(base.Translate("..."), 200);
                    this.startedConversation = true;
                    return;
                }
                else
                {
                    if (RipNSHSave.ripNSH && deathPersistentSaveData.ripMoon)
                    {
                        owner.InitateConversation(MoreSlugcatsEnums.ConversationID.Moon_HR, this);
                        //this.Interrupt(base.Translate("..."), 200);
                        this.startedConversation = true;
                        return;
                    }
                    if (RipNSHSave.ripNSH && deathPersistentSaveData.ripPebbles)
                    {
                        owner.InitateConversation(MoreSlugcatsEnums.ConversationID.Pebbles_HR, this);
                        //this.Interrupt(base.Translate("..."), 200);
                        this.startedConversation = true;
                        return;
                    }
                    if (RipNSHSave.ripNSH)
                    {
                        owner.InitateConversation(NSHConversationID.NSH_HR, this);
                        //this.Interrupt(base.Translate("..."), 200);
                        this.startedConversation = true;
                        return;
                    }
                }
                if (ModManager.MSC && this.oracle.room.world.name == "HR")
                {
                    this.owner.conversation.colorMode = true;
                }
            }
            if (this.owner.conversation != null && !this.owner.conversation.paused && base.player != null && base.player.room != this.owner.oracle.room)
            {
                this.owner.conversation.paused = true;
                this.owner.restartConversationAfterCurrentDialoge = true;
                this.owner.dialogBox.Interrupt(base.Translate("..."), 40);
                this.owner.dialogBox.currentColor = Color.white;
            }
            if ((this.owner.conversation == null || this.owner.conversation.slatedForDeletion) && this.startedConversation)
            {
                this.owner.getToWorking = 1f;
                if (this.dissappearedTimer % 400 == 0)
                {
                    float value = Random.value;
                    if ((double)value < 0.3)
                    {
                        base.movementBehavior = CustomMovementBehavior.Idle;
                    }
                    else if ((double)value > 0.7)
                    {
                        base.movementBehavior = CustomMovementBehavior.KeepDistance;
                    }
                    else
                    {
                        base.movementBehavior = CustomMovementBehavior.Investigate;
                    }
                }
                this.dissappearedTimer++;
            }
            if (RipNSHSave.ripNSH && (deathPersistentSaveData.ripMoon || deathPersistentSaveData.ripPebbles))
            {
                Oracle oracle = null;
                for (int i = 0; i < base.oracle.room.physicalObjects.Length; i++)
                {
                    for (int j = 0; j < base.oracle.room.physicalObjects[i].Count; j++)
                    {
                        if (base.oracle.room.physicalObjects[i][j] is Oracle && (base.oracle.room.physicalObjects[i][j] as Oracle).ID != this.owner.oracle.ID)
                        {
                            oracle = (base.oracle.room.physicalObjects[i][j] as Oracle);
                            //这是让Moon和FP的行为与NSH同步
                            if (oracle != null && (oracle.ID == Oracle.OracleID.SS || oracle.ID == MoreSlugcatsEnums.OracleID.DM || oracle.ID == SRSOracleRegistry.SRSOracle))
                            {
                                if (oracle.oracleBehavior != null)
                                {
                                    if (oracle.ID == Oracle.OracleID.SS || oracle.ID == MoreSlugcatsEnums.OracleID.DM)
                                    {
                                        SSOracleBehavior oracleBehavior = oracle.oracleBehavior as SSOracleBehavior;
                                        oracleBehavior.getToWorking = this.owner.getToWorking;
                                        oracleBehavior.working = this.owner.working;
                                        if (movementBehavior == CustomMovementBehavior.Idle)
                                        {
                                            oracleBehavior.movementBehavior = SSOracleBehavior.MovementBehavior.Idle;
                                        }
                                        else if (movementBehavior == CustomMovementBehavior.KeepDistance)
                                        {
                                            oracleBehavior.movementBehavior = SSOracleBehavior.MovementBehavior.KeepDistance;
                                        }
                                        else if (movementBehavior == CustomMovementBehavior.Investigate)
                                        {
                                            oracleBehavior.movementBehavior = SSOracleBehavior.MovementBehavior.Investigate;
                                        }
                                        else if (movementBehavior == CustomMovementBehavior.Talk)
                                        {
                                            oracleBehavior.movementBehavior = SSOracleBehavior.MovementBehavior.Talk;
                                        }
                                    }
                                    else if (oracle.ID == SRSOracleRegistry.SRSOracle)
                                    {
                                        CustomOracleBehaviour oracleBehavior = oracle.oracleBehavior as CustomOracleBehaviour;
                                        oracleBehavior.getToWorking = this.owner.getToWorking;
                                        oracleBehavior.working = this.owner.working;
                                        oracleBehavior.movementBehavior = this.movementBehavior;
                                    }
                                }
                                oracle.noiseSuppress = base.oracle.noiseSuppress;
                                if (base.oracle.slatedForDeletetion)
                                {
                                    oracle.Destroy();
                                }
                            }
                            else
                            {
                                oracle.Destroy();
                            }
                        }
                    }
                }
            }
            if (this.dissappearedTimer > 320 && this.finalGhostFade == 0)
            {
                if (this.shrineControl == null)
                {
                    for (int k = 0; k < base.oracle.room.updateList.Count; k++)
                    {
                        if (base.oracle.room.updateList[k] is HRKarmaShrine)
                        {
                            this.shrineControl = (base.oracle.room.updateList[k] as HRKarmaShrine);
                        }
                    }
                }
                else
                {
                    this.ghostOut += 0.03666667f;
                    this.shrineControl.EffectFor(this.ghostOut);
                    if (this.ghostOut >= 1f)
                    {
                        this.finalGhostFade = 1;
                    }
                }
            }
            else if (this.finalGhostFade > 0)
            {
                if (this.finalGhostFade == 1)
                {
                    base.oracle.room.PlaySound(SoundID.SB_A14, 0f, 1f, 1f);
                    this.shrineControl.EffectFor(2f);
                    base.oracle.room.AddObject(new GhostHunch(base.oracle.room, null));
                }
                base.oracle.noiseSuppress = Mathf.Min((float)this.finalGhostFade / 20f, 1f);
                if (this.finalGhostFade < 20)
                {
                    for (int l = 0; l < 20; l++)
                    {
                        base.oracle.room.AddObject(new MeltLights.MeltLight(1f, base.oracle.room.RandomPos(), base.oracle.room, RainWorld.GoldRGB));
                    }
                }
                this.finalGhostFade++;
                if (this.finalGhostFade == 35)
                {
                    base.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.hrMelted = true;
                    base.oracle.Destroy();
                    for (int i = 0; i < base.oracle.room.physicalObjects.Length; i++)
                    {
                        for (int j = 0; j < base.oracle.room.physicalObjects[i].Count; j++)
                        {
                            if (base.oracle.room.physicalObjects[i][j] is Oracle && (base.oracle.room.physicalObjects[i][j] as Oracle).ID != this.owner.oracle.ID)
                            {
                                Oracle oracle = (base.oracle.room.physicalObjects[i][j] as Oracle);
                                oracle.Destroy();
                            }
                        }
                    }
                }
            }
            //这是
            if (RipNSHSave.ripNSH && (deathPersistentSaveData.ripMoon || deathPersistentSaveData.ripPebbles || Plugin.ripSRS) && this.owner.oracle.arm != null)
            {
                float x = this.owner.oracle.arm.cornerPositions[0].x;
                float num = this.owner.oracle.arm.cornerPositions[1].x - x;
                float y = this.owner.oracle.arm.cornerPositions[2].y;
                float num2 = this.owner.oracle.arm.cornerPositions[0].y - y;
                float num3 = (this.owner.nextPos.x - x) / num;
                float num4 = (this.owner.baseIdeal.x - x) / num;
                this.owner.nextPos.y = Mathf.Max(this.owner.nextPos.y, y + num2 * num3 + 75f);
                this.owner.baseIdeal.y = Mathf.Max(this.owner.nextPos.y, y + num2 * num4 - 75f);
            }
        }

        public void Interrupt(string text, int delay)
        {
            if (this.owner.conversation != null)
            {
                this.owner.conversation.paused = true;
                this.owner.restartConversationAfterCurrentDialoge = true;
            }
            this.owner.dialogBox.Interrupt(text, delay);
        }

        public void AddConversationEvents(NSHConversation conv, Conversation.ID id)
        {
            int extralingerfactor = oracle.room.game.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.Chinese ? 1 : 0;
            //魔方节点对话
            //四迭都在
            if (id == NSHConversationID.NSH_Moon_Pebbles_SRS_HR)
            {
                conv.events.Add(new NSHConversation.PauseAndWaitForStillEvent(conv, conv.convBehav, 5));
                NSHConversation.LoadEventsFromFile(conv, 135, "NSH_Moon_Pebbles_SRS_HR");
            }
            //NSH，Moon，FP都在
            else if (id == MoreSlugcatsEnums.ConversationID.Moon_Pebbles_HR)
            {
                conv.events.Add(new NSHConversation.PauseAndWaitForStillEvent(conv, conv.convBehav, 5));
                NSHConversation.LoadEventsFromFile(conv, 135, "NSH_Moon_Pebbles_HR");
            }
            //NSH，Moon
            else if (id == MoreSlugcatsEnums.ConversationID.Moon_HR)
            {
                conv.events.Add(new NSHConversation.PauseAndWaitForStillEvent(conv, conv.convBehav, 5));
                NSHConversation.LoadEventsFromFile(conv, 133, "NSH_Moon_HR");
            }
            //NSH，FP
            else if (id == MoreSlugcatsEnums.ConversationID.Pebbles_HR)
            {
                conv.events.Add(new NSHConversation.PauseAndWaitForStillEvent(conv, conv.convBehav, 5));
                NSHConversation.LoadEventsFromFile(conv, 134, "NSH_Pebbles_HR");
            }
            //只有NSH在
            else if (id == NSHConversationID.NSH_HR)
            {
                conv.events.Add(new NSHConversation.PauseAndWaitForStillEvent(conv, conv.convBehav, 5));
                NSHConversation.LoadEventsFromFile(conv, 135, "NSH_HR");
            }
        }
    }
}
