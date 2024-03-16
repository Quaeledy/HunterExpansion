using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomOracleTx;
using UnityEngine;
using Random = UnityEngine.Random;
using RWCustom;
using static CustomOracleTx.CustomOracleBehaviour;
using CustomDreamTx;
using HunterExpansion.CustomDream;
using HunterExpansion;
using HunterExpansion.CustomSave;
using MoreSlugcats;

namespace HunterExpansion.CustomOracle
{
    public class NSHOracleRegistry : CustomOracleTx.CustomOracleTx
    {
        public static Oracle.OracleID NSHOracle = new Oracle.OracleID("NSH", true);

        public static string currentLoadingRoom;

        public string[] roomsToLoad = { "NSH_AI", "HR_AI" };
        public override string LoadRoom
        {
            get
            {
                if (roomsToLoad.Contains(currentLoadingRoom))
                {
                    if (currentLoadingRoom == "HR_AI" && RipNSHSave.ripNSH)
                        return currentLoadingRoom;
                }
                return roomsToLoad[0];
            }
        }
        public override Oracle.OracleID OracleID => NSHOracle;
        public override Oracle.OracleID InheritOracleID => Oracle.OracleID.SS;


        //腐化相关
        public static bool isCorrupted;

        public NSHOracleRegistry()
        {
            gravity = 1f;
            startPos = new Vector2(400f, 450f);
            pearlRegistry = new NSHPearlRegistry();
            CustomOraclePearlRx.ApplyTreatment(pearlRegistry);
        }

        public override void LoadBehaviourAndSurroundings(ref Oracle oracle, Room room)
        {
            isCorrupted = room.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel;

            base.LoadBehaviourAndSurroundings(ref oracle, room);

            oracle.oracleBehavior = new NSHOracleBehaviour(oracle);
            oracle.myScreen = new OracleProjectionScreen(room, oracle.oracleBehavior);
            room.AddObject(oracle.myScreen);

            if (LoadRoom != "HR_AI" && !isCorrupted)
                oracle.SetUpMarbles();

            oracle.arm = new Oracle.OracleArm(oracle);
            room.gravity = (RipNSHSave.ripNSH && LoadRoom != "HR_AI" && !isCorrupted) ? 1f : 0f;
            startPos = (RipNSHSave.ripNSH && LoadRoom != "HR_AI") ? new Vector2(400f, 110f) : new Vector2(350f, 350f);

            for (int n = 0; n < room.updateList.Count; n++)
            {
                if (room.updateList[n] is AntiGravity)
                {
                    (room.updateList[n] as AntiGravity).active = false;
                    break;
                }
            }
            Plugin.Log("Successfully load behaviours and surroundings!");
        }

        public override OracleGraphics InitCustomOracleGraphic(PhysicalObject ow)
        {
            return new NSHOracleGraphics(ow);
        }


        public static void ChangeMarble(CustomOrbitableOraclePearl pearl, Oracle oracle, PhysicalObject orbitObj, Vector2 ps, int circle, float dist, int color)
        {
            if (pearl != null)
            {
                CustomOracleTx.CustomOracleTx.CustomOralceEX customOracleEX;
                if (CustomOracleTx.CustomOracleTx.oracleEx.TryGetValue(oracle, out customOracleEX))
                {
                    pearl.oracle = oracle;
                    pearl.firstChunk.HardSetPosition(ps);
                    pearl.orbitObj = orbitObj;
                    if (orbitObj == null)
                    {
                        pearl.hoverPos = new Vector2?(ps);
                    }
                    pearl.orbitCircle = circle;
                    pearl.orbitDistance = dist;
                    pearl.marbleColor = color;
                }
            }
        }
    }

    public class NSHOracleColor
    {
        public static readonly Color Green = new Color(0f / 255f, 121f / 255f, 101f / 255f);
        public static readonly Color PureGreen = new Color(0f / 255f, 255f / 255f, 0f / 255f);
        public static readonly Color LightGreen = new Color(120f / 255f, 210f / 255f, 94f / 255f);
        public static readonly Color DarkGreen = new Color(0f / 255f, 42f / 255f, 31f / 255f);

        public static readonly Color Pink = new Color(129f / 255f, 75f / 255f, 173f / 255f);
        public static readonly Color Purple = new Color(57f / 255f, 14f / 255f, 91f / 255f);
        public static readonly Color Violet = new Color(41f / 255f, 37f / 255f, 104f / 255f);
        public static readonly Color DarkViolet = new Color(22f / 255f, 13f / 255f, 62f / 255f);
        public static readonly Color Orange = new Color(254f / 255f, 166f / 255f, 92f / 255f);

        public static readonly Color LightGrey = new Color(0.7f, 0.7f, 0.7f);
        public static readonly Color Rose = new Color(255f / 255f, 67f / 255f, 115f / 255f);
        public static readonly Color Blue = new Color(40f / 255f, 102f / 255f, 141f / 255f);

        public static readonly Color CorruptedRed = new Color(0.57255f, 0.11373f, 0.22745f);
    }

