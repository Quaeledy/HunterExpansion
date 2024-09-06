using UnityEngine;
using static CustomOracleTx.CustomOracleBehaviour;
using MoreSlugcats;
using HunterExpansion.CustomDream;
using RWCustom;
using System.Collections.Generic;

namespace HunterExpansion.CustomOracle
{
    public class NSHOracleMeetSpear : NSHConversationBehaviour
    {
        //private AbstractCreature lockedOverseer;
        public bool holdPlayer;

        public NSHOracleMeetSpear(NSHOracleBehaviour owner) : base(owner)
        {
            this.communicationIndex = 0;
            /*
            (this.owner.oracle.room.world.game.session as StoryGameSession).saveState.miscWorldSaveData.playerGuideState.InfluenceLike(1000f, false);
            WorldCoordinate worldCoordinate = new WorldCoordinate(base.oracle.room.world.offScreenDen.index, -1, -1, 0);
            this.lockedOverseer = new AbstractCreature(base.oracle.room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Overseer), null, worldCoordinate, new EntityID(-1, 5));
            if (base.oracle.room.world.GetAbstractRoom(worldCoordinate).offScreenDen)
            {
                base.oracle.room.world.GetAbstractRoom(worldCoordinate).entitiesInDens.Add(this.lockedOverseer);
            }
            else
            {
                base.oracle.room.world.GetAbstractRoom(worldCoordinate).AddEntity(this.lockedOverseer);
            }
            this.lockedOverseer.ignoreCycle = true;
            (this.lockedOverseer.abstractAI as OverseerAbstractAI).spearmasterLockedOverseer = true;
            (this.lockedOverseer.abstractAI as OverseerAbstractAI).SetAsPlayerGuide(3);
            (this.lockedOverseer.abstractAI as OverseerAbstractAI).BringToRoomAndGuidePlayer(base.oracle.room.abstractRoom.index);
            */
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
                movementBehavior = CustomMovementBehavior.KeepDistance;
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
                     action == NSHOracleBehaviorAction.MeetSpear_Talk2)
            {
                if (inActionCounter == 100)
                {
                    PlayerHooks.SpawnOverseerInRoom(oracle.room, "NSH_AI", CreatureTemplate.Type.Overseer, 3);
                }
                if (owner.conversation != null)
                {
                    if (owner.conversation.slatedForDeletion)
                    {
                        //说完继续工作
                        owner.conversation = null;
                        owner.getToWorking = 1f;
                        //喂饱矛大师
                        if (player.FoodInStomach <= player.MaxFoodInStomach)
                        {
                            player.AddFood(player.MaxFoodInStomach);
                        } 
                        return;
                    }
                }
                else
                {
                    //看矛大师
                    owner.lookPoint = base.player.DangerPos;
                    //给矛大师朝向出口的速度
                    if (player != null && player.room != null && player.room == oracle.room)
                    {
                        player.firstChunk.vel *= Custom.LerpMap(player.firstChunk.vel.magnitude, 1f, 6f, 0.9f, 0.5f);
                        player.firstChunk.vel += Vector2.ClampMagnitude(player.room.MiddleOfTile(24, 31) - player.firstChunk.pos, 100f) / 100f * 1f;
                    }
                }
            }
            else if (action == NSHOracleBehaviorAction.MeetSpear_Talk3)
            {
                if (inActionCounter == 100)
                {
                    PlayerHooks.SpawnOverseerInRoom(oracle.room, "NSH_AI", CreatureTemplate.Type.Overseer, 3);
                }
                if (owner.conversation != null)
                { //说第一句话之后展示与srs交流的图像
                    if (owner.conversation.events.Count == 2 && this.communicationIndex == 0)
                    {
                        if ((this.owner as NSHOracleBehaviour).showImage != null)
                        {
                            (this.owner as NSHOracleBehaviour).showImage.Destroy();
                            (this.owner as NSHOracleBehaviour).showImage = null;
                        }
                        (this.owner as NSHOracleBehaviour).showImage = base.oracle.myScreen.AddImage("AIimg2b_NSH");
                        (this.owner as NSHOracleBehaviour).showMediaPos = new Vector2(0.4f * base.oracle.room.PixelWidth, 0.4f * base.oracle.room.PixelHeight);
                        this.communicationIndex++;
                        base.oracle.room.PlaySound(SoundID.SS_AI_Image, 0f, 1f, 1f);
                        (this.owner as NSHOracleBehaviour).showImage.lastPos = (this.owner as NSHOracleBehaviour).showMediaPos;
                        (this.owner as NSHOracleBehaviour).showImage.pos = (this.owner as NSHOracleBehaviour).showMediaPos;
                        (this.owner as NSHOracleBehaviour).showImage.lastAlpha = 0.91f + Random.value * 0.06f;
                        (this.owner as NSHOracleBehaviour).showImage.alpha = 0.91f + Random.value * 0.06f;
                        (this.owner as NSHOracleBehaviour).showImage.setAlpha = new float?(0.91f + Random.value * 0.06f);
                        movementBehavior = CustomMovementBehavior.ShowMedia;
                    }
                    if ((this.owner as NSHOracleBehaviour).showImage != null)
                    {
                        if (Random.value < 0.033333335f)
                        {
                            (this.owner as NSHOracleBehaviour).idealShowMediaPos += Custom.RNV() * Random.value * 30f;
                            (this.owner as NSHOracleBehaviour).showMediaPos += Custom.RNV() * Random.value * 30f;
                        }
                        (this.owner as NSHOracleBehaviour).showImage.setPos = new Vector2?((this.owner as NSHOracleBehaviour).showMediaPos); 
                        this.owner.lookPoint = (this.owner as NSHOracleBehaviour).showMediaPos;
                    }
                    if (owner.conversation.slatedForDeletion)
                    {
                        if ((this.owner as NSHOracleBehaviour).showImage != null)
                        {
                            (this.owner as NSHOracleBehaviour).showImage.Destroy();
                            (this.owner as NSHOracleBehaviour).showImage = null;
                        }
                        /*
                        if (this.holdPlayer && base.player.room == base.oracle.room)
                        {
                            base.player.mainBodyChunk.vel *= Custom.LerpMap((float)base.inActionCounter, 0f, 30f, 1f, 0.95f);
                            base.player.bodyChunks[1].vel *= Custom.LerpMap((float)base.inActionCounter, 0f, 30f, 1f, 0.95f);
                            base.player.mainBodyChunk.vel += Custom.DirVec(base.player.mainBodyChunk.pos, this.holdPlayerPos) * Mathf.Lerp(0.5f, Custom.LerpMap(Vector2.Distance(base.player.mainBodyChunk.pos, this.holdPlayerPos), 30f, 150f, 2.5f, 7f), base.oracle.room.gravity) * Mathf.InverseLerp(0f, 10f, (float)base.inActionCounter) * Mathf.InverseLerp(0f, 30f, Vector2.Distance(base.player.mainBodyChunk.pos, this.holdPlayerPos));
                        }*/
                        //说完继续工作
                        owner.conversation = null;
                        owner.getToWorking = 1f;
                        //喂饱矛大师
                        if (player.FoodInStomach <= player.MaxFoodInStomach)
                        {
                            player.AddFood(player.MaxFoodInStomach);
                        }
                        return;
                    }
                }
                else
                {
                    //看矛大师
                    owner.lookPoint = base.player.DangerPos;
                    //给矛大师朝向出口的速度
                    if (player != null && player.room != null && player.room == oracle.room)
                    {
                        player.firstChunk.vel *= Custom.LerpMap(player.firstChunk.vel.magnitude, 1f, 6f, 0.9f, 0.5f);
                        player.firstChunk.vel += Vector2.ClampMagnitude(player.room.MiddleOfTile(24, 31) - player.firstChunk.pos, 100f) / 100f * 1f;
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
                        movementBehavior = NSHOracleMovementBehavior.Meditate;
                        return;
                    }
                }
            }
            else if (action == NSHOracleBehaviorAction.MeetSpear_AfterAltEnd_1)
            {
                if (inActionCounter == 100)
                {
                    PlayerHooks.SpawnOverseerInRoom(oracle.room, "NSH_AI", CreatureTemplate.Type.Overseer, 3);
                }
                if (owner.conversation != null)
                {
                    //说第一句话时展示收到moon消息的图像
                    if (owner.conversation.events.Count == 3 && this.communicationIndex == 0)
                    {
                        if ((this.owner as NSHOracleBehaviour).showImage != null)
                        {
                            (this.owner as NSHOracleBehaviour).showImage.Destroy();
                            (this.owner as NSHOracleBehaviour).showImage = null;
                        }
                        (this.owner as NSHOracleBehaviour).showImage = base.oracle.myScreen.AddImage("AIimg2a_NSH");
                        (this.owner as NSHOracleBehaviour).showMediaPos = new Vector2(0.25f * base.oracle.room.PixelWidth, 0.4f * base.oracle.room.PixelHeight);
                        base.oracle.room.PlaySound(SoundID.SS_AI_Image, 0f, 1f, 1f);
                        (this.owner as NSHOracleBehaviour).showImage.lastPos = (this.owner as NSHOracleBehaviour).showMediaPos;
                        (this.owner as NSHOracleBehaviour).showImage.pos = (this.owner as NSHOracleBehaviour).showMediaPos;
                        (this.owner as NSHOracleBehaviour).showImage.lastAlpha = 0.91f + Random.value * 0.06f;
                        (this.owner as NSHOracleBehaviour).showImage.alpha = 0.91f + Random.value * 0.06f;
                        (this.owner as NSHOracleBehaviour).showImage.setAlpha = new float?(0.91f + Random.value * 0.06f);
                        this.communicationIndex++;
                        movementBehavior = CustomMovementBehavior.ShowMedia;
                    }
                    //说第三句话时清除收到moon消息的图像
                    if (owner.conversation.events.Count == 1 && this.communicationIndex == 1)
                    {
                        if ((this.owner as NSHOracleBehaviour).showImage != null)
                        {
                            (this.owner as NSHOracleBehaviour).showImage.Destroy();
                            (this.owner as NSHOracleBehaviour).showImage = null;
                        }
                        movementBehavior = CustomMovementBehavior.Talk;
                    }
                    if ((this.owner as NSHOracleBehaviour).showImage != null)
                    {
                        if (Random.value < 0.033333335f)
                        {
                            (this.owner as NSHOracleBehaviour).idealShowMediaPos += Custom.RNV() * Random.value * 30f;
                            (this.owner as NSHOracleBehaviour).showMediaPos += Custom.RNV() * Random.value * 30f;
                        }
                        (this.owner as NSHOracleBehaviour).showImage.setPos = new Vector2?((this.owner as NSHOracleBehaviour).showMediaPos);
                        this.owner.lookPoint = (this.owner as NSHOracleBehaviour).showMediaPos;
                    }
                    if (owner.conversation.slatedForDeletion)
                    {
                        owner.conversation = null;
                        //说完继续工作
                        owner.getToWorking = 1f;
                        movementBehavior = CustomMovementBehavior.ShowMedia;
                        return;
                    }
                }
            }
            else if (action == NSHOracleBehaviorAction.MeetSpear_AfterAltEnd_2 ||
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
        public void AddConversationEvents(NSHConversation conv, Conversation.ID id)
        {
            int extralingerfactor = oracle.room.game.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.Chinese ? 1 : 0;
            //现实对话
            if (id == NSHConversationID.Spear_Talk0)
            {
                NSHConversation.LoadEventsFromFile(conv, 0, oracle.room.world.game.session.characterStats.name.value + "-0");
            }
            else if (id == NSHConversationID.Spear_Talk1)
            {
                NSHConversation.LoadEventsFromFile(conv, 0, oracle.room.world.game.session.characterStats.name.value + "-1");
            }
            else if (id == NSHConversationID.Spear_Talk2)
            {
                NSHConversation.LoadEventsFromFile(conv, 0, oracle.room.world.game.session.characterStats.name.value + "-2");
            }
            else if (id == NSHConversationID.Spear_AfterMeetPebbles_0)
            {
                NSHConversation.LoadEventsFromFile(conv, 0, oracle.room.world.game.session.characterStats.name.value + "-AfterMeetPebbles-0");
            }
            else if (id == NSHConversationID.Spear_AfterMeetPebbles_1)
            {
                NSHConversation.LoadEventsFromFile(conv, 0, oracle.room.world.game.session.characterStats.name.value + "-AfterMeetPebbles-1");
            }
            else if (id == NSHConversationID.Spear_AfterAltEnd_0)
            {
                NSHConversation.LoadEventsFromFile(conv, 0, oracle.room.world.game.session.characterStats.name.value + "-AltEnding-0");
            }
            else if (id == NSHConversationID.Spear_AfterAltEnd_1)
            {
                NSHConversation.LoadEventsFromFile(conv, 0, oracle.room.world.game.session.characterStats.name.value + "-AltEnding-1");
            }
            else if (id == NSHConversationID.Spear_AfterAltEnd_2)
            {
                NSHConversation.LoadEventsFromFile(conv, 0, oracle.room.world.game.session.characterStats.name.value + "-AltEnding-2");
            }
        }

        //用于计算矛大师的时间状态
        public override int GetPlayerEncountersState()
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
