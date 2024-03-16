using System;
using static CustomOracleTx.CustomOracleBehaviour;
using HUD;
using MoreSlugcats;
using CustomOracleTx;
using CustomDreamTx;
using Harmony;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using RWCustom;
using HunterExpansion.CustomSave;
using HunterExpansion.CustomDream;
using static System.Net.Mime.MediaTypeNames;

namespace HunterExpansion.CustomOracle
{
    public class NSHConversation : Conversation
    {
        public NSHOracleState State
        {
            get
            {
                if (this.owner is NSHOracleBehaviour)
                {
                    return (this.owner as NSHOracleBehaviour).State;
                }
                return NSHOracleStateSave.NSHOracleState;
            }
        }

        public NSHConversation(NSHOracleBehaviour owner, NSHOracleMeetHunter convBehav, Conversation.ID id, DialogBox dialogBox, SLOracleBehaviorHasMark.MiscItemType describeItem) : base(owner, id, dialogBox)
        {
            //EncryptAllDialogue();
            if ((owner as NSHOracleBehaviour).canSlugUnderstandlanguage())
            {
                this.owner = owner;
                this.convBehav = convBehav;
                this.describeItem = describeItem;
                this.currentSaveFile = owner.oracle.room.game.GetStorySession.saveStateNumber;
                this.AddEvents();
            }
            else
            {
                switch (Random.Range(0, 5))
                {
                    case 0:
                        owner.oracle.room.PlaySound(NSHOracleSoundID.NSH_AI_LongDialogue_1, 0f, 1f, 1.25f);
                        break;
                    case 1:
                        owner.oracle.room.PlaySound(NSHOracleSoundID.NSH_AI_LongDialogue_2, 0f, 1f, 1.25f);
                        break;
                    case 2:
                        owner.oracle.room.PlaySound(NSHOracleSoundID.NSH_AI_LongDialogue_3, 0f, 1f, 1.25f);
                        break;
                    case 3:
                        owner.oracle.room.PlaySound(NSHOracleSoundID.NSH_AI_ShortDialogue_1, 0f, 1f, 1.25f);
                        break;
                    case 4:
                        owner.oracle.room.PlaySound(NSHOracleSoundID.NSH_AI_ShortDialogue_2, 0f, 1f, 1.25f);
                        break;
                }
            }
        }
        
        //用于加密文件
        public new void EncryptAllDialogue()
        {
            Plugin.Log("Encrypt all dialogue");
            string modPath = "";
            for (int j = ModManager.ActiveMods.Count - 1; j >= 0; j--)
            {
                if(ModManager.ActiveMods[j].id == Plugin.MOD_ID)
                    modPath = ModManager.ActiveMods[j].path;
            }
            if (modPath == "")
            {
                Plugin.Log("Can not find mod path to encrypt all dialogue");
                return;
            }
            for (int i = 0; i < ExtEnum<InGameTranslator.LanguageID>.values.Count; i++)
            {
                InGameTranslator.LanguageID languageID = InGameTranslator.LanguageID.Parse(i);
                string path = modPath + string.Concat(new string[]
                {
                Path.DirectorySeparatorChar.ToString(),
                "Text",
                Path.DirectorySeparatorChar.ToString(),
                "Text_",
                LocalizationTranslator.LangShort(languageID),
                Path.DirectorySeparatorChar.ToString()
                }).ToLowerInvariant();
                if (Directory.Exists(path))
                {
                    string[] files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
                    for (int j = 0; j < files.Length; j++)
                    {
                        InGameTranslator.EncryptDecryptFile(files[j], true, false);
                    }
                }
            }
        }