    public class NSHOracleSoundID
    {
        public static void RegisterValues()
        {
            NSH_AI_Attack_1 = new SoundID("NSH_AI_Attack_1", true);
            NSH_AI_Attack_2 = new SoundID("NSH_AI_Attack_2", true);
            NSH_AI_Attack_3 = new SoundID("NSH_AI_Attack_3", true);
            NSH_AI_Attack_4 = new SoundID("NSH_AI_Attack_4", true);
            NSH_AI_Break_1 = new SoundID("NSH_AI_Break_1", true);
            NSH_AI_Break_2 = new SoundID("NSH_AI_Break_2", true);
            NSH_AI_Break_3 = new SoundID("NSH_AI_Break_3", true);
            NSH_AI_Break_4 = new SoundID("NSH_AI_Break_4", true);
            NSH_AI_LongDialogue_1 = new SoundID("NSH_AI_LongDialogue_1", true);
            NSH_AI_LongDialogue_2 = new SoundID("NSH_AI_LongDialogue_2", true);
            NSH_AI_LongDialogue_3 = new SoundID("NSH_AI_LongDialogue_3", true);
            NSH_AI_Recover_1 = new SoundID("NSH_AI_Recover_1", true);
            NSH_AI_Recover_2 = new SoundID("NSH_AI_Recover_2", true);
            NSH_AI_ShortDialogue_1 = new SoundID("NSH_AI_ShortDialogue_1", true);
            NSH_AI_ShortDialogue_2 = new SoundID("NSH_AI_ShortDialogue_2", true);
        }

        public static void UnregisterValues()
        {
            HunterExpansionEnums.Unregister(NSH_AI_Attack_1);
            HunterExpansionEnums.Unregister(NSH_AI_Attack_2);
            HunterExpansionEnums.Unregister(NSH_AI_Attack_3);
            HunterExpansionEnums.Unregister(NSH_AI_Attack_4);
            HunterExpansionEnums.Unregister(NSH_AI_Break_1);
            HunterExpansionEnums.Unregister(NSH_AI_Break_2);
            HunterExpansionEnums.Unregister(NSH_AI_Break_3);
            HunterExpansionEnums.Unregister(NSH_AI_Break_4);
            HunterExpansionEnums.Unregister(NSH_AI_LongDialogue_1);
            HunterExpansionEnums.Unregister(NSH_AI_LongDialogue_2);
            HunterExpansionEnums.Unregister(NSH_AI_LongDialogue_3);
            HunterExpansionEnums.Unregister(NSH_AI_Recover_1);
            HunterExpansionEnums.Unregister(NSH_AI_Recover_2);
            HunterExpansionEnums.Unregister(NSH_AI_ShortDialogue_1);
            HunterExpansionEnums.Unregister(NSH_AI_ShortDialogue_2);
        }

        public static SoundID NSH_AI_Attack_1;
        public static SoundID NSH_AI_Attack_2;
        public static SoundID NSH_AI_Attack_3;
        public static SoundID NSH_AI_Attack_4;
        public static SoundID NSH_AI_Break_1;
        public static SoundID NSH_AI_Break_2;
        public static SoundID NSH_AI_Break_3;
        public static SoundID NSH_AI_Break_4;
        public static SoundID NSH_AI_LongDialogue_1;
        public static SoundID NSH_AI_LongDialogue_2;
        public static SoundID NSH_AI_LongDialogue_3;
        public static SoundID NSH_AI_Recover_1;
        public static SoundID NSH_AI_Recover_2;
        public static SoundID NSH_AI_ShortDialogue_1;
        public static SoundID NSH_AI_ShortDialogue_2;
    }

