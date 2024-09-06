using RWCustom;
using CustomDreamTx;
using static CustomOracleTx.CustomOracleBehaviour;
using HunterExpansion.CustomSave;
using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using CustomOracleTx;
using MoreSlugcats;
using System;
using Random = UnityEngine.Random;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using HunterExpansion.CustomEnding;
using HunterExpansion.CustomEffects;
using HunterExpansion.CustomDream;

namespace HunterExpansion.CustomOracle
{
    public class NSHOracleMeetHunter : NSHConversationBehaviour
    {
        //梦境相关属性
        public static bool swarmerCreated = false;
        public static bool swarmerReleased = true;
        public static bool setRunIntoTalk = false;
        public static bool giveSwarmerToHunter = false;
        public static int swarmerStolen = 0;
        public static int swarmerApproached = 0;
        public static NSHSwarmer nshSwarmer;
        public static DataPearl aquamarinePearl;
        public static List<GlyphLabel> swarmerLabels;
        public static Creature swarmerGrabber;
        public static Vector2 wantPos = Vector2.zero;

        //结局相关属性
        public static bool hasSayHello = false;
        public static bool isControled = false;
        public static bool playerWaitDeath = false;
        public static int fadeCounter = 0;
        public static FSprite blackRect = new FSprite("pixel");
        public static Vector2 oraclePos = Vector2.zero;
        public bool holdPlayer;
        private bool hasTookPupsAway;


        public NSHOracleMeetHunter(NSHOracleBehaviour owner) : base(owner)
        {
            swarmerCreated = false;
            swarmerReleased = true;
            setRunIntoTalk = false;
            giveSwarmerToHunter = false;
            swarmerStolen = 0;
            swarmerApproached = 0;
            oraclePos = Vector2.zero;
            wantPos = Vector2.zero;

            isControled = false;
            hasSayHello = false;
            playerWaitDeath = false; 
            hasTookPupsAway = false;
            fadeCounter = 0;

            Plugin.Log("NSH Oracle Meet Hunter!");
        }

        public static bool SubBehaviourIsMeetHunter (CustomAction nextAction)
        {
            return nextAction == NSHOracleBehaviorAction.MeetHunter_Init ||
                   nextAction == NSHOracleBehaviorAction.MeetHunter_GiveMark ||
                   nextAction == NSHOracleBehaviorAction.MeetHunter_TalkAfterGiveMark ||
                   nextAction == NSHOracleBehaviorAction.MeetHunter_RunIntoHunter ||
                   nextAction == NSHOracleBehaviorAction.MeetHunter_RunIntoTalk ||
                   nextAction == NSHOracleBehaviorAction.MeetHunter_FarewellTalk ||
                   nextAction == NSHOracleBehaviorAction.MeetHunter_Praise ||
                   nextAction == NSHOracleBehaviorAction.MeetHunter_UnfulfilledMessager1 ||
                   nextAction == NSHOracleBehaviorAction.MeetHunter_UnfulfilledMessager2 ||
                   nextAction == NSHOracleBehaviorAction.MeetHunter_Talk1 ||
                   nextAction == NSHOracleBehaviorAction.MeetHunter_Talk2 ||
                   nextAction == NSHOracleBehaviorAction.MeetHunter_Talk3 ||
                   nextAction == NSHOracleBehaviorAction.MeetHunter_Talk3_Wait ||
                   nextAction == NSHOracleBehaviorAction.LetSlugcatStayAwayFromSwarmer ||
                   nextAction == NSHOracleBehaviorAction.LetSlugcatReleaseSwarmer;
        }

