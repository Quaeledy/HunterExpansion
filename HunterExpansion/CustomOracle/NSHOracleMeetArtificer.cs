using UnityEngine;
using static CustomOracleTx.CustomOracleBehaviour;
using MoreSlugcats;

namespace HunterExpansion.CustomOracle
{
    public class NSHOracleMeetArtificer : CustomConversationBehaviour
    {
        public NSHOracleMeetArtificer(NSHOracleBehaviour owner) : base(owner, NSHOracleBehaviorSubBehavID.MeetArtificer, NSHConversationID.Artificer_Talk0)
        {
        }

        public static bool SubBehaviourIsMeetArtificer(CustomAction nextAction)
        {
            return nextAction == NSHOracleBehaviorAction.MeetArtificer_Init ||
                   nextAction == NSHOracleBehaviorAction.MeetArtificer_Talk1 ||
                   nextAction == NSHOracleBehaviorAction.MeetArtificer_Talk2;
        }

        public override void Update()
        {
            base.Update();
            if (player == null || oracle.room == null || !(oracle.room.world.game.session is StoryGameSession)) 
                return;
            if (oracle.room.world.game.session.characterStats.name != MoreSlugcatsEnums.SlugcatStatsName.Artificer)
            {
                return;
            }

            if (action == NSHOracleBehaviorAction.MeetArtificer_Init)
            {
                movementBehavior = CustomMovementBehavior.KeepDistance;
                //现实行为
                NSHOracleState state = (this.owner as NSHOracleBehaviour).State;
                if (state.playerEncountersWithMark == 0 && inActionCounter > 20)
                {
                    owner.NewAction(NSHOracleBehaviorAction.MeetArtificer_Talk1);
                    (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                    return;
                }
                else if (state.playerEncountersWithMark >= 1 && inActionCounter > 20)
                {
                    owner.NewAction(NSHOracleBehaviorAction.MeetArtificer_Talk2);
                    (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                    return;
                }
                return;
            }
            //现实对话
            else if (action == NSHOracleBehaviorAction.MeetArtificer_Talk1 ||
                     action == NSHOracleBehaviorAction.MeetArtificer_Talk2)
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
        }

        public override void NewAction(CustomAction oldAction, CustomAction newAction)
        {
            base.NewAction(oldAction, newAction);
            if (newAction == NSHOracleBehaviorAction.MeetArtificer_Talk1)
            {
                owner.getToWorking = 0f;
                movementBehavior = CustomMovementBehavior.KeepDistance;
                owner.InitateConversation(NSHConversationID.Artificer_Talk0, this);
            }
            else if (newAction == NSHOracleBehaviorAction.MeetArtificer_Talk2)
            {
                owner.getToWorking = 0f;
                movementBehavior = CustomMovementBehavior.KeepDistance;
                owner.InitateConversation(NSHConversationID.Artificer_Talk1, this);
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

        //与矛大师的所有对话
        public void AddConversationEvents(CustomOracleConversation conv, Conversation.ID id)
        {
            int extralingerfactor = oracle.room.game.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.Chinese ? 1 : 0;
            //现实对话
            if (id == NSHConversationID.Artificer_Talk0)
            {
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Hello! It seems that I have welcomed a citizen. Let me see... Surprisingly, you belongs to Five Pebbles."), 70 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Then we are enemies, please go back!"), 30 * extralingerfactor));
                conv.events.Add(new CustomOracleConversation.PauseAndWaitForStillEvent(conv, conv.convBehav, 5));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Haha, it's just a joke~"), 20 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("I am very dissatisfied with him, but I don't want to vent my anger on small animals yet."), 60 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("You don't seem to have an easy time, but I can't help you. Nevertheless,<LINE>before you depart, you can rest here to your heart's content."), 100 * extralingerfactor));
            }
            else if (id == NSHConversationID.Artificer_Talk1)
            {
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Is there anything here that attracts you?"), 40 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Oh, of course, you can stay here. I have no intention of interfering with<LINE>your decision, as long as you don't disrupt my structure."), 90 * extralingerfactor));
            }
        }
    }
}
