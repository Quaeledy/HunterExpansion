using System;
using System.Collections.Generic;
using System.IO;
using HunterExpansion.CustomSave;
using Menu;
using UnityEngine;
using RWCustom;
using HUD;
using MoreSlugcats;
using System.Text.RegularExpressions;
using System.Linq;
using DevInterface;
using Expedition;
using JollyCoop;
using JollyCoop.JollyMenu;
using Kittehface.Framework20;

namespace HunterExpansion.CustomEnding
{
    public class EndingScenes
    {
        public static void Init()
        {
            //流程构建
            On.RainWorldGame.GoToRedsGameOver += RainWorldGame_GoToRedsGameOver;
            On.RainWorldGame.CommunicateWithUpcomingProcess += RainWorldGame_CommunicateWithUpcomingProcess;
            //cg构建
            On.Menu.SlideShow.ctor += SlideShow_ctor;
            On.Menu.MenuScene.BuildScene += MenuScene_BuildScene;
            On.Menu.SlideShowMenuScene.ApplySceneSpecificAlphas += SlideShowMenuScene_ApplySceneSpecificAlphas;
            On.Menu.MenuDepthIllustration.ctor += MenuDepthIllustration_ctor;
            //修改选猫界面
            On.Menu.SlugcatSelectMenu.ComingFromRedsStatistics += SlugcatSelectMenu_ComingFromRedsStatistics;
            On.Menu.SlugcatSelectMenu.MineForSaveData += SlugcatSelectMenu_MineForSaveData;
            On.Menu.SlugcatSelectMenu.SlugcatPage.AddAltEndingImage += SlugcatPage_AddAltEndingImage;
            On.Menu.SlugcatSelectMenu.SlugcatPageContinue.ctor += SlugcatPageContinue_ctor;
            //修改统计页面
            On.Menu.StoryGameStatisticsScreen.AddBkgIllustration += StoryGameStatisticsScreen_AddBkgIllustration;
        }
        #region 修改选猫界面
        public static void SlugcatSelectMenu_ComingFromRedsStatistics(On.Menu.SlugcatSelectMenu.orig_ComingFromRedsStatistics orig, SlugcatSelectMenu self)
        {
            if (self.saveGameData[Plugin.SlugName].altEnding)
            {
                self.slugcatPageIndex = self.indexFromColor(SlugcatStats.Name.Red);
                self.redIsDead = self.saveGameData[Plugin.SlugName].redsDeath;
                self.UpdateSelectedSlugcatInMiscProg();
                return;
            }
            orig(self);
        }

        public static SlugcatSelectMenu.SaveGameData SlugcatSelectMenu_MineForSaveData(On.Menu.SlugcatSelectMenu.orig_MineForSaveData orig, ProcessManager manager, SlugcatStats.Name slugcat)
        {
            SlugcatSelectMenu.SaveGameData result =  orig(manager, slugcat);

            /*
            if (result != null)
            {
                Plugin.Log("slugcat: " + slugcat.ToString());
                Plugin.Log("result.redsDeath: " + result.redsDeath);
                Plugin.Log("result.altEnding: " + result.altEnding);
                Plugin.Log("manager.rainWorld.progression.currentSaveState != null?" + (manager.rainWorld.progression.currentSaveState != null));
            }*/
            
            if (result != null && result.altEnding && slugcat == Plugin.SlugName && result.cycle < RedsIllness.RedsCycles(result.redsExtraCycles))
            {
                result.redsDeath = false;
            }
            if (result != null && manager.rainWorld.progression.currentSaveState != null &&
                manager.rainWorld.progression.currentSaveState.saveStateNumber == slugcat &&
                result.altEnding && slugcat == Plugin.SlugName)
            {
                SaveState saveState = manager.rainWorld.progression.currentSaveState;
                if (saveState.cycleNumber < RedsIllness.RedsCycles(saveState.redExtraCycles))//saveState.deathPersistentSaveData.redsDeath && 
                {
                    saveState.deathPersistentSaveData.redsDeath = false;
                }
                else
                {
                    saveState.deathPersistentSaveData.redsDeath = true;
                }
                result.redsDeath = saveState.deathPersistentSaveData.redsDeath;
                /*
                if (result != null)
                    Plugin.Log("0 result.redsDeath: " + result.redsDeath);*/
            }
            return result;
        }

        public static void SlugcatPageContinue_ctor(On.Menu.SlugcatSelectMenu.SlugcatPageContinue.orig_ctor orig, SlugcatSelectMenu.SlugcatPageContinue self, Menu.Menu menu, MenuObject owner, int pageIndex, SlugcatStats.Name slugcatNumber)
        {
            orig(self, menu, owner, pageIndex, slugcatNumber);
            //orig必须在前面，否则无法继承base的属性
            if (ModManager.MSC && self.saveGameData.altEnding && slugcatNumber == SlugcatStats.Name.Red)
            {
                if(self.markSquare != null)
                    self.Container.RemoveChild(self.markSquare);
                if (self.markGlow != null)
                    self.Container.RemoveChild(self.markGlow);
                self.subObjects.Remove(self.slugcatImage);
                self.AddAltEndingImage();
                Plugin.Log("Hunter AltEnding Page Has Load!");
            }
            FixRegionName(self, menu, slugcatNumber);
        }

