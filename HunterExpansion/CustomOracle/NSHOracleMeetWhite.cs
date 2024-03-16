using UnityEngine;
using static CustomOracleTx.CustomOracleBehaviour;

namespace HunterExpansion.CustomOracle
{
    public class NSHOracleMeetWhite : CustomConversationBehaviour
    {
        public NSHOracleMeetWhite(NSHOracleBehaviour owner) : base(owner, NSHOracleBehaviorSubBehavID.MeetWhite, NSHConversationID.White_Talk0)
        {
        }

        public static bool SubBehaviourIsMeetWhite(CustomAction nextAction)
        {
            return nextAction == NSHOracleBehaviorAction.MeetWhite_Init ||
                   nextAction == NSHOracleBehaviorAction.MeetWhite_Talk1 ||
                   nextAction == NSHOracleBehaviorAction.MeetWhite_Talk2 ||
                   nextAction == NSHOracleBehaviorAction.MeetWhite_Talk3;
        }

        //玩家离开nsh房间会导致报错
        public override void Update()
        {
            base.Update();
            if (player == null || oracle.room == null || !(oracle.room.world.game.session is StoryGameSession))
                return;
            if (oracle.room.world.game.session.characterStats.name != SlugcatStats.Name.White)
            {
                return;
            }

            if (action == NSHOracleBehaviorAction.MeetWhite_Init)
            {
                movementBehavior = CustomMovementBehavior.KeepDistance;
                //现实行为
                NSHOracleState state = (this.owner as NSHOracleBehaviour).State;
                if (state.playerEncountersWithMark == 0 && inActionCounter > 20)
                {
                    owner.NewAction(NSHOracleBehaviorAction.MeetWhite_Talk1);
                    (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                    return;
                }
                else if (state.playerEncountersWithMark == 1 && inActionCounter > 20)
                {
                    owner.NewAction(NSHOracleBehaviorAction.MeetWhite_Talk2);
                    (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                    return;
                }
                else if (state.playerEncountersWithMark >= 2 && inActionCounter > 20)
                {
                    owner.NewAction(NSHOracleBehaviorAction.MeetWhite_Talk2);
                    (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                    return;
                }
                return;
            }
            //现实对话
            else if (action == NSHOracleBehaviorAction.MeetWhite_Talk1 ||
                     action == NSHOracleBehaviorAction.MeetWhite_Talk2 ||
                     action == NSHOracleBehaviorAction.MeetWhite_Talk3)
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
            if (newAction == NSHOracleBehaviorAction.MeetWhite_Talk1)
            {
                owner.getToWorking = 0f;
                movementBehavior = CustomMovementBehavior.KeepDistance;
                owner.InitateConversation(NSHConversationID.White_Talk0, this);
            }
            else if (newAction == NSHOracleBehaviorAction.MeetWhite_Talk2)
            {
                owner.getToWorking = 0f;
                movementBehavior = CustomMovementBehavior.KeepDistance;
                owner.InitateConversation(NSHConversationID.White_Talk1, this);
            }
            else if (newAction == NSHOracleBehaviorAction.MeetWhite_Talk3)
            {
                owner.getToWorking = 0f;
                movementBehavior = CustomMovementBehavior.KeepDistance;
                owner.InitateConversation(NSHConversationID.White_Talk2, this);
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
            if (id == NSHConversationID.White_Talk0)
            {
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Oh? Hello, I haven't encountered a small customer like you in a while."), 60 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("You look very primitive, but you have been marked. There are only Five Pebbles nearby that<LINE>can do this. It seems that you have walked a long way, little traveler."), 120 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("You can rest here for a while. It used to be known as a temple, but now I don't<LINE>mind being treated as an inn anymore~"), 90 * extralingerfactor));
            }
            else if (id == NSHConversationID.White_Talk1)
            {
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Do you want anything else?"), 20 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("If you want a job, I can accept you as a messenger."), 40 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Just kidding. I'm sorry, but there's not much I can do, I can't even hear your needs."), 70 * extralingerfactor));
            }
            else if (id == NSHConversationID.White_Talk2)
            {
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Okay, if you want to stay here, don't forget my words: no neuron buffet!"), 60 * extralingerfactor));
            }
        }
    }
}