        public override void Update()
        {
            base.Update();
            if (player == null || oracle.room == null || !(oracle.room.world.game.session is StoryGameSession)) 
                return;
            if (oracle.room.world.game.session.characterStats.name != Plugin.SlugName)
            {
                return;
            }

            NSHOracleState state = (this.owner as NSHOracleBehaviour).State;
            if (action == NSHOracleBehaviorAction.MeetHunter_Init)
            {
                movementBehavior = CustomMovementBehavior.KeepDistance; 
                if (state.playerEncountersState != GetPlayerEncountersState())
                {
                    state.playerEncountersState = GetPlayerEncountersState();
                    state.playerEncountersWithMark = 0;
                }
                //梦境行为
                if (CustomDreamRx.currentActivateNormalDream != null && CustomDreamRx.currentActivateNormalDream.IsPerformDream)
                {
                    //根据梦境进行对话
                    if (CustomDreamRx.currentActivateNormalDream.activateDreamID == DreamID.HunterDream_0 &&
                        inActionCounter > 100)
                    {
                        owner.NewAction(NSHOracleBehaviorAction.MeetHunter_GiveMark);
                        return;
                    }
                    else if (CustomDreamRx.currentActivateNormalDream.activateDreamID == DreamID.HunterDream_1 &&
                        inActionCounter > 0)
                    {
                        owner.NewAction(NSHOracleBehaviorAction.MeetHunter_RunIntoHunter);
                        return;
                    }
                    else if (CustomDreamRx.currentActivateNormalDream.activateDreamID == DreamID.HunterDream_3 &&
                        inActionCounter > 40)
                    {
                        owner.NewAction(NSHOracleBehaviorAction.MeetHunter_FarewellTalk);
                        return;
                    }
                    else if (CustomDreamRx.currentActivateNormalDream.activateDreamID == DreamID.HunterDream_5 &&
                        inActionCounter > 20)
                    {
                        owner.NewAction(NSHOracleBehaviorAction.MeetHunter_Praise);
                        return;
                    }
                }
                //现实行为
                else
                {
                    //owner.conversation = null;
                    SaveState saveState = this.oracle.room.game.manager.rainWorld.progression.currentSaveState;
                    if (this.owner.oracle.room.game.rainWorld.progression.currentSaveState.miscWorldSaveData.SLOracleState.neuronsLeft <= 0)
                    {
                        if (state.playerEncountersWithMark == 0 && inActionCounter > 40)
                        {
                            owner.NewAction(NSHOracleBehaviorAction.MeetHunter_UnfulfilledMessager1);
                            (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                            return;
                        }
                        else if (state.playerEncountersWithMark == 1 && inActionCounter > 40)
                        {
                            owner.NewAction(NSHOracleBehaviorAction.MeetHunter_UnfulfilledMessager2);
                            (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                            return;
                        }
                    }
                    else// if (saveState.deathPersistentSaveData.altEnding)
                    {
                        if (state.playerEncountersWithMark == 0 && inActionCounter > 60)
                        {
                            owner.NewAction(NSHOracleBehaviorAction.MeetHunter_Talk1);
                            (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                            return;
                        }
                        else if (state.playerEncountersWithMark == 1 && inActionCounter > 60)
                        {
                            owner.NewAction(NSHOracleBehaviorAction.MeetHunter_Talk2);
                            (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                            return;
                        }
                        else if (state.playerEncountersWithMark >= 2 && inActionCounter > 10)
                        {
                            if (RedsIllness.RedsCycles(saveState.redExtraCycles) - saveState.cycleNumber <= 0)
                            {
                                owner.NewAction(NSHOracleBehaviorAction.MeetHunter_Talk3);
                            }
                            else
                            {
                                owner.NewAction(NSHOracleBehaviorAction.MeetHunter_Talk3_Wait);
                            }
                            (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                            return;
                        }
                    }
                    /*
                    if (saveState.deathPersistentSaveData.altEnding)
                    {
                        if (state.playerEncountersWithMark == 0 && inActionCounter > 60)
                        {
                            owner.NewAction(NSHOracleBehaviorAction.MeetHunter_Talk1);
                            (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                            return;
                        }
                        else if (state.playerEncountersWithMark == 1 && inActionCounter > 60)
                        {
                            owner.NewAction(NSHOracleBehaviorAction.MeetHunter_Talk2);
                            (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                            return;
                        }
                        else if (state.playerEncountersWithMark >= 2 && inActionCounter > 10)
                        {
                            if (RedsIllness.RedsCycles(saveState.redExtraCycles) - saveState.cycleNumber <= 0)
                            {
                                owner.NewAction(NSHOracleBehaviorAction.MeetHunter_Talk3);
                            }
                            else
                            {
                                owner.NewAction(NSHOracleBehaviorAction.MeetHunter_Talk3_Wait);
                            }
                            (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                            return;
                        }
                    }
                    else if (this.owner.oracle.room.game.rainWorld.progression.currentSaveState.miscWorldSaveData.SLOracleState.neuronsLeft <= 0)
                    {
                        if (state.playerEncountersWithMark == 0 && inActionCounter > 40)
                        {
                            owner.NewAction(NSHOracleBehaviorAction.MeetHunter_UnfulfilledMessager1);
                            (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                            return;
                        }
                        else if (state.playerEncountersWithMark == 1 && inActionCounter > 40)
                        {
                            owner.NewAction(NSHOracleBehaviorAction.MeetHunter_UnfulfilledMessager2);
                            (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                            return;
                        }
                    }
                    else
                    {
                        if (inActionCounter < 40)
                            owner.dialogBox.NewMessage(owner.Translate("..."), 60);
                    }*/
                    return;
                }
            }
            else if (action == NSHOracleBehaviorAction.MeetHunter_GiveMark)
            {
                owner.NewAction(NSHOracleBehaviour.CustomAction.General_GiveMark);
            }
            else if (action == NSHOracleBehaviorAction.MeetHunter_TalkAfterGiveMark)
            {
                if (owner.conversation != null)
                {
                    if (owner.conversation.slatedForDeletion)
                    {
                        owner.conversation = null;
                        //说完结束梦境
                        //如果是梦境，让梦境结束
                        if (CustomDreamRx.currentActivateNormalDream != null && CustomDreamRx.currentActivateNormalDream.IsPerformDream)
                        {
                            CustomDreamRx.currentActivateNormalDream.EndDream(oracle.room.game);
                            //nshSwarmer?.Destroy();
                            swarmerCreated = false;
                            //重新生成光环
                            oracle.suppressConnectionFires = false;
                        }
                        return;
                    }
                }
            }
            else if (action == NSHOracleBehaviorAction.MeetHunter_RunIntoHunter)
            {
                //神经元特效！
                swarmerReleased = true;
                NSHSwarmer_Effects(player, oracle, owner);
                movementBehavior = CustomMovementBehavior.Idle;
                //如果看见了猎手
                if (oracle.room.GetTilePosition(player.mainBodyChunk.pos).y < 32 && (inActionCounter > 20 || Custom.DistLess(player.mainBodyChunk.pos, oracle.firstChunk.pos, 150f) || !Custom.DistLess(player.mainBodyChunk.pos, oracle.room.MiddleOfTile(oracle.room.ShortcutLeadingToNode(1).StartTile), 150f)))
                {
                    //如果还没打招呼
                    if (!setRunIntoTalk)
                    {
                        setRunIntoTalk = true;
                        owner.NewAction(NSHOracleBehaviorAction.MeetHunter_RunIntoTalk);
                    }
                }
            }
            else if (action == NSHOracleBehaviorAction.MeetHunter_RunIntoTalk)
            {
                //神经元特效！
                swarmerReleased = true;
                NSHSwarmer_Effects(player, oracle, owner);
                if (owner.conversation != null)
                {
                    movementBehavior = CustomMovementBehavior.KeepDistance;

                    if (owner.conversation.slatedForDeletion)
                    {
                        owner.conversation = null;
                        movementBehavior = CustomMovementBehavior.Idle;
                        //说完继续工作
                        owner.NewAction(NSHOracleBehaviorAction.MeetHunter_RunIntoHunter);
                    }
                }
            }
            else if (action == NSHOracleBehaviorAction.MeetHunter_FarewellTalk)
            {
                movementBehavior = CustomMovementBehavior.KeepDistance;
                if (inActionCounter == 1)
                {
                    //生成神经元
                    AbstractPhysicalObject absNshSwarmer = new AbstractPhysicalObject(oracle.room.world, AbstractPhysicalObject.AbstractObjectType.NSHSwarmer, null, oracle.abstractPhysicalObject.pos, oracle.room.game.GetNewID());
                    nshSwarmer = new NSHSwarmer(absNshSwarmer);
                    nshSwarmer.abstractPhysicalObject.RealizeInRoom();
                    //生成珍珠
                    int num = oracle.room.roomSettings.placedObjects.Count;
                    oracle.room.roomSettings.placedObjects.Add(new PlacedObject(PlacedObject.Type.DataPearl, null));
                    DataPearl.AbstractDataPearl absAquamarinePearl = new DataPearl.AbstractDataPearl(oracle.room.world, 
                                                                                                  AbstractPhysicalObject.AbstractObjectType.DataPearl, 
                                                                                                  null, oracle.abstractPhysicalObject.pos, 
                                                                                                  oracle.room.game.GetNewID(), 
                                                                                                  oracle.room.abstractRoom.index,
                                                                                                  num,
                                                                                                  oracle.room.roomSettings.placedObjects[num].data as PlacedObject.ConsumableObjectData,
                                                                                                  DataPearl.AbstractDataPearl.DataPearlType.Red_stomach);
                    aquamarinePearl = new DataPearl(absAquamarinePearl, oracle.room.world);
                    aquamarinePearl.abstractPhysicalObject.RealizeInRoom();
                }
                if (nshSwarmer != null && nshSwarmer.room != null)
                {
                    //维持在猎手周围（后期）/NSH周围（前期）
                    Vector2 wantPos = (giveSwarmerToHunter) ? player.mainBodyChunk.pos : oracle.firstChunk.pos;
                    nshSwarmer.firstChunk.vel *= Custom.LerpMap(nshSwarmer.firstChunk.vel.magnitude, 1f, 6f, 0.999f, 0.9f);
                    nshSwarmer.firstChunk.vel += Vector2.ClampMagnitude(wantPos - nshSwarmer.firstChunk.pos, 100f) / 100f * 0.4f;
                    //抵消重力（非工作）
                    nshSwarmer.firstChunk.vel += 1f * Vector2.up;
                    //随机速度（非工作）
                    nshSwarmer.firstChunk.vel += Custom.RNV() * Random.value * 0.5f;
                }
                if (aquamarinePearl != null && aquamarinePearl.room != null)
                {
                    //维持在猎手周围（后期）/NSH周围（前期）
                    Vector2 wantPos = (giveSwarmerToHunter) ? (player.mainBodyChunk.pos + 20f * Custom.DirVec(player.firstChunk.pos, new Vector2(oracle.room.PixelWidth / 2, oracle.room.PixelHeight / 2))) : (oracle.firstChunk.pos + 200f * Custom.DirVec(oracle.firstChunk.pos, new Vector2(oracle.room.PixelWidth / 2, oracle.room.PixelHeight / 2)));
                    aquamarinePearl.firstChunk.vel *= Custom.LerpMap(aquamarinePearl.firstChunk.vel.magnitude, 1f, 6f, 0.999f, 0.9f);
                    aquamarinePearl.firstChunk.vel += Vector2.ClampMagnitude(wantPos - aquamarinePearl.firstChunk.pos, 100f) / 100f * 0.4f;
                    //抵消重力（非工作）
                    aquamarinePearl.firstChunk.vel += 1f * Vector2.up;
                }

                if (inActionCounter == 200)
                {
                    //生成闪电
                    OracleSwarmerResync oracleSwarmerResync = new OracleSwarmerResync(oracle, aquamarinePearl, true, 5, 200);
                    oracle.room.AddObject(oracleSwarmerResync);
                }

                if (inActionCounter > 200 && inActionCounter < 400 && inActionCounter % 20 == 0)
                {
                    oracle.room.AddObject(new Explosion.ExplosionLight(aquamarinePearl.firstChunk.pos, 150f, 1f, 15, Color.green));
                    oracle.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Data_Bit, aquamarinePearl.firstChunk.pos, 1f, 1f + Random.value * 2f);
                }

                if (inActionCounter == 450)
                {
                    owner.InitateConversation(NSHConversationID.Hunter_DreamTalk3, this);
                }

                if (owner.conversation != null)
                {
                    if (owner.conversation.slatedForDeletion)
                    {
                        owner.conversation = null;
                        //说完结束梦境
                        //如果是梦境，让梦境结束
                        if (CustomDreamRx.currentActivateNormalDream != null && CustomDreamRx.currentActivateNormalDream.IsPerformDream)
                        {
                            CustomDreamRx.currentActivateNormalDream.EndDream(oracle.room.game);
                        }
                        return;
                    }
                }
            }
            else if (action == NSHOracleBehaviorAction.MeetHunter_Praise)
            {
                if (inActionCounter % 10 == 0)
                {
                    oracle.room.AddObject(new MeltLights.MeltLight(1f, new Vector2(oracle.room.RandomPos().x, oracle.room.PixelHeight + 100f), base.oracle.room, RainWorld.GoldRGB));
                }
                if (owner.conversation != null)
                {
                    movementBehavior = CustomMovementBehavior.KeepDistance;

                    if (owner.conversation.slatedForDeletion)
                    {
                        owner.conversation = null;
                        //说完继续工作
                        owner.getToWorking = 1f;
                        movementBehavior = CustomMovementBehavior.ShowMedia;
                        for (int l = 0; l < 200; l++)
                        {
                            oracle.room.AddObject(new MeltLights.MeltLight(1f, oracle.room.RandomPos(), base.oracle.room, RainWorld.GoldRGB));
                        }
                        //如果是梦境，让梦境结束
                        if (CustomDreamRx.currentActivateNormalDream != null && CustomDreamRx.currentActivateNormalDream.IsPerformDream)
                        {
                            CustomDreamRx.currentActivateNormalDream.EndDream(oracle.room.game);
                        }
                        return;
                    }
                }
            }
            //现实对话
            else if (action == NSHOracleBehaviorAction.MeetHunter_UnfulfilledMessager1 ||
                     action == NSHOracleBehaviorAction.MeetHunter_UnfulfilledMessager2)
            {
                if (owner.conversation != null)
                {
                    if (owner.conversation.slatedForDeletion)
                    {
                        //说完继续工作
                        owner.conversation = null;
                        owner.getToWorking = 1f;
                        //喂饱红猫
                        if (player.FoodInStomach <= player.MaxFoodInStomach)
                        {
                            player.AddFood(player.MaxFoodInStomach);
                        }
                        return;
                    }
                }
                else
                {
                    //看红猫
                    owner.lookPoint = base.player.DangerPos;
                    //给红猫朝向出口的速度
                    if (player != null && player.room != null && player.room == oracle.room)
                    {
                        player.firstChunk.vel *= Custom.LerpMap(player.firstChunk.vel.magnitude, 1f, 6f, 0.9f, 0.5f);
                        player.firstChunk.vel += Vector2.ClampMagnitude(player.room.MiddleOfTile(24, 31) - player.firstChunk.pos, 100f) / 100f * 1f;
                    }
                }
            }
            else if (action == NSHOracleBehaviorAction.MeetHunter_Talk1)
            {
                if (owner.conversation != null)
                {
                    if (owner.conversation.slatedForDeletion)
                    {
                        owner.UnlockShortcuts();
                        //说完继续工作
                        //owner.getToWorking = 1f;
                        //movementBehavior = CustomMovementBehavior.Idle;
                        fadeCounter++;
                        if (!isControled)
                        {
                            isControled = true;
                            oracle.room.game.manager.musicPlayer.FadeOutAllSongs(100f);
                            //添加黑幕
                            if (blackRect == null)
                            {
                                blackRect = new FSprite("pixel");
                            }
                            blackRect.scaleX = Screen.width;
                            blackRect.scaleY = Screen.height;
                            blackRect.x = Screen.width / 2f;
                            blackRect.y = Screen.height / 2f;
                            blackRect.color = Color.black;
                            blackRect.alpha = 0f;
                            Futile.stage.AddChild(blackRect);
                            blackRect.MoveToFront();
                        }
                        if (fadeCounter >= 40 && blackRect != null)
                            blackRect.alpha = Mathf.Min(1f, blackRect.alpha + 0.01f);
                        if (fadeCounter >= 140)
                        {
                            fadeCounter = 0;
                            owner.conversation = null;
                            isControled = false;

                            RainWorldGame game = this.oracle.room.game;
                            for (int l = 0; l < game.world.GetAbstractRoom(player.abstractCreature.pos).creatures.Count; l++)
                            {
                                if (ModManager.MSC && game.world.GetAbstractRoom(player.abstractCreature.pos).creatures[l].creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
                                {
                                    PlayerNPCState slugpup = game.world.GetAbstractRoom(game.Players[0].pos).creatures[l].state as PlayerNPCState;
                                    Plugin.Log("Slugpup foodInStomach (old): " + slugpup.foodInStomach);
                                    Plugin.Log("Slugpup MaxFoodInStomach: " + (slugpup.player.realizedCreature as Player).MaxFoodInStomach);
                                    slugpup.foodInStomach = (slugpup.player.realizedCreature as Player).MaxFoodInStomach;
                                    Plugin.Log("Help to feed slugpup! creature index: " + l);
                                    Plugin.Log("Slugpup foodInStomach (new): " + slugpup.foodInStomach);
                                }
                            }

                            AbstractCreature abstractCreature = game.FirstAlivePlayer;
                            if (abstractCreature == null)
                            {
                                abstractCreature = game.FirstAnyPlayer;
                            }
                            SaveState saveState = game.GetStorySession.saveState;
                            SaveState.forcedEndRoomToAllowwSave = abstractCreature.Room.name;
                            saveState.BringUpToDate(game);
                            SaveState.forcedEndRoomToAllowwSave = "";
                            RainWorldGame.ForceSaveNewDenLocation(game, "NSH_AI", true);
                            game.manager.rainWorld.progression.SaveWorldStateAndProgression(false);
                            game.manager.rainWorld.progression.SaveProgressionAndDeathPersistentDataOfCurrentState(false, false);
                            //player.room.game.GetStorySession.saveState.SessionEnded(player.room.game, true, false);
                            oracle.room.game.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Statistics);
                        }
                        return;
                    }
                }
            }
            else if (action == NSHOracleBehaviorAction.MeetHunter_Talk2)
            {
                if (owner.conversation != null)
                {
                    if (owner.conversation.slatedForDeletion)
                    {
                        //喂饱红猫
                        if (player.FoodInStomach <= player.MaxFoodInStomach)
                        {
                            player.AddFood(player.MaxFoodInStomach);
                        }
                        //帮忙喂猫崽
                        for (int l = 0; l < this.oracle.room.abstractRoom.creatures.Count; l++)
                        {
                            if (ModManager.MSC && this.oracle.room.abstractRoom.creatures[l].creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
                            {
                                PlayerNPCState slugpup = this.oracle.room.game.world.GetAbstractRoom(this.oracle.room.game.Players[0].pos).creatures[l].state as PlayerNPCState;
                                Plugin.Log("Slugpup foodInStomach (old): " + slugpup.foodInStomach);
                                Plugin.Log("Slugpup MaxFoodInStomach: " + (slugpup.player.realizedCreature as Player).MaxFoodInStomach);
                                slugpup.foodInStomach = (slugpup.player.realizedCreature as Player).MaxFoodInStomach;
                                Plugin.Log("Help to feed slugpup! creature index: " + l);
                                Plugin.Log("Slugpup foodInStomach (new): " + slugpup.foodInStomach);
                            }
                        }
                        //说完继续工作
                        owner.conversation = null;
                        owner.getToWorking = 1f;
                        movementBehavior = CustomMovementBehavior.Idle;
                        return;
                    }
                }
            }
            else if (action == NSHOracleBehaviorAction.MeetHunter_Talk3)
            {
                owner.lookPoint = player.firstChunk.pos + Custom.DirVec(oracle.firstChunk.pos, player.DangerPos) * Custom.LerpMap(Mathf.Min(Custom.Dist(oracle.firstChunk.pos, player.DangerPos), 200f), 200f, 0f, 10f, 100f);
                if (!playerWaitDeath)
                {
                    oraclePos = oracle.firstChunk.pos;
                    //有猫崽则让玩家带走猫崽
                    if (owner.IsThereHasSlugPup())
                    {
                        if (!this.hasTookPupsAway)
                        {
                            List<Player> pups = owner.FindSlugPup();
                            owner.dialogBox.NewMessage(owner.Translate("Good morning, messenger."), 60);
                            if (pups.Count == 1)
                                owner.dialogBox.NewMessage(owner.Translate("Oh, you're with your friend. But we need some time alone, I believe. Can you come back for me after you take it to somewhere safe?"), 60);
                            else
                                owner.dialogBox.NewMessage(owner.Translate("Oh, you're with your friends. But we need some time alone, I believe. Can you come back for me after you take them to somewhere safe?"), 60);
                            this.hasTookPupsAway = true;
                        }
                        if (owner.dialogBox.messages.Count == 0)
                            owner.getToWorking = 1f;
                    }
                    //没有猫崽从这里开始
                    else if (player.room != null && player.room == oracle.room)
                    {
                        if (this.hasTookPupsAway)
                        {
                            bool seePlayer = oracle.room.GetTilePosition(player.mainBodyChunk.pos).y < 32 &&
                                             (Custom.DistLess(player.mainBodyChunk.pos, oracle.firstChunk.pos, 150f) ||
                                              !Custom.DistLess(player.mainBodyChunk.pos, oracle.room.MiddleOfTile(oracle.room.ShortcutLeadingToNode(0).StartTile), 150f));
                            if (!seePlayer)
                                return;
                        }
                        if (!EndingSession.goEnding)
                        {
                            owner.getToWorking = 0f;
                            owner.LockShortcuts();
                            //NSH开始下落
                            if (oracle.firstChunk.pos.y > 95f)
                            {
                                movementBehavior = NSHOracleMovementBehavior.Land;
                                (owner as NSHOracleBehaviour).landPos = (oracle.firstChunk.pos.x > 480f) ? new Vector2(700f, 95f) : new Vector2(260f, 95f);
                            }
                            //下落完毕
                            if (oracle.firstChunk.pos.y <= 95f)
                            {
                                movementBehavior = CustomMovementBehavior.ShowMedia;
                                //oracle.firstChunk.vel *= Custom.LerpMap(oracle.firstChunk.vel.magnitude, 0f, 2f, 0.9f, 0.5f);
                                //NSH说第一句话
                                if (!hasSayHello)
                                {
                                    hasSayHello = true;
                                    if (this.hasTookPupsAway)
                                    {
                                        owner.dialogBox.NewMessage(owner.Translate("Welcome back, my messenger. Come, Come here."), 60);
                                    }
                                    else
                                    {
                                        owner.dialogBox.NewMessage(owner.Translate("Good morning. Come here, my little messenger. Come to me."), 60);
                                    }
                                }
                                //NSH说完话后，玩家强制靠近NSH
                                if (owner.dialogBox.messages.Count == 0)
                                    EndingSession.goEnding = true;
                            }
                        }
                        else
                        {
                            movementBehavior = CustomMovementBehavior.ShowMedia;
                            //NSH伸出手
                            int i = (oracle.firstChunk.pos.x > player.firstChunk.pos.x) ? 0 : 1;
                            (oracle.graphicsModule as OracleGraphics).hands[i].vel *= Custom.LerpMap((oracle.graphicsModule as OracleGraphics).hands[i].vel.magnitude, 1f, 6f, 0.999f, 0.9f);
                            (oracle.graphicsModule as OracleGraphics).hands[i].vel += 4f * Custom.DirVec((oracle.graphicsModule as OracleGraphics).hands[i].pos, player.firstChunk.pos + new Vector2(0f, 10f));
                            //任何玩家已经靠近了NSH
                            for (int j = 0; j < oracle.room.game.Players.Count; j++)
                            {
                                Player p = oracle.room.game.Players[j].realizedCreature as Player;
                                if (Custom.Dist(p.DangerPos, oracle.firstChunk.pos) <= 20f)
                                {
                                    owner.InitateConversation(NSHConversationID.Hunter_Talk2, this);
                                    playerWaitDeath = true;
                                    //将最近的玩家图层置于迭代器上方
                                    if (oracle.graphicsModule != null)
                                    {
                                        if (p is IDrawable)
                                            oracle.graphicsModule.AddObjectToInternalContainer(p as IDrawable, 0);
                                        else if (p.graphicsModule != null)
                                            oracle.graphicsModule.AddObjectToInternalContainer(p.graphicsModule, 0);
                                    }
                                }
                            }
                            /*
                            if (Custom.Dist(player.DangerPos, oracle.firstChunk.pos) <= 20f)
                            {
                                owner.InitateConversation(NSHConversationID.Hunter_Talk2, this);
                                playerWaitDeath = true;
                            }*/
                        }
                    }
                }
                //NSH说后面的话
                if (owner.conversation != null)
                {
                    movementBehavior = CustomMovementBehavior.ShowMedia;
                    //NSH对最近的猫伸出手
                    for (int j = 0; j < oracle.room.game.Players.Count; j++)
                    {
                        Player p = oracle.room.game.Players[j].realizedCreature as Player;
                        if (Custom.Dist(p.DangerPos, oracle.firstChunk.pos) <= 20f)
                        {
                            int i = (oracle.firstChunk.pos.x > p.firstChunk.pos.x) ? 0 : 1;
                            (oracle.graphicsModule as OracleGraphics).hands[i].pos = p.firstChunk.pos + new Vector2(0f, 10f);
                        }
                        owner.lookPoint = p.firstChunk.pos + Custom.DirVec(oracle.firstChunk.pos, p.DangerPos) * Custom.LerpMap(Mathf.Min(Custom.Dist(oracle.firstChunk.pos, p.DangerPos), 200f), 200f, 0f, 10f, 100f);
                    }
                    //NSH说完话了
                    if (owner.conversation.slatedForDeletion)
                    {
                        if(fadeCounter == 0)
                        {
                            oracle.room.game.manager.musicPlayer.FadeOutAllSongs(80f);
                        }
                        if (!player.dead)
                        {
                            fadeCounter++;
                            oracle.suppressConnectionFires = true;
                            isControled = true;
                            if(fadeCounter > 60f)
                                this.owner.killFac += 0.0125f;
                            if (this.owner.killFac >= 1f)
                            {
                                for (int k = 0; k < 20; k++)
                                {
                                    //player.room.game.cameras[0].virtualMicrophone.AllQuiet();
                                    player.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, player.firstChunk.pos, 0.5f, 1f);
                                    oracle.room.AddObject(new Spark(player.mainBodyChunk.pos, Custom.RNV() * Random.value * 40f, new Color(1f, 1f, 1f), null, 30, 120));
                                }
                                player.dead = true;
                                playerWaitDeath = false;
                                player.room.world.game.GetStorySession.saveState.deathPersistentSaveData.redsDeath = true;
                                fadeCounter = 0;
                                this.owner.killFac = 0f;
                            }
                        }
                        if(player.dead)
                        {
                            fadeCounter++;
                            if (fadeCounter == 1)
                            {
                                //添加黑幕
                                if (blackRect == null)
                                {
                                    blackRect = new FSprite("pixel");
                                }
                                blackRect.scaleX = Screen.width * 1.1f;
                                blackRect.scaleY = Screen.height * 1.1f;
                                blackRect.x = (Screen.width * 1.1f) / 2f;
                                blackRect.y = (Screen.height * 1.1f) / 2f;
                                blackRect.color = Color.black;
                                blackRect.alpha = 1f;
                                Futile.stage.AddChild(blackRect);
                                blackRect.MoveToFront();
                            }
                            if (fadeCounter >= 150)
                            {
                                player.room.world.game.GetStorySession.saveState.deathPersistentSaveData.ascended = true;
                                fadeCounter = 0;
                                owner.conversation = null;
                                isControled = false;
                                oracle.suppressConnectionFires = false;
                                player.room.game.GetStorySession.saveState.SessionEnded(player.room.game, true, false);
                                oracle.room.game.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Credits);
                            }
                        }
                    }
                }
            }
            else if (action == NSHOracleBehaviorAction.MeetHunter_Talk3_Wait)
            {
                if (owner.conversation != null)
                {
                    if (owner.conversation.slatedForDeletion)
                    {
                        //喂饱红猫
                        if (player.FoodInStomach <= player.MaxFoodInStomach)
                        {
                            player.AddFood(player.MaxFoodInStomach);
                        }
                        //帮忙喂猫崽
                        for (int l = 0; l < this.oracle.room.abstractRoom.creatures.Count; l++)
                        {
                            if (ModManager.MSC && this.oracle.room.abstractRoom.creatures[l].creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
                            {
                                PlayerNPCState slugpup = this.oracle.room.game.world.GetAbstractRoom(this.oracle.room.game.Players[0].pos).creatures[l].state as PlayerNPCState;
                                Plugin.Log("Slugpup foodInStomach (old): " + slugpup.foodInStomach);
                                Plugin.Log("Slugpup MaxFoodInStomach: " + (slugpup.player.realizedCreature as Player).MaxFoodInStomach);
                                slugpup.foodInStomach = (slugpup.player.realizedCreature as Player).MaxFoodInStomach;
                                Plugin.Log("Help to feed slugpup! creature index: " + l);
                                Plugin.Log("Slugpup foodInStomach (new): " + slugpup.foodInStomach);
                            }
                        }
                        //说完继续工作
                        owner.conversation = null;
                        owner.getToWorking = 1f;
                        movementBehavior = CustomMovementBehavior.Idle;
                        return;
                    }
                }
            }
            else if (action == NSHOracleBehaviorAction.LetSlugcatStayAwayFromSwarmer)
            {
                NSHSwarmer_Effects(player, oracle, owner);
                if (owner.conversation != null)
                {
                    if (owner.conversation.slatedForDeletion)
                    {
                        swarmerApproached++;
                        owner.conversation = null;
                        owner.NewAction(NSHOracleBehaviorAction.MeetHunter_RunIntoHunter);
                        return;
                    }
                }
            }
            else if (action == NSHOracleBehaviorAction.LetSlugcatReleaseSwarmer)
            {
                NSHSwarmer_Effects(player, oracle, owner);
                if (inActionCounter == 1)
                {
                    oracle.room.AddObject(new Explosion.ExplosionLight(oracle.firstChunk.pos, 450f, 1f, 12, Color.white));
                    oracle.room.AddObject(new ShockWave(oracle.firstChunk.pos, 180f, 0.1f, 12, false));
                    switch (Random.Range(0, 4))
                    {
                        case 0:
                            oracle.room.PlaySound(NSHOracleSoundID.NSH_AI_Break_1, 0f, 1f, 1.25f);
                            break;
                        case 1:
                            oracle.room.PlaySound(NSHOracleSoundID.NSH_AI_Break_2, 0f, 1f, 1.25f);
                            break;
                        case 2:
                            oracle.room.PlaySound(NSHOracleSoundID.NSH_AI_Break_3, 0f, 1f, 1.25f);
                            break;
                        case 3:
                            oracle.room.PlaySound(NSHOracleSoundID.NSH_AI_Break_4, 0f, 1f, 1.25f);
                            break;
                    }
                    //oracle.room.PlaySound(SoundID.SL_AI_Talk_1, 0f, 1f, 2f);
                    //oracle.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Moon_Panic_Attack, oracle.firstChunk.pos, 1f, 1f);
                    if (nshSwarmer != null && nshSwarmer.room != null && nshSwarmer.grabbedBy.Count > 0)
                    {
                        for (int i = 0; i < nshSwarmer.grabbedBy.Count; i++)
                        {
                            swarmerGrabber = nshSwarmer.grabbedBy[i].grabber;
                        }
                    }
                    if (player.objectInStomach == nshSwarmer.abstractPhysicalObject)
                    {
                        swarmerGrabber = player;
                    }
                }
                if (inActionCounter == 40)
                {
                    switch (Random.Range(0, 2))
                    {
                        case 0:
                            oracle.room.PlaySound(NSHOracleSoundID.NSH_AI_Recover_1, 0f, 0.5f, 1.5f);
                            break;
                        case 1:
                            oracle.room.PlaySound(NSHOracleSoundID.NSH_AI_Recover_2, 0f, 0.5f, 1.5f);
                            break;
                    }
                }
                if (inActionCounter < 60)
                {
                    oracle.room.gravity = 1f;
                    oracle.stun = Mathf.Max(oracle.stun, Random.Range(2, 5));
                    oracle.arm.isActive = false;
                    movementBehavior = CustomMovementBehavior.ShowMedia;
                    //oracle.firstChunk.vel += 1.5f * Vector2.down;
                }
                //再次判断，找到谁拿走了神经元（以防玩家拿了神经元又丢开，导致NSH没有反应，也防止其他生物之后捡起来）
                if (inActionCounter == 60 && nshSwarmer != null && swarmerGrabber == null &&
                    ((nshSwarmer.room != null && nshSwarmer.grabbedBy.Count > 0) || 
                    player.objectInStomach == nshSwarmer.abstractPhysicalObject))
                {
                    oracle.arm.isActive = true;
                    if (nshSwarmer.room != null && nshSwarmer.grabbedBy.Count > 0)
                    {
                        for (int i = 0; i < nshSwarmer.grabbedBy.Count; i++)
                        {
                            swarmerGrabber = nshSwarmer.grabbedBy[i].grabber;
                        }
                    }
                    if (player.objectInStomach == nshSwarmer.abstractPhysicalObject)
                    {
                        swarmerGrabber = player;
                    }
                    wantPos = new Vector2(oracle.room.PixelWidth / 2 * 1.5f - 0.5f * swarmerGrabber.mainBodyChunk.pos.x, oracle.room.PixelHeight / 2 * 1.5f - 0.5f * swarmerGrabber.mainBodyChunk.pos.y);
                }
                //NSH醒来并抓起小偷
                if (inActionCounter > 60 && inActionCounter < 90 &&
                    (nshSwarmer.room != null && nshSwarmer.grabbedBy.Count > 0) ||
                     player.objectInStomach == nshSwarmer.abstractPhysicalObject)
                {
                    oracle.stun = 0;
                    movementBehavior = CustomMovementBehavior.ShowMedia;
                    oracle.firstChunk.vel *= Custom.LerpMap(oracle.firstChunk.vel.magnitude, 1f, 6f, 0.999f, 0.9f);
                    oracle.firstChunk.vel += Vector2.ClampMagnitude(wantPos - oracle.firstChunk.pos, 100f) / 100f * 2f;
                    swarmerGrabber.mainBodyChunk.vel *= Custom.LerpMap(swarmerGrabber.firstChunk.vel.magnitude, 1f, 6f, 0.999f, 0.9f);
                    swarmerGrabber.mainBodyChunk.vel += Vector2.ClampMagnitude(oracle.room.MiddleOfTile(24, 14) - swarmerGrabber.mainBodyChunk.pos, 40f) / 40f * 14f * Mathf.InverseLerp(30f, 160f, (float)this.inActionCounter);
                }
                if (inActionCounter == 90 && swarmerGrabber != null)
                {
                    swarmerReleased = true;
                    bool nshShouldAttackPlayer = false;
                    if (swarmerGrabber == player)
                    {
                        movementBehavior = CustomMovementBehavior.Talk;
                        if (owner.conversation != null)
                        {
                            owner.conversation.Destroy();
                            owner.conversation = null;
                        }
                        owner.InitateConversation(NSHConversationID.WarnSlugcatReleaseSwarmer, this);
                        swarmerStolen++;//这个要写在对话后面，这样有没有松开神经元才有差分
                    }
                    //如果玩家没有丢掉神经元，或者玩家吞下了神经元
                    if ((nshSwarmer != null && nshSwarmer.grabbedBy.Count > 0) ||
                        player.objectInStomach == nshSwarmer.abstractPhysicalObject)
                    {
                        nshShouldAttackPlayer = true;
                        //如果是吞下神经元，则让玩家吐出来
                        if (player.objectInStomach == nshSwarmer.abstractPhysicalObject)
                        {
                            //player.room.abstractRoom.AddEntity(player.objectInStomach);
                            player.objectInStomach.pos = player.abstractCreature.pos;
                            nshSwarmer = new NSHSwarmer(nshSwarmer.abstractPhysicalObject);
                            nshSwarmer.abstractPhysicalObject.RealizeInRoom();
                            player.room.PlaySound(SoundID.Slugcat_Regurgitate_Item, player.mainBodyChunk);
                            player.objectInStomach = null;
                        }
                        //如果是拿着神经元，则释放神经元
                        if (nshSwarmer != null && nshSwarmer.grabbedBy.Count > 0)
                        {
                            for (int i = 0; i < nshSwarmer.grabbedBy.Count; i++)
                            {
                                if (swarmerGrabber != null && swarmerGrabber.grasps != null)
                                {
                                    for (int j = 0; j < swarmerGrabber.grasps.Length; j++)
                                    {
                                        if (swarmerGrabber.grasps[j] != null &&
                                            swarmerGrabber.grasps[j].grabbed != null &&
                                            swarmerGrabber.grasps[j].grabbed == nshSwarmer)
                                        {
                                            swarmerGrabber.ReleaseGrasp(j);
                                        }
                                    }
                                }
                            }
                        }
                        //甩开小偷
                        swarmerGrabber.mainBodyChunk.vel += Custom.RNV() * 10f;
                        swarmerGrabber.bodyChunks[1].vel += Custom.RNV() * 10f;
                        
                        //特效
                        oracle.room.AddObject(new Explosion.ExplosionLight(swarmerGrabber.mainBodyChunk.pos, 150f, 1f, 8, Color.white));
                        for (int j = 0; j < 20; j++)
                        {
                            this.oracle.room.AddObject(new Spark(swarmerGrabber.mainBodyChunk.pos, Custom.RNV() * Random.value * 40f, new Color(1f, 1f, 1f), null, 30, 120));
                        }
                        oracle.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, 0f, 0.5f, 1.5f + Random.value * 0.5f);
                    }
                    //偷盗次数 > 2 就杀了，不是红猫拿的也杀了
                    if (swarmerStolen > 2 || swarmerGrabber != player)
                    {
                        swarmerGrabber.Die();
                    }
                    //否则就击晕
                    else if (nshShouldAttackPlayer)
                    {
                        swarmerGrabber.Stun(500);
                        swarmerStolen++;//偷盗次数加一
                    }
                }
                if (owner.conversation != null && swarmerReleased)
                {
                    if (owner.conversation.slatedForDeletion)
                    {
                        owner.getToWorking = 1f;
                        oracle.room.gravity = 0f;
                        movementBehavior = CustomMovementBehavior.Idle;
                        swarmerGrabber = null;
                        owner.conversation = null;
                        owner.NewAction(NSHOracleBehaviorAction.MeetHunter_RunIntoHunter);
                        return;
                    }
                }
            }
            //普遍行为：保护神经元
            if (owner.getToWorking == 1f && nshSwarmer != null && nshSwarmer.abstractPhysicalObject.realizedObject != null &&
                swarmerReleased && action != NSHOracleBehaviorAction.LetSlugcatReleaseSwarmer)
            {
                //如果迭代器在无重力状态靠神经元太近了，就远离
                if (Custom.Dist(nshSwarmer.firstChunk.pos, oracle.firstChunk.pos) < 100f)
                {
                    Plugin.Log("Too close to the neuron, Oracle starts to move away.");
                    Vector2 dist = (oracle.firstChunk.pos - nshSwarmer.firstChunk.pos).normalized;//dist必须是单位向量，否则反射就错了
                    if (Vector2.Angle(oracle.firstChunk.vel, dist) > 90)
                        oracle.firstChunk.vel = Vector2.Reflect(oracle.firstChunk.vel, dist);
                    //oracle.firstChunk.vel += dist * 300f / Custom.Dist(nshSwarmer.firstChunk.pos, oracle.firstChunk.pos);
                }
                //如果蛞蝓猫在无重力状态靠神经元太近了，就让蛞蝓猫远离
                if (Custom.Dist(nshSwarmer.firstChunk.pos, player.firstChunk.pos) < 80f)
                {
                    Plugin.Log("Too close to the neuron, Oracle starts to move Slugcat away.");
                    Vector2 dist = (player.firstChunk.pos - nshSwarmer.firstChunk.pos).normalized;
                    if (Vector2.Angle(player.firstChunk.vel, dist) > 90)
                        player.firstChunk.vel = Vector2.Reflect(player.firstChunk.vel, dist);
                    //警告蛞蝓猫
                    owner.NewAction(NSHOracleBehaviorAction.LetSlugcatStayAwayFromSwarmer);
                    //player.firstChunk.vel += dist * 300f / Custom.Dist(nshSwarmer.firstChunk.pos, player.firstChunk.pos);
                }
            }
        }

        public override void NewAction(CustomAction oldAction, CustomAction newAction)
        {
            base.NewAction(oldAction, newAction);
            if (newAction == NSHOracleBehaviorAction.MeetHunter_TalkAfterGiveMark)
            {
                owner.getToWorking = 0f;
                //owner.LockShortcuts();
                //owner.UnlockShortcuts();
                owner.InitateConversation(NSHConversationID.Hunter_DreamTalk0, this);
            }
            else if (newAction == NSHOracleBehaviorAction.MeetHunter_RunIntoHunter)
            {
                owner.getToWorking = 1f;
            }
            else if (newAction == NSHOracleBehaviorAction.MeetHunter_RunIntoTalk)
            {
                owner.getToWorking = 1f;
                owner.InitateConversation(NSHConversationID.Hunter_DreamTalk1, this);
            }
            else if (newAction == NSHOracleBehaviorAction.MeetHunter_FarewellTalk)
            {
                owner.getToWorking = 0f;
            }
            else if (newAction == NSHOracleBehaviorAction.MeetHunter_Praise)
            {
                owner.getToWorking = 0f;
                owner.InitateConversation(NSHConversationID.Hunter_DreamTalk5, this);
            }
            else if (newAction == NSHOracleBehaviorAction.MeetHunter_UnfulfilledMessager1)
            {
                owner.getToWorking = 0f;
                movementBehavior = CustomMovementBehavior.Talk;
                owner.InitateConversation(NSHConversationID.Hunter_UnfulfilledMessager0, this);
            }
            else if (newAction == NSHOracleBehaviorAction.MeetHunter_UnfulfilledMessager2)
            {
                owner.getToWorking = 1f;
                movementBehavior = CustomMovementBehavior.Talk;
                owner.InitateConversation(NSHConversationID.Hunter_UnfulfilledMessager1, this);
            }
            else if (newAction == NSHOracleBehaviorAction.MeetHunter_Talk1)
            {
                (owner as NSHOracleBehaviour).LockShortcuts();
                owner.getToWorking = 0f;
                movementBehavior = CustomMovementBehavior.KeepDistance;
                owner.InitateConversation(NSHConversationID.Hunter_Talk0, this);
            }
            else if (newAction == NSHOracleBehaviorAction.MeetHunter_Talk2)
            {
                owner.getToWorking = 1f;
                movementBehavior = CustomMovementBehavior.Talk;
                owner.InitateConversation(NSHConversationID.Hunter_Talk1, this);
            }
            else if (newAction == NSHOracleBehaviorAction.MeetHunter_Talk3)
            {
                owner.getToWorking = 0f;
                //NSHPearl_Move(player, oracle, owner);
            }
            else if (newAction == NSHOracleBehaviorAction.MeetHunter_Talk3_Wait)
            {
                owner.getToWorking = 1f;
                movementBehavior = CustomMovementBehavior.Talk;
                owner.InitateConversation(NSHConversationID.Hunter_Talk2_Wait, this);
            }
            else if (newAction == NSHOracleBehaviorAction.LetSlugcatStayAwayFromSwarmer)
            {
                owner.InitateConversation(NSHConversationID.WarnSlugcatStayAwayFromSwarmer, this);
            }
            else if (newAction == NSHOracleBehaviorAction.LetSlugcatReleaseSwarmer)
            {
                owner.getToWorking = 0f;
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

        //绿神经元的特效
        public static void NSHSwarmer_Effects(Player player, Oracle oracle, CustomOracleBehaviour owner)
        {            
            if (!swarmerCreated)
            {
                //生成神经元
                swarmerCreated = true;
                AbstractPhysicalObject absNshSwarmer = new AbstractPhysicalObject(oracle.room.world, AbstractPhysicalObject.AbstractObjectType.NSHSwarmer, null, oracle.abstractPhysicalObject.pos, oracle.room.game.GetNewID());
                nshSwarmer = new NSHSwarmer(absNshSwarmer);
                swarmerLabels = new List<GlyphLabel>();
                nshSwarmer.abstractPhysicalObject.RealizeInRoom();
            }
            
            //神经元的属性
            if (nshSwarmer != null && nshSwarmer.room != null)
            {
                Vector2 nshSwarmerPos = nshSwarmer.firstChunk.pos;

                //维持在房间中心（工作）/NSH周围（非工作）
                Vector2 wantPos = (owner.getToWorking == 1) ? new Vector2(oracle.room.PixelWidth / 2, oracle.room.PixelHeight / 2) : oracle.firstChunk.pos;
                nshSwarmer.firstChunk.vel *= Custom.LerpMap(nshSwarmer.firstChunk.vel.magnitude, 1f, 6f, 0.999f, 0.9f);
                nshSwarmer.firstChunk.vel += Vector2.ClampMagnitude(wantPos - nshSwarmer.firstChunk.pos, 100f) / 100f * 0.4f;
                //抵消重力（非工作）
                nshSwarmer.firstChunk.vel += (owner.getToWorking == 1) ? Vector2.zero : (1f * Vector2.up);
                //随机速度（非工作）
                nshSwarmer.bodyChunks[0].vel += (owner.getToWorking == 1) ? Vector2.zero : Custom.RNV() * Random.value * 0.5f;
                //神经元在NSH工作时必显示投影
                nshSwarmer.holoFade = owner.getToWorking;

                //如果NSH在工作状态，且神经元没有被抓住
                if (owner.getToWorking == 1 && nshSwarmer.grabbedBy.Count == 0)
                {
                    //自制简陋特效
                    //遍历物品找到珍珠（43颗）
                    for (int i = 0; i < oracle.room.physicalObjects.Length; i++)
                    {
                        for (int j = 0; j < oracle.room.physicalObjects[i].Count; j++)
                        {
                            if (oracle.room.physicalObjects[i][j] is NSHPearl)
                            {
                                NSHPearl pearl = oracle.room.physicalObjects[i][j] as NSHPearl;
                                //0.08%的概率，将珍珠上的字符复制一份
                                if (Random.value < 0.0008f && pearl.label != null)
                                {
                                    GlyphLabel swarmerLabel = new GlyphLabel(pearl.firstChunk.pos, pearl.label.glyphs);
                                    oracle.room.AddObject(swarmerLabel);
                                    swarmerLabels.Add(swarmerLabel);
                                    //加点生成字符的特效
                                    oracle.room.AddObject(new Explosion.ExplosionLight(pearl.firstChunk.pos, 150f, 1f, 8, Color.white));
                                    oracle.room.AddObject(new ShockWave(pearl.firstChunk.pos, 60f, 0.1f, 8, false));
                                    oracle.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Data_Bit, pearl.firstChunk.pos, 1f, 1f + Random.value * 2f);
                                    //该珍珠被电击
                                    OracleSwarmerResync oraclePearlResync = new OracleSwarmerResync(oracle, pearl, false, 0, 0);
                                    oracle.room.AddObject(oraclePearlResync);
                                }
                            }
                        }
                    }
                    //复制的字符向神经元移动
                    for (int i = swarmerLabels.Count - 1; i >= 0; i--)
                    {
                        GlyphLabel label = swarmerLabels[i];
                        label.color = Color.Lerp(NSHOracleColor.PureGreen, NSHOracleColor.Purple, ((float)NSHSwarmerHooks.colorCount) / ((float)NSHSwarmerHooks.colorCycle));
                        label.setPos = label.pos + Vector2.ClampMagnitude(nshSwarmerPos - label.pos, 100f) / 100f * 2.8f;
                        //如果足够靠近，就消失
                        if (Custom.Dist(nshSwarmerPos, label.pos) < 8f)
                        {
                            Vector2 pos = nshSwarmerPos;
                            for (int j = 0; j < 5; j++)
                            {
                                oracle.room.AddObject(new Spark(pos, Custom.RNV(), Color.white, null, 16, 24));
                            }
                            oracle.room.AddObject(new Explosion.ExplosionLight(pos, 150f, 1f, 8, Color.white));
                            oracle.room.AddObject(new ShockWave(pos, 60f, 0.1f, 8, false));
                            oracle.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, 0f, 0.3f, 1.5f + Random.value * 0.5f);
                            //生成闪电
                            OracleSwarmerResync oracleSwarmerResync = new OracleSwarmerResync(oracle, nshSwarmer, false, 0, 0);
                            oracle.room.AddObject(oracleSwarmerResync);

                            swarmerLabels.Remove(label);
                            label.Destroy();
                        }
                    }
                }
                else if (nshSwarmer.grabbedBy.Count > 0 && swarmerReleased)
                {
                    swarmerReleased = false;
                    if (owner.conversation != null)
                    {
                        owner.conversation.Destroy();
                        owner.conversation = null;
                    }
                    owner.NewAction(NSHOracleBehaviorAction.LetSlugcatReleaseSwarmer);
                    for (int i = 0; i < nshSwarmer.grabbedBy.Count; i++)
                    {
                        for (int k = swarmerLabels.Count - 1; k >= 0; k--)
                        {
                            GlyphLabel label = swarmerLabels[k];
                            oracle.room.AddObject(new Explosion.ExplosionLight(label.pos, 150f, 1f, 8, Color.white));
                            oracle.room.AddObject(new ShockWave(label.pos, 60f, 0.1f, 8, false));
                            oracle.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, 0f, 0.5f, 1.5f + Random.value * 0.5f);
                            swarmerLabels.Remove(label);
                            label.Destroy();
                        }
                        swarmerLabels.Clear();
                    }
                }
                //神经元没有被抓住时，非工作状态会清除字符列表
                else if (swarmerLabels != null)
                {
                    for (int i = swarmerLabels.Count - 1; i >= 0; i--)
                    {
                        GlyphLabel label = swarmerLabels[i];
                        /*
                        oracle.room.AddObject(new Explosion.ExplosionLight(label.pos, 150f, 1f, 8, Color.white));
                        oracle.room.AddObject(new ShockWave(label.pos, 60f, 0.1f, 8, false));
                        oracle.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Data_Bit, 0f, 0.5f, 1.5f + Random.value * 0.5f);*/
                        swarmerLabels.Remove(label);
                        label.Destroy();
                    }
                    swarmerLabels.Clear();
                }
            }
        }

        //与红猫的所有对话
        public void AddConversationEvents(NSHConversation conv, Conversation.ID id)
        {
            int extralingerfactor = oracle.room.game.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.Chinese ? 1 : 0;
            //梦境对话
            //第一场梦境，诞生
            if (id == NSHConversationID.Hunter_DreamTalk0)
            {
                NSHConversation.LoadEventsFromFile(conv, 0, "Dream-0");
            }
            //第二场梦境，误入
            else if (id == NSHConversationID.Hunter_DreamTalk1)
            {
                NSHConversation.LoadEventsFromFile(conv, 0, "Dream-1");
            }
            //第三场梦境的对话属于旁白，不在这里，在NSHConversation里
            //第四场梦境，启程
            else if (id == NSHConversationID.Hunter_DreamTalk3)
            {
                NSHConversation.LoadEventsFromFile(conv, 0, "Dream-3");
            }
            //美梦，夸夸
            else if (id == NSHConversationID.Hunter_DreamTalk5)
            {
                NSHConversation.LoadEventsFromFile(conv, 0, "Dream-5");
            }
            //现实对话
            else if (id == NSHConversationID.Hunter_UnfulfilledMessager0)
            {
                NSHConversation.LoadEventsFromFile(conv, 47, "UnfulfilledMessager-1");
            }
            else if (id == NSHConversationID.Hunter_UnfulfilledMessager1)
            {
                NSHConversation.LoadEventsFromFile(conv, 47, "UnfulfilledMessager-2");
            }
            else if (id == NSHConversationID.Hunter_Talk0)
            {
                if (owner.IsThereHasSlugPup())
                {
                    List<Player> pups = owner.FindSlugPup();
                    if (pups.Count == 1)
                        NSHConversation.LoadEventsFromFile(conv, 0, "Ending-0-WithPup");
                    else
                        NSHConversation.LoadEventsFromFile(conv, 0, "Ending-0-WithPups");
                }
                else
                    NSHConversation.LoadEventsFromFile(conv, 0, "Ending-0");
            }
            else if (id == NSHConversationID.Hunter_Talk1)
            {
                if (owner.IsThereHasSlugPup())
                {
                    List<Player> pups = owner.FindSlugPup();
                    if (pups.Count == 1)
                        NSHConversation.LoadEventsFromFile(conv, 0, "Ending-1-WithPup");
                    else
                        NSHConversation.LoadEventsFromFile(conv, 0, "Ending-1-WithPups");
                }
                else
                    NSHConversation.LoadEventsFromFile(conv, 0, "Ending-1");
            }
            else if (id == NSHConversationID.Hunter_Talk2)
            {
                NSHConversation.LoadEventsFromFile(conv, 0, "Ending-2");
            }
            else if (id == NSHConversationID.Hunter_Talk2_Wait)
            {
                if (owner.IsThereHasSlugPup())
                    NSHConversation.LoadEventsFromFile(conv, 0, "Ending-Wait-WithPup");
                else
                    NSHConversation.LoadEventsFromFile(conv, 0, "Ending-Wait");
            }
            //特殊对话
            else if (id == NSHConversationID.WarnSlugcatStayAwayFromSwarmer)
            {
                switch (NSHOracleMeetHunter.swarmerApproached)
                {
                    case 0:
                        conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Please keep out. I don't want anything to go wrong at this stage."), 60 * extralingerfactor));
                        break;
                    case 1:
                        conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("It seems like you're curious about it? I'll hand it over to you at the right time, but not now."), 80 * extralingerfactor));
                        break;
                    case 2:
                        conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("What are you doing, messenger? Please don't touch it."), 60 * extralingerfactor));
                        conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("It will interrupt my process and potentially result in errors and irreversible damage."), 70 * extralingerfactor));
                        conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("It's an unpleasant experience for any iterator, so please stop your attempts."), 65 * extralingerfactor));
                        break;
                    default:
                        switch (Random.Range(0, 8))
                        {
                            case 0:
                                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Please do not approach it."), 30 * extralingerfactor));
                                break;
                            case 1:
                                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("May I hope you don't interfere with my work?"), 20 * extralingerfactor));
                                break;
                            default:
                                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("..."), 20 * extralingerfactor));
                                break;
                        }
                        break;
                }
            }
            else if (id == NSHConversationID.WarnSlugcatReleaseSwarmer)
            {
                switch (NSHOracleMeetHunter.swarmerStolen)
                {
                    case 0:
                        conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("..."), 20 * extralingerfactor));
                        conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("I don't know how you did it, but I hope it won't happen again."), 60 * extralingerfactor));
                        break;
                    case 1:
                        conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Seems like you are unwilling to follow orders?"), 45 * extralingerfactor));
                        conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Such a shame! I have to remind you that my patience is not infinite."), 55 * extralingerfactor));
                        break;
                    default:
                        conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Such a disobedient messenger~"), 25 * extralingerfactor));
                        conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Congratulations! You are free now."), 30 * extralingerfactor));
                        break;
                }
            }
        }

        //用于计算猎手的时间状态
        public override int GetPlayerEncountersState()
        {
            if (this.owner.oracle.room.game.rainWorld.progression.currentSaveState.miscWorldSaveData.SLOracleState.neuronsLeft > 0)
                return 1;
            else
                return 0;
        }
    }
}
