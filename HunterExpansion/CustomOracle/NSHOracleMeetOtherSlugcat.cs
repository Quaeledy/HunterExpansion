using MoreSlugcats;
using System.IO;
using static CustomOracleTx.CustomOracleBehaviour;

namespace HunterExpansion.CustomOracle
{
    public class NSHOracleMeetOtherSlugcat : NSHConversationBehaviour
    {
        public NSHOracleMeetOtherSlugcat(NSHOracleBehaviour owner) : base(owner)
        {
        }

        public static bool SubBehaviourIsMeetOtherSlugcat(CustomAction nextAction)
        {
            return nextAction == NSHOracleBehaviorAction.MeetOtherSlugcat_Init ||
                   nextAction == NSHOracleBehaviorAction.MeetOtherSlugcat_Talk1 ||
                   nextAction == NSHOracleBehaviorAction.MeetOtherSlugcat_Talk2;
        }

        public override void Update()
        {
            base.Update();
            if (player == null || oracle.room == null || !(oracle.room.world.game.session is StoryGameSession))
                return;
            if (oracle.room.world.game.session.characterStats.name == SlugcatStats.Name.White ||
                oracle.room.world.game.session.characterStats.name == SlugcatStats.Name.Yellow ||
                oracle.room.world.game.session.characterStats.name == SlugcatStats.Name.Red ||
                oracle.room.world.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Spear ||
                oracle.room.world.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Artificer ||
                oracle.room.world.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Gourmand ||
                oracle.room.world.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Rivulet ||
                oracle.room.world.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Saint ||
                oracle.room.world.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
            {
                return;
            }

            if (action == NSHOracleBehaviorAction.MeetOtherSlugcat_Init)
            {
                movementBehavior = CustomMovementBehavior.KeepDistance;
                //现实行为
                NSHOracleState state = (this.owner as NSHOracleBehaviour).State;
                if (state.playerEncountersWithMark == 0 && inActionCounter > 20)
                {
                    owner.NewAction(NSHOracleBehaviorAction.MeetOtherSlugcat_Talk1);
                    (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                    return;
                }
                else if (state.playerEncountersWithMark >= 1 && inActionCounter > 20)
                {
                    owner.NewAction(NSHOracleBehaviorAction.MeetOtherSlugcat_Talk2);
                    (owner as NSHOracleBehaviour).PlayerEncountersAdd();
                    return;
                }
                return;
            }
            //现实对话
            else if (action == NSHOracleBehaviorAction.MeetOtherSlugcat_Talk1 ||
                     action == NSHOracleBehaviorAction.MeetOtherSlugcat_Talk2)
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
            if (newAction == NSHOracleBehaviorAction.MeetOtherSlugcat_Talk1)
            {
                owner.getToWorking = 0f;
                movementBehavior = CustomMovementBehavior.KeepDistance;
                owner.InitateConversation(NSHConversationID.OtherSlugcat_Talk0, this);
            }
            else if (newAction == NSHOracleBehaviorAction.MeetOtherSlugcat_Talk2)
            {
                owner.getToWorking = 0f;
                movementBehavior = CustomMovementBehavior.KeepDistance;
                owner.InitateConversation(NSHConversationID.OtherSlugcat_Talk1, this);
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

        //与其他蛞蝓猫的所有对话
        public void AddConversationEvents(NSHConversation conv, Conversation.ID id)
        {
            NSHOracleState state = (this.owner as NSHOracleBehaviour).State;
            string name = oracle.room.world.game.session.characterStats.name.value;
            int suffix = state.playerEncountersWithMark;
            Plugin.Log("Should Find Events File: " + "0-" + name + "-" + suffix.ToString());
            //如果找不到指定文件
            if (TryFindEventsFile(conv, 0, "NSH", name + "-" + suffix.ToString()))
            {
                Plugin.Log("Load Events File: " + "0-" + name + "-" + suffix.ToString());
            }
            //有对应蛞蝓猫，没有对应编号
            else if (TryFindEventsFile(conv, 0, "NSH", name + "-" + "0") &&
                !TryFindEventsFile(conv, 0, "NSH", name + "-" + suffix.ToString()))
            {
                //则找到编号最大的同蛞蝓猫名文件
                for (int i = 1; ; i++)
                {
                    if (!TryFindEventsFile(conv, 0, "NSH", name + "-" + i.ToString()))
                    {
                        suffix = i - 1;
                        break;
                    }
                }
            }
            //没有对应蛞蝓猫，有对应编号
            else if (!TryFindEventsFile(conv, 0, "NSH", name + "-" + "0") &&
                      TryFindEventsFile(conv, 0, "NSH", suffix.ToString()))
            {
                name = "";
            }
            //既没有对应蛞蝓猫，也没有对应编号
            else
            {
                //则找到编号最大的无名文件
                for (int i = 1; ; i++)
                {
                    if (!TryFindEventsFile(conv, 0, "NSH", i.ToString()))
                    {
                        suffix = i - 1;
                        break;
                    }
                }
            }
            NSHConversation.LoadEventsFromFile(conv, 0, name + "-" + suffix.ToString());
        }


        public static bool TryFindEventsFile(Conversation self, int fileName, string subfolderName, string suffix = null, bool oneRandomLine = false, int randomSeed = 0)
        {
            Plugin.Log("~~~TRY FIND " + subfolderName + Path.DirectorySeparatorChar.ToString() + fileName.ToString() + (suffix == null ? "" : "-" + suffix.ToString()));
            InGameTranslator.LanguageID languageID = self.interfaceOwner.rainWorld.inGameTranslator.currentLanguage;
            string text;
            for (; ; )
            {
                text = AssetManager.ResolveFilePath(self.interfaceOwner.rainWorld.inGameTranslator.SpecificTextFolderDirectory(languageID) +
                       Path.DirectorySeparatorChar.ToString() + subfolderName +
                       Path.DirectorySeparatorChar.ToString() + fileName.ToString() + ".txt");
                if (suffix != null)
                {
                    string text2 = text;
                    text = AssetManager.ResolveFilePath(string.Concat(new string[]
                    {
                    self.interfaceOwner.rainWorld.inGameTranslator.SpecificTextFolderDirectory(languageID),
                    Path.DirectorySeparatorChar.ToString(),
                    subfolderName,
                    Path.DirectorySeparatorChar.ToString(),
                    fileName.ToString(),
                    "-",
                    suffix,
                    ".txt"
                    }));
                    if (!File.Exists(text))
                    {
                        Plugin.Log("NOT FOUND " + text);
                        text = text2;
                    }
                }
                if (File.Exists(text))
                {
                    Plugin.Log("~~~TRY FIND " + subfolderName + Path.DirectorySeparatorChar.ToString() + fileName.ToString() + (suffix == null ? "" : "-" + suffix.ToString()) + ": " + "true");
                    return true;
                }
                Plugin.Log("NOT FOUND " + text);
                if (!(languageID != InGameTranslator.LanguageID.English))
                {
                    break;
                }
                Plugin.Log("RETRY WITH ENGLISH");
                languageID = InGameTranslator.LanguageID.English;
            }
            Plugin.Log("~~~TRY FIND " + subfolderName + Path.DirectorySeparatorChar.ToString() + fileName.ToString() + (suffix == null ? "" : "-" + suffix.ToString()) + ": " + "false");
            return false;
        }
    }
}
