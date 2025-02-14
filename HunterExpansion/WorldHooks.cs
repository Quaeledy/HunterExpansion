using System.Collections.Generic;
using UnityEngine;

namespace HunterExpansion
{
    public class WorldHooks
    {
        public static void Init()
        {
            On.World.SpawnPupNPCs += World_SpawnPupNPCs;
            On.RoofTopView.ctor += RoofTopView_ctor;
        }

        public static int World_SpawnPupNPCs(On.World.orig_SpawnPupNPCs orig, World self)
        {
            if (self.game.IsStorySession && self.game.world.region != null && self.game.world.region.name == "NSH")
            {
                return 0;
            }
            return orig(self);
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
                }
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
