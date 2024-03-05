using UnityEngine;
using static CustomOracleTx.CustomOracleBehaviour;
using MoreSlugcats;
using HunterExpansion.CustomDream;

namespace HunterExpansion.CustomOracle
{
    public class NSHOracleMeetSpear : CustomConversationBehaviour
    {
        public NSHOracleMeetSpear(NSHOracleBehaviour owner) : base(owner, NSHOracleBehaviorSubBehavID.MeetSpear, NSHConversationID.Spear_Talk0)
        {
        }

        public static bool SubBehaviourIsMeetSpear(CustomAction nextAction)
        {
            return nextAction == NSHOracleBehaviorAction.MeetSpear_Init ||
                   nextAction == NSHOracleBehaviorAction.MeetSpear_Talk1 ||
                   nextAction == NSHOracleBehaviorAction.MeetSpear_Talk2 ||
                   nextAction == NSHOracleBehaviorAction.MeetSpear_Talk3 ||
                   nextAction == NSHOracleBehaviorAction.MeetSpear_AfterSpearMeetPebbles_1 ||
                   nextAction == NSHOracleBehaviorAction.MeetSpear_AfterSpearMeetPebbles_2 ||
                   nextAction == NSHOracleBehaviorAction.MeetSpear_AfterAltEnd_1 ||
                   nextAction == NSHOracleBehaviorAction.MeetSpear_AfterAltEnd_2 ||
                   nextAction == NSHOracleBehaviorAction.MeetSpear_AfterAltEnd_3;
        }