        private static void SlugcatPage_AddAltEndingImage(On.Menu.SlugcatSelectMenu.SlugcatPage.orig_AddAltEndingImage orig, SlugcatSelectMenu.SlugcatPage self)
        {
            if (self.slugcatNumber == Plugin.SlugName)// && (self as SlugcatSelectMenu.SlugcatPageContinue).saveGameData.moonGivenRobe
            {
                //AltEndingSave.hunterAltEnd为真，使用红猫进入轮回的cg
                if ((self as SlugcatSelectMenu.SlugcatPageContinue).saveGameData.ascended)//AltEndingSave.hunterAltEnd
                {
                    MenuScene.SceneID theHunter_AltEndScene = MenuSceneID.TheHunter_AltEndScene_Final;
                    self.imagePos = new Vector2(683f, 484f);
                    self.slugcatDepth = 2f;
                    self.sceneOffset = new Vector2(10f, 75f);
                    self.sceneOffset.x = self.sceneOffset.x - (1366f - self.menu.manager.rainWorld.options.ScreenSize.x) / 2f;
                    self.slugcatImage = new InteractiveMenuScene(self.menu, self, theHunter_AltEndScene);
                    self.subObjects.Add(self.slugcatImage);
                    return;
                }
                //否则，若红猫死亡，则使用业力花cg
                else if((self as SlugcatSelectMenu.SlugcatPageContinue).saveGameData.redsDeath)
                {
                    MenuScene.SceneID theHunter_AltEndScene = MenuSceneID.TheHunter_AltEndScene_Dead;
                    self.imagePos = new Vector2(683f, 484f);
                    self.slugcatDepth = 2f;
                    self.sceneOffset = new Vector2(10f, 75f);
                    self.sceneOffset.x = self.sceneOffset.x - (1366f - self.menu.manager.rainWorld.options.ScreenSize.x) / 2f;
                    self.slugcatImage = new InteractiveMenuScene(self.menu, self, theHunter_AltEndScene);
                    self.subObjects.Add(self.slugcatImage);
                    return;
                }
                //红猫没死，则使用猫猫与NSH贴贴的选猫cg
                else
                {
                    MenuScene.SceneID theHunter_AltEndScene = MenuSceneID.TheHunter_AltEndScene;
                    self.imagePos = new Vector2(683f, 484f);
                    self.slugcatDepth = 2f;
                    self.sceneOffset = new Vector2(10f, 75f);
                    self.sceneOffset.x = self.sceneOffset.x - (1366f - self.menu.manager.rainWorld.options.ScreenSize.x) / 2f;
                    self.slugcatImage = new InteractiveMenuScene(self.menu, self, theHunter_AltEndScene);
                    self.subObjects.Add(self.slugcatImage);
                    return;
                }
            }
            orig.Invoke(self);
        }
        #endregion
        #region 修改统计界面
        public static void StoryGameStatisticsScreen_AddBkgIllustration(On.Menu.StoryGameStatisticsScreen.orig_AddBkgIllustration orig, StoryGameStatisticsScreen self)
        {
            orig(self);
            SlugcatSelectMenu.SaveGameData saveGameData = SlugcatSelectMenu.MineForSaveData(self.manager, ModManager.MSC ? RainWorld.lastActiveSaveSlot : SlugcatStats.Name.Red);
            if (saveGameData != null && RainWorld.lastActiveSaveSlot == Plugin.SlugName && !saveGameData.ascended && !saveGameData.redsDeath)
            {
                self.pages[0].subObjects.Remove(self.scene);
                self.scene = new InteractiveMenuScene(self, self.pages[0], MenuScene.SceneID.SleepScreen);
                self.pages[0].subObjects.Add(self.scene);
            }
        }
        #endregion
        #region cg构建
        private static void MenuDepthIllustration_ctor(On.Menu.MenuDepthIllustration.orig_ctor orig, MenuDepthIllustration self, Menu.Menu menu, MenuObject owner, string folderName, string fileName, Vector2 pos, float depth, MenuDepthIllustration.MenuShader shader)
        {
            orig.Invoke(self, menu, owner, folderName, fileName, pos, depth, shader);
            MenuScene menuScene = owner as MenuScene;
            if (menuScene != null && !menuScene.flatMode &&
                (menuScene.sceneID == MenuSceneID.TheHunter_Outro1 ||
                menuScene.sceneID == MenuSceneID.TheHunter_Outro2 ||
                menuScene.sceneID == MenuSceneID.TheHunter_Outro3 ||
                menuScene.sceneID == MenuSceneID.TheHunter_Outro4 ||
                menuScene.sceneID == MenuSceneID.TheHunter_Outro5 ||
                menuScene.sceneID == MenuSceneID.TheHunter_Outro7 ||
                menuScene.sceneID == MenuSceneID.TheHunter_Outro8 ||
                menuScene.sceneID == MenuSceneID.TheHunter_Outro10 ||
                menuScene.sceneID == MenuSceneID.TheHunter_Outro11 ||
                menuScene.sceneID == MenuSceneID.TheHunter_Outro12 ||
                menuScene.sceneID == MenuSceneID.TheHunter_Outro5_Dead ||
                menuScene.sceneID == MenuSceneID.TheHunter_Outro7_Dead ||
                menuScene.sceneID == MenuSceneID.TheHunter_Outro8_Dead ||
                menuScene.sceneID == MenuSceneID.TheHunter_Outro11_Dead ||
                menuScene.sceneID == MenuSceneID.TheHunter_Outro12_Dead))
            {
                float num = (float)Screen.width / 2180f * 1.2f;
                self.sprite.scaleX *= num;
                self.sprite.scaleY *= num;
            }
            if (menuScene != null && (!menuScene.flatMode || menuScene.sceneID == MenuSceneID.TheHunter_Outro9_Dead || menuScene.sceneID == MenuSceneID.TheHunter_Outro10_Dead) &&
                (menuScene.sceneID == MenuSceneID.TheHunter_Outro6 ||
                menuScene.sceneID == MenuSceneID.TheHunter_Outro9 ||
                menuScene.sceneID == MenuSceneID.TheHunter_Outro6_Dead ||
                menuScene.sceneID == MenuSceneID.TheHunter_Outro9_Dead ||
                menuScene.sceneID == MenuSceneID.TheHunter_Outro10_Dead))
            {
                float num = (float)Screen.width / 2180f * 1.1f;
                self.sprite.scaleX *= num;
                self.sprite.scaleY *= num;
            }
        }

