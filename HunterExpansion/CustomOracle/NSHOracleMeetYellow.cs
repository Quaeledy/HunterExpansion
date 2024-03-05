using UnityEngine;
using static CustomOracleTx.CustomOracleBehaviour;

namespace HunterExpansion.CustomOracle
{
    public class NSHOracleMeetYellow : CustomConversationBehaviour
    {
        public NSHOracleMeetYellow(NSHOracleBehaviour owner) : base(owner, NSHOracleBehaviorSubBehavID.MeetYellow, NSHConversationID.Yellow_Talk0)
        {
        }

        public static bool SubBehaviourIsMeetYellow(CustomAction nextAction)
        {
            return nextAction == NSHOracleBehaviorAction.MeetYellow_Init ||
                   nextAction == NSHOracleBehaviorAction.MeetYellow_Talk1 ||
                   nextAction == NSHOracleBehaviorAction.MeetYellow_Talk2 ||
                   nextAction == NSHOracleBehaviorAction.MeetYellow_Talk3;
        }

        public override void Update()
        {
            base.Update();
            if (player == null || oracle.room == null || !(oracle.room.world.game.session is StoryGameSession))
                return;
            if (oracle.room.world.game.session.characterStats.name != SlugcatStats.Name.Yellow)
            {
                return;
            }

            if (action == NSHOracleBehaviorAction.MeetYellow_Init)
            {
                movementBehavior = CustomMovementBehavior.Idle;
                //现实行为
                NSHOracleState state = (this.owner as NSHOracleBehaviour).State;
                if (state.playerEncountersWithMark == 0 && inActionCounter > 20)
                {
                    owner.NewAction(NSHOracleBehaviorAction.MeetYellow_Talk1);
                    (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                    return;
                }
                else if (state.playerEncountersWithMark == 1 && inActionCounter > 20)
                {
                    owner.NewAction(NSHOracleBehaviorAction.MeetYellow_Talk2);
                    (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                    return;
                }
                else if (state.playerEncountersWithMark >= 2 && inActionCounter > 20)
                {
                    owner.NewAction(NSHOracleBehaviorAction.MeetYellow_Talk3);
                    (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                    return;
                }
                return;
            }
            //现实对话
            else if (action == NSHOracleBehaviorAction.MeetYellow_Talk1 ||
                     action == NSHOracleBehaviorAction.MeetYellow_Talk2 ||
                     action == NSHOracleBehaviorAction.MeetYellow_Talk3)
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
            if (newAction == NSHOracleBehaviorAction.MeetYellow_Talk1)
            {
                owner.getToWorking = 0f;
                movementBehavior = CustomMovementBehavior.KeepDistance;
                owner.InitateConversation(NSHConversationID.Yellow_Talk0, this);
            }
            else if (newAction == NSHOracleBehaviorAction.MeetYellow_Talk2)
            {
                owner.getToWorking = 0f;
                movementBehavior = CustomMovementBehavior.KeepDistance;
                owner.InitateConversation(NSHConversationID.Yellow_Talk1, this);
            }
            else if (newAction == NSHOracleBehaviorAction.MeetYellow_Talk3)
            {
                owner.getToWorking = 0f;
                movementBehavior = CustomMovementBehavior.KeepDistance;
                owner.InitateConversation(NSHConversationID.Yellow_Talk2, this);
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

        //与黄猫的所有对话
        public void AddConversationEvents(CustomOracleConversation conv, Conversation.ID id)
        {
            int extralingerfactor = oracle.room.game.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.Chinese ? 1 : 0;
            //猫猫有语言印记才会读
            if (this.oracle.room.game.GetStorySession.saveState.deathPersistentSaveData.theMark)
            {
                //现实对话
                if (id == NSHConversationID.Yellow_Talk0)
                {
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Oh, hi! Your kind is really smart, isn't it? It's a rare occurrence for organisms to grope for the<LINE>location of the calculation room, but in hundreds of rain cycles, I actually saw two slugcats."), 180 * extralingerfactor));
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Listen up: The rule of NSH Inn is: YES to overnight, NO to neuron buffet. If you encounter<LINE>someone of your kind, please let them know."), 120 * extralingerfactor));
                }
                else if (id == NSHConversationID.Yellow_Talk1)
                {
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Meeting again, what makes you reluctant to part ways? Let me guess..."), 60 * extralingerfactor));
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Your kind has gone to the jurisdiction of Five Pebbles - if that's what you want to know."), 70 * extralingerfactor));
                }
                else if (id == NSHConversationID.Yellow_Talk2)
                {
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Okay, if you want to stay here, don't forget my words: no neuron buffet!"), 60 * extralingerfactor));
                }
            }
            else
            {
                (this.owner as NSHOracleBehaviour).PlayerEncountersWithoutMark();
            }
        }
    }
}
