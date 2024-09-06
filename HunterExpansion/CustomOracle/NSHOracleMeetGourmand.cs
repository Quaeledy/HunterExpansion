using UnityEngine;
using static CustomOracleTx.CustomOracleBehaviour;
using MoreSlugcats;

namespace HunterExpansion.CustomOracle
{
    public class NSHOracleMeetGourmand : NSHConversationBehaviour
    {
        public NSHOracleMeetGourmand(NSHOracleBehaviour owner) : base(owner)
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
                movementBehavior = CustomMovementBehavior.KeepDistance;
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
        public void AddConversationEvents(NSHConversation conv, Conversation.ID id)
        {
            int extralingerfactor = oracle.room.game.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.Chinese ? 1 : 0;
            //现实对话
            if (id == NSHConversationID.Gourmand_Talk0)
            {
                NSHConversation.LoadEventsFromFile(conv, 0, oracle.room.world.game.session.characterStats.name.value + "-0");
            }
            else if (id == NSHConversationID.Gourmand_Talk1)
            {
                NSHConversation.LoadEventsFromFile(conv, 0, oracle.room.world.game.session.characterStats.name.value + "-1");
            }
            else if (id == NSHConversationID.Gourmand_Talk2)
            {
                NSHConversation.LoadEventsFromFile(conv, 0, oracle.room.world.game.session.characterStats.name.value + "-2");
            }
        }
    }
}
