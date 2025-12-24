using BepInEx.Logging;
using HUD;
using HunterExpansion.CustomSave;
using Menu;
using MonoMod.RuntimeDetour;
using MoreSlugcats;
using System.Globalization;
using System.Reflection;

namespace HunterExpansion
{
    class HUDHooks
    {
        private static BindingFlags propFlags = BindingFlags.Instance | BindingFlags.Public;
        private static BindingFlags methodFlags = BindingFlags.Static | BindingFlags.Public;
        public delegate bool orig_RainCycle_RegionHidesTimer(RainCycle self);

        public static HunterMessionHud hunterHud;

        public static void Init()
        {
            On.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHud;
            On.HUD.HUD.Update += HUD_Update;

            On.HUD.Map.ctor += Map_ctor;

            Hook hook = new Hook(typeof(RainCycle).GetProperty("RegionHidesTimer", HUDHooks.propFlags).GetGetMethod(), typeof(HUDHooks).GetMethod("RainCycle_get_RegionHidesTimer", HUDHooks.methodFlags));
        }
        //给NSH区域搭配无雨的雨计时UI
        public static bool RainCycle_get_RegionHidesTimer(HUDHooks.orig_RainCycle_RegionHidesTimer orig, RainCycle self)
        {
            bool result = orig(self);

            if (!self.world.game.IsStorySession)
            {
                return result;
            }
            if (ModManager.MMF && !MMF.cfgHideRainMeterNoThreat.Value)
            {
                return result;
            }
            if (ModManager.MSC && self.world.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                return result;
            }
            if (self.world.region == null)
            {
                return result;
            }
            if (self.world.region.name == "NSH")
            {
                return true;
            }
            return result;
        }

        //业力门封锁图案
        public static void Map_ctor(On.HUD.Map.orig_ctor orig, Map self, HUD.HUD hud, Map.MapData mapData)
        {
            orig(self, hud, mapData);
            SaveState saveState = null;
            if (hud.owner.GetOwnerType() == HUD.HUD.OwnerType.Player || hud.owner.GetOwnerType() == HUD.HUD.OwnerType.FastTravelScreen || hud.owner.GetOwnerType() == HUD.HUD.OwnerType.RegionOverview || (ModManager.MSC && hud.owner.GetOwnerType() == MoreSlugcatsEnums.OwnerType.SafariOverseer))
            {
                saveState = hud.rainWorld.progression.currentSaveState;
            }
            else if (hud.owner.GetOwnerType() != HUD.HUD.OwnerType.RegionOverview && hud != null && hud.owner != null && hud.owner is SleepAndDeathScreen)
            {
                saveState = (hud.owner as SleepAndDeathScreen).saveState;
            }
            if (mapData.regionName == "NSH")
            {
                for (int k = 0; k < mapData.gateData.Length; k++)
                {
                    bool flag3 = false;
                    int num;
                    if (mapData.gateData[k].karma == null)
                    {
                        flag3 = true;
                    }
                    else if (int.TryParse(mapData.gateData[k].karma.value, NumberStyles.Any, CultureInfo.InvariantCulture, out num))
                    {
                        flag3 = (num - 1 <= mapData.currentKarma);
                    }
                    RegionGate.GateRequirement karma = mapData.gateData[k].karma;
                    if (saveState == null || (hud.owner.GetOwnerType() != HUD.HUD.OwnerType.Player && hud.owner.GetOwnerType() != HUD.HUD.OwnerType.SleepScreen && hud.owner.GetOwnerType() != HUD.HUD.OwnerType.DeathScreen))
                    {
                        flag3 = true;
                    }
                    else if (ModManager.MSC)
                    {
                        if (RainWorld.ShowLogs)
                        {
                            string str = "gate condition on map, karma ";
                            RegionGate.GateRequirement karma2 = mapData.gateData[k].karma;
                            Debug.Log(str + ((karma2 != null) ? karma2.ToString() : null));
                        }
                        if (mapData.gateData[k].karma == HunterExpansionEnums.GateRequirement.NSHLock &&
                            self.mapData.NameOfRoom(mapData.gateData[k].roomIndex) == "GATE_NSH_DGL")
                        {
                            //去掉旧的
                            for (int m = self.mapObjects.Count - 1; m >= 0; m--)
                            {
                                if (self.mapObjects[m] is Map.GateMarker &&
                                    (self.mapObjects[m] as Map.GateMarker).room == mapData.gateData[k].roomIndex)
                                {
                                    self.mapObjects.RemoveAt(m);
                                }
                            }
                            //加上新的
                            flag3 = false;
                            if (HunterExpansionEnums.GateRequirement.customNSHGateRequirements(saveState))
                            {
                                flag3 = true;
                            }
                            if (!flag3)
                            {
                                karma = MoreSlugcatsEnums.GateRequirement.OELock;
                            }
                            else if (self.RegionName == "NSH")
                            {
                                karma = RegionGate.GateRequirement.OneKarma;
                            }
                            else
                            {
                                karma = RegionGate.GateRequirement.OneKarma;
                            }
                            if (RainWorld.ShowLogs)
                            {
                                Plugin.Log("NSH gate condition on map " + flag3.ToString());
                            }
                            self.mapObjects.Add(new Map.GateMarker(self, mapData.gateData[k].roomIndex, karma, flag3));
                        }
                    }
                }
            }
        }

        #region 开局提示
        public static void HUD_Update(On.HUD.HUD.orig_Update orig, HUD.HUD self)
        {
            orig(self);
            if (hunterHud != null)
                hunterHud.Update();
        }

        private static void HUD_InitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam)
        {
            orig(self, cam);

            //判断是否为猎手
            if (self.owner is Player && (self.owner as Player).abstractCreature.world.game.session.characterStats.name == Plugin.SlugName)
            {
                if (hunterHud != null)
                    hunterHud.Destroy();
                Plugin.Log("Try spawn HunterMessionHud.");
                hunterHud = new HunterMessionHud(self);
            }
        }
        #endregion
    }

    class HunterMessionHud
    {
        public HunterMessionHud(HUD.HUD owner)
        {
            this.owner = owner;
        }

        public void Update()
        {
            if (owner == null)
                return;
            if (!(owner.owner is Player))
            {
                Destroy();
                return;
            }

            var room = (owner.owner as Player).room;
            if (room == null)
                return;

            //开局提示
            if (!IntroTextSave.introText[0] && room.abstractRoom .name == "LF_H01")
            {
                room.AddObject(new IntroText1(room));
                IntroTextSave.introText[0] = true;
            }
        }

        public void Destroy()
        {
            if (_hud != null)
                _hud.slatedForDeletion = true;
            owner = null;
            _hud = null;
        }

        MissionHud _hud;
        HUD.HUD owner;
    }

    class MissionHud : HudPart
    {
        public MissionHud(HUD.HUD hud, float aacc, ManualLogSource log) : base(hud)
        {
            acc[1] = aacc;
            _log = log;
        }
        public FSprite[] sprites;
        readonly float[] acc = new float[2] { 1f, 0.0f };
        readonly ManualLogSource _log;
    }
}
