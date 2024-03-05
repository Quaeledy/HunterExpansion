using UnityEngine;
using static CustomOracleTx.CustomOracleBehaviour;
using MoreSlugcats;

namespace HunterExpansion.CustomOracle
{
    public class NSHOracleMeetGourmand : CustomConversationBehaviour
    {
        public NSHOracleMeetGourmand(NSHOracleBehaviour owner) : base(owner, NSHOracleBehaviorSubBehavID.MeetGourmand, NSHConversationID.Gourmand_Talk0)
        {
        }

        public static bool SubBehaviourIsMeetGourmand(CustomAction nextAction)
        {
            return nextAction == NSHOracleBehaviorAction.MeetGourmand_Init ||
                   nextAction == NSHOracleBehaviorAction.MeetGourmand_Talk1 ||
                   nextAction == NSHOracleBehaviorAction.MeetGourmand_Talk2 ||
                   nextAction == NSHOracleBehaviorAction.MeetGourmand_Talk3;
        }

        public override void Update()
        {
            base.Update();
            if (player == null || oracle.room == null || !(oracle.room.world.game.session is StoryGameSession))
                return;
            if (oracle.room.world.game.session.characterStats.name != MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
            {
                return;
            }

            if (action == NSHOracleBehaviorAction.MeetGourmand_Init)
            {
                movementBehavior = CustomMovementBehavior.Idle;
                //现实行为
                NSHOracleState state = (this.owner as NSHOracleBehaviour).State;
                if (state.playerEncountersWithMark == 0 && inActionCounter > 20)
                {
                    owner.NewAction(NSHOracleBehaviorAction.MeetGourmand_Talk1);
                    (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                    return;
                }
                else if (state.playerEncountersWithMark == 1 && inActionCounter > 20)
                {
                    owner.NewAction(NSHOracleBehaviorAction.MeetGourmand_Talk2);
                    (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                    return;
                }
                else if (state.playerEncountersWithMark >= 2 && inActionCounter > 20)
                {
                    owner.NewAction(NSHOracleBehaviorAction.MeetGourmand_Talk3);
                    (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                    return;
                }
                return;
            }
            //现实对话
            else if (action == NSHOracleBehaviorAction.MeetGourmand_Talk1 ||
                     action == NSHOracleBehaviorAction.MeetGourmand_Talk2 ||
                     action == NSHOracleBehaviorAction.MeetGourmand_Talk3)
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
            if (newAction == NSHOracleBehaviorAction.MeetGourmand_Talk1)
            {
                owner.getToWorking = 0f;
                movementBehavior = CustomMovementBehavior.KeepDistance;
                owner.InitateConversation(NSHConversationID.Gourmand_Talk0, this);
            }
            else if (newAction == NSHOracleBehaviorAction.MeetGourmand_Talk2)
            {
                owner.getToWorking = 0f;
                movementBehavior = CustomMovementBehavior.KeepDistance;
                owner.InitateConversation(NSHConversationID.Gourmand_Talk1, this);
            }
            else if (newAction == NSHOracleBehaviorAction.MeetGourmand_Talk3)
            {
                owner.getToWorking = 0f;
                movementBehavior = CustomMovementBehavior.KeepDistance;
                owner.InitateConversation(NSHConversationID.Gourmand_Talk2, this);
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
            //猫猫有语言印记才会读
            if (this.oracle.room.game.GetStorySession.saveState.deathPersistentSaveData.theMark)
            {
                //现实对话
                if (id == NSHConversationID.Gourmand_Talk0)
                {
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("..."), 10 * extralingerfactor));
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("（sound of escaping laughter）"), 0 * extralingerfactor));
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Oh, I'm not laughing at your size - that's something our creator would do. Out of fear of overeating."), 60 * extralingerfactor));
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("But you little creatures and us iterators don't need to be afraid."), 40 * extralingerfactor));
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("You look content, which is a rare thing in our land! It makes my heart sing."), 45 * extralingerfactor));
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Anyway, I'm glad to see you. As long as you don't eat my neurons."), 40 * extralingerfactor));
                }
                else if (id == NSHConversationID.Gourmand_Talk1)
                {
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("You're back. Anything else you need?"), 30 * extralingerfactor));
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Seriously, I can't think of anything you'd need from me, you consummate little creature!"), 50 * extralingerfactor));
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Except for the neurons, of course. This won't work. Give it up."), 40 * extralingerfactor));
                }
                else if (id == NSHConversationID.Gourmand_Talk2)
                {
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Okay, if you want to stay here, remember not to eat my neurons!"), 40 * extralingerfactor));
                }
            }
            else
            {
                (this.owner as NSHOracleBehaviour).PlayerEncountersWithoutMark();
            }
        }
    }
}
