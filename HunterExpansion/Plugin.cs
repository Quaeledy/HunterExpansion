using BepInEx;
using CustomDreamTx;
using CustomOracleTx;
using CustomSaveTx;
using HunterExpansion.CustomCollections;
using HunterExpansion.CustomDream;
using HunterExpansion.CustomEffects;
using HunterExpansion.CustomEnding;
using HunterExpansion.CustomOracle;
using HunterExpansion.CustomSave;
using HunterExpansion.HRTalk;
using MoreSlugcats;
using System;
using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace HunterExpansion
{
    [BepInPlugin("Quaeledy.hunterexpansion", "Hunter Expansion", "1.1.2")]
    public class Plugin : BaseUnityPlugin
    {
        static public readonly string MOD_ID = "Quaeledy.hunterexpansion";
        static public SlugcatStats.Name SlugName = SlugcatStats.Name.Red;
        public static SlugcatStats.Name AllSlugcats = new SlugcatStats.Name("AllSlugcat", false);
        private bool IsInit;

        public static bool ripSRS = false;
        //public static bool ripSRS = true;
        //public static bool gateLock = true;

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
        //把通往墙的业力门挪到右边

        //待测试：
        //音效（已测圣猫倒地音效）
        //禁止猫崽在NSH区域生成
        //禁止探险做梦
        //传送梦境适用于所有猫


        //AddRoomSpecificScript要注意roomsettings里禁止的问题
        //离群从DGL去SU，却传送到了OE（在业力门存档会默认于其中一个区域生成玩家）
        //与Extended Collectibles Tracker可能冲突，梦境结束时会卡在雨眠界面
        //与Sharpener可能冲突，会让cg变得很小


        //应该没什么问题：
        //nsh死亡后闭眼
        //nsh对蝙蝠草、扯线蜂窝、moon神经元、fp神经元、nsh神经元解读
        //moon对nsh神经元、绿珍珠解读
        //NSH区域的独有神经元
        //带猫崽的结局差分


        //已测试：
        //测试一下矛、精英拾荒面具、NSH房间的珍珠对不同猫的差分能否触发
        //猎手出发梦境中，NSH能否正常给出神经元
        //测试对战蜥蜴梦境是否能正常显示对话
        //测试打断解读重新给物品时拒绝解读能否触发
        //过门传送成功了！
        //绿珍珠反复生成的问题
        //绿珍珠是否被追踪
        //红猫带猫崽回nsh，统计之后猫崽消失
        //广播改成一句一行
        //mod猫自定义回程落脚点有问题
        //当着NSH的面吃神经元会让NSH有不同反应

        //已知bug：
        //竞技场选择部分地图（已知有pit）开始游戏后会白屏卡死，挑战模式部分挑战（已知有挑战23、29）会卡死
        //在第二个梦境被nsh杀死后第三个梦境就没有nsh的对话了
        //回到nsh后如果被电猫拍拍死会进入没有回到nsh的CG
        //梦境无稽烦忧在搓神经元的时候拿了神经元就卡了，可能是玩家2拿了的原因？
        //nsh墙方向的业力门不知为何可被激活（更快业力门？），但会卡死
        //没有结局的水猫见nsh好像有bug，画面卡着不动了，但nsh在正常说话
        //可能与Sharpener冲突

        //其他语言的翻译

        //无法触发：
        //带珍珠过业力门卡死
        //roof03夜晚仍如白天
        //怪猫好像没办法从roof3进入roof2

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
                RegionGateHooks.InitIL();
                SLOracleBehaviorHooks.InitIL();
                //修bug相关
                BugFix.Init();
                //魔方节点相关
                HRTalkHooks.Init();
                //迭代器相关
                OracleSwarmerHooks.Init();
                OverseerHooks.Init();
                SLOracleBehaviorHooks.Init();
                //梦境相关
                NSHSwarmerHooks.Init();
                PlayerHooks.Init();
                RoomCameraHooks.Init();
                DaddyLongLegsHooks.Init();
                //结局相关
                EndingScenes.Init();
                EndingSession.Init();
                //区域和业力门相关
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
                //塌掉的地底-外层空间业力门
                RegionGateHooks.Init();
                //绿珍珠的特效
                FixedDataPearlEffect.HooksOn();

                //基于EmgTx的内容
                CustomDreamRx.ApplyTreatment(new HunterDreamRegistry());
                CustomDreamRx.ApplyTreatment(new TravelDreamRegistry());
                CustomOracleRx.ApplyTreatment(new NSHOracleRegistry());
                DeathPersistentSaveDataRx.AppplyTreatment(new FondDreamCompletedSave(Plugin.SlugName));
                DeathPersistentSaveDataRx.AppplyTreatment(new IntroTextSave(Plugin.SlugName));
                DeathPersistentSaveDataRx.AppplyTreatment(new PearlFixedSave(Plugin.SlugName));
                DeathPersistentSaveDataRx.AppplyTreatment(new RipNSHSave(MoreSlugcatsEnums.SlugcatStatsName.Saint));
                DeathPersistentSaveDataRx.AppplyTreatment(new TravelCompletedSave(Plugin.AllSlugcats));
                DeathPersistentSaveDataRx.AppplyTreatment(new AquamarinePearlTokenSave(Plugin.AllSlugcats));
                DeathPersistentSaveDataRx.AppplyTreatment(new OracleSwarmerRegionSave(Plugin.AllSlugcats));

                //这是附送的SRS
                CustomOracleRx.ApplyTreatment(new SRSOracleRegistry());

                HunterExpansionEnums.RegisterAllEnumExtensions();
                this.LoadResources(self);

                UnityEngine.Debug.Log($"Plugin {Plugin.MOD_ID} is loaded!");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                throw;
            }
        }

        public static void Log(string m)
        {
            UnityEngine.Debug.Log("[HunterExpansion] " + m);
        }

        public static void Log(string f, params object[] args)
        {
            UnityEngine.Debug.Log("[HunterExpansion] " + string.Format(f, args));
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