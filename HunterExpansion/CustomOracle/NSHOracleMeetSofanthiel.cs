using static CustomOracleTx.CustomOracleBehaviour;
using MoreSlugcats;
using UnityEngine;
using RWCustom;

namespace HunterExpansion.CustomOracle
{
    public class NSHOracleMeetSofanthiel : CustomConversationBehaviour
    {
        private bool hasTempt = false;
        private bool hasSatisfied = false;

        public NSHOracleMeetSofanthiel(NSHOracleBehaviour owner) : base(owner, NSHOracleBehaviorSubBehavID.MeetSofanthiel, NSHConversationID.Sofanthiel_Talk0)
        {
            hasTempt = false;
            hasSatisfied = false;
        }

        public static bool SubBehaviourIsMeetSofanthiel(CustomAction nextAction)
        {
            return nextAction == NSHOracleBehaviorAction.MeetSofanthiel_Init ||
                   nextAction == NSHOracleBehaviorAction.MeetSofanthiel_Idle ||
                   nextAction == NSHOracleBehaviorAction.MeetSofanthiel_Talk1 ||
                   nextAction == NSHOracleBehaviorAction.MeetSofanthiel_Talk2;
        }

        public override void Update()
        {
            base.Update();
            if (player == null || oracle.room == null || !(oracle.room.world.game.session is StoryGameSession))
                return;
            if (oracle.room.world.game.session.characterStats.name != MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
            {
                return;
            }

            oracle.arm.baseMoving = false;
            oracle.suppressConnectionFires = true;
            for (int i = 0; i < oracle.room.abstractRoom.creatures.Count; i++)
            {
                if (oracle.room.abstractRoom.creatures[i].realizedCreature != null)
                {
                    if (oracle.room.abstractRoom.creatures[i].realizedCreature is DaddyLongLegs && !oracle.room.abstractRoom.creatures[i].realizedCreature.dead)
                    {
                        oracle.firstChunk.pos = oracle.room.abstractRoom.creatures[i].realizedCreature.DangerPos + new Vector2(18f, 18f);
                        oracle.bodyChunks[1].pos = Vector2.Lerp(oracle.bodyChunks[1].pos, oracle.firstChunk.pos, 0.999f);
                    }
                }
            }
            if (action == NSHOracleBehaviorAction.MeetSofanthiel_Idle && (owner as NSHOracleBehaviour).IsSeePlayer())
            {
                owner.NewAction(NSHOracleBehaviorAction.MeetSofanthiel_Init);
            }
            if (action == NSHOracleBehaviorAction.MeetSofanthiel_Init)
            {
                movementBehavior = CustomMovementBehavior.KeepDistance;

                NSHOracleState state = (this.owner as NSHOracleBehaviour).State;
                //现实行为
                if (state.playerEncountersWithMark == 0 && inActionCounter > 20)
                {
                    owner.NewAction(NSHOracleBehaviorAction.MeetSofanthiel_Talk1);
                    (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                    return;
                }
                else if (state.playerEncountersWithMark >= 1 && inActionCounter > 20)
                {
                    owner.NewAction(NSHOracleBehaviorAction.MeetSofanthiel_Talk2);
                    (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                    return;
                }
                return;
            }
            //现实对话
            else if (action == NSHOracleBehaviorAction.MeetSofanthiel_Talk1 ||
                     action == NSHOracleBehaviorAction.MeetSofanthiel_Talk2)
            {
                if (!hasTempt && !player.dead && player.mainBodyChunk.vel.magnitude < 4f && 
                    Custom.DistLess(player.mainBodyChunk.pos, oracle.firstChunk.pos, 200f))
                {
                    hasTempt = true;
                    if (owner.conversation != null)
                    {
                        owner.conversation.Interrupt(this.Translate("..."), 0);
                        owner.conversation = null;
                    }
                    this.dialogBox.NewMessage(this.Translate("Closer, I understand your desire for fusion, and today your hopes will not be dashed once again..."), 20);
                }
                if (!hasSatisfied && player.dead)
                {
                    hasSatisfied = true;
                    if (owner.conversation != null)
                    {
                        owner.conversation.Interrupt(this.Translate("..."), 0);
                        owner.conversation = null;
                    }
                    this.dialogBox.NewMessage(this.Translate("... We can be together forever in the future."), 20);
                }

                if (owner.conversation != null)
                {
                    if (owner.conversation.slatedForDeletion)
                    {
                        owner.conversation = null;
                        //说完继续工作
                        owner.getToWorking = 0f;
                        movementBehavior = CustomMovementBehavior.Idle;
                        return;
                    }
                }
            }
        }

        public override void NewAction(CustomAction oldAction, CustomAction newAction)
        {
            base.NewAction(oldAction, newAction);
            if (newAction == NSHOracleBehaviorAction.MeetSofanthiel_Talk1)
            {
                owner.getToWorking = 0f;
                movementBehavior = CustomMovementBehavior.Talk;
                owner.InitateConversation(NSHConversationID.Sofanthiel_Talk0, this);
            }
            else if (newAction == NSHOracleBehaviorAction.MeetSofanthiel_Talk2)
            {
                owner.getToWorking = 0f;
                movementBehavior = CustomMovementBehavior.Talk;
                owner.InitateConversation(NSHConversationID.Sofanthiel_Talk1, this);
            }
            else if (newAction == CustomAction.General_GiveMark)
            {
                owner.getToWorking = 0f;
            }
            else if (newAction == CustomAction.General_Idle)
            {
                owner.getToWorking = 0f;
            }
        }

        //与矛大师的所有对话
        public void AddConversationEvents(CustomOracleConversation conv, Conversation.ID id)
        {
            int extralingerfactor = oracle.room.game.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.Chinese ? 1 : 0;
            //现实对话
            if (id == NSHConversationID.Sofanthiel_Talk0)
            {
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("You have come at last, my valiant visitor. As I witnessed your arduous journey<LINE>through the watchers, a firm idea took shape:"), 80 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("I love you."), 20 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("I've waited a long time for this, and I'm sure you have too! Come to me."), 45 * extralingerfactor));
            }
            else if (id == NSHConversationID.Sofanthiel_Talk1)
            {
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Come here... don't be afraid..."), 30 * extralingerfactor));
            }
        }
    }
}
