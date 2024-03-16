using BepInEx;
using System;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using MoreSlugcats;
using CustomOracleTx;
using CustomDreamTx;
using CustomSaveTx;
using HunterExpansion.CustomOracle;
using HunterExpansion.CustomDream;
using HunterExpansion.CustomSave;
using HunterExpansion.CustomEnding;
using HunterExpansion.HRTalk;
using HunterExpansion.CustomCollections;
using HunterExpansion.CustomEffects;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace HunterExpansion
{
    [BepInPlugin("Quaeledy.hunterexpansion", "Hunter Expansion", "0.0.1")]
    public class Plugin : BaseUnityPlugin
    {
        static public readonly string MOD_ID = "Quaeledy.hunterexpansion";
        static public SlugcatStats.Name SlugName = SlugcatStats.Name.Red;
        private bool IsInit;

        public static bool ripSRS = false;
        public static bool gateLock = true;

        // Add hooks
        public void OnEnable()
        {
            //On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
            // Put your custom hooks here!
            On.RainWorld.OnModsInit += new On.RainWorld.hook_OnModsInit(this.RainWorld_OnModsInit);
        }

        //待完成事项：
        //文本修改（等校对）
        //文本加密（记得发布前去掉注释加密一遍）
        //优化NSH的好感度系统
        //NSH的内置小游戏

        //待测试：
        //音效（已测圣猫倒地音效）
        //禁止猫崽在NSH区域生成

        //已测试：
        //

        //已知bug：
        //


        //CRS:
        //加入了依赖项
        //Landscape，标题，Safari图标，所有猫可选Safari，广播，珍珠

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig.Invoke(self);

            if (IsInit) return;
            IsInit = true;

            try
            {
                //各种IL
                HRTalkHooks.InitIL();
                CollectionsMenuHooks.InitIL();
                BugFix.InitIL();
                //修bug相关
                BugFix.Init();
                //魔方节点相关
                HRTalkHooks.Init();
                //迭代器相关
                OverseerHooks.Init();
                OracleBehaviorHooks.Init();
                //梦境相关
                NSHSwarmerHooks.Init();
                PlayerHooks.Init();
                RoomCameraHooks.Init();
                //结局相关
                EndingScenes.Init();
                EndingSession.Init();
                RegionHooks.Init();
                //存档相关
                DecipheredNSHPearlsSave.Init();
                NSHOracleStateSave.Init();
                //提示相关
                HUDHooks.Init();
                //收藏相关
                CollectionsMenuHooks.Init();
                //墙顶城市和猫崽生成
                WorldHooks.Init();
                //房间特殊事件
                RoomSpecificScript.Init();

                //绿珍珠的特效
                FixedDataPearlEffect.HooksOn();

                //基于EmgTx的内容
                CustomDreamRx.ApplyTreatment(new HunterDreamRegistry());
                CustomOracleRx.ApplyTreatment(new NSHOracleRegistry());
                DeathPersistentSaveDataRx.AppplyTreatment(new FondDreamCompletedSave(Plugin.SlugName));
                DeathPersistentSaveDataRx.AppplyTreatment(new IntroTextSave(Plugin.SlugName));
                DeathPersistentSaveDataRx.AppplyTreatment(new PearlFixedSave(Plugin.SlugName));
                DeathPersistentSaveDataRx.AppplyTreatment(new RipNSHSave(MoreSlugcatsEnums.SlugcatStatsName.Saint));
                //这是附送的SRS
                //CustomOracleRx.ApplyTreatment(new SRSOracleRegistry());

                HunterExpansionEnums.RegisterAllEnumExtensions();
                this.LoadResources(self);

                Debug.Log($"Plugin {Plugin.MOD_ID} is loaded!");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                throw;
            }
        }

        public static void Log(string m)
        {
            Debug.Log("[HunterExpansion] " + m);
        }

        public static void Log(string f, params object[] args)
        {
            Debug.Log("[HunterExpansion] " + string.Format(f, args));
        }

        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
            Futile.atlasManager.LoadAtlas("atlases/signNSH");
            Futile.atlasManager.LoadAtlas("atlases/signSRS");
            Futile.atlasManager.LoadAtlas("atlases/guidanceNSH");
            Futile.atlasManager.LoadAtlas("atlases/smallKarmaNoRingNSH");
            Futile.atlasManager.LoadAtlas("atlases/gateSymbolNSH");
            Futile.atlasManager.LoadAtlas("atlases/NSHRibbonTex");
        }
    }
}