using System.Linq;
using UnityEngine;
using static CustomOracleTx.CustomOracleBehaviour;

namespace HunterExpansion.CustomOracle
{
    public class SRSOracleRegistry : CustomOracleTx.CustomOracleTx
    {
        public static Oracle.OracleID SRSOracle = new Oracle.OracleID("SRS", true);
        public static string currentLoadingRoom;

        public string[] roomsToLoad = { "SRS_AI", "HR_AI" };
        public override string LoadRoom
        {
            get
            {
                if (roomsToLoad.Contains(currentLoadingRoom))
                {
                    if (currentLoadingRoom == "HR_AI" && Plugin.ripSRS)
                        return currentLoadingRoom;
                }
                return roomsToLoad[0];
            }
        }
        public override Oracle.OracleID OracleID => SRSOracle;
        public override Oracle.OracleID InheritOracleID => Oracle.OracleID.SS;

        public SRSOracleRegistry()
        {
            gravity = 1f;
            startPos = new Vector2(600f, 450f);
        }

        public override void LoadBehaviourAndSurroundings(ref Oracle oracle, Room room)
        {
            base.LoadBehaviourAndSurroundings(ref oracle, room);

            oracle.oracleBehavior = new SRSOracleBehaviour(oracle);

            oracle.arm = new Oracle.OracleArm(oracle);
            room.gravity = 0f;
            startPos = (LoadRoom != "HR_AI") ? new Vector2(400f, 110f) : new Vector2(300f, 200f);

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
            return new SRSOracleGraphics(ow);
        }
    }

    public class SRSOracleColor
    {
        public static readonly Color Yellow = new Color(246f / 255f, 184f / 255f, 100f / 255f);
        public static readonly Color LightYellow = new Color(255f / 255f, 222f / 255f, 151f / 255f);
        public static readonly Color Orange = new Color(255f / 255f, 149f / 255f, 112f / 255f);
        public static readonly Color OrangeRed = new Color(211f / 255f, 82f / 255f, 59f / 255f);
        public static readonly Color Red = new Color(170f / 255f, 16f / 255f, 7f / 255f);

        public static readonly Color Violet = new Color(106f / 255f, 62f / 255f, 94f / 255f);
        public static readonly Color Brown = new Color(120f / 255f, 58f / 255f, 73f / 255f);

        public static readonly Color Rose = new Color(255f / 255f, 67f / 255f, 115f / 255f);
        public static readonly Color Blue = new Color(40f / 255f, 102f / 255f, 141f / 255f);
    }

    public class SRSOracleBehaviorAction
    {
        public static void RegisterValues()
        {
            //总览
            Rubicon_Init = new CustomAction("SRSInRubicon_Init", true);
        }

        public static void UnregisterValues()
        {
            //总览
            HunterExpansionEnums.Unregister(Rubicon_Init);
        }

        //魔方节点
        public static CustomAction Rubicon_Init;
    }

    public class SRSOracleBehaviorSubBehavID
    {
        public static void RegisterValues()
        {
            Rubicon = new CustomSubBehaviour.CustomSubBehaviourID("SRSInRubicon", true);
        }

        public static void UnregisterValues()
        {
            HunterExpansionEnums.Unregister(Rubicon);
        }

        public static CustomSubBehaviour.CustomSubBehaviourID Rubicon;
    }
}