    public class NSHOracleBehaviorAction
    {
        public static void RegisterValues()
        {
            //总览
            MeetHunter_Init = new CustomAction("NSHMeeetHunter_Init", true);
            MeetSpear_Init = new CustomAction("NSHMeeetSpear_Init", true);
            MeetArtificer_Init = new CustomAction("NSHMeeetArtificer_Init", true);
            MeetGourmand_Init = new CustomAction("NSHMeeetGourmand_Init", true);
            MeetWhite_Init = new CustomAction("NSHMeeetWhite_Init", true);
            MeetYellow_Init = new CustomAction("NSHMeeetYellow_Init", true);
            MeetRivulet_Init = new CustomAction("NSHMeeetRivulet_Init", true);
            MeetSaint_Init = new CustomAction("NSHMeeetSaint_Init", true);
            MeetSofanthiel_Init = new CustomAction("NSHMeeetSofanthiel_Init", true);
            MeetOtherSlugcat_Init = new CustomAction("NSHMeeetOtherSlugcat_Init", true);
            MeetOracle_Init = new CustomAction("NSHMeeetOracle_Init", true);
            Rubicon_Init = new CustomAction("NSHInRubicon_Init", true);
            General_Meditate = new CustomAction("NSHGeneral_Meditate", true);
            //矛大师
            MeetSpear_Talk1 = new CustomAction("NSHMeetSpear_Talk1", true);
            MeetSpear_Talk2 = new CustomAction("NSHMeetSpear_Talk2", true);
            MeetSpear_Talk3 = new CustomAction("NSHMeetSpear_Talk3", true);
            MeetSpear_AfterSpearMeetPebbles_1 = new CustomAction("MeetSpear_AfterSpearMeetPebbles_1", true);
            MeetSpear_AfterSpearMeetPebbles_2 = new CustomAction("MeetSpear_AfterSpearMeetPebbles_2", true);
            MeetSpear_AfterAltEnd_1 = new CustomAction("MeetSpear_AfterAltEnd_1", true);
            MeetSpear_AfterAltEnd_2 = new CustomAction("MeetSpear_AfterAltEnd_2", true);
            MeetSpear_AfterAltEnd_3 = new CustomAction("MeetSpear_AfterAltEnd_3", true);
            //工匠
            MeetArtificer_Talk1 = new CustomAction("NSHMeetArtificer_Talk1", true);
            MeetArtificer_Talk2 = new CustomAction("NSHMeetArtificer_Talk2", true);
            //猎手
            MeetHunter_GiveMark = new CustomAction("NSHMeeetHunter_GiveMark", true);//第一场梦境
            MeetHunter_TalkAfterGiveMark = new CustomAction("NSHMeeetHunter_TalkAfterGiveMark", true);//第一场梦境
            MeetHunter_RunIntoHunter = new CustomAction("NSHMeeetHunter_RunIntoHunter", true);//第二场梦境
            MeetHunter_RunIntoTalk = new CustomAction("NSHMeeetHunter_RunIntoTalk", true);//第二场梦境
            MeetHunter_FarewellTalk = new CustomAction("NSHMeeetHunter_FarewellTalk", true);//第四场梦境
            MeetHunter_Praise = new CustomAction("NSHMeeetHunter_Praise", true);//美梦
            MeetHunter_UnfulfilledMessager1 = new CustomAction("NSHMeetHunter_UnfulfilledMessager1", true);
            MeetHunter_UnfulfilledMessager2 = new CustomAction("NSHMeetHunter_UnfulfilledMessager2", true);
            MeetHunter_Talk1 = new CustomAction("NSHMeetHunter_Talk1", true);
            MeetHunter_Talk2 = new CustomAction("NSHMeetHunter_Talk2", true);
            MeetHunter_Talk3 = new CustomAction("NSHMeetHunter_Talk3", true);
            MeetHunter_Talk3_Wait = new CustomAction("NSHMeetHunter_Talk3_Wait", true);
            LetSlugcatStayAwayFromSwarmer = new CustomAction("LetSlugcatStayAwayFromSwarmer", true);
            LetSlugcatReleaseSwarmer = new CustomAction("LetSlugcatReleaseSwarmer", true);
            //饕餮
            MeetGourmand_Talk1 = new CustomAction("NSHMeetGourmand_Talk1", true);
            MeetGourmand_Talk2 = new CustomAction("NSHMeetGourmand_Talk2", true);
            MeetGourmand_Talk3 = new CustomAction("NSHMeetGourmand_Talk3", true);
            //求生者
            MeetWhite_Talk1 = new CustomAction("NSHMeetWhite_Talk1", true);
            MeetWhite_Talk2 = new CustomAction("NSHMeetWhite_Talk2", true);
            MeetWhite_Talk3 = new CustomAction("NSHMeetWhite_Talk3", true);
            //僧侣
            MeetYellow_Talk1 = new CustomAction("NSHMeetYellow_Talk1", true);
            MeetYellow_Talk2 = new CustomAction("NSHMeetYellow_Talk2", true);
            MeetYellow_Talk3 = new CustomAction("NSHMeetYellow_Talk3", true);
            //溪流
            MeetRivulet_Talk1 = new CustomAction("NSHMeetRivulet_Talk1", true);
            MeetRivulet_Talk2 = new CustomAction("NSHMeetRivulet_Talk2", true);
            MeetRivulet_Talk3 = new CustomAction("NSHMeetRivulet_Talk3", true);
            MeetRivulet_AfterAltEnd_1 = new CustomAction("MeetRivulet_AfterAltEnd_1", true);
            MeetRivulet_AfterAltEnd_2 = new CustomAction("MeetRivulet_AfterAltEnd_2", true);
            MeetRivulet_AfterAltEnd_3 = new CustomAction("MeetRivulet_AfterAltEnd_3", true);
            //圣徒
            MeetSaint_Idle = new CustomAction("NSHMeetSaint_Idle", true);
            MeetSaint_Talk1 = new CustomAction("NSHMeetSaint_Talk1", true);
            MeetSaint_Talk2 = new CustomAction("NSHMeetSaint_Talk2", true);
            MeetSaint_Talk3 = new CustomAction("NSHMeetSaint_Talk3", true);
            //怪猫
            MeetSofanthiel_Idle = new CustomAction("NSHMeetSofanthiel_Idle", true);
            MeetSofanthiel_Talk1 = new CustomAction("NSHMeetSofanthiel_Talk1", true);
            MeetSofanthiel_Talk2 = new CustomAction("NSHMeetSofanthiel_Talk2", true);
            //其他蛞蝓猫
            MeetOtherSlugcat_Talk1 = new CustomAction("NSHMeetOtherSlugcat_Talk1", true);
            MeetOtherSlugcat_Talk2 = new CustomAction("NSHMeetOtherSlugcat_Talk2", true);
        }

