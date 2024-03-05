﻿using HunterExpansion.CustomSave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CustomOracleTx.CustomOracleBehaviour;
using MoreSlugcats;
using UnityEngine;

namespace HunterExpansion.CustomOracle
{
    public class NSHOracleMeetSaint : CustomConversationBehaviour
    {
        public NSHOracleMeetSaint(NSHOracleBehaviour owner) : base(owner, NSHOracleBehaviorSubBehavID.MeetSaint, NSHConversationID.Saint_Talk0)
        {
        }

        public static bool SubBehaviourIsMeetSaint(CustomAction nextAction)
        {
            return nextAction == NSHOracleBehaviorAction.MeetSaint_Init ||
                   nextAction == NSHOracleBehaviorAction.MeetSaint_Talk1 ||
                   nextAction == NSHOracleBehaviorAction.MeetSaint_Talk2 ||
                   nextAction == NSHOracleBehaviorAction.MeetSaint_Talk3;
        }

        public override void Update()
        {
            base.Update();
            if (player == null || oracle.room == null || !(oracle.room.world.game.session is StoryGameSession))
                return;
            if (oracle.room.world.game.session.characterStats.name != MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                return;
            }
            if (action == NSHOracleBehaviorAction.MeetSaint_Init)
            {
                movementBehavior = CustomMovementBehavior.Idle;
                //现实行为
                NSHOracleState state = (this.owner as NSHOracleBehaviour).State;
                if (state.playerEncountersWithMark == 0 && inActionCounter > 20)
                {
                    owner.NewAction(NSHOracleBehaviorAction.MeetSaint_Talk1);
                    (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                    return;
                }
                else if (state.playerEncountersWithMark == 1 && inActionCounter > 20)
                {
                    owner.NewAction(NSHOracleBehaviorAction.MeetSaint_Talk2);
                    (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                    return;
                }
                else if (state.playerEncountersWithMark >= 2 && inActionCounter > 20)
                {
                    owner.NewAction(NSHOracleBehaviorAction.MeetSaint_Talk3);
                    (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                    return;
                }
                return;
            }
            //现实对话
            else if (action == NSHOracleBehaviorAction.MeetSaint_Talk1 ||
                     action == NSHOracleBehaviorAction.MeetSaint_Talk2 ||
                     action == NSHOracleBehaviorAction.MeetSaint_Talk3)
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

            if (oracle.health == 0)
            {
                movementBehavior = CustomMovementBehavior.ShowMedia;
                oracle.firstChunk.vel += 1.5f * Vector2.down;
            }
        }

        public override void NewAction(CustomAction oldAction, CustomAction newAction)
        {
            base.NewAction(oldAction, newAction);
            if (newAction == NSHOracleBehaviorAction.MeetSaint_Talk1)
            {
                owner.getToWorking = 0f;
                movementBehavior = CustomMovementBehavior.KeepDistance;
                owner.InitateConversation(NSHConversationID.Saint_Talk0, this);
            }
            else if (newAction == NSHOracleBehaviorAction.MeetSaint_Talk2)
            {
                owner.getToWorking = 0f;
                movementBehavior = CustomMovementBehavior.KeepDistance;
                owner.InitateConversation(NSHConversationID.Saint_Talk1, this);
            }
            else if (newAction == NSHOracleBehaviorAction.MeetSaint_Talk3)
            {
                owner.getToWorking = 0f;
                movementBehavior = CustomMovementBehavior.KeepDistance;
                owner.InitateConversation(NSHConversationID.Saint_Talk2, this);
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
            //猫猫没有语言印记也读
            //现实对话
            if (id == NSHConversationID.Saint_Talk0)
            {
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Ha, it's really strange! After the weather turns cold, have slugcats evolved hair?"), 70 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Come on, although my room may not be warm, it can barely keep warm. I have always<LINE>loved the companionship of your kind, especially recently."), 110 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("To this day, this land of immortality is still being consumed day after day with us old folks..."), 80 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("I hope you can leave anytime you want. You know, the void sea has always been open for you."), 80 * extralingerfactor));
            }
            else if (id == NSHConversationID.Saint_Talk1)
            {
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Do you want to stay a little longer? I am very welcome."), 50 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("You remind me of the past. Although it may sound incredible, your species has<LINE>always been a good partner for iterators..."), 100 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("- When you're not feasting on neurons."), 30 * extralingerfactor));
            }
            else if (id == NSHConversationID.Saint_Talk2)
            {
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Do you want to stay a little longer? I am very welcome."), 50 * extralingerfactor));
            }
        }
    }
}