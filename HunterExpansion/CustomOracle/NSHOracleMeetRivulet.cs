using static CustomOracleTx.CustomOracleBehaviour;
using MoreSlugcats;
using UnityEngine;
using RWCustom;
using System.Collections.Generic;

namespace HunterExpansion.CustomOracle
{
    public class NSHOracleMeetRivulet : NSHConversationBehaviour
    {
        public NSHOracleMeetRivulet(NSHOracleBehaviour owner) : base(owner)
        {
            this.communicationIndex = 0;
        }

        public static bool SubBehaviourIsMeetRivulet(CustomAction nextAction)
        {
            return nextAction == NSHOracleBehaviorAction.MeetRivulet_Init ||
                   nextAction == NSHOracleBehaviorAction.MeetRivulet_Talk1 ||
                   nextAction == NSHOracleBehaviorAction.MeetRivulet_Talk2 ||
                   nextAction == NSHOracleBehaviorAction.MeetRivulet_Talk3 ||
                   nextAction == NSHOracleBehaviorAction.MeetRivulet_AfterAltEnd_1 ||
                   nextAction == NSHOracleBehaviorAction.MeetRivulet_AfterAltEnd_2 ||
                   nextAction == NSHOracleBehaviorAction.MeetRivulet_AfterAltEnd_3;
        }

