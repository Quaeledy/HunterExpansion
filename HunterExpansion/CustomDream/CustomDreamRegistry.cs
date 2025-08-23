using CustomDreamTx;
using HunterExpansion.CustomEnding;
using HunterExpansion.CustomOracle;
using HunterExpansion.CustomSave;
using Menu;
using RWCustom;

namespace HunterExpansion.CustomDream
{
    public class HunterDreamRegistry : CustomNormalDreamTx
    {
        public HunterDreamRegistry() : base(Plugin.SlugName)
        {
        }

        public override void DecideDreamID(
            SaveState saveState,
            string currentRegion,
            string denPosition,
            ref int cyclesSinceLastDream,
            ref int cyclesSinceLastFamilyDream,
            ref int cyclesSinceLastGuideDream,
            ref int inGWOrSHCounter,
            ref DreamsState.DreamID upcomingDream,
            ref DreamsState.DreamID eventDream,
            ref bool everSleptInSB,
            ref bool everSleptInSB_S01,
            ref bool guideHasShownHimselfToPlayer,
            ref int guideThread,
            ref bool guideHasShownMoonThisRound,
            ref int familyThread)
        {
            if (dreamFinished || saveState.malnourished || (ModManager.Expedition && Custom.rainWorld.ExpeditionMode)) return;

            upcomingDream = null;
            cyclesSinceLastFamilyDream = 0;//屏蔽FamilyDream计数，防止被原本的方法干扰

            Plugin.Log("Hunter DreamState : cycleSinceLastDream: {0}, FamilyThread: {1}", cyclesSinceLastDream, familyThread);

            int interval = 3;
            if (saveState.miscWorldSaveData.SLOracleState.neuronsLeft > 0)
            {
                interval = 2;
            }
            if (saveState.deathPersistentSaveData.altEnding)
            {
                interval = 1;
            }
            switch (familyThread)
            {
                case 0:
                    if (saveState.cycleNumber >= 1 && cyclesSinceLastDream > 0)//saveState.cycleNumber >= 0，会导致第一个循环死亡时进入梦境，但迭代器不更新，退出重进后梦境也不结束
                    {
                        saveState.deathPersistentSaveData.theMark = false;
                        upcomingDream = DreamID.HunterDream_0;
                    }
                    break;
                case 1:
                    if (cyclesSinceLastDream > interval)
                        upcomingDream = DreamID.HunterDream_1;
                    break;
                case 2:
                    if (cyclesSinceLastDream > interval)
                        upcomingDream = DreamID.HunterDream_2;
                    break;
                case 3:
                    if (cyclesSinceLastDream > interval)
                        upcomingDream = DreamID.HunterDream_3;
                    break;
                case 4:
                    if (saveState.cycleNumber >= Plugin.RedsCycles(saveState.redExtraCycles, saveState))
                        upcomingDream = DreamID.HunterDream_4;
                    break;
            }
            //如果救了月，则直接出现美梦
            if (!saveState.deathPersistentSaveData.altEnding &&
                saveState.miscWorldSaveData.SLOracleState.neuronsLeft > 0 &&
                !FondDreamCompletedSave.fondDreamCompleted)
            {
                upcomingDream = DreamID.HunterDream_5;
                FondDreamCompletedSave.fondDreamCompleted = true;
            }

            if (upcomingDream != null && upcomingDream != DreamID.HunterDream_5)
            {
                familyThread++;
                cyclesSinceLastDream = 0;
            }
            //清除旧梦境的影响
            PlayerHooks.hasEnterDream = false;
            if (NSHOracleMeetHunter.nshSwarmer != null)
            {
                NSHOracleMeetHunter.nshSwarmer.Destroy();
            }
        }

        public override bool IsPerformDream => (activateDreamID == DreamID.HunterDream_0 ||
                                                activateDreamID == DreamID.HunterDream_1 ||
                                                activateDreamID == DreamID.HunterDream_2 ||
                                                activateDreamID == DreamID.HunterDream_3 ||
                                                activateDreamID == DreamID.HunterDream_4 ||
                                                activateDreamID == DreamID.HunterDream_5);

        public override CustomDreamRx.BuildDreamWorldParams GetBuildDreamWorldParams()
        {
            //诞生，启程，夸夸
            if (activateDreamID == DreamID.HunterDream_0 ||
                activateDreamID == DreamID.HunterDream_3 ||
                activateDreamID == DreamID.HunterDream_5)
            {
                return new CustomDreamRx.BuildDreamWorldParams()
                {
                    firstRoom = "NSH_AI",
                    singleRoomWorld = false,
                    overridePlayerPos = new IntVector2(24, 6),//在地上
                    playAs = Plugin.SlugName,
                };
            }
            //误入演算室
            else if (activateDreamID == DreamID.HunterDream_1)
            {
                return new CustomDreamRx.BuildDreamWorldParams()
                {
                    firstRoom = "NSH_AI",
                    singleRoomWorld = false,
                    overridePlayerPos = new IntVector2(24, 34),//在管道口
                    playAs = Plugin.SlugName,
                };
            }
            //对战金蜥蜴
            else if (activateDreamID == DreamID.HunterDream_2)
            {
                return new CustomDreamRx.BuildDreamWorldParams()
                {
                    firstRoom = "NSH_A01",
                    singleRoomWorld = false,
                    overridePlayerPos = new IntVector2(10, 25),
                    playAs = Plugin.SlugName,
                };
            }
            //腐化的噩梦
            else if (activateDreamID == DreamID.HunterDream_4)
            {
                return new CustomDreamRx.BuildDreamWorldParams()
                {
                    firstRoom = "NSH_AITEST",
                    singleRoomWorld = false,
                    overridePlayerPos = new IntVector2(24, 12),//在地上
                    playAs = Plugin.SlugName,
                };
            }
            else
            {
                return null;
            }
        }
        /*
        public override void EndDream(RainWorldGame game)
        {
            base.EndDream(game);
            PlayerHooks.hasEnterDream = false;
        }*/
    }

