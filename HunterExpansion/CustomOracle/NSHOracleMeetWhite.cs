using UnityEngine;
using static CustomOracleTx.CustomOracleBehaviour;

namespace HunterExpansion.CustomOracle
{
    public class NSHOracleMeetWhite : NSHConversationBehaviour
    {
        public NSHOracleMeetWhite(NSHOracleBehaviour owner) : base(owner)
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

        //与求生者的所有对话
        public void AddConversationEvents(NSHConversation conv, Conversation.ID id)
        {
            //现实对话
            if (id == NSHConversationID.White_Talk0)
            {
                NSHConversation.LoadEventsFromFile(conv, 0, oracle.room.world.game.session.characterStats.name.value + "-0");
            }
            else if (id == NSHConversationID.White_Talk1)
            {
                NSHConversation.LoadEventsFromFile(conv, 0, oracle.room.world.game.session.characterStats.name.value + "-1");
            }
            else if (id == NSHConversationID.White_Talk2)
            {
                NSHConversation.LoadEventsFromFile(conv, 0, oracle.room.world.game.session.characterStats.name.value + "-2");
            }
        }
    }
}