        public static void UnregisterValues()
        {
            //总览
            HunterExpansionEnums.Unregister(MeetHunter_Init);
            HunterExpansionEnums.Unregister(MeetSpear_Init);
            HunterExpansionEnums.Unregister(MeetArtificer_Init);
            HunterExpansionEnums.Unregister(MeetGourmand_Init);
            HunterExpansionEnums.Unregister(MeetWhite_Init);
            HunterExpansionEnums.Unregister(MeetYellow_Init);
            HunterExpansionEnums.Unregister(MeetRivulet_Init);
            HunterExpansionEnums.Unregister(MeetSaint_Init);
            HunterExpansionEnums.Unregister(MeetSofanthiel_Init);
            HunterExpansionEnums.Unregister(MeetOtherSlugcat_Init);
            HunterExpansionEnums.Unregister(MeetOracle_Init);
            HunterExpansionEnums.Unregister(Rubicon_Init);
            HunterExpansionEnums.Unregister(General_Meditate);
            //矛大师
            HunterExpansionEnums.Unregister(MeetSpear_Talk1);
            HunterExpansionEnums.Unregister(MeetSpear_Talk2);
            HunterExpansionEnums.Unregister(MeetSpear_Talk3);
            HunterExpansionEnums.Unregister(MeetSpear_AfterSpearMeetPebbles_1);
            HunterExpansionEnums.Unregister(MeetSpear_AfterSpearMeetPebbles_2);
            HunterExpansionEnums.Unregister(MeetSpear_AfterAltEnd_1);
            HunterExpansionEnums.Unregister(MeetSpear_AfterAltEnd_2);
            HunterExpansionEnums.Unregister(MeetSpear_AfterAltEnd_3);
            //工匠
            HunterExpansionEnums.Unregister(MeetArtificer_Talk1);
            HunterExpansionEnums.Unregister(MeetArtificer_Talk2);
            //猎手
            HunterExpansionEnums.Unregister(MeetHunter_GiveMark);
            HunterExpansionEnums.Unregister(MeetHunter_TalkAfterGiveMark);
            HunterExpansionEnums.Unregister(MeetHunter_RunIntoHunter);
            HunterExpansionEnums.Unregister(MeetHunter_RunIntoTalk);
            HunterExpansionEnums.Unregister(MeetHunter_FarewellTalk);
            HunterExpansionEnums.Unregister(MeetHunter_Praise);
            HunterExpansionEnums.Unregister(MeetHunter_UnfulfilledMessager1);
            HunterExpansionEnums.Unregister(MeetHunter_UnfulfilledMessager2);
            HunterExpansionEnums.Unregister(MeetHunter_Talk1);
            HunterExpansionEnums.Unregister(MeetHunter_Talk2);
            HunterExpansionEnums.Unregister(MeetHunter_Talk3);
            HunterExpansionEnums.Unregister(MeetHunter_Talk3_Wait);
            HunterExpansionEnums.Unregister(LetSlugcatStayAwayFromSwarmer);
            HunterExpansionEnums.Unregister(LetSlugcatReleaseSwarmer);
            //饕餮
            HunterExpansionEnums.Unregister(MeetGourmand_Talk1);
            HunterExpansionEnums.Unregister(MeetGourmand_Talk2);
            HunterExpansionEnums.Unregister(MeetGourmand_Talk3);
            //求生者
            HunterExpansionEnums.Unregister(MeetWhite_Talk1);
            HunterExpansionEnums.Unregister(MeetWhite_Talk2);
            HunterExpansionEnums.Unregister(MeetWhite_Talk3);
            //僧侣
            HunterExpansionEnums.Unregister(MeetYellow_Talk1);
            HunterExpansionEnums.Unregister(MeetYellow_Talk2);
            HunterExpansionEnums.Unregister(MeetYellow_Talk3);
            //溪流
            HunterExpansionEnums.Unregister(MeetRivulet_Talk1);
            HunterExpansionEnums.Unregister(MeetRivulet_Talk2);
            HunterExpansionEnums.Unregister(MeetRivulet_Talk3);
            HunterExpansionEnums.Unregister(MeetRivulet_AfterAltEnd_1);
            HunterExpansionEnums.Unregister(MeetRivulet_AfterAltEnd_2);
            HunterExpansionEnums.Unregister(MeetRivulet_AfterAltEnd_3);
            //圣徒
            HunterExpansionEnums.Unregister(MeetSaint_Idle);
            HunterExpansionEnums.Unregister(MeetSaint_Talk1);
            HunterExpansionEnums.Unregister(MeetSaint_Talk2);
            HunterExpansionEnums.Unregister(MeetSaint_Talk3);
            //怪猫
            HunterExpansionEnums.Unregister(MeetSofanthiel_Idle);
            HunterExpansionEnums.Unregister(MeetSofanthiel_Talk1);
            HunterExpansionEnums.Unregister(MeetSofanthiel_Talk2);
            //其他蛞蝓猫
            HunterExpansionEnums.Unregister(MeetOtherSlugcat_Talk1);
            HunterExpansionEnums.Unregister(MeetOtherSlugcat_Talk2);
        }

