using CustomDreamTx;
using CustomOracleTx;
using HunterExpansion.CustomSave;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using static CustomOracleTx.CustomOracleBehaviour;

namespace HunterExpansion.CustomOracle
{
    public class NSHMeetOtherOracle : CustomConversationBehaviour
    {
        //现实行为
        public static CustomAction MeetMoon_Talk1 = new CustomAction("NSHMeetMoon_Talk1", true);
        public static CustomAction MeetMoon_Talk2 = new CustomAction("NSHMeetMoon_Talk2", true);
        public static CustomAction MeetPebbles_Talk1 = new CustomAction("NSHMeetPebbles_Talk1", true);
        public static CustomAction MeetPebbles_Talk2 = new CustomAction("NSHMeetPebbles_Talk2", true);

        //现实对话
        public static Conversation.ID Moon_Talk0 = new Conversation.ID("NSH_Moon_Talk0", true);
        public static Conversation.ID Moon_Talk1 = new Conversation.ID("NSH_Moon_Talk1", true);
        public static Conversation.ID Pebbles_Talk0 = new Conversation.ID("NSH_Pebbles_Talk0", true);
        public static Conversation.ID Pebbles_Talk1 = new Conversation.ID("NSH_Pebbles_Talk1", true);

        Vector2 wantPos = Vector2.zero;
        public Oracle inspectOracle;

        public NSHMeetOtherOracle(NSHOracleBehaviour owner) : base(owner, NSHOracleBehaviour.MeetOracle, Moon_Talk0)
        {
            //this.inspectOracle = owner.inspectOracle;
        }

        public static bool SubBehaviourIsMeetOracle(CustomAction nextAction)
        {
            return nextAction == NSHOracleBehaviour.MeetOracle_Init || 
                   nextAction == MeetMoon_Talk1 ||
                   nextAction == MeetMoon_Talk2 ||
                   nextAction == MeetPebbles_Talk1 ||
                   nextAction == MeetPebbles_Talk2;
        }