    public class TravelDreamRegistry : CustomNormalDreamTx
    {
        public TravelDreamRegistry() : base(Plugin.AllSlugcats)//要想办法让它对所有猫生效
        {
            modifyScribbleDreamScreen = false;
        }

        public static bool modifyScribbleDreamScreen;

        public override void DecideDreamID(
            SaveState saveState,
            string currentRegion,
            string denPosition,
            ref int cyclesSinceLastDream,
            ref int cyclesSinceLastFamilyDream,
            ref int cyclesSinceLastGuideDream,
            ref int inGWOrSHCounter,
            ref DreamsState.DreamID upcomingDream,
            ref DreamsState.DreamID eventDream,
            ref bool everSleptInSB,
            ref bool everSleptInSB_S01,
            ref bool guideHasShownHimselfToPlayer,
            ref int guideThread,
            ref bool guideHasShownMoonThisRound,
            ref int familyThread)
        {
            if (dreamFinished) return;

            //upcomingDream = null;

            Plugin.Log("Travel DreamState : cycleSinceLastDream: {0}, travelCompleted: {1}", cyclesSinceLastDream, TravelCompletedSave.travelCompleted);

            this.focusSlugcat = Plugin.AllSlugcats;
            //其他猫去NSH的过场动画
            if (denPosition == "GATE_NSH_DGL" && !TravelCompletedSave.travelCompleted)
            {
                EndingScenes.slugName = saveState.saveStateNumber;
                upcomingDream = DreamID.GoToNSH;
                TravelCompletedSave.travelCompleted = true;
                modifyScribbleDreamScreen = true;
            }
            else if ((denPosition == "GATE_SB_OE" || denPosition == "GATE_OE_SU") && !TravelCompletedSave.travelCompleted)
            {
                EndingScenes.slugName = saveState.saveStateNumber;
                upcomingDream = DreamID.LeaveNSH;
                TravelCompletedSave.travelCompleted = true;
                modifyScribbleDreamScreen = true;
            }

            if (upcomingDream != null)
            {
                cyclesSinceLastDream = 0;
            }
        }

        public override bool IsPerformDream => false;

        public override CustomDreamRx.BuildDreamWorldParams GetBuildDreamWorldParams()
        {
            return null;
        }

        public override MenuScene.SceneID SceneFromDream(DreamsState.DreamID dreamID)
        {
            if (dreamID == DreamID.GoToNSH)
            {
                return MenuSceneID.GoToNSH;
            }
            if (dreamID == DreamID.LeaveNSH)
            {
                return MenuSceneID.LeaveNSH;
            }
            return SceneFromDream(dreamID);
        }


        public override void EndDream(RainWorldGame game)
        {
            base.EndDream(game);
            this.focusSlugcat = Plugin.AllSlugcats;
        }
    }

    public class DreamID
    {
        public static void RegisterValues()
        {
            //红猫诞生，被告知是信使（幼崽体型）
            HunterDream_0 = new DreamsState.DreamID("HunterDream_0", true);
            //红猫误入演算室，NSH在制造密钥
            HunterDream_1 = new DreamsState.DreamID("HunterDream_1", true);
            //红猫出现在一个房间中，将在NSH的安排下与一只金蜥蜴对战
            HunterDream_2 = new DreamsState.DreamID("HunterDream_2", true);
            //NSH写珍珠，把密钥给红猫
            HunterDream_3 = new DreamsState.DreamID("HunterDream_3", true);
            //负循环的噩梦：NSH演算室，但里面只有一只香菇
            HunterDream_4 = new DreamsState.DreamID("HunterDream_4", true);
            //送完信的美梦：NSH夸夸信使
            HunterDream_5 = new DreamsState.DreamID("HunterDream_5", true);
            //其他猫去NSH的过场cg
            GoToNSH = new DreamsState.DreamID("GoToNSH", true);
            LeaveNSH = new DreamsState.DreamID("LeaveNSH", true);
        }

        public static void UnregisterValues()
        {
            HunterExpansionEnums.Unregister(HunterDream_0);
            HunterExpansionEnums.Unregister(HunterDream_1);
            HunterExpansionEnums.Unregister(HunterDream_2);
            HunterExpansionEnums.Unregister(HunterDream_3);
            HunterExpansionEnums.Unregister(HunterDream_4);
            HunterExpansionEnums.Unregister(HunterDream_5);
            HunterExpansionEnums.Unregister(GoToNSH);
            HunterExpansionEnums.Unregister(LeaveNSH);
        }

        //红猫诞生，被告知是信使（幼崽体型）
        public static DreamsState.DreamID HunterDream_0;
        //红猫误入演算室，NSH在制造密钥
        public static DreamsState.DreamID HunterDream_1;
        //红猫出现在一个房间中，将在NSH的安排下与一只金蜥蜴对战
        public static DreamsState.DreamID HunterDream_2;
        //NSH写珍珠，把密钥给红猫
        public static DreamsState.DreamID HunterDream_3;
        //负循环的噩梦：NSH演算室，但里面只有一只香菇
        public static DreamsState.DreamID HunterDream_4;
        //送完信的美梦：NSH夸夸信使
        public static DreamsState.DreamID HunterDream_5;
        //其他猫去NSH的过场cg
        public static DreamsState.DreamID GoToNSH;
        public static DreamsState.DreamID LeaveNSH;
    }
}