        //总览
        public static CustomAction MeetHunter_Init;
        public static CustomAction MeetSpear_Init;
        public static CustomAction MeetArtificer_Init;
        public static CustomAction MeetGourmand_Init;
        public static CustomAction MeetWhite_Init;
        public static CustomAction MeetYellow_Init;
        public static CustomAction MeetRivulet_Init;
        public static CustomAction MeetSaint_Init;
        public static CustomAction MeetSofanthiel_Init;
        public static CustomAction MeetOtherSlugcat_Init;
        public static CustomAction MeetOracle_Init;
        //魔方节点
        public static CustomAction Rubicon_Init;
        //闲置
        public static CustomAction General_Meditate;
        //矛大师
        public static CustomAction MeetSpear_Talk1;
        public static CustomAction MeetSpear_Talk2;
        public static CustomAction MeetSpear_Talk3;
        public static CustomAction MeetSpear_AfterSpearMeetPebbles_1;
        public static CustomAction MeetSpear_AfterSpearMeetPebbles_2;
        public static CustomAction MeetSpear_AfterAltEnd_1;
        public static CustomAction MeetSpear_AfterAltEnd_2;
        public static CustomAction MeetSpear_AfterAltEnd_3;
        //工匠
        public static CustomAction MeetArtificer_Talk1;
        public static CustomAction MeetArtificer_Talk2;
        //猎手
        public static CustomAction MeetHunter_GiveMark;//第一场梦境
        public static CustomAction MeetHunter_TalkAfterGiveMark;//第一场梦境
        public static CustomAction MeetHunter_RunIntoHunter;//第二场梦境
        public static CustomAction MeetHunter_RunIntoTalk;//第二场梦境
        public static CustomAction MeetHunter_FarewellTalk;//第四场梦境
        public static CustomAction MeetHunter_Praise;//美梦
        public static CustomAction MeetHunter_UnfulfilledMessager1;
        public static CustomAction MeetHunter_UnfulfilledMessager2;
        public static CustomAction MeetHunter_Talk1;
        public static CustomAction MeetHunter_Talk2;
        public static CustomAction MeetHunter_Talk3;
        public static CustomAction MeetHunter_Talk3_Wait;
        public static CustomAction LetSlugcatStayAwayFromSwarmer;
        public static CustomAction LetSlugcatReleaseSwarmer;
        //饕餮
        public static CustomAction MeetGourmand_Talk1;
        public static CustomAction MeetGourmand_Talk2;
        public static CustomAction MeetGourmand_Talk3;
        //求生者
        public static CustomAction MeetWhite_Talk1;
        public static CustomAction MeetWhite_Talk2;
        public static CustomAction MeetWhite_Talk3;
        //僧侣
        public static CustomAction MeetYellow_Talk1;
        public static CustomAction MeetYellow_Talk2;
        public static CustomAction MeetYellow_Talk3;
        //溪流
        public static CustomAction MeetRivulet_Talk1;
        public static CustomAction MeetRivulet_Talk2;
        public static CustomAction MeetRivulet_Talk3;
        public static CustomAction MeetRivulet_AfterAltEnd_1;
        public static CustomAction MeetRivulet_AfterAltEnd_2;
        public static CustomAction MeetRivulet_AfterAltEnd_3;
        //圣徒
        public static CustomAction MeetSaint_Idle;
        public static CustomAction MeetSaint_Talk1;
        public static CustomAction MeetSaint_Talk2;
        public static CustomAction MeetSaint_Talk3;
        //
        public static CustomAction MeetSofanthiel_Idle;
        public static CustomAction MeetSofanthiel_Talk1;
        public static CustomAction MeetSofanthiel_Talk2;
        //其他蛞蝓猫
        public static CustomAction MeetOtherSlugcat_Talk1;
        public static CustomAction MeetOtherSlugcat_Talk2;
    }

    public class NSHOracleBehaviorSubBehavID
    {
        public static void RegisterValues()
        {
            MeetHunter = new CustomSubBehaviour.CustomSubBehaviourID("NSHMeetHunter", true);
            MeetSpear = new CustomSubBehaviour.CustomSubBehaviourID("NSHMeetSpear", true);
            MeetArtificer = new CustomSubBehaviour.CustomSubBehaviourID("NSHMeetArtificer", true);
            MeetGourmand = new CustomSubBehaviour.CustomSubBehaviourID("NSHMeetGourmand", true);
            MeetWhite = new CustomSubBehaviour.CustomSubBehaviourID("NSHMeetWhite", true);
            MeetYellow = new CustomSubBehaviour.CustomSubBehaviourID("NSHMeetYellow", true);
            MeetRivulet = new CustomSubBehaviour.CustomSubBehaviourID("NSHMeetRivulet", true);
            MeetSaint = new CustomSubBehaviour.CustomSubBehaviourID("NSHMeetSaint", true);
            MeetSofanthiel = new CustomSubBehaviour.CustomSubBehaviourID("NSHMeetSofanthiel", true);
            MeetOtherSlugcat = new CustomSubBehaviour.CustomSubBehaviourID("NSHMeetOtherSlugcat", true);
            MeetOracle = new CustomSubBehaviour.CustomSubBehaviourID("NSHMeetOracle", true);
            Rubicon = new CustomSubBehaviour.CustomSubBehaviourID("NSHInRubicon", true);
        }

