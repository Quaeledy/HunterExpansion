using MoreSlugcats;
using UnityEngine;
using static CustomOracleTx.CustomOracleBehaviour;
using OraclePanicDisplay = HunterExpansion.CustomEffects.OraclePanicDisplay;

namespace HunterExpansion.CustomOracle
{
    public class NSHOracleMeetSaint : NSHConversationBehaviour
    {
        public OraclePanicDisplay panicObject;
        public int panicTimer;
        public int timeUntilNextPanic;
        public float lastGetToWork;
        public float lowGravity;
        public bool gravOn;
        public bool firstMetOnThisCycle;

        public override bool Gravity
        {
            get
            {
                return this.gravOn;
            }
        }

        public override float LowGravity
        {
            get
            {
                return this.lowGravity;
            }
        }

        public NSHOracleMeetSaint(NSHOracleBehaviour owner) : base(owner)
        {
            this.PickNextPanicTime();
            this.lowGravity = -1f;
            (this.owner as NSHOracleBehaviour).TurnOffSSMusic(true);
            owner.getToWorking = 1f;
            this.gravOn = true;
            if (this.owner.conversation != null)
            {
                this.owner.conversation.Destroy();
                this.owner.conversation = null;
            }
        }

        public static bool SubBehaviourIsMeetSaint(CustomAction nextAction)
        {
            return nextAction == NSHOracleBehaviorAction.MeetSaint_Init ||
                   nextAction == NSHOracleBehaviorAction.MeetSaint_Idle ||
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
            if (action == NSHOracleBehaviorAction.MeetSaint_Idle && (owner as NSHOracleBehaviour).IsSeePlayer())
            {
                owner.NewAction(NSHOracleBehaviorAction.MeetSaint_Init);
            }
            if (action == NSHOracleBehaviorAction.MeetSaint_Init)
            {
                movementBehavior = CustomMovementBehavior.KeepDistance;
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

            if (this.panicObject == null || this.panicObject.slatedForDeletetion)
            {
                if (this.panicObject != null)
                {
                    this.owner.getToWorking = this.lastGetToWork;
                }
                this.panicObject = null;
                this.lastGetToWork = this.owner.getToWorking;
            }
            else
            {
                this.owner.getToWorking = 1f;
                /*
                if (owner.conversation != null)
                {
                    owner.conversation.Interrupt(this.Translate("..."), 100);
                    //this.conversation.RestartCurrent();//配套的恢复是这个方法
                }*/
                if (this.lowGravity < 0f)
                {
                    this.lowGravity = 0f;
                }
                if (this.panicObject.gravOn)
                {
                    this.lowGravity = Mathf.Lerp(this.lowGravity, 0.5f, 0.01f);
                }
                this.gravOn = this.panicObject.gravOn;
                movementBehavior = NSHOracleMovementBehavior.Meditate;
                this.owner.SetNewDestination(base.oracle.firstChunk.pos);
            }

            if (this.panicObject == null)
            {
                this.lowGravity = -1f;
                if (owner.conversation == null)
                {
                    this.panicTimer++;
                    if (movementBehavior != NSHOracleMovementBehavior.Meditate && movementBehavior != CustomMovementBehavior.Idle)
                    {
                        movementBehavior = NSHOracleMovementBehavior.Meditate;
                    }
                }
                if (this.panicTimer > this.timeUntilNextPanic)
                {
                    this.panicTimer = 0;
                    this.PickNextPanicTime();
                    this.panicObject = new OraclePanicDisplay(base.oracle);
                    base.oracle.room.AddObject(this.panicObject);
                }
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

        //与圣徒的所有对话
        public void AddConversationEvents(NSHConversation conv, Conversation.ID id)
        {
            //猫猫没有语言印记也读
            //现实对话
            if (id == NSHConversationID.Saint_Talk0)
            {
                NSHConversation.LoadEventsFromFile(conv, 0, oracle.room.world.game.session.characterStats.name.value + "-0");
            }
            else if (id == NSHConversationID.Saint_Talk1)
            {
                NSHConversation.LoadEventsFromFile(conv, 0, oracle.room.world.game.session.characterStats.name.value + "-1");
            }
            else if (id == NSHConversationID.Saint_Talk2)
            {
                NSHConversation.LoadEventsFromFile(conv, 0, oracle.room.world.game.session.characterStats.name.value + "-2");
            }
        }

        public void PickNextPanicTime()
        {
            this.timeUntilNextPanic = Random.Range(800, 2400);
        }
    }
}
