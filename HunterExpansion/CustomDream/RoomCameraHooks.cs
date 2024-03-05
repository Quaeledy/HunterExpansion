using CustomDreamTx;
using RWCustom;
using UnityEngine;
using MoreSlugcats;
using HunterExpansion.CustomSave;
using HunterExpansion.CustomEnding;
using HunterExpansion.CustomDream;
using System.Collections.Generic;

namespace HunterExpansion
{
    public class RoomCameraHooks
    {
        public static void Init()
        {
            On.RoomCamera.DrawUpdate += RoomCamera_DrawUpdate;
            On.RoofTopView.ctor += RoofTopView_ctor;
        }

        public static void RoomCamera_DrawUpdate(On.RoomCamera.orig_DrawUpdate orig, RoomCamera self, float timeStacker, float timeSpeed)
        {
            orig(self, timeStacker, timeSpeed);
            if (CustomDreamRx.currentActivateNormalDream != null &&
                (CustomDreamRx.currentActivateNormalDream.activateDreamID == DreamID.HunterDream_2 ||
                 CustomDreamRx.currentActivateNormalDream.activateDreamID == DreamID.HunterDream_4 ||
                 CustomDreamRx.currentActivateNormalDream.activateDreamID == DreamID.HunterDream_5))
            {
                Vector2 vector = Vector2.Lerp(self.lastPos, self.pos, timeStacker);
                if (self.microShake > 0f)
                {
                    vector += Custom.RNV() * 8f * self.microShake * Random.value;
                }
                if (!self.voidSeaMode)
                {
                    vector.x = Mathf.Clamp(vector.x, self.CamPos(self.currentCameraPosition).x + self.hDisplace + 8f - 20f, self.CamPos(self.currentCameraPosition).x + self.hDisplace + 8f + 20f);
                    vector.y = Mathf.Clamp(vector.y, self.CamPos(self.currentCameraPosition).y + 8f - 7f - (self.splitScreenMode ? 192f : 0f), self.CamPos(self.currentCameraPosition).y + 33f + (self.splitScreenMode ? 192f : 0f));
                }
                else
                {
                    vector.y = Mathf.Max(vector.y, self.room.PixelHeight + 128f);
                }
                vector = new Vector2(Mathf.Floor(vector.x), Mathf.Floor(vector.y));
                vector.x -= 0.02f;
                vector.y -= 0.02f;
                vector += self.offset;
                vector += self.hardLevelGfxOffset;
                //对战蜥蜴的重力回归
                if (CustomDreamRx.currentActivateNormalDream.activateDreamID == DreamID.HunterDream_2)
                {
                    if (self.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.ZeroG) == null)
                    {
                        self.room.roomSettings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.ZeroG, 1f, false));
                    }
                    if (self.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.ZeroGSpecks) == null)
                    {
                        self.room.roomSettings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.ZeroGSpecks, 1f, false));
                    }
                    if (PlayerHooks.lizardSpawn >= 50)
                    {
                        if (self.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.ZeroG).amount > 0f)
                            self.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.ZeroG).amount -= 0.01f;
                        if (self.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.ZeroGSpecks).amount > 0f)
                            self.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.ZeroGSpecks).amount -= 0.01f;
                    }
                }
                //美梦的虚空海特效
                if (CustomDreamRx.currentActivateNormalDream.activateDreamID == DreamID.HunterDream_5)
                {
                    if (self.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.VoidMelt) == null)
                    {
                        self.room.roomSettings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.VoidMelt, 0f, false));

                        self.SetUpFullScreenEffect("Bloom");
                        self.fullScreenEffect.shader = self.game.rainWorld.Shaders["LevelMelt2"];
                        self.lightBloomAlphaEffect = RoomSettings.RoomEffect.Type.VoidMelt;
                        self.fullScreenEffect.alpha = self.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.VoidMelt);
                    }
                    if (self.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Hue) == null)
                    {
                        self.room.roomSettings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.Hue, 0f, false));
                    }
                    if (self.lightBloomAlphaEffect == RoomSettings.RoomEffect.Type.VoidMelt && self.fullScreenEffect != null)
                    {
                        if (!CustomDreamRx.currentActivateNormalDream.dreamFinished)
                        {
                            if (self.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.VoidMelt).amount < 0.85f)
                                self.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.VoidMelt).amount += 0.01f;
                            if (self.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Hue).amount < 0.65f)
                                self.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Hue).amount += 0.01f;
                        }
                        else
                        {
                            if (self.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.VoidMelt).amount < 0.99f)
                                self.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.VoidMelt).amount += 0.01f;
                        }
                    }
                }
                //噩梦的腐化特效
                if (CustomDreamRx.currentActivateNormalDream.activateDreamID == DreamID.HunterDream_4)
                {
                    //生成特效
                    if (CustomDreamRx.currentActivateNormalDream.dreamFinished)
                    {
                        for (int l = 0; l < 2; l++)
                        {
                            self.room.AddObject(new MeltLights.MeltLight(1f, self.room.RandomPos(), self.room, Color.red));
                        }
                    }
                }
            }
        }

        private static void RoofTopView_ctor(On.RoofTopView.orig_ctor orig, RoofTopView self, Room room, RoomSettings.RoomEffect effect)
        {
            if ((room.world.region != null && room.world.region.name == "NSH") || room.abstractRoom.name.StartsWith("NSH_"))
            {
                string str = "_NSH";
                self.floorLevel = 26f;
                self.room = room;
                self.elements = new List<BackgroundScene.BackgroundSceneElement>();
                self.atmosphereColor = new Color(0.16078432f, 0.23137255f, 0.31764707f);
                self.effect = effect;
                self.sceneOrigo = self.RoomToWorldPos(room.abstractRoom.size.ToVector2() * 10f) + Vector2.down * 250f;
                room.AddObject(new RoofTopView.DustpuffSpawner());
                self.daySky = new BackgroundScene.Simple2DBackgroundIllustration(self, "Rf_Sky" + str, new Vector2(683f, 384f));
                self.duskSky = new BackgroundScene.Simple2DBackgroundIllustration(self, "Rf_DuskSky" + str, new Vector2(683f, 384f));
                self.nightSky = new BackgroundScene.Simple2DBackgroundIllustration(self, "Rf_NightSky" + str, new Vector2(683f, 384f));
                self.isLC = false;//(ModManager.MSC && ((room.world.region != null && room.world.region.name == "LC") || self.room.abstractRoom.name.StartsWith("LC_")));
                bool flag2 = false;
                if (self.room.dustStorm)
                {
                    self.dustWaves = new List<RoofTopView.DustWave>();
                    float num = 2500f;
                    float num2 = 0f;
                    self.dustWaves.Add(new RoofTopView.DustWave(self, "RF_CityA" + str, new Vector2(self.PosFromDrawPosAtNeutralCamPos(new Vector2(300f + (flag2 ? -300f : 0f), 0f), -num2).x, self.floorLevel / 4f - num * 40f), 370f, 0f));
                    self.dustWaves.Add(new RoofTopView.DustWave(self, "RF_CityA" + str, new Vector2(self.PosFromDrawPosAtNeutralCamPos(new Vector2(300f + (flag2 ? 300f : 0f), 0f), -num2).x, self.floorLevel / 5f - num * 30f), 290f, 0f));
                    self.dustWaves.Add(new RoofTopView.DustWave(self, "RF_CityA" + str, new Vector2(self.PosFromDrawPosAtNeutralCamPos(new Vector2(300f + (flag2 ? -300f : 0f), 0f), -num2).x, self.floorLevel / 6f - num * 20f), 210f, 0f));
                    self.dustWaves.Add(new RoofTopView.DustWave(self, "RF_CityA" + str, new Vector2(self.PosFromDrawPosAtNeutralCamPos(new Vector2(300f + (flag2 ? -300f : 0f), 0f), -num2).x, self.floorLevel / 7f - num * 10f), 130f, 0f));
                    RoofTopView.DustWave dustWave = new RoofTopView.DustWave(self, "RF_CityA" + str, new Vector2(self.PosFromDrawPosAtNeutralCamPos(new Vector2(300f + (flag2 ? -300f : 0f), 0f), -num2).x, self.floorLevel / 8f), 50f, 0f);
                    dustWave.isTopmost = true;
                    self.dustWaves.Add(dustWave);
                    foreach (RoofTopView.DustWave element in self.dustWaves)
                    {
                        self.AddElement(element);
                    }
                }/*
                bool isLC = self.isLC;
                if (isLC)
                {
                    self.daySky = new BackgroundScene.Simple2DBackgroundIllustration(self, "AtC_Sky", new Vector2(683f, 384f));
                    self.duskSky = new BackgroundScene.Simple2DBackgroundIllustration(self, "AtC_DuskSky", new Vector2(683f, 384f));
                    self.nightSky = new BackgroundScene.Simple2DBackgroundIllustration(self, "AtC_NightSky", new Vector2(683f, 384f));
                    self.AddElement(self.nightSky);
                    self.AddElement(self.duskSky);
                    self.AddElement(self.daySky);
                    self.floorLevel = self.room.world.RoomToWorldPos(new Vector2(0f, 0f), self.room.abstractRoom.index).y - 30992.8f;
                    self.floorLevel *= 22f;
                    self.floorLevel = -self.floorLevel;
                    float num3 = self.room.world.RoomToWorldPos(new Vector2(0f, 0f), self.room.abstractRoom.index).x - 11877f;
                    num3 *= 0.01f;
                    Shader.SetGlobalVector("_AboveCloudsAtmosphereColor", self.atmosphereColor);
                    Shader.SetGlobalVector("_MultiplyColor", Color.white);
                    Shader.SetGlobalVector("_SceneOrigoPosition", self.sceneOrigo);
                    self.AddElement(new RoofTopView.Building(self, "city2", new Vector2(self.PosFromDrawPosAtNeutralCamPos(new Vector2(880f, 0f), 200f - num3).x, self.floorLevel * 0.2f - 170000f), 420.5f, 2f));
                    self.AddElement(new RoofTopView.Building(self, "city1", new Vector2(self.PosFromDrawPosAtNeutralCamPos(new Vector2(880f, 0f), 70f - num3 * 0.5f).x, self.floorLevel * 0.25f - 116000f), 340f, 2f));
                    self.AddElement(new RoofTopView.Building(self, "city3", new Vector2(self.PosFromDrawPosAtNeutralCamPos(new Vector2(880f, 0f), 70f - num3 * 0.5f).x, self.floorLevel * 0.3f - 85000f), 260f, 2f));
                    self.AddElement(new RoofTopView.Building(self, "city2", new Vector2(self.PosFromDrawPosAtNeutralCamPos(new Vector2(880f, 0f), 40f - num3 * 0.5f).x, self.floorLevel * 0.35f - 42000f), 180f, 2f));
                    self.AddElement(new RoofTopView.Building(self, "city1", new Vector2(self.PosFromDrawPosAtNeutralCamPos(new Vector2(880f, 0f), 90f - num3 * 0.2f).x, self.floorLevel * 0.4f + 5000f), 100f, 2f));
                    self.AddElement(new RoofTopView.Floor(self, "floor", new Vector2(0f, self.floorLevel * 0.2f - 90000f), 400.5f, 500.5f));
                }
                else
                {*/
                self.AddElement(self.nightSky);
                self.AddElement(self.duskSky);
                self.AddElement(self.daySky);
                Shader.SetGlobalVector("_MultiplyColor", Color.white);
                self.AddElement(new RoofTopView.Floor(self, "floor" + str, new Vector2(0f, self.floorLevel), 1f, 12f));
                Shader.SetGlobalVector("_AboveCloudsAtmosphereColor", self.atmosphereColor);
                Shader.SetGlobalVector("_SceneOrigoPosition", self.sceneOrigo);
                for (int i = 0; i < 16; i++)
                {
                    float f = (float)i / 15f;
                    self.AddElement(new RoofTopView.Rubble(self, "Rf_Rubble" + str, new Vector2(0f, self.floorLevel), Mathf.Lerp(1.5f, 8f, Mathf.Pow(f, 1.5f)), i));
                }
                self.AddElement(new RoofTopView.DistantBuilding(self, "Rf_HoleFix" + str, new Vector2(-2676f, 9f), 1f, 0f));
                self.AddElement(new RoofTopView.Building(self, "city2", new Vector2(self.PosFromDrawPosAtNeutralCamPos(new Vector2(1780f, 0f), 11.5f).x, self.floorLevel), 11.5f, 3f));
                self.AddElement(new RoofTopView.Building(self, "city1", new Vector2(self.PosFromDrawPosAtNeutralCamPos(new Vector2(880f, 0f), 10.5f).x, self.floorLevel), 10.5f, 3f));
                self.AddElement(new RoofTopView.DistantBuilding(self, "RF_CityA" + str, new Vector2(self.PosFromDrawPosAtNeutralCamPos(new Vector2(300f + (flag2 ? -300f : 0f), 0f), 8.5f).x, self.floorLevel - 25.5f), 8.5f, 0f));
                self.AddElement(new RoofTopView.DistantBuilding(self, "RF_CityB" + str, new Vector2(self.PosFromDrawPosAtNeutralCamPos(new Vector2(515f + (flag2 ? -300f : 0f), 0f), 6.5f).x, self.floorLevel - 13f), 6.5f, 0f));
                self.AddElement(new RoofTopView.DistantBuilding(self, "RF_CityC" + str, new Vector2(self.PosFromDrawPosAtNeutralCamPos(new Vector2(400f + (flag2 ? -300f : 0f), 0f), 5f).x, self.floorLevel - 8.5f), 5f, 0f));
                self.LoadGraphic("smoke1", false, false);
                self.AddElement(new RoofTopView.Smoke(self, new Vector2(0f, self.floorLevel + 560f), 7f, 0, 2.5f, 0.1f, 0.8f, false));
                self.AddElement(new RoofTopView.Smoke(self, new Vector2(0f, self.floorLevel), 4.2f, 0, 0.2f, 0.1f, 0f, true));
                self.AddElement(new RoofTopView.Smoke(self, new Vector2(0f, self.floorLevel + 28f), 2f, 0, 0.5f, 0.1f, 0f, true));
                self.AddElement(new RoofTopView.Smoke(self, new Vector2(0f, self.floorLevel + 14f), 1.2f, 0, 0.75f, 0.1f, 0f, true));
                Plugin.Log("Modify RoofTop view");
                //}
            }
            else
            {
                orig.Invoke(self, room, effect);
            }
        }
    }
}