        public static void UnregisterValues()
        {
            HunterExpansionEnums.Unregister(MeetHunter);
            HunterExpansionEnums.Unregister(MeetSpear);
            HunterExpansionEnums.Unregister(MeetArtificer);
            HunterExpansionEnums.Unregister(MeetGourmand);
            HunterExpansionEnums.Unregister(MeetWhite);
            HunterExpansionEnums.Unregister(MeetYellow);
            HunterExpansionEnums.Unregister(MeetRivulet);
            HunterExpansionEnums.Unregister(MeetSaint);
            HunterExpansionEnums.Unregister(MeetSofanthiel);
            HunterExpansionEnums.Unregister(MeetOtherSlugcat);
            HunterExpansionEnums.Unregister(MeetOracle);
            HunterExpansionEnums.Unregister(Rubicon);
        }

        public static CustomSubBehaviour.CustomSubBehaviourID MeetHunter;
        public static CustomSubBehaviour.CustomSubBehaviourID MeetSpear;
        public static CustomSubBehaviour.CustomSubBehaviourID MeetArtificer;
        public static CustomSubBehaviour.CustomSubBehaviourID MeetGourmand;
        public static CustomSubBehaviour.CustomSubBehaviourID MeetWhite;
        public static CustomSubBehaviour.CustomSubBehaviourID MeetYellow;
        public static CustomSubBehaviour.CustomSubBehaviourID MeetRivulet;
        public static CustomSubBehaviour.CustomSubBehaviourID MeetSaint;
        public static CustomSubBehaviour.CustomSubBehaviourID MeetSofanthiel;
        public static CustomSubBehaviour.CustomSubBehaviourID MeetOtherSlugcat;
        public static CustomSubBehaviour.CustomSubBehaviourID MeetOracle;
        public static CustomSubBehaviour.CustomSubBehaviourID Rubicon;
    }

    public class NSHConversationID
    {
        public static void RegisterValues()
        {
            //魔方节点
            NSH_HR = new Conversation.ID("NSH_HR", true);
            NSH_Moon_Pebbles_SRS_HR = new Conversation.ID("NSH_Moon_Pebbles_SRS_HR", true);
            //矛大师
            Spear_Talk0 = new Conversation.ID("NSH_Spear_Talk0", true);
            Spear_Talk1 = new Conversation.ID("NSH_Spear_Talk1", true);
            Spear_Talk2 = new Conversation.ID("NSH_Spear_Talk2", true);
            Spear_AfterMeetPebbles_0 = new Conversation.ID("NSH_Spear_AfterMeetPebbles_0", true);
            Spear_AfterMeetPebbles_1 = new Conversation.ID("NSH_Spear_AfterMeetPebbles_1", true);
            Spear_AfterAltEnd_0 = new Conversation.ID("NSH_Spear_AfterAltEnd_0", true);
            Spear_AfterAltEnd_1 = new Conversation.ID("NSH_Spear_AfterAltEnd_1", true);
            Spear_AfterAltEnd_2 = new Conversation.ID("NSH_Spear_AfterAltEnd_2", true);
            //工匠
            Artificer_Talk0 = new Conversation.ID("NSH_Artificer_Talk0", true);
            Artificer_Talk1 = new Conversation.ID("NSH_Artificer_Talk1", true);
            //猎手
            Hunter_DreamTalk0 = new Conversation.ID("NSH_Hunter_DreamTalk0", true);//梦境对话
            Hunter_DreamTalk1 = new Conversation.ID("NSH_Hunter_DreamTalk1", true);
            Hunter_DreamTalk3 = new Conversation.ID("NSH_Hunter_DreamTalk3", true);
            Hunter_DreamTalk5 = new Conversation.ID("NSH_Hunter_DreamTalk5", true);
            Hunter_UnfulfilledMessager0 = new Conversation.ID("NSH_Hunter_UnfulfilledMessager0", true);
            Hunter_UnfulfilledMessager1 = new Conversation.ID("NSH_Hunter_UnfulfilledMessager1", true);
            Hunter_Talk0 = new Conversation.ID("NSH_Hunter_Talk0", true);//现实对话
            Hunter_Talk1 = new Conversation.ID("NSH_Hunter_Talk1", true);
            Hunter_Talk2 = new Conversation.ID("NSH_Hunter_Talk2", true);
            Hunter_Talk2_Wait = new Conversation.ID("NSH_Hunter_Talk2_Wait", true);
            WarnSlugcatStayAwayFromSwarmer = new Conversation.ID("NSH_WarnSlugcatStayAwayFromSwarmer", true);//特殊对话
            WarnSlugcatReleaseSwarmer = new Conversation.ID("NSH_WarnSlugcatReleaseSwarmer", true);
            //饕餮
            Gourmand_Talk0 = new Conversation.ID("NSH_Gourmand_Talk0", true);
            Gourmand_Talk1 = new Conversation.ID("NSH_Gourmand_Talk1", true);
            Gourmand_Talk2 = new Conversation.ID("NSH_Gourmand_Talk2", true);
            //求生者
            White_Talk0 = new Conversation.ID("NSH_White_Talk0", true);
            White_Talk1 = new Conversation.ID("NSH_White_Talk1", true);
            White_Talk2 = new Conversation.ID("NSH_White_Talk2", true);
            //僧侣
            Yellow_Talk0 = new Conversation.ID("NSH_Yellow_Talk0", true);
            Yellow_Talk1 = new Conversation.ID("NSH_Yellow_Talk1", true);
            Yellow_Talk2 = new Conversation.ID("NSH_Yellow_Talk2", true);
            //溪流
            Rivulet_Talk0 = new Conversation.ID("NSH_Rivulet_Talk0", true);
            Rivulet_Talk1 = new Conversation.ID("NSH_Rivulet_Talk1", true);
            Rivulet_Talk2 = new Conversation.ID("NSH_Rivulet_Talk2", true);
            Rivulet_AfterAltEnd_0 = new Conversation.ID("NSH_Rivulet_AfterAltEnd_0", true);
            Rivulet_AfterAltEnd_1 = new Conversation.ID("NSH_Rivulet_AfterAltEnd_1", true);
            Rivulet_AfterAltEnd_2 = new Conversation.ID("NSH_Rivulet_AfterAltEnd_2", true);
            //圣徒
            Saint_Talk0 = new Conversation.ID("NSH_Saint_Talk0", true);
            Saint_Talk1 = new Conversation.ID("NSH_Saint_Talk1", true);
            Saint_Talk2 = new Conversation.ID("NSH_Saint_Talk2", true);
            //怪猫
            Sofanthiel_Talk0 = new Conversation.ID("NSH_Sofanthiel_Talk0", true);
            Sofanthiel_Talk1 = new Conversation.ID("NSH_Sofanthiel_Talk1", true);
            //其他蛞蝓猫
            OtherSlugcat_Talk0 = new Conversation.ID("NSH_OtherSlugcat_Talk0", true);
            OtherSlugcat_Talk1 = new Conversation.ID("NSH_OtherSlugcat_Talk1", true);
            //对话
            RefusingToInterpretItems = new Conversation.ID("NSH_RefusingToInterpretItems", true);
            //珍珠
            NSH_Pearl_NSH_Box = new Conversation.ID("NSH_Pearl_NSH_Box", true);
            NSH_Pearl_NSH_Top = new Conversation.ID("NSH_Pearl_NSH_Top", true);
        }