        private static void MenuScene_BuildScene(On.Menu.MenuScene.orig_BuildScene orig, MenuScene self)
        {
            orig.Invoke(self);
            if (self.sceneID == MenuSceneID.TheHunter_Outro1)
            {
                self.sceneFolder = string.Concat(new string[]
                {
                    "Scenes", Path.DirectorySeparatorChar.ToString(), "slugcat - hunter", Path.DirectorySeparatorChar.ToString(), "outro 1"
                });
                if (self.flatMode)
                {
                    self.AddIllustration(new MenuIllustration(self.menu, self, self.sceneFolder, "CG-1-flat", new Vector2(683f, 384f), false, true));
                    return;
                }
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-1-1", new Vector2(-140f, -100f), 6.0f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-1-2", new Vector2(-140f, -100f), 3.5f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-1-3", new Vector2(-140f, -100f), 2.4f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-1-4", new Vector2(-140f, -100f), 1.8f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-1-5", new Vector2(-140f, -100f), 1.2f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-1-6", new Vector2(-140f, -100f), 0.8f, MenuDepthIllustration.MenuShader.Basic));
            }
            else if (self.sceneID == MenuSceneID.TheHunter_Outro2)
            {
                self.sceneFolder = string.Concat(new string[]
                {
                    "Scenes", Path.DirectorySeparatorChar.ToString(), "slugcat - hunter", Path.DirectorySeparatorChar.ToString(), "outro 2"
                });
                if (self.flatMode)
                {
                    self.AddIllustration(new MenuIllustration(self.menu, self, self.sceneFolder, "CG-2-flat", new Vector2(683f, 384f), false, true));
                    return;
                }
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-2-1", new Vector2(-140f, -100f), 4.5f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-2-2", new Vector2(-140f, -100f), 2.5f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-2-3", new Vector2(-140f, -100f), 1.9f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-2-4", new Vector2(-140f, -100f), 0.8f, MenuDepthIllustration.MenuShader.Basic));
            }
            else if (self.sceneID == MenuSceneID.TheHunter_Outro3)
            {
                self.sceneFolder = string.Concat(new string[]
                {
                    "Scenes", Path.DirectorySeparatorChar.ToString(), "slugcat - hunter", Path.DirectorySeparatorChar.ToString(), "outro 3"
                }); 
                if (self.flatMode)
                {
                    self.AddIllustration(new MenuIllustration(self.menu, self, self.sceneFolder, "CG-3-flat", new Vector2(683f, 384f), false, true));
                    return;
                }
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-3-1", new Vector2(-140f, -100f), 6.0f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-3-2", new Vector2(-140f, -100f), 3.5f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-3-3", new Vector2(-140f, -100f), 2.4f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-3-4", new Vector2(-140f, -100f), 3.2f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-3-5", new Vector2(-225f, -30f), -1.2f, MenuDepthIllustration.MenuShader.Basic));
            }
            else if (self.sceneID == MenuSceneID.TheHunter_Outro4)
            {
                self.sceneFolder = string.Concat(new string[]
                {
                    "Scenes", Path.DirectorySeparatorChar.ToString(), "slugcat - hunter", Path.DirectorySeparatorChar.ToString(), "outro 4"
                });
                if (self.flatMode)
                {
                    self.AddIllustration(new MenuIllustration(self.menu, self, self.sceneFolder, "CG-4-flat", new Vector2(683f, 384f), false, true));
                    return;
                }
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-4-1", new Vector2(-140f, -100f), 6.0f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-4-2", new Vector2(-140f, -100f), 3.5f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-4-3", new Vector2(-140f, -100f), 2.4f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-4-4", new Vector2(-140f, -100f), 1.8f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-4-5", new Vector2(-140f, -100f), 1.2f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-4-6", new Vector2(-140f, -100f), 0.8f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-4-7", new Vector2(-140f, -100f), -1.3f, MenuDepthIllustration.MenuShader.Basic));
            }
            else if (self.sceneID == MenuSceneID.TheHunter_Outro5)
            {
                self.sceneFolder = string.Concat(new string[]
                {
                    "Scenes", Path.DirectorySeparatorChar.ToString(), "slugcat - hunter", Path.DirectorySeparatorChar.ToString(), "outro 5"
                });
                if (self.flatMode)
                {
                    self.AddIllustration(new MenuIllustration(self.menu, self, self.sceneFolder, "CG-5-flat-he", new Vector2(683f, 384f), false, true));
                    return;
                }
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-5-1", new Vector2(-140f, -100f), 5f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-5-2-he", new Vector2(-140f, -100f), 5f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-5-3", new Vector2(-140f, -100f), 2.4f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-5-4", new Vector2(-140f, -100f), 1.8f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-5-5", new Vector2(-140f, -100f), 1.2f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-5-6", new Vector2(-140f, -100f), 0.8f, MenuDepthIllustration.MenuShader.Basic));
            }
            else if (self.sceneID == MenuSceneID.TheHunter_Outro6)
            {
                self.sceneFolder = string.Concat(new string[]
                {
                    "Scenes", Path.DirectorySeparatorChar.ToString(), "slugcat - hunter", Path.DirectorySeparatorChar.ToString(), "outro 6"
                });
                if (self.flatMode)
                {
                    self.AddIllustration(new MenuIllustration(self.menu, self, self.sceneFolder, "CG-6-flat-he", new Vector2(683f, 384f), false, true));
                    return;
                }
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-6-1", new Vector2(-70f, -50f), 4.5f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-6-2-he-1", new Vector2(-70f, -50f), 2.2f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-6-2-he-2", new Vector2(-70f, -50f), 2.2f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-6-3", new Vector2(-70f, -50f), 0.8f, MenuDepthIllustration.MenuShader.Basic));
            }
            else if (self.sceneID == MenuSceneID.TheHunter_Outro7)
            {
                self.sceneFolder = string.Concat(new string[]
                {
                    "Scenes", Path.DirectorySeparatorChar.ToString(), "slugcat - hunter", Path.DirectorySeparatorChar.ToString(), "outro 7"
                });
                if (self.flatMode)
                {
                    self.AddIllustration(new MenuIllustration(self.menu, self, self.sceneFolder, "CG-7-flat-he", new Vector2(683f, 384f), false, true));
                    return;
                }
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-7-1", new Vector2(-140f, -100f), 4.5f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-7-2-he", new Vector2(-140f, -100f), 2.2f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-7-3", new Vector2(-140f, -100f), 0.8f, MenuDepthIllustration.MenuShader.Basic));
            }
            else if (self.sceneID == MenuSceneID.TheHunter_Outro8)
            {
                self.sceneFolder = string.Concat(new string[]
                {
                    "Scenes", Path.DirectorySeparatorChar.ToString(), "slugcat - hunter", Path.DirectorySeparatorChar.ToString(), "outro 8"
                });
                if (self.flatMode)
                {
                    self.AddIllustration(new MenuIllustration(self.menu, self, self.sceneFolder, "CG-8-flat-he", new Vector2(683f, 384f), false, true));
                    return;
                }
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-8-1", new Vector2(-140f, -100f), 6.0f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-8-2", new Vector2(-140f, -100f), 3.5f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-8-3", new Vector2(-140f, -100f), 2.0f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-8-4", new Vector2(-140f, -100f), 3.4f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-8-5", new Vector2(-140f, -100f), 1.2f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-8-6-he", new Vector2(-140f, -100f), 0.9f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-8-7-Multiply", new Vector2(-140f, -100f), 100f, MenuDepthIllustration.MenuShader.Multiply));
            }
            else if (self.sceneID == MenuSceneID.TheHunter_Outro9)
            {
                self.sceneFolder = string.Concat(new string[]
                {
                    "Scenes", Path.DirectorySeparatorChar.ToString(), "slugcat - hunter", Path.DirectorySeparatorChar.ToString(), "outro 9"
                });
                if (self.flatMode)
                {
                    self.AddIllustration(new MenuIllustration(self.menu, self, self.sceneFolder, "HE-1-flat", new Vector2(683f, 384f), false, true));
                    return;
                }
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HE-1-1", new Vector2(-70f, -50f), 6.0f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HE-1-2", new Vector2(-70f, -50f), 3.5f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HE-1-3", new Vector2(-70f, -50f), 2.4f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HE-1-4", new Vector2(-70f, -50f), 1.8f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HE-1-5", new Vector2(-70f, -50f), 1.2f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HE-1-6", new Vector2(-70f, -50f), 0.8f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HE-1-7", new Vector2(-70f, -50f), 0.5f, MenuDepthIllustration.MenuShader.Basic));
            }
            else if (self.sceneID == MenuSceneID.TheHunter_Outro10)
            {
                self.sceneFolder = string.Concat(new string[]
                {
                    "Scenes", Path.DirectorySeparatorChar.ToString(), "slugcat - hunter", Path.DirectorySeparatorChar.ToString(), "outro 10"
                });
                if (self.flatMode)
                {
                    self.AddIllustration(new MenuIllustration(self.menu, self, self.sceneFolder, "HE-2-flat", new Vector2(683f, 384f), false, true));
                    return;
                }
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HE-2-1", new Vector2(-140f, -100f), 6.0f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HE-2-2", new Vector2(-140f, -100f), 2.8f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HE-2-3", new Vector2(-140f, -100f), 3.4f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HE-2-4", new Vector2(-140f, -100f), 2.8f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HE-2-5", new Vector2(-140f, -100f), 1.8f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HE-2-6", new Vector2(-140f, -100f), 1.2f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HE-2-7", new Vector2(-140f, -100f), 0.8f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HE-2-8", new Vector2(-140f, -100f), 0.5f, MenuDepthIllustration.MenuShader.Basic));
            }
            else if (self.sceneID == MenuSceneID.TheHunter_Outro11)
            {
                self.sceneFolder = string.Concat(new string[]
                {
                    "Scenes", Path.DirectorySeparatorChar.ToString(), "slugcat - hunter", Path.DirectorySeparatorChar.ToString(), "outro 11"
                });
                if (self.flatMode)
                {
                    self.AddIllustration(new MenuIllustration(self.menu, self, self.sceneFolder, "HE-3-flat", new Vector2(683f, 384f), false, true));
                    return;
                }
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HE-3-1", new Vector2(-140f, -100f), 6.0f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HE-3-2", new Vector2(-140f, -100f), 2.5f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HE-3-3", new Vector2(-140f, -100f), 1.8f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HE-3-4", new Vector2(-140f, -100f), 1.2f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HE-3-5", new Vector2(-140f, -100f), 0.8f, MenuDepthIllustration.MenuShader.Basic));
            }
            else if (self.sceneID == MenuSceneID.TheHunter_Outro12)
            {
                self.sceneFolder = string.Concat(new string[]
                {
                    "Scenes", Path.DirectorySeparatorChar.ToString(), "slugcat - hunter", Path.DirectorySeparatorChar.ToString(), "outro 12"
                });
                if (self.flatMode)
                {
                    self.AddIllustration(new MenuIllustration(self.menu, self, self.sceneFolder, "HE-4-flat", new Vector2(683f, 384f), false, true));
                    return;
                }
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HE-4-1", new Vector2(-140f, -100f), 6.0f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HE-4-2", new Vector2(-140f, -100f), 3.5f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HE-4-3", new Vector2(-140f, -100f), 2.4f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HE-4-4", new Vector2(-140f, -100f), 1.8f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HE-4-5", new Vector2(-140f, -100f), 1.2f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HE-4-6", new Vector2(-140f, -100f), 0.8f, MenuDepthIllustration.MenuShader.Basic));

                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HE-5-1", new Vector2(-140f, -100f), 6.0f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HE-5-2", new Vector2(-140f, -100f), 3.5f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HE-5-3", new Vector2(-140f, -100f), 2.4f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HE-5-4-1", new Vector2(-140f, -100f), 1.8f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HE-5-4-2", new Vector2(-140f, -100f), 1.8f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HE-5-5", new Vector2(-140f, -100f), 1.2f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HE-5-6", new Vector2(-140f, -100f), 0.8f, MenuDepthIllustration.MenuShader.Basic));
            }
            else if (self.sceneID == MenuSceneID.TheHunter_AltEndScene)
            {
                self.sceneFolder = string.Concat(new string[]
                {
                    "Scenes", Path.DirectorySeparatorChar.ToString(), "slugcat - hunter", Path.DirectorySeparatorChar.ToString(), "altend"
                });
                if (self.flatMode)
                {
                    self.AddIllustration(new MenuIllustration(self.menu, self, self.sceneFolder, "HunterAltEnd - flat", new Vector2(683f, 384f), false, true));
                    return;
                }
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HunterAltEnd - black", Vector2.zero, 100f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HunterAltEnd - 3", Vector2.zero, 3f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HunterAltEnd - 2 - Lighten", Vector2.zero, 2.5f, MenuDepthIllustration.MenuShader.Lighten));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HunterAltEnd - 1", Vector2.zero, 2f, MenuDepthIllustration.MenuShader.Basic));
            }
            else if (self.sceneID == MenuSceneID.TheHunter_AltEndScene_Final)
            {
                self.sceneFolder = string.Concat(new string[]
                {
                    "Scenes", Path.DirectorySeparatorChar.ToString(), "slugcat - hunter", Path.DirectorySeparatorChar.ToString(), "altend - final"
                });
                if (self.flatMode)
                {
                    self.AddIllustration(new MenuIllustration(self.menu, self, self.sceneFolder, "HunterAltEnd_Final - flat", new Vector2(683f, 384f), false, true));
                    return;
                }
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HunterAltEnd_Final - 3", Vector2.zero, 3f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HunterAltEnd_Final - 2", Vector2.zero, 2.5f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HunterAltEnd_Final - 1", Vector2.zero, 2f, MenuDepthIllustration.MenuShader.Basic));
            }
            //坏结局
            else if (self.sceneID == MenuSceneID.TheHunter_Outro5_Dead)
            {
                self.sceneFolder = string.Concat(new string[]
                {
                    "Scenes", Path.DirectorySeparatorChar.ToString(), "slugcat - hunter", Path.DirectorySeparatorChar.ToString(), "outro 5"
                });
                if (self.flatMode)
                {
                    self.AddIllustration(new MenuIllustration(self.menu, self, self.sceneFolder, "CG-5-flat", new Vector2(683f, 384f), false, true));
                    return;
                }
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-5-1", new Vector2(-140f, -100f), 5f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-5-2", new Vector2(-140f, -100f), 5f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-5-3", new Vector2(-140f, -100f), 2.4f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-5-4", new Vector2(-140f, -100f), 1.8f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-5-5", new Vector2(-140f, -100f), 1.2f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-5-6", new Vector2(-140f, -100f), 0.8f, MenuDepthIllustration.MenuShader.Basic));
            }
            else if (self.sceneID == MenuSceneID.TheHunter_Outro6_Dead)
            {
                self.sceneFolder = string.Concat(new string[]
                {
                    "Scenes", Path.DirectorySeparatorChar.ToString(), "slugcat - hunter", Path.DirectorySeparatorChar.ToString(), "outro 6"
                });
                if (self.flatMode)
                {
                    self.AddIllustration(new MenuIllustration(self.menu, self, self.sceneFolder, "CG-6-flat", new Vector2(683f, 384f), false, true));
                    return;
                }
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-6-1", new Vector2(-70f, -50f), 4.5f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-6-2-1", new Vector2(-70f, -50f), 2.2f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-6-2-2", new Vector2(-70f, -50f), 2.2f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-6-3", new Vector2(-70f, -50f), 0.8f, MenuDepthIllustration.MenuShader.Basic));
            }
            else if (self.sceneID == MenuSceneID.TheHunter_Outro7_Dead)
            {
                self.sceneFolder = string.Concat(new string[]
                {
                    "Scenes", Path.DirectorySeparatorChar.ToString(), "slugcat - hunter", Path.DirectorySeparatorChar.ToString(), "outro 7"
                });
                if (self.flatMode)
                {
                    self.AddIllustration(new MenuIllustration(self.menu, self, self.sceneFolder, "CG-7-flat", new Vector2(683f, 384f), false, true));
                    return;
                }
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-7-1", new Vector2(-140f, -100f), 4.5f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-7-2", new Vector2(-140f, -100f), 2.2f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-7-3", new Vector2(-140f, -100f), 0.8f, MenuDepthIllustration.MenuShader.Basic));
            }
            else if (self.sceneID == MenuSceneID.TheHunter_Outro8_Dead)
            {
                self.sceneFolder = string.Concat(new string[]
                {
                    "Scenes", Path.DirectorySeparatorChar.ToString(), "slugcat - hunter", Path.DirectorySeparatorChar.ToString(), "outro 8"
                });
                if (self.flatMode)
                {
                    self.AddIllustration(new MenuIllustration(self.menu, self, self.sceneFolder, "CG-8-flat", new Vector2(683f, 384f), false, true));
                    return;
                }
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-8-1", new Vector2(-140f, -100f), 6.0f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-8-2", new Vector2(-140f, -100f), 3.5f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-8-3", new Vector2(-140f, -100f), 2.4f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-8-4", new Vector2(-140f, -100f), 1.8f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-8-5", new Vector2(-140f, -100f), 1.2f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-8-6", new Vector2(-140f, -100f), 0.9f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "CG-8-7-Multiply", new Vector2(-140f, -100f), 100f, MenuDepthIllustration.MenuShader.Multiply));
            }
            else if (self.sceneID == MenuSceneID.TheHunter_Outro9_Dead)
            {
                self.sceneFolder = string.Concat(new string[]
                {
                    "Scenes", Path.DirectorySeparatorChar.ToString(), "slugcat - hunter", Path.DirectorySeparatorChar.ToString(), "outro 9 - dead"
                });
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "BE-1-0", new Vector2(-70f, -50f), 1.2f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "BE-2-0", new Vector2(-70f, -50f), 1.2f, MenuDepthIllustration.MenuShader.Basic));
            }
            else if (self.sceneID == MenuSceneID.TheHunter_Outro10_Dead)
            {
                self.sceneFolder = string.Concat(new string[]
                {
                    "Scenes", Path.DirectorySeparatorChar.ToString(), "slugcat - hunter", Path.DirectorySeparatorChar.ToString(), "outro 10 - dead"
                });
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "BE-3-1", new Vector2(-70f, -50f), 0.8f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "BE-3-2", new Vector2(-70f, -50f), 0.8f, MenuDepthIllustration.MenuShader.Basic));
            }
            else if (self.sceneID == MenuSceneID.TheHunter_Outro11_Dead)
            {
                self.sceneFolder = string.Concat(new string[]
                {
                    "Scenes", Path.DirectorySeparatorChar.ToString(), "slugcat - hunter", Path.DirectorySeparatorChar.ToString(), "outro 11 - dead"
                });
                if (self.flatMode)
                {
                    self.AddIllustration(new MenuIllustration(self.menu, self, self.sceneFolder, "BE-4-flat", new Vector2(683f, 384f), false, true));
                    return;
                }
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "BE-4-1", new Vector2(-140f, -100f), 6.0f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "BE-4-2", new Vector2(-140f, -100f), 3.5f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "BE-4-3", new Vector2(-140f, -100f), 2.4f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "BE-4-4", new Vector2(-140f, -100f), 1.8f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "BE-4-5", new Vector2(-140f, -100f), 1.2f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "BE-4-6", new Vector2(-140f, -100f), 0.8f, MenuDepthIllustration.MenuShader.Basic));
            }
            else if (self.sceneID == MenuSceneID.TheHunter_Outro12_Dead)
            {
                self.sceneFolder = string.Concat(new string[]
                {
                    "Scenes", Path.DirectorySeparatorChar.ToString(), "slugcat - hunter", Path.DirectorySeparatorChar.ToString(), "outro 12 - dead"
                });
                if (self.flatMode)
                {
                    self.AddIllustration(new MenuIllustration(self.menu, self, self.sceneFolder, "BE-5-flat", new Vector2(683f, 384f), false, true));
                    return;
                }
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "BE-5-1", new Vector2(-140f, -100f), 6.0f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "BE-5-2", new Vector2(-140f, -100f), 3.5f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "BE-5-3", new Vector2(-140f, -100f), 2.4f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "BE-5-4", new Vector2(-140f, -100f), 1.8f, MenuDepthIllustration.MenuShader.Basic));
            }
            else if (self.sceneID == MenuSceneID.TheHunter_AltEndScene_Dead)
            {
                self.sceneFolder = string.Concat(new string[]
                {
                    "Scenes", Path.DirectorySeparatorChar.ToString(), "slugcat - hunter", Path.DirectorySeparatorChar.ToString(), "altend - dead"
                });
                if (self.flatMode)
                {
                    self.AddIllustration(new MenuIllustration(self.menu, self, self.sceneFolder, "HunterAltEnd_Dead - flat", new Vector2(683f, 384f), false, true));
                    return;
                }
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HunterAltEnd_Dead - 1", Vector2.zero, 2.8f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HunterAltEnd_Dead - 2", Vector2.zero, 2.5f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HunterAltEnd_Dead - 3", Vector2.zero, 2.2f, MenuDepthIllustration.MenuShader.Basic));
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "HunterAltEnd_Dead - 4", Vector2.zero, 2f, MenuDepthIllustration.MenuShader.Basic));
            }
        }

        private static void SlideShow_ctor(On.Menu.SlideShow.orig_ctor orig, SlideShow self, ProcessManager manager, SlideShow.SlideShowID slideShowID)
        {
            orig.Invoke(self, manager, slideShowID);
            Plugin.Log(string.Format("{0}", slideShowID));
            if (slideShowID == SlideShowID.HunterAltEnd)
            {
                self.playList = new List<SlideShow.Scene>();
                if (manager.musicPlayer != null)
                {
                    self.waitForMusic = "RW_Intro_Theme";
                    self.stall = true;
                    manager.musicPlayer.MenuRequestsSong(self.waitForMusic, 1.5f, 10f);
                }
                self.playList.Add(new SlideShow.Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
                self.playList.Add(new SlideShow.Scene(MenuSceneID.TheHunter_Outro1, self.ConvertTime(0, 1, 20), self.ConvertTime(0, 5, 26), self.ConvertTime(0, 8, 6)));
                self.playList.Add(new SlideShow.Scene(MenuSceneID.TheHunter_Outro2, self.ConvertTime(0, 9, 6), self.ConvertTime(0, 13, 15), self.ConvertTime(0, 15, 0)));
                self.playList.Add(new SlideShow.Scene(MenuSceneID.TheHunter_Outro3, self.ConvertTime(0, 17, 21), self.ConvertTime(0, 22, 26), self.ConvertTime(0, 24, 3)));
                self.playList.Add(new SlideShow.Scene(MenuSceneID.TheHunter_Outro4, self.ConvertTime(0, 24, 26), self.ConvertTime(0, 29, 19), self.ConvertTime(0, 31, 15)));
                self.playList.Add(new SlideShow.Scene(MenuScene.SceneID.Empty, self.ConvertTime(0, 32, 25), self.ConvertTime(0, 33, 15), self.ConvertTime(0, 33, 15)));
                self.playList.Add(new SlideShow.Scene(MenuSceneID.TheHunter_Outro5, self.ConvertTime(0, 36, 15), self.ConvertTime(0, 39, 15), self.ConvertTime(0, 42, 15)));
                self.playList.Add(new SlideShow.Scene(MenuSceneID.TheHunter_Outro6, self.ConvertTime(0, 43, 0), self.ConvertTime(0, 44, 0), self.ConvertTime(0, 46, 3)));
                self.playList.Add(new SlideShow.Scene(MenuSceneID.TheHunter_Outro7, self.ConvertTime(0, 50, 20), self.ConvertTime(0, 53, 26), self.ConvertTime(0, 55, 21)));
                self.playList.Add(new SlideShow.Scene(MenuSceneID.TheHunter_Outro8, self.ConvertTime(0, 57, 2), self.ConvertTime(1, 2, 8), self.ConvertTime(1, 3, 1)));
                self.playList.Add(new SlideShow.Scene(MenuSceneID.TheHunter_Outro9, self.ConvertTime(1, 4, 1), self.ConvertTime(1, 8, 26), self.ConvertTime(1, 9, 26)));
                self.playList.Add(new SlideShow.Scene(MenuSceneID.TheHunter_Outro10, self.ConvertTime(1, 11, 10), self.ConvertTime(1, 13, 10), self.ConvertTime(1, 14, 28)));
                self.playList.Add(new SlideShow.Scene(MenuSceneID.TheHunter_Outro11, self.ConvertTime(1, 15, 12), self.ConvertTime(1, 17, 12), self.ConvertTime(1, 18, 12)));
                self.playList.Add(new SlideShow.Scene(MenuScene.SceneID.Empty, self.ConvertTime(1, 19, 1), self.ConvertTime(1, 19, 21), self.ConvertTime(1, 19, 21)));
                self.playList.Add(new SlideShow.Scene(MenuSceneID.TheHunter_Outro12, self.ConvertTime(1, 19, 21), self.ConvertTime(1, 20, 1), self.ConvertTime(1, 26, 24)));
                self.playList.Add(new SlideShow.Scene(MenuScene.SceneID.Empty, self.ConvertTime(1, 27, 0), 0f, 0f));
                for (int i = 1; i < self.playList.Count; i++)
                {
                    self.playList[i].startAt += 0.6f;
                    self.playList[i].fadeInDoneAt += 0.6f;
                    self.playList[i].fadeOutStartAt += 0.6f;
                }
                self.processAfterSlideShow = ProcessManager.ProcessID.Game;
                self.preloadedScenes = new SlideShowMenuScene[self.playList.Count];
                for (int j = 0; j < self.preloadedScenes.Length; j++)
                {
                    self.preloadedScenes[j] = new SlideShowMenuScene(self, self.pages[0], self.playList[j].sceneID);
                    self.preloadedScenes[j].Hide();
                }
                self.current = 0;
                self.NextScene();
                Plugin.Log(string.Format("{0},{1},{2}", slideShowID, self.current, self.scene));
            }
            if (slideShowID == SlideShowID.HunterAltEnd_Dead)
            {
                self.playList = new List<SlideShow.Scene>();
                if (manager.musicPlayer != null)
                {
                    self.waitForMusic = "RW_Intro_Theme";
                    self.stall = true;
                    manager.musicPlayer.MenuRequestsSong(self.waitForMusic, 1.5f, 10f);
                }
                self.playList.Add(new SlideShow.Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
                self.playList.Add(new SlideShow.Scene(MenuSceneID.TheHunter_Outro1, self.ConvertTime(0, 1, 20), self.ConvertTime(0, 5, 26), self.ConvertTime(0, 8, 6)));
                self.playList.Add(new SlideShow.Scene(MenuSceneID.TheHunter_Outro2, self.ConvertTime(0, 9, 6), self.ConvertTime(0, 13, 15), self.ConvertTime(0, 15, 0)));
                self.playList.Add(new SlideShow.Scene(MenuSceneID.TheHunter_Outro3, self.ConvertTime(0, 17, 21), self.ConvertTime(0, 22, 26), self.ConvertTime(0, 24, 3)));
                self.playList.Add(new SlideShow.Scene(MenuSceneID.TheHunter_Outro4, self.ConvertTime(0, 24, 26), self.ConvertTime(0, 29, 19), self.ConvertTime(0, 31, 15)));
                self.playList.Add(new SlideShow.Scene(MenuScene.SceneID.Empty, self.ConvertTime(0, 32, 25), self.ConvertTime(0, 33, 15), self.ConvertTime(0, 33, 15)));
                self.playList.Add(new SlideShow.Scene(MenuSceneID.TheHunter_Outro5_Dead, self.ConvertTime(0, 36, 15), self.ConvertTime(0, 39, 15), self.ConvertTime(0, 42, 15)));
                self.playList.Add(new SlideShow.Scene(MenuSceneID.TheHunter_Outro6_Dead, self.ConvertTime(0, 43, 0), self.ConvertTime(0, 44, 0), self.ConvertTime(0, 46, 3)));
                self.playList.Add(new SlideShow.Scene(MenuSceneID.TheHunter_Outro7_Dead, self.ConvertTime(0, 50, 20), self.ConvertTime(0, 53, 26), self.ConvertTime(0, 55, 21)));
                self.playList.Add(new SlideShow.Scene(MenuSceneID.TheHunter_Outro8_Dead, self.ConvertTime(0, 57, 2), self.ConvertTime(1, 2, 8), self.ConvertTime(1, 3, 1)));
                self.playList.Add(new SlideShow.Scene(MenuSceneID.TheHunter_Outro9_Dead, self.ConvertTime(1, 4, 1), self.ConvertTime(1, 8, 25), self.ConvertTime(1, 10, 25)));
                self.playList.Add(new SlideShow.Scene(MenuSceneID.TheHunter_Outro10_Dead, self.ConvertTime(1, 11, 25), self.ConvertTime(1, 12, 10), self.ConvertTime(1, 17, 28)));
                self.playList.Add(new SlideShow.Scene(MenuSceneID.TheHunter_Outro11_Dead, self.ConvertTime(1, 19, 2), self.ConvertTime(1, 20, 6), self.ConvertTime(1, 20, 6)));
                self.playList.Add(new SlideShow.Scene(MenuSceneID.TheHunter_Outro12_Dead, self.ConvertTime(1, 22, 22), self.ConvertTime(1, 23, 15), self.ConvertTime(1, 26, 24)));
                self.playList.Add(new SlideShow.Scene(MenuScene.SceneID.Empty, self.ConvertTime(1, 27, 0), 0f, 0f));
                for (int i = 1; i < self.playList.Count; i++)
                {
                    self.playList[i].startAt += 0.6f;
                    self.playList[i].fadeInDoneAt += 0.6f;
                    self.playList[i].fadeOutStartAt += 0.6f;
                }
                self.processAfterSlideShow = ProcessManager.ProcessID.Statistics;//这一句没用，得去RainWorldGame_CommunicateWithUpcomingProcess里改
                self.preloadedScenes = new SlideShowMenuScene[self.playList.Count];
                for (int j = 0; j < self.preloadedScenes.Length; j++)
                {
                    self.preloadedScenes[j] = new SlideShowMenuScene(self, self.pages[0], self.playList[j].sceneID);
                    self.preloadedScenes[j].Hide();
                }
                self.current = 0;
                self.NextScene();
                Plugin.Log(string.Format("{0},{1},{2}", slideShowID, self.current, self.scene));
            }
        }

        private static void SlideShowMenuScene_ApplySceneSpecificAlphas(On.Menu.SlideShowMenuScene.orig_ApplySceneSpecificAlphas orig, SlideShowMenuScene self)
        {
            orig(self);

            if (self.sceneID == MenuSceneID.TheHunter_Outro6)
            {
                self.depthIllustrations[self.depthIllustrations.Count - 2].setAlpha = new float?(Mathf.InverseLerp(0.45f, 0.5f, self.displayTime));
                self.depthIllustrations[self.depthIllustrations.Count - 3].setAlpha = new float?(Mathf.InverseLerp(0.5f, 0.45f, self.displayTime));
            }
            if (self.sceneID == MenuSceneID.TheHunter_Outro8)
            {
                self.depthIllustrations[self.depthIllustrations.Count - 1].setAlpha = 0f;
            }
            if (self.sceneID == MenuSceneID.TheHunter_Outro12)
            {
                self.depthIllustrations[self.depthIllustrations.Count - 8].setAlpha = new float?(Mathf.InverseLerp(0.3f, 0.25f, self.displayTime));
                self.depthIllustrations[self.depthIllustrations.Count - 9].setAlpha = new float?(Mathf.InverseLerp(0.3f, 0.25f, self.displayTime));
                self.depthIllustrations[self.depthIllustrations.Count - 10].setAlpha = new float?(Mathf.InverseLerp(0.3f, 0.25f, self.displayTime));
                self.depthIllustrations[self.depthIllustrations.Count - 11].setAlpha = new float?(Mathf.InverseLerp(0.3f, 0.25f, self.displayTime));
                self.depthIllustrations[self.depthIllustrations.Count - 12].setAlpha = new float?(Mathf.InverseLerp(0.3f, 0.25f, self.displayTime));
                self.depthIllustrations[self.depthIllustrations.Count - 13].setAlpha = new float?(Mathf.InverseLerp(0.3f, 0.25f, self.displayTime));

                self.depthIllustrations[self.depthIllustrations.Count - 1].setAlpha = new float?(Mathf.InverseLerp(0.25f, 0.3f, self.displayTime));
                self.depthIllustrations[self.depthIllustrations.Count - 2].setAlpha = new float?(Mathf.InverseLerp(0.25f, 0.3f, self.displayTime));
                self.depthIllustrations[self.depthIllustrations.Count - 3].setAlpha = new float?(Mathf.InverseLerp(0.5f, 0.55f, self.displayTime));
                self.depthIllustrations[self.depthIllustrations.Count - 4].setAlpha = new float?(Mathf.Min(Mathf.InverseLerp(0.25f, 0.3f, self.displayTime), Mathf.InverseLerp(0.55f, 0.5f, self.displayTime)));
                self.depthIllustrations[self.depthIllustrations.Count - 5].setAlpha = new float?(Mathf.InverseLerp(0.25f, 0.3f, self.displayTime));
                self.depthIllustrations[self.depthIllustrations.Count - 6].setAlpha = new float?(Mathf.InverseLerp(0.25f, 0.3f, self.displayTime));
                self.depthIllustrations[self.depthIllustrations.Count - 7].setAlpha = new float?(Mathf.InverseLerp(0.25f, 0.3f, self.displayTime));
            }
            if (self.sceneID == MenuSceneID.TheHunter_Outro6_Dead)
            {
                self.depthIllustrations[self.depthIllustrations.Count - 2].setAlpha = new float?(Mathf.InverseLerp(0.45f, 0.5f, self.displayTime));
                self.depthIllustrations[self.depthIllustrations.Count - 3].setAlpha = new float?(Mathf.InverseLerp(0.5f, 0.45f, self.displayTime));
            }
            if (self.sceneID == MenuSceneID.TheHunter_Outro9_Dead)
            {
                self.depthIllustrations[self.depthIllustrations.Count - 1].setAlpha = new float?(Mathf.InverseLerp(0.52f, 0.57f, self.displayTime));
                self.depthIllustrations[self.depthIllustrations.Count - 2].setAlpha = new float?(Mathf.InverseLerp(0.57f, 0.52f, self.displayTime));
            }
            if (self.sceneID == MenuSceneID.TheHunter_Outro10_Dead)
            {
                self.depthIllustrations[self.depthIllustrations.Count - 1].setAlpha = new float?(Mathf.InverseLerp(0.5f, 0.55f, self.displayTime));
                self.depthIllustrations[self.depthIllustrations.Count - 2].setAlpha = new float?(Mathf.InverseLerp(0.55f, 0.5f, self.displayTime));
            }
        }
        #endregion
        #region 流程构建
        private static void RainWorldGame_GoToRedsGameOver(On.RainWorldGame.orig_GoToRedsGameOver orig, RainWorldGame self)
        {
            SaveState saveState = self.GetStorySession.saveState;
            if (saveState.saveStateNumber == Plugin.SlugName && PearlFixedSave.pearlFixed &&
                self.world.region.name == "NSH" && self.manager.upcomingProcess == null)
            {
                if (!saveState.deathPersistentSaveData.altEnding)
                {
                    saveState.deathPersistentSaveData.altEnding = true;
                }
                //IL_167:
                if (self.manager.musicPlayer != null)
                {
                    self.manager.musicPlayer.FadeOutAllSongs(20f);
                }
                //分支结局：时间足够回去
                if (RedsIllness.RedsCycles(saveState.redExtraCycles) - saveState.cycleNumber >= 15)
                {
                    Plugin.Log("Hunter AltEnd: Return!");
                    saveState.deathPersistentSaveData.redsDeath = false;
                    saveState.cycleNumber = RedsIllness.RedsCycles(saveState.redExtraCycles) - 2;
                    self.manager.statsAfterCredits = false;
                    self.manager.nextSlideshow = SlideShowID.HunterAltEnd;
                }
                //分支结局：时间不够回去
                else
                {
                    Plugin.Log("Hunter AltEnd: Dead!");
                    saveState.deathPersistentSaveData.redsDeath = true;
                    saveState.cycleNumber += 15;
                    self.manager.statsAfterCredits = false;
                    self.manager.nextSlideshow = SlideShowID.HunterAltEnd_Dead;
                }
                AbstractCreature abstractCreature = self.FirstAlivePlayer;
                if (abstractCreature == null)
                {
                    abstractCreature = self.FirstAnyPlayer;
                }
                SaveState.forcedEndRoomToAllowwSave = abstractCreature.Room.name;
                saveState.BringUpToDate(self);
                SaveState.forcedEndRoomToAllowwSave = "";
                //saveState.AppendCycleToStatistics(abstractCreature.realizedCreature as Player, self.GetStorySession, false, 0);
                RainWorldGame.ForceSaveNewDenLocation(self, "NSH_AI", false);
                self.manager.rainWorld.progression.SaveWorldStateAndProgression(false);
                //saveState.SessionEnded(self, true, false);
                self.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SlideShow);
            }
            orig.Invoke(self);
        }

        public static void RainWorldGame_CommunicateWithUpcomingProcess(On.RainWorldGame.orig_CommunicateWithUpcomingProcess orig, RainWorldGame self, MainLoopProcess nextProcess)
        {
            orig(self, nextProcess);
            if (self.StoryCharacter == SlugcatStats.Name.Red && nextProcess is SlideShow && self.manager.nextSlideshow == SlideShowID.HunterAltEnd)
            {
                int karma = self.GetStorySession.saveState.deathPersistentSaveData.karmaCap;
                Debug.Log("Hunter AltEnding savKarma: " + karma.ToString());
                self.GetStorySession.saveState.deathPersistentSaveData.redsDeath = false;
                (nextProcess as SlideShow).processAfterSlideShow = ProcessManager.ProcessID.Game;
            }
            /*
            if (self.StoryCharacter == SlugcatStats.Name.Red && nextProcess is SlideShow && self.manager.nextSlideshow == HunterAltEnd_Dead)
            {
                self.GetStorySession.saveState.deathPersistentSaveData.redsDeath = true;
                (nextProcess as SlideShow).processAfterSlideShow = ProcessManager.ProcessID.Statistics;
            }*/
        }
        #endregion

        private static void FixRegionName(SlugcatSelectMenu.SlugcatPageContinue self, Menu.Menu menu, SlugcatStats.Name slugcatNumber)
        {
            if (self.saveGameData.shelterName != null && self.saveGameData.shelterName.Length > 2)
            {
                string text = "";
                text = Region.GetRegionFullName(self.saveGameData.shelterName.Substring(0, 2), slugcatNumber);
                if (Regex.Split(self.saveGameData.shelterName, "_")[0] == "NSH")
                {
                    text = Region.GetRegionFullName("NSH", slugcatNumber);
                }
                if (text.Length > 0)
                {
                    text = menu.Translate(text);
                    text = string.Concat(new string[]
                    {
                            text,
                            " - ",
                            menu.Translate("Cycle"),
                            " ",
                            ((slugcatNumber == SlugcatStats.Name.Red) ? (RedsIllness.RedsCycles(self.saveGameData.redsExtraCycles) - self.saveGameData.cycle) : self.saveGameData.cycle).ToString()
                    });
                    if (ModManager.MMF)
                    {
                        TimeSpan timeSpan = TimeSpan.FromSeconds((double)self.saveGameData.gameTimeAlive + (double)self.saveGameData.gameTimeDead);
                        text = text + " (" + SpeedRunTimer.TimeFormat(timeSpan) + ")";
                    }
                }
                self.regionLabel = new MenuLabel(menu, self, text, new Vector2(-1000f, self.imagePos.y - 249f), new Vector2(200f, 30f), true, null);
                self.regionLabel.label.alignment = FLabelAlignment.Center;
                self.subObjects.Add(self.regionLabel);
            }
        }
    }

    public class SlideShowID
    {
        public static void RegisterValues()
        {
            HunterAltEnd = new SlideShow.SlideShowID("HunterAltEnd", true);
            HunterAltEnd_Dead = new SlideShow.SlideShowID("HunterAltEnd_Dead", true);
        }

        public static void UnregisterValues()
        {
            HunterExpansionEnums.Unregister(HunterAltEnd);
            HunterExpansionEnums.Unregister(HunterAltEnd_Dead);
        }

        public static SlideShow.SlideShowID HunterAltEnd;
        public static SlideShow.SlideShowID HunterAltEnd_Dead;
    }

    public class MenuSceneID
    {
        public static void RegisterValues()
        {
            TheHunter_Outro1 = new MenuScene.SceneID("TheHunter_Outro1", true);
            TheHunter_Outro2 = new MenuScene.SceneID("TheHunter_Outro2", true);
            TheHunter_Outro3 = new MenuScene.SceneID("TheHunter_Outro3", true);
            TheHunter_Outro4 = new MenuScene.SceneID("TheHunter_Outro4", true);
            TheHunter_Outro5 = new MenuScene.SceneID("TheHunter_Outro5", true);
            TheHunter_Outro6 = new MenuScene.SceneID("TheHunter_Outro6", true);
            TheHunter_Outro7 = new MenuScene.SceneID("TheHunter_Outro7", true);
            TheHunter_Outro8 = new MenuScene.SceneID("TheHunter_Outro8", true);
            TheHunter_Outro9 = new MenuScene.SceneID("TheHunter_Outro9", true);
            TheHunter_Outro10 = new MenuScene.SceneID("TheHunter_Outro10", true);
            TheHunter_Outro11 = new MenuScene.SceneID("TheHunter_Outro11", true);
            TheHunter_Outro12 = new MenuScene.SceneID("TheHunter_Outro12", true);

            TheHunter_Outro5_Dead = new MenuScene.SceneID("TheHunter_Outro5_Dead", true);
            TheHunter_Outro6_Dead = new MenuScene.SceneID("TheHunter_Outro6_Dead", true);
            TheHunter_Outro7_Dead = new MenuScene.SceneID("TheHunter_Outro7_Dead", true);
            TheHunter_Outro8_Dead = new MenuScene.SceneID("TheHunter_Outro8_Dead", true);
            TheHunter_Outro9_Dead = new MenuScene.SceneID("TheHunter_Outro9_Dead", true);
            TheHunter_Outro10_Dead = new MenuScene.SceneID("TheHunter_Outro10_Dead", true);
            TheHunter_Outro11_Dead = new MenuScene.SceneID("TheHunter_Outro11_Dead", true);
            TheHunter_Outro12_Dead = new MenuScene.SceneID("TheHunter_Outro12_Dead", true);

            TheHunter_AltEndScene = new MenuScene.SceneID("TheHunter_AltEndScene", true);
            TheHunter_AltEndScene_Final = new MenuScene.SceneID("TheHunter_AltEndScene_Final", true);
            TheHunter_AltEndScene_Dead = new MenuScene.SceneID("TheHunter_AltEndScene_Dead", true);
        }

        public static void UnregisterValues()
        {
            HunterExpansionEnums.Unregister(TheHunter_Outro1);
            HunterExpansionEnums.Unregister(TheHunter_Outro2);
            HunterExpansionEnums.Unregister(TheHunter_Outro3);
            HunterExpansionEnums.Unregister(TheHunter_Outro4);
            HunterExpansionEnums.Unregister(TheHunter_Outro5);
            HunterExpansionEnums.Unregister(TheHunter_Outro6);
            HunterExpansionEnums.Unregister(TheHunter_Outro7);
            HunterExpansionEnums.Unregister(TheHunter_Outro8);
            HunterExpansionEnums.Unregister(TheHunter_Outro9);
            HunterExpansionEnums.Unregister(TheHunter_Outro10);
            HunterExpansionEnums.Unregister(TheHunter_Outro11);
            HunterExpansionEnums.Unregister(TheHunter_Outro12);

            HunterExpansionEnums.Unregister(TheHunter_Outro5_Dead);
            HunterExpansionEnums.Unregister(TheHunter_Outro6_Dead);
            HunterExpansionEnums.Unregister(TheHunter_Outro7_Dead);
            HunterExpansionEnums.Unregister(TheHunter_Outro8_Dead);
            HunterExpansionEnums.Unregister(TheHunter_Outro9_Dead);
            HunterExpansionEnums.Unregister(TheHunter_Outro10_Dead);
            HunterExpansionEnums.Unregister(TheHunter_Outro11_Dead);
            HunterExpansionEnums.Unregister(TheHunter_Outro12_Dead);

            HunterExpansionEnums.Unregister(TheHunter_AltEndScene);
            HunterExpansionEnums.Unregister(TheHunter_AltEndScene_Final);
            HunterExpansionEnums.Unregister(TheHunter_AltEndScene_Dead);
        }

        public static MenuScene.SceneID TheHunter_Outro1;
        public static MenuScene.SceneID TheHunter_Outro2;
        public static MenuScene.SceneID TheHunter_Outro3;
        public static MenuScene.SceneID TheHunter_Outro4;
        public static MenuScene.SceneID TheHunter_Outro5;
        public static MenuScene.SceneID TheHunter_Outro6;
        public static MenuScene.SceneID TheHunter_Outro7;
        public static MenuScene.SceneID TheHunter_Outro8;
        public static MenuScene.SceneID TheHunter_Outro9;
        public static MenuScene.SceneID TheHunter_Outro10;
        public static MenuScene.SceneID TheHunter_Outro11;
        public static MenuScene.SceneID TheHunter_Outro12;

        public static MenuScene.SceneID TheHunter_Outro5_Dead;
        public static MenuScene.SceneID TheHunter_Outro6_Dead;
        public static MenuScene.SceneID TheHunter_Outro7_Dead;
        public static MenuScene.SceneID TheHunter_Outro8_Dead;
        public static MenuScene.SceneID TheHunter_Outro9_Dead;
        public static MenuScene.SceneID TheHunter_Outro10_Dead;
        public static MenuScene.SceneID TheHunter_Outro11_Dead;
        public static MenuScene.SceneID TheHunter_Outro12_Dead;

        public static MenuScene.SceneID TheHunter_AltEndScene;
        public static MenuScene.SceneID TheHunter_AltEndScene_Final;
        public static MenuScene.SceneID TheHunter_AltEndScene_Dead;
    }
}
