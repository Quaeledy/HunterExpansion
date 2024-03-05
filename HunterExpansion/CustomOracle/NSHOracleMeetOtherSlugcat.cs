using MoreSlugcats;
using UnityEngine;
using static CustomOracleTx.CustomOracleBehaviour;

namespace HunterExpansion.CustomOracle
{
    public class NSHOracleMeetOtherSlugcat : CustomConversationBehaviour
    {
        public NSHOracleMeetOtherSlugcat(NSHOracleBehaviour owner) : base(owner, NSHOracleBehaviorSubBehavID.MeetOtherSlugcat, NSHConversationID.OtherSlugcat_Talk0)
        {
        }

        public static bool SubBehaviourIsMeetOtherSlugcat(CustomAction nextAction)
        {
            return nextAction == NSHOracleBehaviorAction.MeetOtherSlugcat_Init ||
                   nextAction == NSHOracleBehaviorAction.MeetOtherSlugcat_Talk1 ||
                   nextAction == NSHOracleBehaviorAction.MeetOtherSlugcat_Talk2;
        }

        public override void Update()
        {
            base.Update();
            if (player == null || oracle.room == null || !(oracle.room.world.game.session is StoryGameSession))
                return;
            if (oracle.room.world.game.session.characterStats.name == SlugcatStats.Name.White ||
                oracle.room.world.game.session.characterStats.name == SlugcatStats.Name.Yellow ||
                oracle.room.world.game.session.characterStats.name == SlugcatStats.Name.Red ||
                oracle.room.world.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Spear ||
                oracle.room.world.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Artificer ||
                oracle.room.world.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Gourmand ||
                oracle.room.world.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Rivulet ||
                oracle.room.world.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Saint ||
                oracle.room.world.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
            {
                return;
            }

            if (action == NSHOracleBehaviorAction.MeetOtherSlugcat_Init)
            {
                movementBehavior = CustomMovementBehavior.Idle;
                //现实行为
                NSHOracleState state = (this.owner as NSHOracleBehaviour).State;
                if (state.playerEncountersWithMark == 0 && inActionCounter > 20)
                {
                    owner.NewAction(NSHOracleBehaviorAction.MeetOtherSlugcat_Talk1);
                    (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                    return;
                }
                else if (state.playerEncountersWithMark >= 1 && inActionCounter > 20)
                {
                    owner.NewAction(NSHOracleBehaviorAction.MeetOtherSlugcat_Talk2);
                    (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                    return;
                }
                return;
            }
            //现实对话
            else if (action == NSHOracleBehaviorAction.MeetOtherSlugcat_Talk1 ||
                     action == NSHOracleBehaviorAction.MeetOtherSlugcat_Talk2)
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
            if (newAction == NSHOracleBehaviorAction.MeetOtherSlugcat_Talk1)
            {
                owner.getToWorking = 0f;
                movementBehavior = CustomMovementBehavior.KeepDistance;
                owner.InitateConversation(NSHConversationID.OtherSlugcat_Talk0, this);
            }
            else if (newAction == NSHOracleBehaviorAction.MeetOtherSlugcat_Talk2)
            {
                owner.getToWorking = 0f;
                movementBehavior = CustomMovementBehavior.KeepDistance;
                owner.InitateConversation(NSHConversationID.OtherSlugcat_Talk1, this);
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
                if (id == NSHConversationID.OtherSlugcat_Talk0)
                {
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Strange little guy, hello."), 20 * extralingerfactor));
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("You are such an interesting little creature... different from the guests who used to come to my place."), 80 * extralingerfactor));
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Anyway, I welcome you to stay here and rest, as long as you don't intend to consume my neurons."), 70 * extralingerfactor));
                }
                else if (id == NSHConversationID.OtherSlugcat_Talk1)
                {
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Do you want to stay a little longer?"), 25 * extralingerfactor));
                    conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Of course you can! As long as you don't eat my neurons."), 40 * extralingerfactor));
                }
            }
            else
            {
                (this.owner as NSHOracleBehaviour).PlayerEncountersWithoutMark();
            }
        }
    }
}
