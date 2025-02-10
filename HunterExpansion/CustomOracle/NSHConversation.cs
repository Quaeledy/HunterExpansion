using System;
using static CustomOracleTx.CustomOracleBehaviour;
using HUD;
using MoreSlugcats;
using CustomOracleTx;
using CustomDreamTx;
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
using BepInEx;

namespace HunterExpansion.CustomOracle
{
    public class NSHConversation : Conversation
    {
        public static string oracleID = "NSH";

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
                string path1 = modPath + string.Concat(new string[]
                {
                    Path.DirectorySeparatorChar.ToString(),
                    "Text",
                    Path.DirectorySeparatorChar.ToString(),
                    "Text_",
                    LocalizationTranslator.LangShort(languageID),
                    Path.DirectorySeparatorChar.ToString()
                }).ToLowerInvariant();
                string path2 = modPath + string.Concat(new string[]
                {
                    Path.DirectorySeparatorChar.ToString(),
                    "Newest",
                    Path.DirectorySeparatorChar.ToString(),
                    "Text",
                    Path.DirectorySeparatorChar.ToString(),
                    "Text_",
                    LocalizationTranslator.LangShort(languageID),
                    Path.DirectorySeparatorChar.ToString()
                }).ToLowerInvariant();
                if (Directory.Exists(path1))
                {
                    string[] files = Directory.GetFiles(path1, "*.txt", SearchOption.AllDirectories);
                    for (int j = 0; j < files.Length; j++)
                    {
                        InGameTranslator.EncryptDecryptFile(files[j], true, false);//加密
                        //InGameTranslator.EncryptDecryptFile(files[j], false, false);//解密
                    }
                }
                if (Directory.Exists(path2))
                {
                    string[] files = Directory.GetFiles(path2, "*.txt", SearchOption.AllDirectories);
                    for (int j = 0; j < files.Length; j++)
                    {
                        InGameTranslator.EncryptDecryptFile(files[j], true, false);//加密
                        //InGameTranslator.EncryptDecryptFile(files[j], false, false);//解密
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
                    this.events.Add(new NSHConversation.PauseAndWaitForStillEvent(this, this.convBehav, 5));
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
                    if (this.describeItem == NSHMiscItemType.SLOracleSwarmer)
                    {
                        LoadEventsFromFile(this, 0, "Object_SLOracleSwarmer");
                        return;
                    }
                    if (this.describeItem == NSHMiscItemType.SSOracleSwarmer)
                    {
                        LoadEventsFromFile(this, 0, "Object_SSOracleSwarmer");
                        return;
                    }
                    if (this.describeItem == NSHMiscItemType.NSHOracleSwarmer)
                    {
                        LoadEventsFromFile(this, 0, "Object_NSHOracleSwarmer");
                        return;
                    }
                    if (this.describeItem == NSHMiscItemType.SporePlant)
                    {
                        LoadEventsFromFile(this, 0, "Object_SporePlant");
                        return;
                    }
                    if (this.describeItem == NSHMiscItemType.FlyLure)
                    {
                        LoadEventsFromFile(this, 0, "Object_FlyLure");
                        return;
                    }
                    if (this.describeItem == SLOracleBehaviorHasMark.MiscItemType.Spear)
                    {
                        LoadEventsFromFile(this, 0, "Object_Spear");
                        return;/*
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
                        }*/
                    }
                    if (this.describeItem == SLOracleBehaviorHasMark.MiscItemType.FireSpear)
                    {
                        LoadEventsFromFile(this, 0, "Object_FireSpear");
                        return;/*
                        if (this.currentSaveFile == Plugin.SlugName)
                        {
                            this.events.Add(new Conversation.TextEvent(this, 10, this.Translate("This is an explosive weapon, just throw it like a rebar. Remember<LINE>to keep a certain distance from the explosion point."), 0));
                            this.events.Add(new Conversation.TextEvent(this, 10, this.Translate("However, I guess you have already figured out its usage!"), 0));
                            return;
                        }
                        else
                        {
                            this.events.Add(new Conversation.TextEvent(this, 10, this.Translate("A type of explosive weapon. Be careful, I have seen many creatures<LINE>use it to harm themselves."), 0));
                            return;
                        }
                        }*/
                    }
                    if (this.describeItem == SLOracleBehaviorHasMark.MiscItemType.Rock)
                    {
                        LoadEventsFromFile(this, 0, "Object_Rock");
                        return;/*
                        if (this.currentSaveFile == Plugin.SlugName)
                        {
                            this.events.Add(new Conversation.TextEvent(this, 10, this.Translate("A rock, despite its non-lethality, can still play a unique role in your face of enemies. I think you are already familiar with it~"), 0));
                            return;
                        }
                        else
                        {
                            this.events.Add(new Conversation.TextEvent(this, 10, this.Translate("This is a rock. I seriously suspect that you are trying to use me to compile an encyclopedia,<LINE>or showcase your strange collection addiction."), 0));
                            return;
                        }*/
                    }
                    if (this.describeItem == SLOracleBehaviorHasMark.MiscItemType.KarmaFlower)
                    {
                        LoadEventsFromFile(this, 25);
                        return;
                    }
                    if (this.describeItem == SLOracleBehaviorHasMark.MiscItemType.WaterNut)
                    {
                        LoadEventsFromFile(this, 0, "Object_WaterNut");
                        return;/*
                        this.events.Add(new Conversation.TextEvent(this, 10, this.Translate("This is a plant that swells when exposed to water, and it is edible.<LINE>It is said to have a great taste."), 0));
                        this.events.Add(new Conversation.TextEvent(this, 10, this.Translate("Yes, I can't eat it. Don't show off in front of me~"), 0));
                        return;*/
                    }
                    if (this.describeItem == SLOracleBehaviorHasMark.MiscItemType.DangleFruit)
                    {
                        LoadEventsFromFile(this, 26);
                        return;
                    }
                    if (this.describeItem == SLOracleBehaviorHasMark.MiscItemType.FlareBomb)
                    {
                        LoadEventsFromFile(this, 27);
                        return;
                    }
                    if (this.describeItem == SLOracleBehaviorHasMark.MiscItemType.VultureMask)
                    {
                        LoadEventsFromFile(this, 28);
                        return;
                    }
                    if (this.describeItem == SLOracleBehaviorHasMark.MiscItemType.PuffBall)
                    {
                        LoadEventsFromFile(this, 29);
                        return;
                    }
                    if (this.describeItem == SLOracleBehaviorHasMark.MiscItemType.JellyFish)
                    {
                        LoadEventsFromFile(this, 30);
                        return;
                    }
                    if (this.describeItem == SLOracleBehaviorHasMark.MiscItemType.Lantern)
                    {
                        LoadEventsFromFile(this, 31);
                        return;/*
                        if (this.currentSaveFile == MoreSlugcatsEnums.SlugcatStatsName.Saint)
                        {
                            LoadEventsFromFile(this, 31);
                            return;
                        }
                        else
                        {
                            LoadEventsFromFile(this, 31);
                            return;
                        }*/
                    }
                    if (this.describeItem == SLOracleBehaviorHasMark.MiscItemType.Mushroom)
                    {
                        LoadEventsFromFile(this, 32);
                        return;
                    }
                    if (this.describeItem == SLOracleBehaviorHasMark.MiscItemType.FirecrackerPlant)
                    {
                        LoadEventsFromFile(this, 33);
                        return;
                    }
                    if (this.describeItem == SLOracleBehaviorHasMark.MiscItemType.SlimeMold)
                    {
                        LoadEventsFromFile(this, 34);
                        return;
                    }
                    if (this.describeItem == SLOracleBehaviorHasMark.MiscItemType.ScavBomb)
                    {
                        LoadEventsFromFile(this, 44);
                        return;
                    }
                    if (this.describeItem == NSHMiscItemType.NSHSwarmer)
                    {
                        if (this.currentSaveFile == Plugin.SlugName)
                        {
                            if (this.owner.oracle.room.game.rainWorld.progression.currentSaveState.miscWorldSaveData.SLOracleState.neuronsLeft <= 0)
                                LoadEventsFromFile(this, 50, "Before");
                            else
                                LoadEventsFromFile(this, 50, "After");
                        }
                        else
                        {
                            LoadEventsFromFile(this, 46);
                        }
                        return;
                    }
                    if (this.describeItem == SLOracleBehaviorHasMark.MiscItemType.OverseerRemains)
                    {
                        LoadEventsFromFile(this, 52);
                        return;
                    }
                    if (this.describeItem == SLOracleBehaviorHasMark.MiscItemType.BubbleGrass)
                    {
                        LoadEventsFromFile(this, 53);
                        return;
                    }
                    if (ModManager.MSC)
                    {
                        if (this.describeItem == MoreSlugcatsEnums.MiscItemType.EnergyCell)
                        {
                            this.State.shownEnergyCell = true;
                            LoadEventsFromFile(this, 110);
                            return;
                        }
                        if (this.describeItem == MoreSlugcatsEnums.MiscItemType.ElectricSpear)
                        {
                            LoadEventsFromFile(this, 112);
                            return;
                        }
                        if (this.describeItem == MoreSlugcatsEnums.MiscItemType.InspectorEye)
                        {
                            LoadEventsFromFile(this, 113);
                            return;
                        }
                        if (this.describeItem == MoreSlugcatsEnums.MiscItemType.GooieDuck)
                        {
                            LoadEventsFromFile(this, 114);
                            return;
                        }
                        if (this.describeItem == MoreSlugcatsEnums.MiscItemType.NeedleEgg)
                        {
                            LoadEventsFromFile(this, 116);
                            return;
                        }
                        if (this.describeItem == MoreSlugcatsEnums.MiscItemType.LillyPuck)
                        {
                            LoadEventsFromFile(this, 117);
                            return;
                        }
                        if (this.describeItem == MoreSlugcatsEnums.MiscItemType.GlowWeed)
                        {
                            LoadEventsFromFile(this, 118);
                            return;
                        }
                        if (this.describeItem == MoreSlugcatsEnums.MiscItemType.DandelionPeach)
                        {
                            LoadEventsFromFile(this, 122);
                            return;
                        }
                        if (this.describeItem == MoreSlugcatsEnums.MiscItemType.MoonCloak)
                        {
                            LoadEventsFromFile(this, 123);
                            return;
                        }
                        if (this.describeItem == MoreSlugcatsEnums.MiscItemType.SingularityGrenade)
                        {
                            LoadEventsFromFile(this, 127);
                            return;
                        }
                        if (this.describeItem == MoreSlugcatsEnums.MiscItemType.EliteMask)
                        {
                            LoadEventsFromFile(this, 136);
                            return;/*
                            if (this.currentSaveFile == MoreSlugcatsEnums.SlugcatStatsName.Spear ||
                                this.currentSaveFile == MoreSlugcatsEnums.SlugcatStatsName.Artificer ||
                                this.currentSaveFile == SlugcatStats.Name.Red)
                                LoadEventsFromFile(this, 136, "Before");
                            else
                                LoadEventsFromFile(this, 136);
                            return;*/
                        }
                        if (this.describeItem == MoreSlugcatsEnums.MiscItemType.KingMask)
                        {
                            LoadEventsFromFile(this, 137);
                            return;
                        }
                        if (this.describeItem == MoreSlugcatsEnums.MiscItemType.FireEgg)
                        {
                            LoadEventsFromFile(this, 164);
                            return;
                        }
                        if (this.describeItem == MoreSlugcatsEnums.MiscItemType.SpearmasterSpear)
                        {
                            LoadEventsFromFile(this, 166);
                            return;
                        }
                        if (this.describeItem == MoreSlugcatsEnums.MiscItemType.Seed)
                        {
                            LoadEventsFromFile(this, 167);
                            return;
                        }
                    }
                }
                else
                {
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
                        LoadEventsFromFile(this, 7);
                        return;
                    }
                    if (this.id == Conversation.ID.Moon_Pearl_LF_west)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(this, 10);
                        return;
                    }
                    if (this.id == Conversation.ID.Moon_Pearl_LF_bottom)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(this, 11);
                        return;
                    }
                    if (this.id == Conversation.ID.Moon_Pearl_HI)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(this, 12);
                        return;
                    }
                    if (this.id == Conversation.ID.Moon_Pearl_SH)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(this, 13);
                        return;
                    }
                    if (this.id == Conversation.ID.Moon_Pearl_DS)//已写
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(this, 14);
                        return;
                    }
                    if (this.id == Conversation.ID.Moon_Pearl_SB_filtration)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(this, 15);
                        return;
                    }
                    if (this.id == Conversation.ID.Moon_Pearl_GW)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(this, 16);
                        return;
                    }
                    if (this.id == Conversation.ID.Moon_Pearl_SL_bridge)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(this, 17);
                        return;
                    }
                    if (this.id == Conversation.ID.Moon_Pearl_SL_moon)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(this, 18);
                        return;
                    }
                    if (this.id == Conversation.ID.Moon_Pearl_SI_west)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(this, 20);
                        return;
                    }
                    if (this.id == Conversation.ID.Moon_Pearl_SI_top)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(this, 21);
                        return;
                    }
                    if (ModManager.MSC && this.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_SI_chat3)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(this, 22);
                        return;
                    }
                    if (ModManager.MSC && this.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_SI_chat4)
                    {
                        this.PearlIntro();
                        if (this.owner.oracle.room.game.IsMoonHeartActive())
                        {
                            LoadEventsFromFile(this, 23, "MoonHeartActive");
                        }
                        else
                        {
                            LoadEventsFromFile(this, 23);
                        }
                        return;
                    }
                    if (ModManager.MSC && this.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_SI_chat5)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(this, 24);
                        return;
                    }
                    if (this.id == Conversation.ID.Moon_Pearl_SU)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(this, 41);
                        return;
                    }
                    if (this.id == Conversation.ID.Moon_Pearl_UW)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(this, 42);
                        return;
                    }
                    if (this.id == Conversation.ID.Moon_Pearl_SB_ravine)
                    {
                        this.PearlIntro();
                        if (this.owner.oracle.room.game.rainWorld.progression.currentSaveState.deathPersistentSaveData.altEnding)
                        {
                            LoadEventsFromFile(this, 43, "SpearAfter");
                        }
                        else if (this.owner.oracle.room.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Spear)
                        {
                            LoadEventsFromFile(this, 43, "SpearBefore");
                        }
                        else
                        {
                            LoadEventsFromFile(this, 43);
                        }
                        return;
                    }
                    if (this.id == Conversation.ID.Moon_Pearl_Red_stomach)
                    {
                        this.PearlIntro();
                        if (this.owner.oracle.room.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Spear ||
                            this.owner.oracle.room.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
                        {
                            LoadEventsFromFile(this, 51, "Before");
                        }
                        else if (this.owner.oracle.room.game.session.characterStats.name == Plugin.SlugName)
                        {
                            LoadEventsFromFile(this, 51, "Red");
                        }
                        else if (this.owner.oracle.room.game.IsMoonHeartActive())
                        {
                            LoadEventsFromFile(this, 51, "Final");
                        }
                        else
                        {
                            LoadEventsFromFile(this, 51);
                        }
                        return;
                    }
                    if (this.id == Conversation.ID.Moon_Pearl_SL_chimney)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(this, 54);
                        if (State.GetOpinion == NSHOracleState.PlayerOpinion.Likes)
                        {
                            this.events.Add(new Conversation.TextEvent(this, 10, this.Translate("I have to say, you are really cute!"), 0));
                        }
                        return;
                    }
                    if (ModManager.MSC && this.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_SU_filt)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(this, 101);
                        return;
                    }
                    if (ModManager.MSC && this.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_DM)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(this, 102);
                        return;
                    }
                    if (ModManager.MSC && this.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_LC)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(this, 103);
                        return;
                    }
                    if (ModManager.MSC && this.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_OE)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(this, 104);
                        return;
                    }
                    if (ModManager.MSC && this.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_MS)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(this, 105);
                        return;
                    }
                    if (ModManager.MSC && this.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_RM)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(this, 106);
                        return;
                    }
                    if (ModManager.MSC && this.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_Rivulet_stomach)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(this, 119);
                        return;
                    }
                    if (ModManager.MSC && this.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_LC_second)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(this, 121);
                        return;
                    }
                    if (ModManager.MSC && this.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_VS)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(this, 128);
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
                        LoadEventsFromFile(this, 140);
                        return;
                    }
                    if (ModManager.MSC && this.id == MoreSlugcatsEnums.ConversationID.Moon_Spearmaster_Pearl)
                    {
                        LoadEventsFromFile(this, 142);
                        return;
                    }
                    //NSH区域独有珍珠
                    /*
                    if (this.id == NSHConversationID.NSH_Pearl_NSH_Top)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(this, 0, "NSH_Top");
                        return;
                    }
                    if (this.id == NSHConversationID.NSH_Pearl_NSH_Box)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(this, 0, "NSH_Box");
                        return;
                    }*/
                    //其他mod珍珠
                    if (this.id != null && this.describeItem == NSHMiscItemType.DataPearl)
                    {
                        this.PearlIntro();
                        LoadEventsFromFile(this, 0, this.id.ToString());
                        return;
                    }
                }
                if (this.id == NSHConversationID.NSHMeetSlugcatFriends)
                {
                    if (this.State.GetOpinion == NSHOracleState.PlayerOpinion.Likes)
                    {
                        if (ModManager.MSC && this.owner.CheckSlugpupsInRoom())
                        {
                            this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("I do enjoy the company though. You and your family are always welcome here."), 5));
                            return;
                        }
                        if (ModManager.MMF && this.owner.CheckStrayCreatureInRoom() != CreatureTemplate.Type.StandardGroundCreature)
                        {
                            this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("I do enjoy the company of you and your friend though, <PlayerName>."), 5));
                            return;
                        }
                        this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("I do enjoy the company though. You're welcome to stay a while, quiet little thing."), 5));
                        return;
                    }
                }
                //不是以上情况时，加载AddConversationEvents方法
                this.owner.AddConversationEvents(this, this.id);
                return;
            }
            else
            {
                this.owner.PlayerEncountersWithoutMark();
            }
                
        }

        public void ItemIntro()
        {
            switch (this.State.totalItemsBrought)
            {
                case 0:
                    this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("What is that?"), 10));
                    return;
                case 1:
                    if (this.State.GetOpinion == NSHOracleState.PlayerOpinion.Likes)
                        this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("Seems like you found something new again! Let me take a look."), 10));

                    else
                        this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("Seems like you found something new again. Let me take a look."), 10));
                        return;
                case 2:
                    if (this.State.GetOpinion == NSHOracleState.PlayerOpinion.Likes)
                        this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("Ah-Hah! You are making this into a habit! Let me take a close look."), 10));
                    else
                    {
                        this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("If you continue to be polite, I will explain to you."), 10));
                        this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("I have the right to refuse to entertain impolite guests, don't you think?"), 10));
                    }
                    return;
                case 3:
                    if (this.State.GetOpinion == NSHOracleState.PlayerOpinion.Likes)
                        this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("Let's see, what do you have here this time?"), 10));
                    else
                        this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("What is it again?"), 10));
                        return;
                default:
                    if (this.State.GetOpinion == NSHOracleState.PlayerOpinion.Likes) 
                    {
                        int i = (this.owner is NSHOracleBehaviour && (this.owner as NSHOracleBehaviour).holdingObject != null) ? (this.owner as NSHOracleBehaviour).holdingObject.abstractPhysicalObject.ID.RandomSeed : Random.Range(0, 100000);
                        NSHConversation.LoadEventsFromFile(this, 205, null, true, i);
                    }
                    else
                    {
                        int i = (this.owner is NSHOracleBehaviour && (this.owner as NSHOracleBehaviour).holdingObject != null) ? (this.owner as NSHOracleBehaviour).holdingObject.abstractPhysicalObject.ID.RandomSeed : Random.Range(0, 100000);
                        NSHConversation.LoadEventsFromFile(this, 206, null, true, i);
                    }/*
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
                    }*/
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
                        this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("The scavengers will be jealous of your horde!"), 10));
                        return;
                    }
                    break;
                default:
                    int i = (this.owner is NSHOracleBehaviour && (this.owner as NSHOracleBehaviour).holdingObject != null) ? (this.owner as NSHOracleBehaviour).holdingObject.abstractPhysicalObject.ID.RandomSeed : Random.Range(0, 100000);
                    NSHConversation.LoadEventsFromFile(this, 203, null, true, i);/*
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
                    }*/
                    break;
            }
        }

        private void PebblesPearlIntro()
        {
            int i = (this.owner is NSHOracleBehaviour && (this.owner as NSHOracleBehaviour).holdingObject != null) ? (this.owner as NSHOracleBehaviour).holdingObject.abstractPhysicalObject.ID.RandomSeed : Random.Range(0, 100000);
            NSHConversation.LoadEventsFromFile(this, 204, null, true, i);/*
            switch (Random.Range(0, 5))
            {
                case 0:
                    this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("<CapPlayerName>, do you want me to read this?"), 10));
                    this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("This pearl is still very warm, it seems that it had been read recently."), 10));
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
            }*/
        }

        private void MiscPearl(bool miscPearl2)
        {
            int i = (this.owner is NSHOracleBehaviour && (this.owner as NSHOracleBehaviour).holdingObject != null) ? (this.owner as NSHOracleBehaviour).holdingObject.abstractPhysicalObject.ID.RandomSeed : Random.Range(0, 100000);
            LoadEventsFromFile(this, 38, null, true, i);
            NSHOracleState state = this.State;
            int miscPearlCounter = state.miscPearlCounter;
            state.miscPearlCounter = miscPearlCounter + 1;
        }

        private void BroadcastMisc()
        {
            int i = (this.owner is NSHOracleBehaviour && (this.owner as NSHOracleBehaviour).holdingObject != null) ? ((this.owner as NSHOracleBehaviour).holdingObject.abstractPhysicalObject.ID.RandomSeed) : Random.Range(0, 100000);
            LoadEventsFromFile(this, 132, null, true, i);
            this.State.miscPearlCounter++;
        }

        private void PebblesPearl()
        {
            int i = (this.owner is NSHOracleBehaviour && (this.owner as NSHOracleBehaviour).holdingObject != null) ? (this.owner as NSHOracleBehaviour).holdingObject.abstractPhysicalObject.ID.RandomSeed : Random.Range(0, 100000);
            if (this.owner.oracle.room.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
            {
                PebblesPearlIntro();
                LoadEventsFromFile(this, 40);
            }
            else if (this.owner.oracle.room.game.session.characterStats.name != MoreSlugcatsEnums.SlugcatStatsName.Rivulet &&
                     this.owner.oracle.room.game.IsMoonHeartActive())
            {
                LoadEventsFromFile(this, 40, "MoonHeartActive", true, i);
            }
            else
            {
                PebblesPearlIntro();
                LoadEventsFromFile(this, 40, null, true, i);
            }
        }

        public static void LoadEventsFromFile(NSHConversation self, int fileName, string suffix = null, bool oneRandomLine = false, int randomSeed = 0)
        {
            string subfolderName = oracleID;
            if (self == null)
            {
                Plugin.Log("LoadEventsFromFile: Error, NSHConversation is not exist!");
                return;
            }    
            SlugcatStats.Name currentSaveFile = self.currentSaveFile;
            LoadEventsFromFile(self, fileName, subfolderName, currentSaveFile, suffix, oneRandomLine, randomSeed);
        }

        public static void LoadEventsFromFile(SLOracleBehaviorHasMark.MoonConversation self, int fileName, string suffix = null, bool oneRandomLine = false, int randomSeed = 0)
        {
            string subfolderName = oracleID;
            if (self == null)
            {
                Plugin.Log("LoadEventsFromFile: Error, NSHConversation is not exist!");
                return;
            }
            SlugcatStats.Name currentSaveFile = self.currentSaveFile;
            LoadEventsFromFile(self, fileName, subfolderName, currentSaveFile, suffix, oneRandomLine, randomSeed);
        }

        public static void LoadEventsFromFile(Conversation self, int fileName, string subfolderName, SlugcatStats.Name currentSaveFile, string suffix = null, bool oneRandomLine = false, int randomSeed = 0)
        {
            Plugin.Log("~~~LOAD CONVO " + subfolderName + Path.DirectorySeparatorChar.ToString() + fileName.ToString() + (suffix == null ? "" : "-" + suffix.ToString()));
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
                        "-",
                        currentSaveFile.ToString(),
                        ".txt"
                    }));
                    if (!File.Exists(text))
                    {
                        Plugin.Log("NOT FOUND " + text);
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
                }
                else if (currentSaveFile != null)
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
                        currentSaveFile.ToString(),
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
                    goto IL_117;
                }
                Plugin.Log("NOT FOUND " + text);
                if (!(languageID != InGameTranslator.LanguageID.English))
                {
                    break;
                }
                Plugin.Log("RETRY WITH ENGLISH");
                languageID = InGameTranslator.LanguageID.English;
            }
            return;
            IL_117:
            Plugin.Log("FOUND FILE!!! Load from flie: " + text);
            string text3 = File.ReadAllText(text, Encoding.UTF8);
            if (text3[0] != '0')
            {
                text3 = Custom.xorEncrypt(text3, 54 + fileName + (int)self.interfaceOwner.rainWorld.inGameTranslator.currentLanguage * 7);
            }
            string[] array = Regex.Split(ReplaceParts(self, currentSaveFile, text3), "\r\n");
            try
            {
                if (Regex.Split(array[0], "-")[1] == fileName.ToString())
                {
                    if (oneRandomLine)
                    {
                        List<string> lines = new List<string>(); 
                        for (int i = 1; i < array.Length; i++)
                        {
                            string[] array2 = LocalizationTranslator.ConsolidateLineInstructions(array[i]);
                            if (array2.Length >= 2 || (array2.Length == 1 && array2[0].Length > 0))
                                lines.Add(array[i]);
                        }
                        if (lines.Count > 0)
                        {
                            Random.State state = Random.state;
                            Random.InitState(randomSeed);
                            string item = lines[Random.Range(0, lines.Count)];
                            Random.state = state;

                            string[] line = LocalizationTranslator.ConsolidateLineInstructions(item);
                            if (line.Length == 3)
                            {
                                string[] array3 = Regex.Split(line[2], "<NEXT>");
                                if (array3.Length > 1)
                                    for (int j = 0; j < array3.Length; j++)
                                        self.events.Add(new Conversation.TextEvent(self, int.Parse(line[0], NumberStyles.Any, CultureInfo.InvariantCulture), array3[j], int.Parse(line[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
                            }
                            else if (line.Length == 2)
                            {
                                if (line[0] == "SPECEVENT")
                                {
                                    string[] array3 = Regex.Split(line[1], "<NEXT>");
                                    Plugin.Log("SPECEVENT : " + array3[0]);
                                    self.events.Add(new Conversation.SpecialEvent(self, 0, array3[0]));
                                    if (array3.Length > 1)
                                    {//注意，j = 0是事件名，所以从 1 开始
                                        for (int j = 1; j < array3.Length; j++)
                                            self.events.Add(new Conversation.TextEvent(self, 0, array3[j], 0));
                                    }
                                }
                                else if (line[0] == "PEBBLESWAIT")
                                {
                                    Plugin.Log("WAIT : " + int.Parse(line[1], NumberStyles.Any, CultureInfo.InvariantCulture));
                                    if (self is NSHConversation)
                                        self.events.Add(new NSHConversation.PauseAndWaitForStillEvent(self, null, int.Parse(line[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
                                    else
                                        self.events.Add(new SSOracleBehavior.PebblesConversation.PauseAndWaitForStillEvent(self, null, int.Parse(line[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
                                    //备注：文件中的时间约为代码中的8倍。
                                }
                            }
                            else if (line.Length == 1 && line[0].Length > 0)
                            {
                                string[] array3 = Regex.Split(line[0], "<NEXT>");
                                for (int j = 0; j < array3.Length; j++)
                                {
                                    self.events.Add(new Conversation.TextEvent(self, 0, array3[j], 0));
                                }
                            }
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
                                    self.events.Add(new Conversation.TextEvent(self, int.Parse(array3[0], NumberStyles.Any, CultureInfo.InvariantCulture), array3[1], int.Parse(array3[2], NumberStyles.Any, CultureInfo.InvariantCulture)));
                                }
                                else
                                {
                                    self.events.Add(new Conversation.TextEvent(self, int.Parse(array3[0], NumberStyles.Any, CultureInfo.InvariantCulture), array3[2], int.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
                                }
                            }
                            else if (array3.Length == 2)
                            {
                                if (array3[0] == "SPECEVENT")
                                {
                                    Plugin.Log("SPECEVENT : " + array3[1]);
                                    self.events.Add(new Conversation.SpecialEvent(self, 0, array3[1]));
                                }
                                else if (array3[0] == "PEBBLESWAIT")
                                {
                                    Plugin.Log("WAIT : " + int.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture));
                                    if (self is NSHConversation)
                                        self.events.Add(new NSHConversation.PauseAndWaitForStillEvent(self, null, int.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
                                    else
                                        self.events.Add(new SSOracleBehavior.PebblesConversation.PauseAndWaitForStillEvent(self, null, int.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
                                    //备注：文件中的时间约为代码中的8倍。
                                }
                            }
                            else if (array3.Length == 1 && array3[0].Length > 0)
                            {
                                self.events.Add(new Conversation.TextEvent(self, 0, array3[0], 0));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.Log("TEXT ERROR");
                self.events.Add(new Conversation.TextEvent(self, 0, "TEXT ERROR", 100));
                UnityEngine.Debug.LogException(e);
            }
        }

        public static void LoadEventsFromFile(RoomChatTx self, int fileName, string subfolderName, DialogBox dialogBox, string suffix = null, bool oneRandomLine = false, int randomSeed = 0)
        {
            Plugin.Log("~~~LOAD CONVO " + subfolderName + Path.DirectorySeparatorChar.ToString() + fileName.ToString() + (suffix == null ? "" : "-" + suffix.ToString()));
            InGameTranslator.LanguageID languageID = self.room.game.rainWorld.inGameTranslator.currentLanguage;
            string text;
            for (; ; )
            {
                text = AssetManager.ResolveFilePath(self.room.game.rainWorld.inGameTranslator.SpecificTextFolderDirectory(languageID) +
                       Path.DirectorySeparatorChar.ToString() + subfolderName +
                       Path.DirectorySeparatorChar.ToString() + fileName.ToString() + ".txt");
                if (suffix != null)
                {
                    string text2 = text;
                    text = AssetManager.ResolveFilePath(string.Concat(new string[]
                    {
                    self.room.game.rainWorld.inGameTranslator.SpecificTextFolderDirectory(languageID),
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
                if (suffix != null)
                {
                    string text2 = text;
                    text = AssetManager.ResolveFilePath(string.Concat(new string[]
                    {
                    self.room.game.rainWorld.inGameTranslator.SpecificTextFolderDirectory(languageID),
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
                    goto IL_117;
                }
                Plugin.Log("NOT FOUND " + text);
                if (!(languageID != InGameTranslator.LanguageID.English))
                {
                    break;
                }
                Plugin.Log("RETRY WITH ENGLISH");
                languageID = InGameTranslator.LanguageID.English;
            }
            return;
            IL_117:
            Plugin.Log("FOUND FILE!!! Load from flie: " + text);
            string text3 = File.ReadAllText(text, Encoding.UTF8);
            if (text3[0] != '0')
            {
                text3 = Custom.xorEncrypt(text3, 54 + fileName + (int)self.room.game.rainWorld.inGameTranslator.currentLanguage * 7);
            }
            string[] array = Regex.Split(text3, "\r\n");
            try
            {
                if (Regex.Split(array[0], "-")[1] == fileName.ToString())
                {
                    if (oneRandomLine)
                    {
                        List<string> lines = new List<string>();
                        for (int i = 1; i < array.Length; i++)
                        {
                            string[] array2 = LocalizationTranslator.ConsolidateLineInstructions(array[i]);
                            if (array2.Length >= 2 || (array2.Length == 1 && array2[0].Length > 0))
                                lines.Add(array[i]);
                        }
                        if (lines.Count > 0)
                        {
                            Random.State state = Random.state;
                            Random.InitState(randomSeed);
                            string item = lines[Random.Range(0, lines.Count)];
                            Random.state = state;

                            string[] line = LocalizationTranslator.ConsolidateLineInstructions(item);
                            if (line.Length == 3)
                            {
                                string[] array3 = Regex.Split(line[2], "<NEXT>");
                                if (array3.Length > 1)
                                    for (int j = 0; j < array3.Length; j++)
                                        self.events.Add(new RoomChatTx.TextEvent(self, array3[j], dialogBox, int.Parse(line[0], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(line[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
                            }
                            else if (line.Length == 2)
                            {
                                if (line[0] == "SPECEVENT")
                                {
                                    string[] array3 = Regex.Split(line[1], "<NEXT>");
                                    Plugin.Log("SPECEVENT : " + array3[0]);
                                    //self.events.Add(new Conversation.SpecialEvent(self, 0, array3[0]));
                                    if (array3.Length > 1)
                                    {//注意，j = 0是事件名，所以从 1 开始
                                        for (int j = 1; j < array3.Length; j++)
                                            self.events.Add(new RoomChatTx.TextEvent(self, array3[j], dialogBox, 0, 0));
                                    }
                                }
                                else if (line[0] == "PEBBLESWAIT")
                                {
                                    Plugin.Log("WAIT : " + int.Parse(line[1], NumberStyles.Any, CultureInfo.InvariantCulture));/*
                                    if (self is NSHConversation)
                                        self.events.Add(new NSHConversation.PauseAndWaitForStillEvent(self, null, int.Parse(line[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
                                    else
                                        self.events.Add(new SSOracleBehavior.PebblesConversation.PauseAndWaitForStillEvent(self, null, int.Parse(line[1], NumberStyles.Any, CultureInfo.InvariantCulture)));*/
                                    //备注：文件中的时间约为代码中的8倍。
                                }
                            }
                            else if (line.Length == 1 && line[0].Length > 0)
                            {
                                string[] array3 = Regex.Split(line[0], "<NEXT>");
                                for (int j = 0; j < array3.Length; j++)
                                {
                                    self.events.Add(new RoomChatTx.TextEvent(self, array3[j], dialogBox, 0, 0));
                                }
                            }
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
                                    self.events.Add(new RoomChatTx.TextEvent(self, array3[1], dialogBox, int.Parse(array3[0], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(array3[2], NumberStyles.Any, CultureInfo.InvariantCulture)));
                                }
                                else
                                {
                                    self.events.Add(new RoomChatTx.TextEvent(self, array3[2], dialogBox, int.Parse(array3[0], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
                                }
                            }/*
                            else if (array3.Length == 2)
                            {
                                if (array3[0] == "SPECEVENT")
                                {
                                    Plugin.Log("SPECEVENT : " + array3[1]);
                                    self.events.Add(new RoomChatTx.SpecialEvent(self, 0, array3[1]));
                                }
                                else if (array3[0] == "PEBBLESWAIT")
                                {
                                    Plugin.Log("WAIT : " + int.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture));
                                    if (self is NSHConversation)
                                        self.events.Add(new NSHConversation.PauseAndWaitForStillEvent(self, null, int.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
                                    else
                                        self.events.Add(new SSOracleBehavior.PebblesConversation.PauseAndWaitForStillEvent(self, null, int.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
                                    //备注：文件中的时间约为代码中的8倍。
                                }
                            }*/
                            else if (array3.Length == 1 && array3[0].Length > 0)
                            {
                                self.events.Add(new RoomChatTx.TextEvent(self, array3[0], dialogBox, 0));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.Log("TEXT ERROR");
                self.events.Add(new RoomChatTx.TextEvent(self, "TEXT ERROR", dialogBox, 0, 100));
                UnityEngine.Debug.LogException(e);
            }
        }

        public static SLOracleBehaviorHasMark.MiscItemType TypeOfMiscItem(PhysicalObject testItem)
        {
            if (testItem is NSHSwarmer)
            {
                return NSHMiscItemType.NSHSwarmer;
            }
            if (testItem is SSOracleSwarmer && //注意，NSH的神经元需要比FP的神经元先判断
                 OracleSwarmerHooks.OracleSwarmerData.TryGetValue(testItem as OracleSwarmer, out var nshOracleSwarmer) &&
                 nshOracleSwarmer.spawnRegion == "NSH")
            {
                return NSHMiscItemType.NSHOracleSwarmer;
            }
            if (testItem is SLOracleSwarmer ||
                (testItem is SSOracleSwarmer && 
                 OracleSwarmerHooks.OracleSwarmerData.TryGetValue(testItem as OracleSwarmer, out var slOracleSwarmer) &&
                 slOracleSwarmer.spawnRegion == "DM"))
            {
                return NSHMiscItemType.SLOracleSwarmer;
            }
            if (testItem is SSOracleSwarmer ||
                (testItem is SSOracleSwarmer &&
                 OracleSwarmerHooks.OracleSwarmerData.TryGetValue(testItem as OracleSwarmer, out var ssOracleSwarmer) &&
                 (ssOracleSwarmer.spawnRegion == "SS" || ssOracleSwarmer.spawnRegion == "RM" || ssOracleSwarmer.spawnRegion == "CL")))
            {
                return NSHMiscItemType.SSOracleSwarmer;
            }
            if (testItem is DataPearl)
            {
                return NSHMiscItemType.DataPearl;
            }
            if (testItem is SporePlant)
            {
                return NSHMiscItemType.SporePlant;
            }
            if (testItem is FlyLure)
            {
                return NSHMiscItemType.FlyLure;
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

        public static string ReplaceParts(Conversation self, SlugcatStats.Name currentSaveFile, string s)
        {
            NSHOracleState state = null;
            if (self is NSHConversation)
            {
                state = (self as NSHConversation).State;
            }
            s = Regex.Replace(s, "<PlayerName>", NameForPlayer(self, currentSaveFile, false));
            s = Regex.Replace(s, "<CapPlayerName>", NameForPlayer(self, currentSaveFile, true));
            s = Regex.Replace(s, "<ItemPlayerName>", ItemNameForPlayer(self, currentSaveFile, false));
            s = Regex.Replace(s, "<CapItemPlayerName>", ItemNameForPlayer(self, currentSaveFile, true));
            return s;
        }

        public static string NameForPlayer(Conversation self, SlugcatStats.Name currentSaveFile, bool capitalized)
        {
            string text = "little";
            string str = "one";
            if (currentSaveFile == Plugin.SlugName)
            {
                str = "messenger";
            }
            if (currentSaveFile == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
            {
                str = "pellet";
            }
            if (currentSaveFile.value == "Outsider")
            {
                str = "moth";
            }
            if (capitalized && InGameTranslator.LanguageID.UsesCapitals(self.interfaceOwner.rainWorld.inGameTranslator.currentLanguage))
            {
                text = char.ToUpper(text[0]).ToString() + text.Substring(1);
            }
            return Custom.rainWorld.inGameTranslator.Translate(text + " " + str);
        }

        public static string ItemNameForPlayer(Conversation self, SlugcatStats.Name currentSaveFile, bool capitalized, NSHOracleState state = null)
        {
            string text = "little";
            string str = "one";
            int i = 0;
            if (state != null && (state.GetOpinion == NSHOracleState.PlayerOpinion.Likes || state.totalPearlsBrought > 5))
                i = Random.Range(0, 5);
            else
                i = Random.Range(0, 2);
            switch (i)
            {
                case 0:
                    str = "Wanderer";
                    break;
                case 1:
                    str = "Garbage Worm";
                    break;
                case 2:
                    str = "Apprentice";
                    break;
                case 3:
                    str = "Scholar";
                    break;
                default:
                    str = "Naturalist";
                    break;
            }
            if (capitalized && InGameTranslator.LanguageID.UsesCapitals(self.interfaceOwner.rainWorld.inGameTranslator.currentLanguage))
            {
                text = char.ToUpper(text[0]).ToString() + text.Substring(1);
            }
            return Custom.rainWorld.inGameTranslator.Translate(text + " " + str);
        }

        public override void Update()
        {
            this.age++;
            if (this.waitForStill)
            {
                if (!this.convBehav.CurrentlyCommunicating && this.convBehav.communicationPause > 0)
                {
                    this.convBehav.communicationPause--;
                }
                if (!this.convBehav.CurrentlyCommunicating && this.convBehav.communicationPause < 1 && this.owner.allStillCounter > 20)
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
            return ReplaceParts(this, this.currentSaveFile, this.owner.Translate(s));
        }

        public NSHOracleBehaviour owner;
        public NSHConversationBehaviour convBehav;
        public SLOracleBehaviorHasMark.MiscItemType describeItem;
        public bool waitForStill;
        public int age;
        public delegate void AddEventDelegate(Conversation.ID id, CustomOracleBehaviour owner);

        public class PauseAndWaitForStillEvent : Conversation.DialogueEvent
        {
            public PauseAndWaitForStillEvent(Conversation owner, NSHConversationBehaviour _convBehav, int pauseFrames) : base(owner, 0)
            {
                this.convBehav = _convBehav;
                if (this.convBehav == null && owner is NSHConversation)
                {
                    this.convBehav = (owner as NSHConversation).convBehav;
                }
                this.pauseFrames = pauseFrames;
            }

            public override void Activate()
            {
                base.Activate();
                if (this.convBehav != null)
                {
                    this.convBehav.communicationPause = this.pauseFrames;
                    if (owner is NSHConversation)
                        (this.owner as NSHConversation).waitForStill = true;
                }
            }

            public NSHConversationBehaviour convBehav;
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
            NSHConversation.LoadEventsFromFile(this, 0, "NSH", dialogBox, "Dream-2");/*
            this.events.Add(new RoomChatTx.TextEvent(this, base.Translate("NSH: This time I'll set you up with a lizard."), dialogBox, 0, this.extraLingerFactor * 50));
            this.events.Add(new RoomChatTx.TextEvent(this, base.Translate("NSH: Keep your spirits up, soon after the situation you're facing would be far more dangerous than this."), dialogBox, 0, this.extraLingerFactor * 100));
            this.events.Add(new RoomChatTx.TextEvent(this, base.Translate("NSH: We need to ensure that everything is flawless."), dialogBox, 0, this.extraLingerFactor * 50));*/
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
                    NSHConversation.LoadEventsFromFile(this, 0, "NSH", dialogBox, "Dream-2-0");/*
                    this.events.Add(new RoomChatTx.TextEvent(this, base.Translate("NSH: Good. Hope you can maintain the same level of proficiency and caution along the way."), dialogBox, 0, this.extraLingerFactor * 90));*/
                    break;
                case 1:
                case 2:
                    NSHConversation.LoadEventsFromFile(this, 0, "NSH", dialogBox, "Dream-2-1");/*
                    this.events.Add(new RoomChatTx.TextEvent(this, base.Translate("NSH: Well done, but still, you need my help."), dialogBox, 0, this.extraLingerFactor * 50)) ;
                    this.events.Add(new RoomChatTx.TextEvent(this, base.Translate("NSH: This can't be satisfying, isn't it?"), dialogBox, 0, this.extraLingerFactor * 100));
                    this.events.Add(new RoomChatTx.TextEvent(this, base.Translate("NSH: There are not many opportunities left for us."), dialogBox, 0, this.extraLingerFactor * 50));*/
                    break;
                case 3:
                case 4:
                case 5:
                    NSHConversation.LoadEventsFromFile(this, 0, "NSH", dialogBox, "Dream-2-2");/*
                    this.events.Add(new RoomChatTx.TextEvent(this, base.Translate("NSH: Calm down, your body have far more protential than you think."), dialogBox, 0, this.extraLingerFactor * 50));
                    this.events.Add(new RoomChatTx.TextEvent(this, base.Translate("NSH: Learn to manipulate it perfectly. It will not be difficult for your race."), dialogBox, 0, this.extraLingerFactor * 100));
                    this.events.Add(new RoomChatTx.TextEvent(this, base.Translate("NSH: Concentrate, we just need more time."), dialogBox, 0, this.extraLingerFactor * 50));*/
                    break;
                default:
                    NSHConversation.LoadEventsFromFile(this, 0, "NSH", dialogBox, "Dream-2-3");/*
                    this.events.Add(new RoomChatTx.TextEvent(this, base.Translate("NSH: ..."), dialogBox, 0, this.extraLingerFactor * 10));
                    this.events.Add(new RoomChatTx.TextEvent(this, base.Translate("NSH: Are you acting out?"), dialogBox, 0, this.extraLingerFactor * 40));
                    this.events.Add(new RoomChatTx.TextEvent(this, base.Translate("NSH: I have no intention of making things ugly for us. A quarter rain cycle - I hope you can recover later."), dialogBox, 0, this.extraLingerFactor * 100));
                    this.events.Add(new RoomChatTx.TextEvent(this, base.Translate("NSH: My messenger, I have no other choice on this matter. I can only hope that you understands."), dialogBox, 0, this.extraLingerFactor * 100));*/
                    break;
            }
            //this.events.Add(new RoomChatTx.TextEvent(this, base.Translate("Don't forget to watch your step when climbing those coolers."), dialogBox, this.extraLingerFactor * 80, SoundID.SL_AI_Talk_4, 40));
            base.AddTextEvents(dialogBox);
        }
    }
}
