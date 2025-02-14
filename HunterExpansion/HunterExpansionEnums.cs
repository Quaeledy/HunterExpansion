using HunterExpansion.CustomDream;
using HunterExpansion.CustomEnding;
using HunterExpansion.CustomOracle;

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

                AbstractObjectType.RegisterValues();

                SlideShowID.RegisterValues();
                MenuSceneID.RegisterValues();

                DreamID.RegisterValues();

                GateRequirement.RegisterValues();

                //附赠的SRS
                SRSOracleBehaviorAction.RegisterValues();
                SRSOracleBehaviorSubBehavID.RegisterValues();

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

                AbstractObjectType.UnregisterValues();

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

            public static bool customNSHGateRequirements(RegionGate self)
            {
                if (!ModManager.MSC)
                {
                    return false;
                }
                //只有在剧情模式，并且不是红猫，NSH的外缘业力门才会开启
                if ((self.room.game.session is StoryGameSession) && (self.room.game.session as StoryGameSession).saveStateNumber != Plugin.SlugName)
                {
                    return true;
                }
                return false;
            }
            public static bool customNSHGateRequirements(SaveState saveState)
            {
                if (!ModManager.MSC)
                {
                    return false;
                }
                //只有在剧情模式（这里不判断了），并且不是红猫，NSH的外缘业力门才会开启
                if (saveState.saveStateNumber != Plugin.SlugName)
                {
                    return true;
                }
                return false;
            }

            public static RegionGate.GateRequirement NSHLock;
        }

        public class AbstractObjectType
        {
            public static void RegisterValues()
            {
                NSHOracleSwarmer = new AbstractPhysicalObject.AbstractObjectType("NSHOracleSwarmer", true);
            }

            public static void UnregisterValues()
            {
                HunterExpansionEnums.Unregister(NSHOracleSwarmer);
            }

            public static AbstractPhysicalObject.AbstractObjectType NSHOracleSwarmer;
        }
    }
}