        public override void Update()
        {
            base.Update();
            if (player == null || oracle.room == null || !(oracle.room.world.game.session is StoryGameSession))
                return;
            if (oracle.room.world.game.session.characterStats.name != MoreSlugcatsEnums.SlugcatStatsName.Spear)
            {
                return;
            }

            NSHOracleState state = (this.owner as NSHOracleBehaviour).State;
            if (action == NSHOracleBehaviorAction.MeetSpear_Init)
            {
                movementBehavior = CustomMovementBehavior.Talk;
                if (state.playerEncountersState != GetPlayerEncountersState())
                {
                    state.playerEncountersState = GetPlayerEncountersState();
                    state.playerEncountersWithMark = 0;
                }
                //现实行为
                //结局后对话
                if (oracle.room.game.rainWorld.progression.currentSaveState.deathPersistentSaveData.altEnding)
                {
                    if (state.playerEncountersWithMark == 0 && inActionCounter > 20)
                    {
                        owner.NewAction(NSHOracleBehaviorAction.MeetSpear_AfterAltEnd_1);
                        (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                        return;
                    }
                    else if(state.playerEncountersWithMark == 1 && inActionCounter > 20)
                    {
                        owner.NewAction(NSHOracleBehaviorAction.MeetSpear_AfterAltEnd_2);
                        (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                        return;
                    }
                    else if (state.playerEncountersWithMark >= 2 && inActionCounter > 20)
                    {
                        owner.NewAction(NSHOracleBehaviorAction.MeetSpear_AfterAltEnd_3);
                        (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                        return;
                    }
                }
                //矛大师已经见过FP后的对话，用FP是否读过矛大师珍珠来判断
                else if (this.owner.rainWorld.progression.miscProgressionData.decipheredPebblesPearls.Contains(MoreSlugcatsEnums.DataPearlType.Spearmasterpearl))
                {
                    if (state.playerEncountersWithMark == 0 && inActionCounter > 20)
                    {
                        owner.NewAction(NSHOracleBehaviorAction.MeetSpear_AfterSpearMeetPebbles_1);
                        (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                        return;
                    }
                    else if (state.playerEncountersWithMark >= 1 && inActionCounter > 20)
                    {
                        owner.NewAction(NSHOracleBehaviorAction.MeetSpear_AfterSpearMeetPebbles_2);
                        (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                        return;
                    }
                }
                //其他对话
                else
                {
                    if (state.playerEncountersWithMark == 0 && inActionCounter > 20)
                    {
                        owner.NewAction(NSHOracleBehaviorAction.MeetSpear_Talk1);
                        (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                        return;
                    }
                    else if (state.playerEncountersWithMark == 1 && inActionCounter > 20)
                    {
                        owner.NewAction(NSHOracleBehaviorAction.MeetSpear_Talk2);
                        (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                        return;
                    }
                    else if (state.playerEncountersWithMark >= 2 && inActionCounter > 20)
                    {
                        owner.NewAction(NSHOracleBehaviorAction.MeetSpear_Talk3);
                        (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                        return;
                    }
                }
                return;
            }
            //现实对话
            else if (action == NSHOracleBehaviorAction.MeetSpear_Talk1 ||
                     action == NSHOracleBehaviorAction.MeetSpear_Talk2 ||
                     action == NSHOracleBehaviorAction.MeetSpear_Talk3)
            {
                if (inActionCounter == 100)
                {
                    PlayerHooks.SpawnOverseerInRoom(oracle.room, "NSH_AI", CreatureTemplate.Type.Overseer, 3);
                }
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
            else if (action == NSHOracleBehaviorAction.MeetSpear_AfterSpearMeetPebbles_1 ||
                     action == NSHOracleBehaviorAction.MeetSpear_AfterSpearMeetPebbles_2)
            {
                if (inActionCounter == 100)
                {
                    PlayerHooks.SpawnOverseerInRoom(oracle.room, "NSH_AI", CreatureTemplate.Type.Overseer, 3);
                }
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
            else if (action == NSHOracleBehaviorAction.MeetSpear_AfterAltEnd_1 ||
                     action == NSHOracleBehaviorAction.MeetSpear_AfterAltEnd_2 ||
                     action == NSHOracleBehaviorAction.MeetSpear_AfterAltEnd_3)
            {
                if (inActionCounter == 100)
                {
                    PlayerHooks.SpawnOverseerInRoom(oracle.room, "NSH_AI", CreatureTemplate.Type.Overseer, 3);
                }
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
            if (newAction == NSHOracleBehaviorAction.MeetSpear_Talk1)
            {
                owner.getToWorking = 0f;
                owner.InitateConversation(NSHConversationID.Spear_Talk0, this);
            }
            else if (newAction == NSHOracleBehaviorAction.MeetSpear_Talk2)
            {
                owner.getToWorking = 0f;
                owner.InitateConversation(NSHConversationID.Spear_Talk1, this);
            }
            else if (newAction == NSHOracleBehaviorAction.MeetSpear_Talk3)
            {
                owner.getToWorking = 0f;
                owner.InitateConversation(NSHConversationID.Spear_Talk2, this);
            }
            else if (newAction == NSHOracleBehaviorAction.MeetSpear_AfterSpearMeetPebbles_1)
            {
                owner.getToWorking = 0f;
                movementBehavior = CustomMovementBehavior.Investigate;
                owner.InitateConversation(NSHConversationID.Spear_AfterMeetPebbles_0, this);
            }
            else if (newAction == NSHOracleBehaviorAction.MeetSpear_AfterSpearMeetPebbles_2)
            {
                owner.getToWorking = 1f;
                owner.InitateConversation(NSHConversationID.Spear_AfterMeetPebbles_1, this);
            }
            else if (newAction == NSHOracleBehaviorAction.MeetSpear_AfterAltEnd_1)
            {
                owner.getToWorking = 0f;
                owner.InitateConversation(NSHConversationID.Spear_AfterAltEnd_0, this);
            }
            else if (newAction == NSHOracleBehaviorAction.MeetSpear_AfterAltEnd_2)
            {
                owner.getToWorking = 1f;
                owner.InitateConversation(NSHConversationID.Spear_AfterAltEnd_1, this);
            }
            else if (newAction == NSHOracleBehaviorAction.MeetSpear_AfterAltEnd_3)
            {
                owner.getToWorking = 1f;
                owner.InitateConversation(NSHConversationID.Spear_AfterAltEnd_2, this);
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
                if (id == NSHConversationID.Spear_Talk0)
                {
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Are you..."), 10 * extralingerfactor));
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("The messenger of Suns."), 20 * extralingerfactor));
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Are you lost? The situation is urgent now, and we all need you to rush to Five Pebbles as soon as possible."), 100 * extralingerfactor));
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Please set off quickly, I will guide you on the way."), 50 * extralingerfactor));
                }
                else if (id == NSHConversationID.Spear_Talk1)
                {
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Is there anything I can do? I'm not the iterator you're looking for, little idiot."), 80 * extralingerfactor));
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("She doesn't have much time, please hurry up and get on the road."), 60 * extralingerfactor));
                }
                else if (id == NSHConversationID.Spear_Talk2)
                {
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Suns, what's wrong with your messenger?"), 40 * extralingerfactor));
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Little one, set off quickly, please."), 40 * extralingerfactor));
                }
                else if (id == NSHConversationID.Spear_AfterMeetPebbles_0)
                {
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Messenger? Come here and take a break. It's not easy to make this trip, is it."), 50 * extralingerfactor));
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("I know that guy is hard to persuade, but I didn't expect it to end in this way."), 60 * extralingerfactor));
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Ha, he has already treated Moon like this, what's so unexpected?"), 80 * extralingerfactor));
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("I will think of other methods."), 80 * extralingerfactor));
                }
                else if (id == NSHConversationID.Spear_AfterMeetPebbles_1)
                {
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("......"), 10 * extralingerfactor));
                }
                else if (id == NSHConversationID.Spear_AfterAltEnd_0)
                {
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("I received her... message."), 50 * extralingerfactor));
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Did you do it, little messenger? Thank you."), 60 * extralingerfactor));
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Your mission has been completed and your return journey is smooth."), 80 * extralingerfactor));
                }
                else if (id == NSHConversationID.Spear_AfterAltEnd_1)
                {
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Is there anything you need?"), 50 * extralingerfactor));
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("If not, do you mind leaving me alone for a while? I need to think."), 60 * extralingerfactor));
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Goodbye, excellent messenger."), 80 * extralingerfactor));
                }
                else if (id == NSHConversationID.Spear_AfterAltEnd_2)
                {
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Do you still want to stay here and rest? I don't object."), 50 * extralingerfactor));
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("But don't forget that Suns is still waiting for you, messenger."), 60 * extralingerfactor));
                }
            }
            else
            {
                (this.owner as NSHOracleBehaviour).PlayerEncountersWithoutMark();
            }
        }

        //用于计算矛大师的时间状态
        private int GetPlayerEncountersState()
        {
            if (oracle.room.game.rainWorld.progression.currentSaveState.deathPersistentSaveData.altEnding)
                return 2;
            else if (this.owner.rainWorld.progression.miscProgressionData.decipheredPebblesPearls.Contains(MoreSlugcatsEnums.DataPearlType.Spearmasterpearl))
                return 1;
            else
                return 0;
        }
    }
}
