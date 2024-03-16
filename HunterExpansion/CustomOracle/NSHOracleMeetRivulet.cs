using static CustomOracleTx.CustomOracleBehaviour;
using MoreSlugcats;
using UnityEngine;
using RWCustom;
using System.Collections.Generic;

namespace HunterExpansion.CustomOracle
{
    public class NSHOracleMeetRivulet : CustomConversationBehaviour
    {
        public NSHOracleMeetRivulet(NSHOracleBehaviour owner) : base(owner, NSHOracleBehaviorSubBehavID.MeetRivulet, NSHConversationID.Rivulet_Talk0)
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
        public void AddConversationEvents(CustomOracleConversation conv, Conversation.ID id)
        {
            int extralingerfactor = oracle.room.game.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.Chinese ? 1 : 0;
            //现实对话
            if (id == NSHConversationID.Rivulet_Talk0)
            {
                conv.events.Add(new CustomOracleConversation.PauseAndWaitForStillEvent(conv, conv.convBehav, 20));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Wow, what a surprise! The last time I saw your species was a long time ago, let alone how special you look."), 80 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("I hope you don't mind me scanning your structure. As a passionate fan of slugcats,<LINE>I find it difficult to restrain my curiosity."), 90 * extralingerfactor));
                conv.events.Add(new CustomOracleConversation.PauseAndWaitForStillEvent(conv, conv.convBehav, 20));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Let me see... you have a unique pair of gills, but your ancestors didn't. It seems that<LINE>your ethnic group has unique adaptability."), 90 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Welcome to rest here with me. If you plan to visit the jurisdiction of<LINE>Looks To The Moon, please help me say hello to her."), 90 * extralingerfactor));
            }
            else if (id == NSHConversationID.Rivulet_Talk1)
            {
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("You can stay, but if I lose a dozen neurons again due to my hospitality..."), 60 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("I swear I can be much more tricky than a bunch of angry scavengers."), 55 * extralingerfactor));
            }
            else if (id == NSHConversationID.Rivulet_Talk2)
            {
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Do you have any further questions?"), 30 * extralingerfactor));
            }
            else if (id == NSHConversationID.Rivulet_AfterAltEnd_0)
            {
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Hello, I am entangled in a strange matter. A while ago, the area of Five Pebbles stopped raining,<LINE>and after a few cycles, the rain clouds began to condense over Looks To The Moon."), 130 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("It is obvious that someone installed his battery for Moon. Considering that her structure has<LINE>been submerged by water, this is almost an impossible task."), 100 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Now that I see you, everything has a clue. Did I guess correctly, the magical little one?<LINE>If you did this, please accept my gratitude."), 90 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("I hope this means that Five Pebbles have already digested his anger. Anyway, what you<LINE>have accomplished has extraordinary significance."), 90 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Okay, you're the only slugcat worth enjoying the neuron buffet, just take it as my thank you."), 70 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Uh, be merciful under your tongue."), 30 * extralingerfactor));
            }
            else if (id == NSHConversationID.Rivulet_AfterAltEnd_1)
            {
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("I calculated your movement speed based on the time interval of battery installation.<LINE>You're really agile, aren't you?"), 80 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("You can simply race against a frightened eggbug, and my fellows may even bet on you."), 60 * extralingerfactor));
            }
            else if (id == NSHConversationID.Rivulet_AfterAltEnd_2)
            {
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Of course you can stay, but I am planning to start studying your body structure."), 60 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Do you mind seeing your own scanned slices?"), 30 * extralingerfactor));
            }
        }

        //用于计算水猫的时间状态
        private int GetPlayerEncountersState()
        {
            if (oracle.room.game.rainWorld.progression.currentSaveState.deathPersistentSaveData.altEnding)
                return 1;
            else
                return 0;
        }
    }
}