        public static void UnregisterValues()
        {
            //魔方节点
            HunterExpansionEnums.Unregister(NSH_HR);
            HunterExpansionEnums.Unregister(NSH_Moon_Pebbles_SRS_HR);
            //矛大师
            HunterExpansionEnums.Unregister(Spear_Talk0);
            HunterExpansionEnums.Unregister(Spear_Talk1);
            HunterExpansionEnums.Unregister(Spear_Talk2);
            HunterExpansionEnums.Unregister(Spear_AfterMeetPebbles_0);
            HunterExpansionEnums.Unregister(Spear_AfterMeetPebbles_1);
            HunterExpansionEnums.Unregister(Spear_AfterAltEnd_0);
            HunterExpansionEnums.Unregister(Spear_AfterAltEnd_1);
            HunterExpansionEnums.Unregister(Spear_AfterAltEnd_2);
            //工匠
            HunterExpansionEnums.Unregister(Artificer_Talk0);
            HunterExpansionEnums.Unregister(Artificer_Talk1);
            //猎手
            HunterExpansionEnums.Unregister(Hunter_DreamTalk0);//梦境对话
            HunterExpansionEnums.Unregister(Hunter_DreamTalk1);
            HunterExpansionEnums.Unregister(Hunter_DreamTalk3);
            HunterExpansionEnums.Unregister(Hunter_DreamTalk5);
            HunterExpansionEnums.Unregister(Hunter_UnfulfilledMessager0);
            HunterExpansionEnums.Unregister(Hunter_UnfulfilledMessager1);
            HunterExpansionEnums.Unregister(Hunter_Talk0);//现实对话
            HunterExpansionEnums.Unregister(Hunter_Talk1);
            HunterExpansionEnums.Unregister(Hunter_Talk2);
            HunterExpansionEnums.Unregister(Hunter_Talk2_Wait);
            HunterExpansionEnums.Unregister(WarnSlugcatStayAwayFromSwarmer);//特殊对话
            HunterExpansionEnums.Unregister(WarnSlugcatReleaseSwarmer);
            //饕餮
            HunterExpansionEnums.Unregister(Gourmand_Talk0);
            HunterExpansionEnums.Unregister(Gourmand_Talk1);
            HunterExpansionEnums.Unregister(Gourmand_Talk2);
            //求生者
            HunterExpansionEnums.Unregister(White_Talk0);
            HunterExpansionEnums.Unregister(White_Talk1);
            HunterExpansionEnums.Unregister(White_Talk2);
            //僧侣
            HunterExpansionEnums.Unregister(Yellow_Talk0);
            HunterExpansionEnums.Unregister(Yellow_Talk1);
            HunterExpansionEnums.Unregister(Yellow_Talk2);
            //溪流
            HunterExpansionEnums.Unregister(Rivulet_Talk0);
            HunterExpansionEnums.Unregister(Rivulet_Talk1);
            HunterExpansionEnums.Unregister(Rivulet_Talk2);
            HunterExpansionEnums.Unregister(Rivulet_AfterAltEnd_0);
            HunterExpansionEnums.Unregister(Rivulet_AfterAltEnd_1);
            HunterExpansionEnums.Unregister(Rivulet_AfterAltEnd_2);
            //圣徒
            HunterExpansionEnums.Unregister(Saint_Talk0);
            HunterExpansionEnums.Unregister(Saint_Talk1);
            HunterExpansionEnums.Unregister(Saint_Talk2);
            //怪猫
            HunterExpansionEnums.Unregister(Sofanthiel_Talk0);
            HunterExpansionEnums.Unregister(Sofanthiel_Talk1);
            //其他蛞蝓猫
            HunterExpansionEnums.Unregister(OtherSlugcat_Talk0);
            HunterExpansionEnums.Unregister(OtherSlugcat_Talk1);
            //对话
            HunterExpansionEnums.Unregister(RefusingToInterpretItems);
            //珍珠
            HunterExpansionEnums.Unregister(NSH_Pearl_NSH_Top);
            HunterExpansionEnums.Unregister(NSH_Pearl_NSH_Box);
        }
        //魔方节点
        public static Conversation.ID NSH_HR;
        public static Conversation.ID NSH_Moon_Pebbles_SRS_HR;
        //矛大师
        public static Conversation.ID Spear_Talk0;
        public static Conversation.ID Spear_Talk1;
        public static Conversation.ID Spear_Talk2;
        public static Conversation.ID Spear_AfterMeetPebbles_0;
        public static Conversation.ID Spear_AfterMeetPebbles_1;
        public static Conversation.ID Spear_AfterAltEnd_0;
        public static Conversation.ID Spear_AfterAltEnd_1;
        public static Conversation.ID Spear_AfterAltEnd_2;
        //工匠
        public static Conversation.ID Artificer_Talk0;
        public static Conversation.ID Artificer_Talk1;
        //猎手
        public static Conversation.ID Hunter_DreamTalk0;//梦境对话
        public static Conversation.ID Hunter_DreamTalk1;
        public static Conversation.ID Hunter_DreamTalk3;
        public static Conversation.ID Hunter_DreamTalk5;
        public static Conversation.ID Hunter_UnfulfilledMessager0;
        public static Conversation.ID Hunter_UnfulfilledMessager1;
        public static Conversation.ID Hunter_Talk0;//现实对话
        public static Conversation.ID Hunter_Talk1;
        public static Conversation.ID Hunter_Talk2;
        public static Conversation.ID Hunter_Talk2_Wait;
        public static Conversation.ID WarnSlugcatStayAwayFromSwarmer;//特殊对话
        public static Conversation.ID WarnSlugcatReleaseSwarmer;
        //饕餮
        public static Conversation.ID Gourmand_Talk0;
        public static Conversation.ID Gourmand_Talk1;
        public static Conversation.ID Gourmand_Talk2;
        //求生者
        public static Conversation.ID White_Talk0;
        public static Conversation.ID White_Talk1;
        public static Conversation.ID White_Talk2;
        //僧侣
        public static Conversation.ID Yellow_Talk0;
        public static Conversation.ID Yellow_Talk1;
        public static Conversation.ID Yellow_Talk2;
        //溪流
        public static Conversation.ID Rivulet_Talk0;
        public static Conversation.ID Rivulet_Talk1;
        public static Conversation.ID Rivulet_Talk2;
        public static Conversation.ID Rivulet_AfterAltEnd_0;
        public static Conversation.ID Rivulet_AfterAltEnd_1;
        public static Conversation.ID Rivulet_AfterAltEnd_2;
        //圣徒
        public static Conversation.ID Saint_Talk0;
        public static Conversation.ID Saint_Talk1;
        public static Conversation.ID Saint_Talk2;
        //怪猫
        public static Conversation.ID Sofanthiel_Talk0;
        public static Conversation.ID Sofanthiel_Talk1;
        //其他蛞蝓猫
        public static Conversation.ID OtherSlugcat_Talk0;
        public static Conversation.ID OtherSlugcat_Talk1;
        //对话
        public static Conversation.ID RefusingToInterpretItems;
        //珍珠
        public static Conversation.ID NSH_Pearl_NSH_Top;
        public static Conversation.ID NSH_Pearl_NSH_Box;
    }

    public class NSHOracleMovementBehavior
    {
        public static void RegisterValues()
        {
            Land = new CustomMovementBehavior("Land", true);
            Meditate = new CustomMovementBehavior("Meditate", true);
        }

        public static void UnregisterValues()
        {
            HunterExpansionEnums.Unregister(Land);
            HunterExpansionEnums.Unregister(Meditate);
        }

        public static CustomMovementBehavior Land;
        public static CustomMovementBehavior Meditate;
    }

    public class NSHMiscItemType
    {
        public static void RegisterValues()
        {
            NSHSwarmer = new SLOracleBehaviorHasMark.MiscItemType("NSHSwarmer", true);
        }

        public static void UnregisterValues()
        {
            HunterExpansionEnums.Unregister(NSHSwarmer);
        }

        public static SLOracleBehaviorHasMark.MiscItemType NSHSwarmer;
    }
}
