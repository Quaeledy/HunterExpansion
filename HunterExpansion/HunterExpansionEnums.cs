using HunterExpansion.CustomOracle;
using HunterExpansion.CustomEnding;
using HunterExpansion.CustomDream;

namespace HunterExpansion
{
    public class HunterExpansionEnums
    {
        public static bool registed;

        public static void RegisterAllEnumExtensions()
        {
            bool flag = registed;
            if (!flag)
            {
                NSHOracleSoundID.RegisterValues();
                NSHOracleBehaviorAction.RegisterValues();
                NSHOracleBehaviorSubBehavID.RegisterValues();
                NSHConversationID.RegisterValues();
                NSHOracleMovementBehavior.RegisterValues();
                NSHMiscItemType.RegisterValues();

                SlideShowID.RegisterValues();
                MenuSceneID.RegisterValues();

                DreamID.RegisterValues();

                GateRequirement.RegisterValues();

                //附赠的SRS
                SRSOracleBehaviorAction.RegisterValues();
                SRSOracleBehaviorSubBehavID.RegisterValues() ;

                registed = true;
            }

        }

        public static void UnregisterAllEnumExtensions()
        {
            bool flag = registed;
            if (flag)
            {
                NSHOracleSoundID.UnregisterValues();
                NSHOracleBehaviorAction.UnregisterValues();
                NSHOracleBehaviorSubBehavID.UnregisterValues();
                NSHConversationID.UnregisterValues();
                NSHOracleMovementBehavior.UnregisterValues();
                NSHMiscItemType.UnregisterValues();

                SlideShowID.UnregisterValues();
                MenuSceneID.UnregisterValues();

                DreamID.UnregisterValues();

                GateRequirement.UnregisterValues();

                //附赠的SRS
                SRSOracleBehaviorAction.UnregisterValues();
                SRSOracleBehaviorSubBehavID.UnregisterValues();

                registed = false;
            }
        }

        public static void Unregister<T>(ExtEnum<T> extEnum) where T : ExtEnum<T>
        {
            if (extEnum != null)
            {
                extEnum.Unregister();
            }
        }

        public class GateRequirement
        {
            public static void RegisterValues()
            {
                NSHLock = new RegionGate.GateRequirement("NSH", true);
            }

            public static void UnregisterValues()
            {
                HunterExpansionEnums.Unregister(NSHLock);
            }

            public static RegionGate.GateRequirement NSHLock;
        }
    }
}