        public override void AddEvents()
        {
            //猫猫有语言印记才会读
            if ((owner as NSHOracleBehaviour).canSlugUnderstandlanguage())
            {
                if (this.id == NSHConversationID.RefusingToInterpretItems)
                {
                    this.events.Add(new CustomOracleConversation.PauseAndWaitForStillEvent(this, this.convBehav, 5));
                    this.events.Add(new Conversation.TextEvent(this, 10, this.Translate("..."), 0));
                    return;
                }
                if (this.id == Conversation.ID.Moon_Misc_Item)
                {
                    if (ModManager.MMF && this.owner.isRepeatedDiscussion)
                    {
                        this.events.Add(new Conversation.TextEvent(this, 0, this.owner.AlreadyDiscussedItemString(false), 10));
                    }
                    else
                    {
                        ItemIntro();
                    }
                    if (this.describeItem == SLOracleBehaviorHasMark.MiscItemType.Spear)
                    {
                        if (this.currentSaveFile == Plugin.SlugName)
                        {
                            this.events.Add(new Conversation.TextEvent(this, 10, this.Translate("This is a weapon. We are already quite familiar with it, aren't we?"), 0));
                            return;
                        }
                        else if (ModManager.MSC && this.currentSaveFile == MoreSlugcatsEnums.SlugcatStatsName.Saint)
                        {
                            this.events.Add(new Conversation.TextEvent(this, 10, this.Translate("This is a heavy rebar. If you can't use it, please feel free to leave it here."), 0));
                            return;
                        }
                        else
                        {
                            this.events.Add(new Conversation.TextEvent(this, 10, this.Translate("This is a sharpened rebar. As an apprentice in nature,<LINE>I guess you are not unaware of its purpose."), 0));
                            return;
                        }
                    }
                    if (this.describeItem == SLOracleBehaviorHasMark.MiscItemType.FireSpear)
                    {
                        if (this.currentSaveFile == Plugin.SlugName)
                        {
                            this.events.Add(new Conversation.TextEvent(this, 10, this.Translate("This is an explosive weapon, just throw it like a rebar. Remember<LINE>to keep a certain distance from the explosion point."), 0));
                            this.events.Add(new Conversation.TextEvent(this, 10, this.Translate("However, I guess you have already figured out its usage!"), 0));
                            return;
                        }
                        else
                        {
                            this.events.Add(new Conversation.TextEvent(this, 10, this.Translate("A type of explosive weapon. Be careful, I have seen many creatures<LINE>use it to harm themselves."), 0));
                        }
                    }
                    if (this.describeItem == SLOracleBehaviorHasMark.MiscItemType.Rock)
                    {
                        if (this.currentSaveFile == Plugin.SlugName)
                        {
                            this.events.Add(new Conversation.TextEvent(this, 10, this.Translate("A rock, despite its non-lethality, can still play a unique role in your face of enemies. I think you are already familiar with it~"), 0));
                            return;
                        }
                        else
                        {
                            this.events.Add(new Conversation.TextEvent(this, 10, this.Translate("This is a rock. I seriously suspect that you are trying to use me to compile an encyclopedia,<LINE>or showcase your strange collection addiction."), 0));
                            return;
                        }
                    }
                    if (this.describeItem == SLOracleBehaviorHasMark.MiscItemType.KarmaFlower)
                    {
                        LoadEventsFromFile(25, "NSH");
                        return;
                    }
                    if (this.describeItem == SLOracleBehaviorHasMark.MiscItemType.WaterNut)
                    {
                        this.events.Add(new Conversation.TextEvent(this, 10, this.Translate("This is a plant that swells when exposed to water, and it is edible.<LINE>It is said to have a great taste."), 0));
                        this.events.Add(new Conversation.TextEvent(this, 10, this.Translate("Yes, I can't eat it. Don't show off in front of me~"), 0));
                        return;
                    }
                    if (this.describeItem == SLOracleBehaviorHasMark.MiscItemType.DangleFruit)
                    {
                        LoadEventsFromFile(26, "NSH");
                        return;
                    }
                    if (this.describeItem == SLOracleBehaviorHasMark.MiscItemType.FlareBomb)
                    {
                        LoadEventsFromFile(27, "NSH");
                        return;
                    }
                    if (this.describeItem == SLOracleBehaviorHasMark.MiscItemType.VultureMask)
                    {
                        if (this.currentSaveFile == Plugin.SlugName)
                        {
                            LoadEventsFromFile(28, "NSH-Hunter");
                            return;
                        }
                        else if (this.currentSaveFile == MoreSlugcatsEnums.SlugcatStatsName.Spear)
                        {
                            LoadEventsFromFile(28, "NSH-Spear");
                            return;
                        }
                        else
                        {
                            LoadEventsFromFile(28, "NSH");
                            return;
                        }
                    }
                    if (this.describeItem == SLOracleBehaviorHasMark.MiscItemType.PuffBall)
                    {
                        LoadEventsFromFile(29, "NSH");
                        return;
                    }
                    if (this.describeItem == SLOracleBehaviorHasMark.MiscItemType.JellyFish)
                    {
                        LoadEventsFromFile(30, "NSH");
                        return;
                    }
                    if (this.describeItem == SLOracleBehaviorHasMark.MiscItemType.Lantern)
                    {
                        if (this.currentSaveFile == MoreSlugcatsEnums.SlugcatStatsName.Saint)
                        {
                            LoadEventsFromFile(31, "NSH-Saint");
                            return;
                        }
                        else
                        {
                            LoadEventsFromFile(31, "NSH");
                            return;
                        }
                    }
                    if (this.describeItem == SLOracleBehaviorHasMark.MiscItemType.Mushroom)
                    {
                        LoadEventsFromFile(32, "NSH");
                        return;
                    }
                    if (this.describeItem == SLOracleBehaviorHasMark.MiscItemType.FirecrackerPlant)
                    {
                        LoadEventsFromFile(33, "NSH");
                        return;
                    }
                    if (this.describeItem == SLOracleBehaviorHasMark.MiscItemType.SlimeMold)
                    {
                        LoadEventsFromFile(34, "NSH");
                        return;
                    }
                    if (this.describeItem == SLOracleBehaviorHasMark.MiscItemType.ScavBomb)
                    {
                        LoadEventsFromFile(44, "NSH");
                        return;
                    }
                    if (this.describeItem == NSHMiscItemType.NSHSwarmer)
                    {
                        if (this.currentSaveFile == Plugin.SlugName)
                        {
                            if (this.owner.oracle.room.game.rainWorld.progression.currentSaveState.miscWorldSaveData.SLOracleState.neuronsLeft <= 0)
                                LoadEventsFromFile(50, "NSH-Before");
                            else
                                LoadEventsFromFile(50, "NSH-After");
                        }
                        else
                        {
                            LoadEventsFromFile(46, "NSH");
                        }
                        return;
                    }
                    if (this.describeItem == SLOracleBehaviorHasMark.MiscItemType.OverseerRemains)
                    {
                        LoadEventsFromFile(52, "NSH");
                        return;
                    }
                    if (this.describeItem == SLOracleBehaviorHasMark.MiscItemType.BubbleGrass)
                    {
                        LoadEventsFromFile(53, "NSH");
                        return;
                    }
                    if (ModManager.MSC)
                    {
                        if (this.describeItem == MoreSlugcatsEnums.MiscItemType.EnergyCell)
                        {
                            this.State.shownEnergyCell = true;
                            LoadEventsFromFile(110, "NSH");
                            return;
                        }
                        if (this.describeItem == MoreSlugcatsEnums.MiscItemType.ElectricSpear)
                        {
                            LoadEventsFromFile(112, "NSH");
                            return;
                        }
                        if (this.describeItem == MoreSlugcatsEnums.MiscItemType.InspectorEye)
                        {
                            LoadEventsFromFile(113, "NSH");
                            return;
                        }
                        if (this.describeItem == MoreSlugcatsEnums.MiscItemType.GooieDuck)
                        {
                            LoadEventsFromFile(114, "NSH");
                            return;
                        }
                        if (this.describeItem == MoreSlugcatsEnums.MiscItemType.NeedleEgg)
                        {
                            LoadEventsFromFile(116, "NSH");
                            return;
                        }
                        if (this.describeItem == MoreSlugcatsEnums.MiscItemType.LillyPuck)
                        {
                            LoadEventsFromFile(117, "NSH");
                            return;
                        }
                        if (this.describeItem == MoreSlugcatsEnums.MiscItemType.GlowWeed)
                        {
                            LoadEventsFromFile(118, "NSH");
                            return;
                        }
                        if (this.describeItem == MoreSlugcatsEnums.MiscItemType.DandelionPeach)
                        {
                            LoadEventsFromFile(122, "NSH");
                            return;
                        }
                        if (this.describeItem == MoreSlugcatsEnums.MiscItemType.MoonCloak)
                        {
                            LoadEventsFromFile(123, "NSH");
                            return;
                        }
                        if (this.describeItem == MoreSlugcatsEnums.MiscItemType.SingularityGrenade)
                        {
                            LoadEventsFromFile(127, "NSH");
                            return;
                        }
                        if (this.describeItem == MoreSlugcatsEnums.MiscItemType.EliteMask)
                        {
                            if (this.currentSaveFile == MoreSlugcatsEnums.SlugcatStatsName.Spear ||
                                this.currentSaveFile == MoreSlugcatsEnums.SlugcatStatsName.Artificer ||
                                this.currentSaveFile == SlugcatStats.Name.Red)
                                LoadEventsFromFile(136, "NSH-Before");
                            else
                                LoadEventsFromFile(136, "NSH");
                            return;
                        }
                        if (this.describeItem == MoreSlugcatsEnums.MiscItemType.KingMask)
                        {
                            LoadEventsFromFile(137, "NSH");
                            return;
                        }
                        if (this.describeItem == MoreSlugcatsEnums.MiscItemType.FireEgg)
                        {
                            LoadEventsFromFile(164, "NSH");
                            return;
                        }
                        if (this.describeItem == MoreSlugcatsEnums.MiscItemType.SpearmasterSpear)
                        {
                            LoadEventsFromFile(166, "NSH");
                            return;
                        }
                        if (this.describeItem == MoreSlugcatsEnums.MiscItemType.Seed)
                        {
                            LoadEventsFromFile(167, "NSH");
                            return;
                        }
                    }
                }
                else
                {
                    //NSH区域独有珍珠
                    if (this.id == NSHConversationID.NSH_Pearl_NSH_Top)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(0, "NSH_Top-NSH");
                        return;
                    }
                    if (this.id == NSHConversationID.NSH_Pearl_NSH_Box)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(0, "NSH_Box-NSH");
                        return;
                    }
                    //
                    if (this.id == Conversation.ID.Moon_Pearl_Misc)
                    {
                        this.PearlIntro();
                        this.MiscPearl(false);
                        return;
                    }
                    if (this.id == Conversation.ID.Moon_Pearl_Misc2)
                    {
                        this.PearlIntro();
                        this.MiscPearl(true);
                        return;
                    }
                    if (this.id == Conversation.ID.Moon_Pebbles_Pearl)
                    {
                        this.PebblesPearl();
                        return;
                    }
                    if (this.id == Conversation.ID.Moon_Pearl_CC)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(7, "NSH");
                        return;
                    }
                    if (this.id == Conversation.ID.Moon_Pearl_LF_west)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(10, "NSH");
                        return;
                    }
                    if (this.id == Conversation.ID.Moon_Pearl_LF_bottom)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(11, "NSH");
                        return;
                    }
                    if (this.id == Conversation.ID.Moon_Pearl_HI)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(12, "NSH");
                        return;
                    }
                    if (this.id == Conversation.ID.Moon_Pearl_SH)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(13, "NSH");
                        return;
                    }
                    if (this.id == Conversation.ID.Moon_Pearl_DS)//已写
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(14, "NSH");
                        return;
                    }
                    if (this.id == Conversation.ID.Moon_Pearl_SB_filtration)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(15, "NSH");
                        return;
                    }
                    if (this.id == Conversation.ID.Moon_Pearl_GW)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(16, "NSH");
                        return;
                    }
                    if (this.id == Conversation.ID.Moon_Pearl_SL_bridge)
                    {
                        this.PearlIntro();
                        if (this.owner.oracle.room.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Spear)
                        {
                            LoadEventsFromFile(17, "NSH-Spear");
                        }
                        else
                        {
                            LoadEventsFromFile(17, "NSH");
                        }
                        return;
                    }
                    if (this.id == Conversation.ID.Moon_Pearl_SL_moon)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(18, "NSH");
                        return;
                    }
                    if (this.id == Conversation.ID.Moon_Pearl_SI_west)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(20, "NSH");
                        return;
                    }
                    if (this.id == Conversation.ID.Moon_Pearl_SI_top)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(21, "NSH");
                        return;
                    }
                    if (ModManager.MSC && this.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_SI_chat3)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(22, "NSH");
                        return;
                    }
                    if (ModManager.MSC && this.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_SI_chat4)
                    {
                        this.PearlIntro();
                        if (this.owner.oracle.room.game.IsMoonHeartActive())
                        {
                            LoadEventsFromFile(23, "NSH-After");
                        }
                        else
                        {
                            LoadEventsFromFile(23, "NSH");
                        }
                        return;
                    }
                    if (ModManager.MSC && this.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_SI_chat5)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(24, "NSH");
                        return;
                    }
                    if (this.id == Conversation.ID.Moon_Pearl_SU)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(41, "NSH");
                        return;
                    }
                    if (this.id == Conversation.ID.Moon_Pearl_UW)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(42, "NSH");
                        return;
                    }
                    if (this.id == Conversation.ID.Moon_Pearl_SB_ravine)
                    {
                        this.PearlIntro();
                        if (this.owner.oracle.room.game.rainWorld.progression.currentSaveState.deathPersistentSaveData.altEnding)
                        {
                            LoadEventsFromFile(43, "NSH-SpearAfter");
                        }
                        else if (this.owner.oracle.room.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Spear)
                        {
                            LoadEventsFromFile(43, "NSH-SpearBefore");
                        }
                        else
                        {
                            LoadEventsFromFile(43, "NSH");
                        }
                        return;
                    }
                    if (this.id == Conversation.ID.Moon_Pearl_Red_stomach)
                    {
                        this.PearlIntro();
                        if (this.owner.oracle.room.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Spear ||
                            this.owner.oracle.room.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
                        {
                            LoadEventsFromFile(51, "NSH-Before");
                        }
                        else if (this.owner.oracle.room.game.session.characterStats.name == Plugin.SlugName)
                        {
                            LoadEventsFromFile(51, "NSH-Hunter");
                        }
                        else if (this.owner.oracle.room.game.IsMoonHeartActive())
                        {
                            LoadEventsFromFile(51, "NSH-Final");
                        }
                        else
                        {
                            LoadEventsFromFile(51, "NSH");
                        }
                        return;
                    }
                    if (this.id == Conversation.ID.Moon_Pearl_SL_chimney)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(54, "NSH");
                        if (State.GetOpinion == NSHOracleState.PlayerOpinion.Likes)
                        {
                            this.events.Add(new Conversation.TextEvent(this, 10, this.Translate("I have to say, you are really cute!"), 0));
                        }
                        return;
                    }
                    if (ModManager.MSC && this.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_SU_filt)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(101, "NSH");
                        return;
                    }
                    if (ModManager.MSC && this.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_DM)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(102, "NSH");
                        return;
                    }
                    if (ModManager.MSC && this.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_LC)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(103, "NSH");
                        return;
                    }
                    if (ModManager.MSC && this.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_OE)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(104, "NSH");
                        return;
                    }
                    if (ModManager.MSC && this.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_MS)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(105, "NSH");
                        return;
                    }
                    if (ModManager.MSC && this.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_RM)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(106, "NSH");
                        return;
                    }
                    if (ModManager.MSC && this.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_Rivulet_stomach)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(119, "NSH");
                        return;
                    }
                    if (ModManager.MSC && this.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_LC_second)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(121, "NSH");
                        return;
                    }
                    if (ModManager.MSC && this.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_VS)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(128, "NSH");
                        return;
                    }
                    if (ModManager.MSC && this.id == MoreSlugcatsEnums.ConversationID.Moon_PearlBleaching)//褪色珍珠
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(129);//, "NSH"
                        return;
                    }
                    if (ModManager.MSC && this.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_BroadcastMisc)//广播珍珠
                    {
                        this.PearlIntro();
                        this.BroadcastMisc();
                        return;
                    }
                    if (ModManager.MSC && this.id == MoreSlugcatsEnums.ConversationID.Pebbles_Spearmaster_Read_Pearl)
                    {
                        LoadEventsFromFile(140);//, "NSH"
                        return;
                    }
                    if (ModManager.MSC && this.id == MoreSlugcatsEnums.ConversationID.Moon_Spearmaster_Pearl)
                    {
                        if (this.owner.oracle.room.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Spear)
                        {
                            LoadEventsFromFile(142, "NSH-Spear");
                        }
                        else if (this.owner.oracle.room.game.session.characterStats.name == Plugin.SlugName)
                        {
                            LoadEventsFromFile(142, "NSH-Hunter");
                        }
                        else
                        {
                            LoadEventsFromFile(142, "NSH");
                        }
                        return;
                    }
                }
            }
            else
            {
                (this.owner as NSHOracleBehaviour).PlayerEncountersWithoutMark();
            }
                
        }

        public void ItemIntro()
        {
            switch (this.State.totalPearlsBrought + this.State.miscPearlCounter)
            {
                case 0:
                    this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("What is that?"), 10));
                    return;
                case 1:
                    if (this.State.GetOpinion == NSHOracleState.PlayerOpinion.Likes)
                        this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("It seems that you has found something new again! Let me take a look."), 10));

                    else
                        this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("It seems that you has found something new again. Let me take a look."), 10));
                        return;
                case 2:
                    if (this.State.GetOpinion == NSHOracleState.PlayerOpinion.Likes)
                        this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("Haha, you have developed a habit! Let me take a look."), 10));
                    else
                    {
                        this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("If you continue to be polite, I will explain to you."), 10));
                        this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("I have the right to refuse to entertain impolite guests, are you right?"), 10));
                    }
                    return;
                case 3:
                    if (this.State.GetOpinion == NSHOracleState.PlayerOpinion.Likes)
                        this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("Let's see, what kind of good thing is it?"), 10));
                    else
                        this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("What kind of thing is it?"), 10));
                        return;
                default:
                    switch (Random.Range(0, 8))
                    {
                        case 0:
                            if (this.State.GetOpinion == NSHOracleState.PlayerOpinion.Likes)
                                this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("Welcome! Searching, please wait a moment~"), 10));
                            else
                                this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("You are such a difficult little thing, aren't you? Come on, let me take a look."), 10));
                            break;
                        case 1:
                            if (this.State.GetOpinion == NSHOracleState.PlayerOpinion.Likes)
                                this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("You're really tireless. Come on, let's take a look."), 10));
                            else
                                this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("Let me take a look."), 10));
                            break;
                        case 2:
                            if (this.State.GetOpinion == NSHOracleState.PlayerOpinion.Likes)
                                this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("Let's take a look. Today's little lecture of NSH is about..."), 10));
                            else
                                this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("What is this?"), 10));
                            break;
                        case 3:
                            if (this.State.GetOpinion == NSHOracleState.PlayerOpinion.Likes)
                                this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("Is there another one? Let me take a look."), 10));
                            else
                                this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("Another one? Let me take a look."), 10));
                            break;
                        case 4:
                            if (this.State.GetOpinion == NSHOracleState.PlayerOpinion.Likes)
                                this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("Ah, you're here. Is there anything I want to see?"), 10));
                            else
                                this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("Okay,  I will look at it."), 10));
                            break;
                        case 5:
                            if (this.State.GetOpinion == NSHOracleState.PlayerOpinion.Likes)
                                this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("Welcome back! Show me, <itemPlayerName>."), 10));
                            else
                                this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("Do you want to show it to me?"), 10));
                            break;
                        case 6:
                            this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("Oh, what is that, <ItemPlayerName>?"), 10));
                            break;
                        default:
                            this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("<itemPlayerName>, let me see what you found again?"), 10));
                            break;
                    }
                    break;
            }
        }

        public void PearlIntro()
        {
            switch (this.State.totalPearlsBrought + this.State.miscPearlCounter)
            {
                case 0:
                    this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("Do you want to give it to me? Or just want me to read it?"), 10));
                    this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("Let me take a look at this pearl..."), 10));
                    return;
                case 1:
                    this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("Are you curious about the content in this pearl? Okay, I'm curious~"), 10));
                    return;
                case 2:
                    this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("Another pearl? It seems that you really like them!"), 10));
                    return;
                case 3:
                    this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("Seeing you appear with pearls, I think I already know your purpose for coming here."), 10));
                    if (this.State.GetOpinion == NSHOracleState.PlayerOpinion.Likes)
                    {
                        this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("The scavengers will be jealous of your collection ability!"), 10));
                        return;
                    }
                    break;
                default:
                    switch (Random.Range(0, 5))
                    {
                        case 0:
                            this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("<CapPlayerName>, you would like me to read this?"), 10));
                            break;
                        case 1:
                            this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("Would you like me to read this pearl?"), 10));
                            break;
                        case 2:
                            this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("Are you curious about the content of this pearl, <PlayerName>?"), 10));
                            break;
                        case 3:
                            this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("A pearl! Let me see what's written inside."), 10));
                            break;
                        default:
                            this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("Do you want me to read this pearl for you?"), 10));
                            break;
                    }
                    break;
            }
        }

        private void PebblesPearlIntro()
        {
            switch (Random.Range(0, 5))
            {
                case 0:
                    this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("<CapPlayerName>, do you want me to read this?"), 10));
                    this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("This pearl is still very warm, it seems that it has been used recently."), 10));
                    break;
                case 1:
                    this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("A beautiful pearl... and it was also used recently! Do you want me to read this?"), 10));
                    break;
                case 2:
                    this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("This pearl was used not long ago! I'm curious where you found it."), 10));
                    break;
                case 3:
                    this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("Oh... a warm pearl. Do you want to know what is written here?"), 10));
                    this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("I'm also very curious! Let me read it to you."), 10));
                    break;
                default:
                    this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("A fresh pearl! This is a rare find, let's take a look at what it says."), 10));
                    break;
            }
        }

        private void MiscPearl(bool miscPearl2)
        {
            int i = (this.owner is NSHOracleBehaviour && (this.owner as NSHOracleBehaviour).holdingObject != null) ? (this.owner as NSHOracleBehaviour).holdingObject.abstractPhysicalObject.ID.RandomSeed : Random.Range(0, 100000);
            LoadEventsFromFile(38, "NSH-Hunter", true, i);
            NSHOracleState state = this.State;
            int miscPearlCounter = state.miscPearlCounter;
            state.miscPearlCounter = miscPearlCounter + 1;
        }

        private void BroadcastMisc()
        {
            int i = (this.owner is NSHOracleBehaviour && (this.owner as NSHOracleBehaviour).holdingObject != null) ? ((this.owner as NSHOracleBehaviour).holdingObject.abstractPhysicalObject.ID.RandomSeed) : Random.Range(0, 100000);
            LoadEventsFromFile(132, "NSH-Hunter", true, i);
            this.State.miscPearlCounter++;
        }

        private void PebblesPearl()
        {
            int i = (this.owner is NSHOracleBehaviour && (this.owner as NSHOracleBehaviour).holdingObject != null) ? (this.owner as NSHOracleBehaviour).holdingObject.abstractPhysicalObject.ID.RandomSeed : Random.Range(0, 100000);
            if (this.owner.oracle.room.game.session.characterStats.name == Plugin.SlugName)
            {
                PebblesPearlIntro();
                LoadEventsFromFile(40, "NSH-Hunter", true, i);
            }
            else if (this.owner.oracle.room.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
            {
                this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("This pearl has traces of corruption, although very faint... Well, I can guess who wrote it."), 10));
                this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("I don't want to read this pearl. Seeing his information is no more enjoyable than seeing corruption."), 10));
                this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("If possible, please take it with you when you leave."), 10));
            }
            else if (this.owner.oracle.room.game.session.characterStats.name != MoreSlugcatsEnums.SlugcatStatsName.Rivulet &&
                     this.owner.oracle.room.game.IsMoonHeartActive())
            {
                switch (Random.Range(0, 1))
                {
                    case 0:
                        this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("This pearl has suffered severe corruption before. There's nothing inside now."), 10));
                        break;
                    default:
                        this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("This pearl is even more blank than blank - it has been completely corrupted<LINE>and cannot perform any read or write operations."), 10));
                        break;
                }
            }
            else
            {
                PebblesPearlIntro();
                LoadEventsFromFile(40, "NSH", true, i);
            }
        }

        public void LoadEventsFromFile(int fileName, string suffix)
        {
            Debug.Log("~~~LOAD CONVO " + fileName.ToString());
            InGameTranslator.LanguageID languageID = this.interfaceOwner.rainWorld.inGameTranslator.currentLanguage;
            string text;
            for (; ; )
            {
                text = AssetManager.ResolveFilePath(this.interfaceOwner.rainWorld.inGameTranslator.SpecificTextFolderDirectory(languageID) + Path.DirectorySeparatorChar.ToString() + fileName.ToString() + ".txt");
                if (suffix != null)
                {
                    string text2 = text;
                    text = AssetManager.ResolveFilePath(string.Concat(new string[]
                    {
                    this.interfaceOwner.rainWorld.inGameTranslator.SpecificTextFolderDirectory(languageID),
                    Path.DirectorySeparatorChar.ToString(),
                    fileName.ToString(),
                    "-",
                    suffix,
                    ".txt"
                    }));
                    if (!File.Exists(text))
                    {
                        text = text2;
                    }
                }
                if (File.Exists(text))
                {
                    goto IL_117;
                }
                Debug.Log("NOT FOUND " + text);
                if (!(languageID != InGameTranslator.LanguageID.English))
                {
                    break;
                }
                Debug.Log("RETRY WITH ENGLISH");
                languageID = InGameTranslator.LanguageID.English;
            }
            return;
            IL_117:
            string text3 = File.ReadAllText(text, Encoding.UTF8);
            if (text3[0] != '0')
            {
                text3 = Custom.xorEncrypt(text3, 54 + fileName + (int)this.interfaceOwner.rainWorld.inGameTranslator.currentLanguage * 7);
            }
            string[] array = Regex.Split(text3, "\r\n");
            try
            {
                if (Regex.Split(array[0], "-")[1] == fileName.ToString())
                {
                    for (int j = 1; j < array.Length; j++)
                    {// j是行数
                        string[] array3 = LocalizationTranslator.ConsolidateLineInstructions(ReplaceParts(array[j]));
                        if (array3.Length == 3)
                        {
                            int num;
                            int num2;
                            if (ModManager.MSC && !int.TryParse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture, out num) && int.TryParse(array3[2], NumberStyles.Any, CultureInfo.InvariantCulture, out num2))
                            {
                                this.events.Add(new Conversation.TextEvent(this, int.Parse(array3[0], NumberStyles.Any, CultureInfo.InvariantCulture), array3[1], int.Parse(array3[2], NumberStyles.Any, CultureInfo.InvariantCulture)));
                            }
                            else
                            {
                                this.events.Add(new Conversation.TextEvent(this, int.Parse(array3[0], NumberStyles.Any, CultureInfo.InvariantCulture), array3[2], int.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
                            }
                        }
                        else if (array3.Length == 2)
                        {
                            if (array3[0] == "SPECEVENT")
                            {
                                this.events.Add(new Conversation.SpecialEvent(this, 0, array3[1]));
                            }
                            else if (array3[0] == "PEBBLESWAIT")
                            {
                                this.events.Add(new SSOracleBehavior.PebblesConversation.PauseAndWaitForStillEvent(this, null, int.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
                            }
                        }
                        else if (array3.Length == 1 && array3[0].Length > 0)
                        {
                            this.events.Add(new Conversation.TextEvent(this, 0, array3[0], 0));
                        }
                    }
                }
            }
            catch
            {
                Debug.Log("TEXT ERROR");
                this.events.Add(new Conversation.TextEvent(this, 0, "TEXT ERROR", 100));
            }
        }

        public void LoadEventsFromFile(int fileName, string suffix, bool oneRandomLine, int randomSeed)
        {
            Debug.Log("~~~LOAD CONVO " + fileName.ToString());
            InGameTranslator.LanguageID languageID = this.interfaceOwner.rainWorld.inGameTranslator.currentLanguage;
            string text;
            for (; ; )
            {
                text = AssetManager.ResolveFilePath(this.interfaceOwner.rainWorld.inGameTranslator.SpecificTextFolderDirectory(languageID) + Path.DirectorySeparatorChar.ToString() + fileName.ToString() + ".txt");
                if (suffix != null)
                {
                    string text2 = text;
                    text = AssetManager.ResolveFilePath(string.Concat(new string[]
                    {
                    this.interfaceOwner.rainWorld.inGameTranslator.SpecificTextFolderDirectory(languageID),
                    Path.DirectorySeparatorChar.ToString(),
                    fileName.ToString(),
                    "-",
                    suffix,
                    ".txt"
                    }));
                    if (!File.Exists(text))
                    {
                        text = text2;
                    }
                }
                if (File.Exists(text))
                {
                    goto IL_117;
                }
                Debug.Log("NOT FOUND " + text);
                if (!(languageID != InGameTranslator.LanguageID.English))
                {
                    break;
                }
                Debug.Log("RETRY WITH ENGLISH");
                languageID = InGameTranslator.LanguageID.English;
            }
            return;
        IL_117:
            string text3 = File.ReadAllText(text, Encoding.UTF8);
            if (text3[0] != '0')
            {
                text3 = Custom.xorEncrypt(text3, 54 + fileName + (int)this.interfaceOwner.rainWorld.inGameTranslator.currentLanguage * 7);
            }
            string[] array = Regex.Split(text3, "\r\n");
            try
            {
                if (Regex.Split(array[0], "-")[1] == fileName.ToString())
                {
                    if (oneRandomLine)
                    {
                        List<Conversation.TextEvent> list = new List<Conversation.TextEvent>();
                        for (int i = 1; i < array.Length; i++)
                        {
                            string[] array2 = LocalizationTranslator.ConsolidateLineInstructions(array[i]);
                            if (array2.Length == 3)
                            {
                                list.Add(new Conversation.TextEvent(this, int.Parse(array2[0], NumberStyles.Any, CultureInfo.InvariantCulture), array2[2], int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
                            }
                            else if (array2.Length == 1 && array2[0].Length > 0)
                            {
                                list.Add(new Conversation.TextEvent(this, 0, array2[0], 0));
                            }
                        }
                        if (list.Count > 0)
                        {
                            Random.State state = Random.state;
                            Random.InitState(randomSeed);
                            Conversation.TextEvent item = list[Random.Range(0, list.Count)];
                            Random.state = state;
                            this.events.Add(item);
                        }
                    }
                    else
                    {
                        for (int j = 1; j < array.Length; j++)
                        {
                            string[] array3 = LocalizationTranslator.ConsolidateLineInstructions(array[j]);
                            if (array3.Length == 3)
                            {
                                int num;
                                int num2;
                                if (ModManager.MSC && !int.TryParse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture, out num) && int.TryParse(array3[2], NumberStyles.Any, CultureInfo.InvariantCulture, out num2))
                                {
                                    this.events.Add(new Conversation.TextEvent(this, int.Parse(array3[0], NumberStyles.Any, CultureInfo.InvariantCulture), array3[1], int.Parse(array3[2], NumberStyles.Any, CultureInfo.InvariantCulture)));
                                }
                                else
                                {
                                    this.events.Add(new Conversation.TextEvent(this, int.Parse(array3[0], NumberStyles.Any, CultureInfo.InvariantCulture), array3[2], int.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
                                }
                            }
                            else if (array3.Length == 2)
                            {
                                if (array3[0] == "SPECEVENT")
                                {
                                    this.events.Add(new Conversation.SpecialEvent(this, 0, array3[1]));
                                }
                                else if (array3[0] == "PEBBLESWAIT")
                                {
                                    this.events.Add(new SSOracleBehavior.PebblesConversation.PauseAndWaitForStillEvent(this, null, int.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
                                }
                            }
                            else if (array3.Length == 1 && array3[0].Length > 0)
                            {
                                this.events.Add(new Conversation.TextEvent(this, 0, array3[0], 0));
                            }
                        }
                    }
                }
            }
            catch
            {
                Debug.Log("TEXT ERROR");
                this.events.Add(new Conversation.TextEvent(this, 0, "TEXT ERROR", 100));
            }
        }

        public static SLOracleBehaviorHasMark.MiscItemType TypeOfMiscItem(PhysicalObject testItem)
        {
            if (testItem is NSHSwarmer)
            {
                return NSHMiscItemType.NSHSwarmer;
            }
            if (testItem is WaterNut || testItem is SwollenWaterNut)
            {
                return SLOracleBehaviorHasMark.MiscItemType.WaterNut;
            }
            if (testItem is Rock)
            {
                return SLOracleBehaviorHasMark.MiscItemType.Rock;
            }
            if (testItem is ExplosiveSpear)
            {
                return SLOracleBehaviorHasMark.MiscItemType.FireSpear;
            }
            if (ModManager.MSC && testItem is ElectricSpear)
            {
                return MoreSlugcatsEnums.MiscItemType.ElectricSpear;
            }
            if (ModManager.MSC && testItem is Spear && (testItem as Spear).IsNeedle)
            {
                return MoreSlugcatsEnums.MiscItemType.SpearmasterSpear;
            }
            if (testItem is Spear)
            {
                return SLOracleBehaviorHasMark.MiscItemType.Spear;
            }
            if (testItem is KarmaFlower)
            {
                return SLOracleBehaviorHasMark.MiscItemType.KarmaFlower;
            }
            if (testItem is DangleFruit)
            {
                return SLOracleBehaviorHasMark.MiscItemType.DangleFruit;
            }
            if (testItem is FlareBomb)
            {
                return SLOracleBehaviorHasMark.MiscItemType.FlareBomb;
            }
            if (testItem is VultureMask)
            {
                if (ModManager.MSC)
                {
                    if ((testItem as VultureMask).AbstrMsk.scavKing)
                    {
                        return MoreSlugcatsEnums.MiscItemType.KingMask;
                    }
                    if ((testItem as VultureMask).AbstrMsk.spriteOverride != "")
                    {
                        return MoreSlugcatsEnums.MiscItemType.EliteMask;
                    }
                }
                return SLOracleBehaviorHasMark.MiscItemType.VultureMask;
            }
            if (testItem is PuffBall)
            {
                return SLOracleBehaviorHasMark.MiscItemType.PuffBall;
            }
            if (testItem is JellyFish)
            {
                return SLOracleBehaviorHasMark.MiscItemType.JellyFish;
            }
            if (testItem is Lantern)
            {
                return SLOracleBehaviorHasMark.MiscItemType.Lantern;
            }
            if (testItem is Mushroom)
            {
                return SLOracleBehaviorHasMark.MiscItemType.Mushroom;
            }
            if (testItem is FirecrackerPlant)
            {
                return SLOracleBehaviorHasMark.MiscItemType.FirecrackerPlant;
            }
            if (testItem is SlimeMold)
            {
                if (ModManager.MSC && testItem.abstractPhysicalObject.type == MoreSlugcatsEnums.AbstractObjectType.Seed)
                {
                    return MoreSlugcatsEnums.MiscItemType.Seed;
                }
                return SLOracleBehaviorHasMark.MiscItemType.SlimeMold;
            }
            else
            {
                if (testItem is ScavengerBomb)
                {
                    return SLOracleBehaviorHasMark.MiscItemType.ScavBomb;
                }
                if (testItem is OverseerCarcass && (!ModManager.MSC || !(testItem.abstractPhysicalObject as OverseerCarcass.AbstractOverseerCarcass).InspectorMode))
                {
                    return SLOracleBehaviorHasMark.MiscItemType.OverseerRemains;
                }
                if (testItem is BubbleGrass)
                {
                    return SLOracleBehaviorHasMark.MiscItemType.BubbleGrass;
                }
                if (ModManager.MSC)
                {
                    if (testItem is SingularityBomb)
                    {
                        return MoreSlugcatsEnums.MiscItemType.SingularityGrenade;
                    }
                    if (testItem is FireEgg)
                    {
                        return MoreSlugcatsEnums.MiscItemType.FireEgg;
                    }
                    if (testItem is EnergyCell)
                    {
                        return MoreSlugcatsEnums.MiscItemType.EnergyCell;
                    }
                    if (testItem is OverseerCarcass && (testItem.abstractPhysicalObject as OverseerCarcass.AbstractOverseerCarcass).InspectorMode)
                    {
                        return MoreSlugcatsEnums.MiscItemType.InspectorEye;
                    }
                    if (testItem is GooieDuck)
                    {
                        return MoreSlugcatsEnums.MiscItemType.GooieDuck;
                    }
                    if (testItem is NeedleEgg)
                    {
                        return MoreSlugcatsEnums.MiscItemType.NeedleEgg;
                    }
                    if (testItem is LillyPuck)
                    {
                        return MoreSlugcatsEnums.MiscItemType.LillyPuck;
                    }
                    if (testItem is GlowWeed)
                    {
                        return MoreSlugcatsEnums.MiscItemType.GlowWeed;
                    }
                    if (testItem is DandelionPeach)
                    {
                        return MoreSlugcatsEnums.MiscItemType.DandelionPeach;
                    }
                    if (testItem is MoonCloak)
                    {
                        return MoreSlugcatsEnums.MiscItemType.MoonCloak;
                    }
                }
                return SLOracleBehaviorHasMark.MiscItemType.NA;
            }
        }

        public string ReplaceParts(string s)
        {
            s = Regex.Replace(s, "<PlayerName>", this.NameForPlayer(false));
            s = Regex.Replace(s, "<CapPlayerName>", this.NameForPlayer(true));
            s = Regex.Replace(s, "<ItemPlayerName>", this.ItemNameForPlayer(false));
            s = Regex.Replace(s, "<CapItemPlayerName>", this.ItemNameForPlayer(true));
            return s;
        }

        public string NameForPlayer(bool capitalized)
        {
            string text = "little";
            string str = "one";
            if (this.owner.oracle.room.game.session.characterStats.name == Plugin.SlugName)
            {
                str = "messenger";
            }
            if (this.owner.oracle.room.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
            {
                str = "pellet";
            }
            if (this.owner.oracle.room.game.session.characterStats.name.value == "Outsider")
            {
                str = "moth";
            }
            if (capitalized && InGameTranslator.LanguageID.UsesCapitals(this.owner.oracle.room.game.rainWorld.inGameTranslator.currentLanguage))
            {
                text = char.ToUpper(text[0]).ToString() + text.Substring(1);
            }
            return this.owner.Translate(text + " " + str);
        }

        public string ItemNameForPlayer(bool capitalized)
        {
            string text = "little";
            string str = "one";
            int i = 0;
            if (this.State.GetOpinion == NSHOracleState.PlayerOpinion.Likes || this.State.totalPearlsBrought > 5)
                i = Random.Range(0, 5);
            else 
                i = Random.Range(0, 2);
                switch (i)
                {
                    case 0:
                        str = "wanderer";
                        break;
                    case 1:
                        str = "garbage worm";
                        break;
                    case 2:
                        str = "apprentice";
                        break;
                    case 3:
                        str = "scholar";
                        break;
                    default:
                        str = "naturalist";
                        break;
                }
            if (capitalized && InGameTranslator.LanguageID.UsesCapitals(this.owner.oracle.room.game.rainWorld.inGameTranslator.currentLanguage))
            {
                text = char.ToUpper(text[0]).ToString() + text.Substring(1);
            }
            return this.owner.Translate(text + " " + str);
        }

        public override void Update()
        {
            this.age++;
            bool flag = this.waitForStill;
            if (flag)
            {
                bool flag2 = !this.convBehav.CurrentlyCommunicating && this.convBehav.communicationPause > 0;
                if (flag2)
                {
                    this.convBehav.communicationPause--;
                }
                bool flag3 = !this.convBehav.CurrentlyCommunicating && this.convBehav.communicationPause < 1 && this.owner.allStillCounter > 20;
                if (flag3)
                {
                    this.waitForStill = false;
                }
            }
            else
            {
                base.Update();
            }
        }

        public string Translate(string s)
        {
            return this.ReplaceParts(this.owner.Translate(s));
        }

        public CustomOracleBehaviour owner;
        public CustomOracleBehaviour.CustomConversationBehaviour convBehav;
        public SLOracleBehaviorHasMark.MiscItemType describeItem;
        public bool waitForStill;
        public int age;
        public delegate void AddEventDelegate(Conversation.ID id, CustomOracleBehaviour owner);

        public class PauseAndWaitForStillEvent : Conversation.DialogueEvent
        {
            public PauseAndWaitForStillEvent(Conversation owner, CustomOracleBehaviour.CustomConversationBehaviour _convBehav, int pauseFrames) : base(owner, 0)
            {
                this.convBehav = _convBehav;
                bool flag = this.convBehav == null && owner is CustomOracleBehaviour.CustomOracleConversation;
                if (flag)
                {
                    this.convBehav = (owner as CustomOracleBehaviour.CustomOracleConversation).convBehav;
                }
                this.pauseFrames = pauseFrames;
            }

            public override void Activate()
            {
                base.Activate();
                this.convBehav.communicationPause = this.pauseFrames;
                (this.owner as CustomOracleBehaviour.CustomOracleConversation).waitForStill = true;
            }

            public CustomOracleBehaviour.CustomConversationBehaviour convBehav;
            public int pauseFrames;
        }
    }

    //生成蜥蜴前的发言
    public class NSHNarration : RoomChatTx
    {
        public NSHNarration(Room room) : base(room, 30, "The Temporary Arena")
        {
        }

        public override void Destroy()
        {
            base.Destroy();
        }

        public override void AddTextEvents(DialogBox dialogBox)
        {
            this.events.Add(new RoomChatTx.TextEvent(this, base.Translate("NSH: This time I'll let you deal with a lizard."), dialogBox, 0, this.extraLingerFactor * 50));
            this.events.Add(new RoomChatTx.TextEvent(this, base.Translate("NSH: Keep your spirits up, soon after the situation you're facing would be far more dangerous than this."), dialogBox, 0, this.extraLingerFactor * 100));
            this.events.Add(new RoomChatTx.TextEvent(this, base.Translate("NSH: We need to ensure that everything is flawless."), dialogBox, 0, this.extraLingerFactor * 50));
            //this.events.Add(new RoomChatTx.TextEvent(this, base.Translate("Don't forget to watch your step when climbing those coolers."), dialogBox, this.extraLingerFactor * 80, SoundID.SL_AI_Talk_4, 40));
            base.AddTextEvents(dialogBox);
        }
    }

    //杀死蜥蜴后的结语
    public class NSHConclusion : RoomChatTx
    {
        public NSHConclusion(Room room) : base(room, 0)
        {
        }

        public override void Destroy()
        {
            base.Destroy();
            CustomDreamRx.currentActivateNormalDream.EndDream(room.game);
        }

        public override void AddTextEvents(DialogBox dialogBox)
        {
            switch (PlayerHooks.conclusionLevel)
            {
                case 0:
                    this.events.Add(new RoomChatTx.TextEvent(this, base.Translate("NSH: Good. Hope you can maintain the same level of proficiency and caution along the way."), dialogBox, 0, this.extraLingerFactor * 90));
                    break;
                case 1:
                case 2:
                    this.events.Add(new RoomChatTx.TextEvent(this, base.Translate("NSH: Well done, but still, you need my help."), dialogBox, 0, this.extraLingerFactor * 50)) ;
                    this.events.Add(new RoomChatTx.TextEvent(this, base.Translate("NSH: This can hardly let us satisfied, isn't it?"), dialogBox, 0, this.extraLingerFactor * 100));
                    this.events.Add(new RoomChatTx.TextEvent(this, base.Translate("NSH: There are not many opportunities left for us."), dialogBox, 0, this.extraLingerFactor * 50));
                    break;
                case 3:
                case 4:
                case 5:
                    this.events.Add(new RoomChatTx.TextEvent(this, base.Translate("NSH: Be calm, your body can reach a level far beyond that."), dialogBox, 0, this.extraLingerFactor * 50));
                    this.events.Add(new RoomChatTx.TextEvent(this, base.Translate("NSH: Learning to perfectly manipulate it is not difficult for your race."), dialogBox, 0, this.extraLingerFactor * 100));
                    this.events.Add(new RoomChatTx.TextEvent(this, base.Translate("NSH: Concentrate, we just need more time."), dialogBox, 0, this.extraLingerFactor * 50));
                    break;
                default:
                    this.events.Add(new RoomChatTx.TextEvent(this, base.Translate("NSH: ......"), dialogBox, 0, this.extraLingerFactor * 10));
                    this.events.Add(new RoomChatTx.TextEvent(this, base.Translate("NSH: Is this any kind of acting out?"), dialogBox, 0, this.extraLingerFactor * 40));
                    this.events.Add(new RoomChatTx.TextEvent(this, base.Translate("NSH: I have no intention of making us at odds. A quarter rain cycle - I hope you can recover later."), dialogBox, 0, this.extraLingerFactor * 100));
                    this.events.Add(new RoomChatTx.TextEvent(this, base.Translate("NSH: My messenger, I have no other choice in this matter. I can only hope to receive your understanding."), dialogBox, 0, this.extraLingerFactor * 100));
                    break;
            }
            //this.events.Add(new RoomChatTx.TextEvent(this, base.Translate("Don't forget to watch your step when climbing those coolers."), dialogBox, this.extraLingerFactor * 80, SoundID.SL_AI_Talk_4, 40));
            base.AddTextEvents(dialogBox);
        }
    }
}
