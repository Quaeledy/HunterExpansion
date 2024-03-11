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
        //音效(等合作者做完）
        //魔方结点是否有变化
        //矛大师朝向出口的速度
        //禁止猫崽在NSH区域生成

        //已测试：
        //修复了NSH区域的其他业力门也能触发结局的问题
        //多人联机时，播放结局cg后，无法传送到nsh，仍在业力门房间
        //多人联机时，传送到nsh后不能进入结算界面
        //多人联机时，梦境结束会卡在雨眠界面，雨眠cg疯狂抖动（应该是emgtx的问题）
        //第一次睡觉以后，挨饿再睡一觉，见完nsh后会回到挨饿前，然后再挨饿睡会再次进这个梦境，然后nsh说完话不动
        //修复了与亡命徒的冲突

        //已知bug：
        //没有睡过避难所的情况下，触发结局会让玩家的位置回到最开头
        //warp传送到sb_oe业力门时，先传送到oe区域，再想传送到sb区域，会传送不了
        //红猫传送到NSH房间疑似会死亡

        //CRS:
        //加入了依赖项
        //Landscape，标题，Safari图标，所有猫可选Safari

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
                CustomOracleRx.ApplyTreatment(new SRSOracleRegistry());

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