        public override void Update()
        {
            base.Update();
            if (player == null || oracle.room == null || !(oracle.room.world.game.session is StoryGameSession))
                return;
            if (oracle.room.world.game.session.characterStats.name != MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
            {
                return;
            }

            if (action == NSHOracleBehaviorAction.MeetRivulet_Init)
            {
                movementBehavior = CustomMovementBehavior.KeepDistance;
                //现实行为
                NSHOracleState state = (this.owner as NSHOracleBehaviour).State;
                if (state.playerEncountersState != GetPlayerEncountersState())
                {
                    state.playerEncountersState = GetPlayerEncountersState();
                    state.playerEncountersWithMark = 0;
                }
                //结局后对话
                if (oracle.room.game.rainWorld.progression.currentSaveState.deathPersistentSaveData.altEnding)
                {
                    if (state.playerEncountersWithMark == 0 && inActionCounter > 20)
                    {
                        state.likesPlayer = 1f;
                        owner.NewAction(NSHOracleBehaviorAction.MeetRivulet_AfterAltEnd_1);
                        (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                        return;
                    }
                    else if (state.playerEncountersWithMark == 1 && inActionCounter > 20)
                    {
                        owner.NewAction(NSHOracleBehaviorAction.MeetRivulet_AfterAltEnd_2);
                        (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                        return;
                    }
                    else if (state.playerEncountersWithMark >= 2 && inActionCounter > 20)
                    {
                        owner.NewAction(NSHOracleBehaviorAction.MeetRivulet_AfterAltEnd_3);
                        (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                        return;
                    }
                }
                else
                {
                    if (state.playerEncountersWithMark == 0 && inActionCounter > 20)
                    {
                        owner.NewAction(NSHOracleBehaviorAction.MeetRivulet_Talk1);
                        (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                        return;
                    }
                    else if (state.playerEncountersWithMark == 1 && inActionCounter > 20)
                    {
                        owner.NewAction(NSHOracleBehaviorAction.MeetRivulet_Talk2);
                        (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                        return;
                    }
                    else if (state.playerEncountersWithMark >= 2 && inActionCounter > 20)
                    {
                        owner.NewAction(NSHOracleBehaviorAction.MeetRivulet_Talk3);
                        (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                        return;
                    }
                }
                return;
            }
            //现实对话
            else if (action == NSHOracleBehaviorAction.MeetRivulet_Talk1)
            {
                if (owner.conversation != null)
                {
                    if (owner.conversation.events.Count == 4 && this.communicationIndex == 0)
                    {
                        if ((this.owner as NSHOracleBehaviour).showImage != null)
                        {
                            (this.owner as NSHOracleBehaviour).showImage.Destroy();
                            (this.owner as NSHOracleBehaviour).showImage = null;
                        }
                        (this.owner as NSHOracleBehaviour).showImage = base.oracle.myScreen.AddImage(new List<string>
                        {
                             "AIimg1_DM",
                             "AIimg1_NSH"
                        }, 15);
                        (this.owner as NSHOracleBehaviour).showMediaPos = new Vector2(0.4f * base.oracle.room.PixelWidth, 0.4f * base.oracle.room.PixelHeight);
                        base.oracle.room.PlaySound(SoundID.SS_AI_Image, 0f, 1f, 1f);
                        (this.owner as NSHOracleBehaviour).showImage.lastPos = (this.owner as NSHOracleBehaviour).showMediaPos;
                        (this.owner as NSHOracleBehaviour).showImage.pos = (this.owner as NSHOracleBehaviour).showMediaPos;
                        (this.owner as NSHOracleBehaviour).showImage.lastAlpha = 0.91f + Random.value * 0.06f;
                        (this.owner as NSHOracleBehaviour).showImage.alpha = 0.91f + Random.value * 0.06f;
                        (this.owner as NSHOracleBehaviour).showImage.setAlpha = new float?(0.91f + Random.value * 0.06f);
                        this.communicationIndex++;
                        movementBehavior = CustomMovementBehavior.ShowMedia;
                    }
                    if (owner.conversation.events.Count == 2 && this.communicationIndex == 1)
                    {
                        if ((this.owner as NSHOracleBehaviour).showImage != null)
                        {
                            (this.owner as NSHOracleBehaviour).showImage.Destroy();
                            (this.owner as NSHOracleBehaviour).showImage = null;
                        }
                        (this.owner as NSHOracleBehaviour).showImage = base.oracle.myScreen.AddImage(new List<string>
                        {
                             "AIimg1a_NSH",
                             "AIimg1c_NSH"
                        }, 15);
                        (this.owner as NSHOracleBehaviour).showMediaPos = new Vector2(0.4f * base.oracle.room.PixelWidth, 0.4f * base.oracle.room.PixelHeight);
                        base.oracle.room.PlaySound(SoundID.SS_AI_Image, 0f, 1f, 1f);
                        (this.owner as NSHOracleBehaviour).showImage.lastPos = (this.owner as NSHOracleBehaviour).showMediaPos;
                        (this.owner as NSHOracleBehaviour).showImage.pos = (this.owner as NSHOracleBehaviour).showMediaPos;
                        (this.owner as NSHOracleBehaviour).showImage.lastAlpha = 0.91f + Random.value * 0.06f;
                        (this.owner as NSHOracleBehaviour).showImage.alpha = 0.91f + Random.value * 0.06f;
                        (this.owner as NSHOracleBehaviour).showImage.setAlpha = new float?(0.91f + Random.value * 0.06f);
                        this.communicationIndex++;
                        movementBehavior = CustomMovementBehavior.ShowMedia;
                    }
                    if ((this.owner as NSHOracleBehaviour).showImage != null)
                    {
                        if (Random.value < 0.033333335f)
                        {
                            (this.owner as NSHOracleBehaviour).idealShowMediaPos += Custom.RNV() * Random.value * 30f;
                            (this.owner as NSHOracleBehaviour).showMediaPos += Custom.RNV() * Random.value * 30f;
                        }
                        (this.owner as NSHOracleBehaviour).showImage.setPos = new Vector2?((this.owner as NSHOracleBehaviour).showMediaPos);
                        this.owner.lookPoint = (this.owner as NSHOracleBehaviour).showMediaPos;
                    }
                    if (owner.conversation.slatedForDeletion)
                    {
                        if ((this.owner as NSHOracleBehaviour).showImage != null)
                        {
                            (this.owner as NSHOracleBehaviour).showImage.Destroy();
                            (this.owner as NSHOracleBehaviour).showImage = null;
                        }
                        owner.conversation = null;
                        //说完继续工作
                        owner.getToWorking = 1f;
                        movementBehavior = CustomMovementBehavior.Idle;
                        return;
                    }
                }
            }
            else if (action == NSHOracleBehaviorAction.MeetRivulet_Talk2 ||
                     action == NSHOracleBehaviorAction.MeetRivulet_Talk3 ||
                     action == NSHOracleBehaviorAction.MeetRivulet_AfterAltEnd_1 ||
                     action == NSHOracleBehaviorAction.MeetRivulet_AfterAltEnd_2)
            {
                if (owner.conversation != null)
                {
                    if (owner.conversation.slatedForDeletion)
                    {
                        owner.conversation = null;
                        //说完继续工作
                        owner.getToWorking = 1f;
                        movementBehavior = CustomMovementBehavior.Idle;
                        return;
                    }
                }
            }
            else if (action == NSHOracleBehaviorAction.MeetRivulet_AfterAltEnd_3)
            {
                if (owner.conversation != null)
                {
                    //说话时不展示水猫图像
                    if ((this.owner as NSHOracleBehaviour).showImage != null)
                    {
                        (this.owner as NSHOracleBehaviour).showImage.Destroy();
                        (this.owner as NSHOracleBehaviour).showImage = null;
                    }
                    //说完展示水猫图像
                    if (owner.conversation.slatedForDeletion)
                    {
                        (this.owner as NSHOracleBehaviour).showImage = base.oracle.myScreen.AddImage(new List<string>
                        {
                             "AIimg1a_NSH",
                             "AIimg1b_NSH"
                        }, 15);
                        (this.owner as NSHOracleBehaviour).showMediaPos = new Vector2(0.4f * base.oracle.room.PixelWidth, 0.4f * base.oracle.room.PixelHeight);

                        owner.conversation = null;
                        owner.getToWorking = 1f;
                        movementBehavior = CustomMovementBehavior.ShowMedia;
                    }
                }
                if ((this.owner as NSHOracleBehaviour).showImage != null)
                {
                    if (Random.value < 0.033333335f)
                    {
                        (this.owner as NSHOracleBehaviour).idealShowMediaPos += Custom.RNV() * Random.value * 30f;
                        (this.owner as NSHOracleBehaviour).showMediaPos += Custom.RNV() * Random.value * 30f;
                    }
                    base.oracle.room.PlaySound(SoundID.SS_AI_Image, 0f, 1f, 1f);
                    (this.owner as NSHOracleBehaviour).showImage.lastPos = (this.owner as NSHOracleBehaviour).showMediaPos;
                    (this.owner as NSHOracleBehaviour).showImage.pos = (this.owner as NSHOracleBehaviour).showMediaPos;
                    (this.owner as NSHOracleBehaviour).showImage.lastAlpha = 0.91f + Random.value * 0.06f;
                    (this.owner as NSHOracleBehaviour).showImage.alpha = 0.91f + Random.value * 0.06f;
                    (this.owner as NSHOracleBehaviour).showImage.setAlpha = new float?(0.91f + Random.value * 0.06f);
                    (this.owner as NSHOracleBehaviour).showImage.setPos = new Vector2?((this.owner as NSHOracleBehaviour).showMediaPos);
                    this.owner.lookPoint = (this.owner as NSHOracleBehaviour).showMediaPos;
                }
            }
        }

        public override void NewAction(CustomAction oldAction, CustomAction newAction)
        {
            base.NewAction(oldAction, newAction);
            if (newAction == NSHOracleBehaviorAction.MeetRivulet_Talk1)
            {
                owner.getToWorking = 0f;
                movementBehavior = CustomMovementBehavior.KeepDistance;
                owner.InitateConversation(NSHConversationID.Rivulet_Talk0, this);
            }
            else if (newAction == NSHOracleBehaviorAction.MeetRivulet_Talk2)
            {
                owner.getToWorking = 0f;
                movementBehavior = CustomMovementBehavior.KeepDistance;
                owner.InitateConversation(NSHConversationID.Rivulet_Talk1, this);
            }
            else if (newAction == NSHOracleBehaviorAction.MeetRivulet_Talk3)
            {
                owner.getToWorking = 0f;
                movementBehavior = CustomMovementBehavior.KeepDistance;
                owner.InitateConversation(NSHConversationID.Rivulet_Talk2, this);
            }
            else if (newAction == NSHOracleBehaviorAction.MeetRivulet_AfterAltEnd_1)
            {
                owner.getToWorking = 0f;
                movementBehavior = CustomMovementBehavior.KeepDistance;
                owner.InitateConversation(NSHConversationID.Rivulet_AfterAltEnd_0, this);
            }
            else if (newAction == NSHOracleBehaviorAction.MeetRivulet_AfterAltEnd_2)
            {
                owner.getToWorking = 0f;
                movementBehavior = CustomMovementBehavior.KeepDistance;
                owner.InitateConversation(NSHConversationID.Rivulet_AfterAltEnd_1, this);
            }
            else if (newAction == NSHOracleBehaviorAction.MeetRivulet_AfterAltEnd_3)
            {
                owner.getToWorking = 0f;
                movementBehavior = CustomMovementBehavior.KeepDistance;
                owner.InitateConversation(NSHConversationID.Rivulet_AfterAltEnd_2, this);
            }
            else if (newAction == CustomAction.General_GiveMark)
            {
                owner.getToWorking = 0f;
            }
            else if (newAction == CustomAction.General_Idle)
            {
                owner.getToWorking = 1f;
            }
        }

        //与水猫的所有对话
        public void AddConversationEvents(NSHConversation conv, Conversation.ID id)
        {
            int extralingerfactor = oracle.room.game.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.Chinese ? 1 : 0;
            //现实对话
            if (id == NSHConversationID.Rivulet_Talk0)
            {
                NSHConversation.LoadEventsFromFile(conv, 0, oracle.room.world.game.session.characterStats.name.value + "-0");
            }
            else if (id == NSHConversationID.Rivulet_Talk1)
            {
                NSHConversation.LoadEventsFromFile(conv, 0, oracle.room.world.game.session.characterStats.name.value + "-1");
            }
            else if (id == NSHConversationID.Rivulet_Talk2)
            {
                NSHConversation.LoadEventsFromFile(conv, 0, oracle.room.world.game.session.characterStats.name.value + "-2");
            }
            else if (id == NSHConversationID.Rivulet_AfterAltEnd_0)
            {
                NSHConversation.LoadEventsFromFile(conv, 0, oracle.room.world.game.session.characterStats.name.value + "-AltEnding-0");
            }
            else if (id == NSHConversationID.Rivulet_AfterAltEnd_1)
            {
                NSHConversation.LoadEventsFromFile(conv, 0, oracle.room.world.game.session.characterStats.name.value + "-AltEnding-1");
            }
            else if (id == NSHConversationID.Rivulet_AfterAltEnd_2)
            {
                NSHConversation.LoadEventsFromFile(conv, 0, oracle.room.world.game.session.characterStats.name.value + "-AltEnding-2");
            }
        }

        //用于计算水猫的时间状态
        public override int GetPlayerEncountersState()
        {
            if (oracle.room.game.rainWorld.progression.currentSaveState.deathPersistentSaveData.altEnding)
                return 1;
            else
                return 0;
        }
    }
}