        public override void Update()
        {
            base.Update();
            if (player == null) return;
            if (inspectOracle == null) return;

            string oracleName = inspectOracle.ID.value;
            if (oracleName != null && !NSHTalkSave.talkNum.ContainsKey(oracleName))
            {
                NSHTalkSave.talkNum.Add(oracleName, 0);
            }

            if (action == NSHOracleBehaviour.MeetOracle_Init)
            {
                movementBehavior = CustomMovementBehavior.Idle;
                if (oracleName == "SL" || oracleName == "DM")
                {
                    if (NSHTalkSave.talkNum["SL"] == 0 || NSHTalkSave.talkNum["DM"] == 0)
                    {
                        owner.NewAction(MeetMoon_Talk1);
                        NSHTalkSave.talkNum["SL"]++;
                        NSHTalkSave.talkNum["DM"]++;
                        return;
                    }
                    else if (NSHTalkSave.talkNum["SL"] > 1 || NSHTalkSave.talkNum["DM"] > 1)
                    {
                        owner.NewAction(MeetMoon_Talk2);
                        NSHTalkSave.talkNum["SL"]++;
                        NSHTalkSave.talkNum["DM"]++;
                        return;
                    }
                }
                if (oracleName == "SS" || oracleName == "CL")
                {
                    if (NSHTalkSave.talkNum["SS"] == 0 || NSHTalkSave.talkNum["CL"] == 0)
                    {
                        owner.NewAction(MeetPebbles_Talk1);
                        NSHTalkSave.talkNum["SS"]++;
                        NSHTalkSave.talkNum["CL"]++;
                        return;
                    }
                    else if (NSHTalkSave.talkNum["SS"] > 1 || NSHTalkSave.talkNum["CL"] > 1)
                    {
                        owner.NewAction(MeetPebbles_Talk2);
                        NSHTalkSave.talkNum["SS"]++;
                        NSHTalkSave.talkNum["CL"]++;
                        return;
                    }
                }
                //owner.NewAction(CustomAction.General_Idle);
                return;
            }
            else if (action == MeetMoon_Talk1)
            {
                if (inActionCounter == 1)
                {
                    //如果是拿着迭代器，则释放迭代器
                    if (inspectOracle != null && inspectOracle.grabbedBy.Count > 0)
                    {
                        for (int i = 0; i < inspectOracle.grabbedBy.Count; i++)
                        {
                            Creature grabber = inspectOracle.grabbedBy[i].grabber;
                            if (grabber != null && grabber.grasps != null)
                            {
                                for (int j = 0; j < grabber.grasps.Length; j++)
                                {
                                    if (grabber.grasps[j] != null &&
                                        grabber.grasps[j].grabbed != null &&
                                        grabber.grasps[j].grabbed == inspectOracle)
                                    {
                                        grabber.ReleaseGrasp(j);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    //Moon维持在NSH周围
                    Vector2 wantPos = oracle.firstChunk.pos;
                    inspectOracle.firstChunk.vel *= Custom.LerpMap(inspectOracle.firstChunk.vel.magnitude, 1f, 6f, 0.999f, 0.9f);
                    inspectOracle.firstChunk.vel += Vector2.ClampMagnitude(wantPos - inspectOracle.firstChunk.pos, 100f) / 100f * 0.4f;
                    //NSH到Moon旁边
                    Vector2 NSHwantPos = inspectOracle.firstChunk.pos;
                    oracle.firstChunk.vel *= Custom.LerpMap(oracle.firstChunk.vel.magnitude, 1f, 6f, 0.999f, 0.9f);
                    oracle.firstChunk.vel += Vector2.ClampMagnitude(NSHwantPos - oracle.firstChunk.pos, 100f) / 100f * 1f;
                }
                if (owner.conversation != null)
                {
                    if (owner.conversation.slatedForDeletion)
                    {
                        owner.conversation = null;
                        //说完继续工作
                        owner.getToWorking = 1f;
                        movementBehavior = CustomMovementBehavior.KeepDistance;
                        return;
                    }
                }
            }
            else if (action == MeetMoon_Talk2)
            {
                //维持在NSH周围
                Vector2 wantPos = oracle.firstChunk.pos;
                inspectOracle.firstChunk.vel *= Custom.LerpMap(inspectOracle.firstChunk.vel.magnitude, 1f, 6f, 0.999f, 0.9f);
                inspectOracle.firstChunk.vel += Vector2.ClampMagnitude(wantPos - inspectOracle.firstChunk.pos, 100f) / 100f * 0.4f;
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
            else if (action == MeetPebbles_Talk1)
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
            else if (action == MeetPebbles_Talk2)
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
            if (newAction == MeetMoon_Talk1)
            {
                owner.getToWorking = 0.5f;
                owner.InitateConversation(Moon_Talk0, this);
            }
            else if (newAction == MeetMoon_Talk2)
            {
                owner.getToWorking = 0.5f;
                owner.InitateConversation(Moon_Talk1, this);
            }
            else if (newAction == MeetPebbles_Talk1)
            {
                owner.getToWorking = 0.5f;
                owner.InitateConversation(Pebbles_Talk0, this);
            }
            else if (newAction == MeetPebbles_Talk2)
            {
                owner.getToWorking = 0.5f;
                owner.InitateConversation(Pebbles_Talk1, this);
            }
        }

        //与红猫的所有对话
        public void AddConversationEvents(CustomOracleConversation conv, Conversation.ID id)
        {
            int extralingerfactor = oracle.room.game.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.Chinese ? 1 : 0;
            if (id == Moon_Talk0)
            {
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Let go of her!"), 10 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("......"), 10 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("......"), 10 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("You don't know what you did, do you, little beast?"), 40 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Damn it. With your simple mind, you may even consider it a gift. But things don't work that way."), 80 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("You should go out now. We must make up for this huge mistake."), 50 * extralingerfactor));
            }
            else if (id == Moon_Talk1)
            {
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("test2.1"), 50 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("test2.2"), 50 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("test2.3"), 50 * extralingerfactor));
            }
            else if (id == Pebbles_Talk0)
            {
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("test2.1"), 50 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("test2.1"), 60 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("test2.1"), 80 * extralingerfactor));
            }
            else if (id == Pebbles_Talk1)
            {
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("test2.1"), 50 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("test2.1"), 60 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("test2.1"), 80 * extralingerfactor));
            }
        }
    }
}
