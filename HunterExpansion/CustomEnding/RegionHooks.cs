using HunterExpansion.CustomSave;
using MonoMod.RuntimeDetour;
using MoreSlugcats;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace HunterExpansion.CustomEnding
{
    public class RegionHooks
    {
        private static BindingFlags propFlags = BindingFlags.Instance | BindingFlags.Public;
        private static BindingFlags methodFlags = BindingFlags.Static | BindingFlags.Public;
        public delegate bool orig_RegionGate_MeetRequirement(RegionGate self);

        //public static int animationTicker = 0;

        public static void Init()
        {
            //区域名相关
            On.Region.GetProperRegionAcronym += Region_GetProperRegionAcronym;
            On.OverWorld.GetRegion_string += OverWorld_GetRegion_string;

            //业力门相关（还有一个地图显示业力门符号的方法在HUD里）
            On.RegionGate.KarmaBlinkRed += RegionGate_KarmaBlinkRed;
            On.GateKarmaGlyph.Update += GateKarmaGlyph_Update;
            Hook hook = new Hook(typeof(RegionGate).GetProperty("MeetRequirement", RegionHooks.propFlags).GetGetMethod(), typeof(RegionHooks).GetMethod("RegionGate_get_MeetRequirement", RegionHooks.methodFlags));
            /*
            //自定义的倒塌业力门相关
            On.RegionGate.ChangeDoorStatus += RegionGate_ChangeDoorStatus;
            On.RegionGate.DetectZone += RegionGate_DetectZone;
            On.RegionGateGraphics.ctor += RegionGateGraphics_ctor;
            On.RegionGateGraphics.DoorGraphic.ctor += DoorGraphic_ctor;
            On.GateKarmaGlyph.ctor += GateKarmaGlyph_ctor;*/
        }

        public static bool RegionGate_KarmaBlinkRed(On.RegionGate.orig_KarmaBlinkRed orig, RegionGate self)
        {
            bool result = orig(self);
            if (ModManager.MSC && self.karmaRequirements[self.letThroughDir ? 0 : 1] == HunterExpansionEnums.GateRequirement.NSHLock)
            {
                result = false;
            }
            return result;
        }

        private static void GateKarmaGlyph_Update(On.GateKarmaGlyph.orig_Update orig, GateKarmaGlyph self, bool eu)
        {
            orig.Invoke(self, eu);
            //NSH封锁的业力门
            if (ModManager.MSC && self.requirement == HunterExpansionEnums.GateRequirement.NSHLock)
            {
                self.redSine += 1f;
                self.col = new Color(1f, Mathf.Sin(self.redSine / 25f) * 0.5f + 0.5f, Mathf.Sin(self.redSine / 25f) * 0.5f + 0.5f);
            }
            //珍珠打开业力门
            if (ModManager.MSC && ShouldPlayAnimation(self) != 0)
            {
                if (PearlFixedSave.pearlFixed && (EndingSession.openCount > 0 || EndingSession.openGate))
                {
                    /*
                    if (animationTicker % 3 == 0 && !self.animationFinished)
                    {
                        self.animationIndex++;
                    }
                    if (animationTicker % 15 == 0)
                    {
                        self.glyphIndex++;
                        if (self.glyphIndex < 10)
                        {
                            self.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Data_Bit, self.pos, 1f, 0.5f + UnityEngine.Random.value * 2f);
                        }
                    }
                    if (self.animationIndex > 9)
                    {
                        self.animationIndex = 0;
                    }
                    if (self.glyphIndex >= 10)
                    {
                        self.animationFinished = true;
                    }
                    else
                    {
                        self.animationFinished = false;
                        Vector2 pos = self.pos;
                        pos.x += (float)(self.glyphIndex % 3 * 9) - 8f;
                        pos.y += (float)(self.glyphIndex / 3 * 9) - 5f;
                    }
                    if (self.animationFinished && self.mismatchLabel != null && ShouldPlayAnimation(self) < 0)
                    {
                        self.mismatchLabel.NewPhrase(51);
                    }*/
                    if (EndingSession.openCount < 40)
                    {
                        self.col = Color.Lerp(self.col, new Color(1f, 0f, 0f), ((float)EndingSession.openCount) / 40f);
                    }
                    else if (EndingSession.openCount >= 40 && EndingSession.openCount < 225)
                    {
                        if (EndingSession.openCount % 20 <= 5)
                        {
                            self.col = Color.Lerp(self.col, new Color(0f, 1f, 0f), ((float)EndingSession.openCount % 20) / 5f);
                        }
                        else
                        {
                            self.col = Color.Lerp(self.col, new Color(1f, 0f, 0f), ((float)EndingSession.openCount % 20 - 5f) / 15f);
                        }
                    }
                    else if (EndingSession.openCount <= 300)
                    {
                        self.col = Color.Lerp(self.col, new Color(0f, 1f, 0f), Mathf.Min(((float)EndingSession.openCount - 225f) / 40f, 1f));
                    }
                    if (EndingSession.openGate && EndingSession.openGateName == self.room.abstractRoom.name)
                    {
                        self.col = new Color(0f, 1f, 0f);
                    }
                }
            }
        }

        public static bool RegionGate_get_MeetRequirement(RegionHooks.orig_RegionGate_MeetRequirement orig, RegionGate self)
        {
            AbstractCreature firstAlivePlayer = self.room.game.FirstAlivePlayer;
            if (self.room.game.Players.Count == 0 || firstAlivePlayer == null || (firstAlivePlayer.realizedCreature == null && ModManager.CoopAvailable))
            {
                return false;
            }
            Player player;
            if (ModManager.CoopAvailable && self.room.game.AlivePlayers.Count > 0)
            {
                player = (self.room.game.FirstAlivePlayer.realizedCreature as Player);
            }
            else
            {
                player = (self.room.game.Players[0].realizedCreature as Player);
            }
            if (player == null)
            {
                return false;
            }
            bool result = orig(self);
            //NSH封锁了业力门
            if (self.room.world.region.name == "NSH" &&
                self.room.abstractRoom.name.Contains("AVA") ||
                (self.room.abstractRoom.name.Contains("DGL") && self.karmaRequirements[(!self.letThroughDir) ? 1 : 0] == HunterExpansionEnums.GateRequirement.NSHLock))
            {
                return HunterExpansionEnums.GateRequirement.customNSHGateRequirements(self);
            }
            //珍珠解锁业力门
            if ((self.room.game.session as StoryGameSession).saveStateNumber == SlugcatStats.Name.Red &&
                EndingSession.openGate && EndingSession.openGateName == self.room.abstractRoom.name && self.EnergyEnoughToOpen)
            {
                return true;
            }
            //倒塌的地底-外层空间业力门
            if (self.room.world.region.name == "SB" &&
                self.room.abstractRoom.name.Contains("NSH") && self.karmaRequirements[(!self.letThroughDir) ? 1 : 0] == MoreSlugcatsEnums.GateRequirement.OELock)
            {
                return HunterExpansionEnums.GateRequirement.customNSHGateRequirements(self);
            }
            return result;
        }

        public static string Region_GetProperRegionAcronym(On.Region.orig_GetProperRegionAcronym orig, SlugcatStats.Name character, string baseAcronym)
        {
            Plugin.Log("Get Proper Region Acronym (Orinal): " + baseAcronym);
            string result = orig(character, baseAcronym);
            string text = baseAcronym;
            if (PearlFixedSave.pearlFixed &&
                (EndingSession.openGate || (character != Plugin.SlugName && EndingSession.openCount > 0))
                && text == "OE")
            {
                text = "NSH";
                baseAcronym = "NSH";
                string[] array = AssetManager.ListDirectory("World", true, false);
                for (int i = 0; i < array.Length; i++)
                {
                    string path = AssetManager.ResolveFilePath(string.Concat(new string[]
                    {
                        "World",
                        Path.DirectorySeparatorChar.ToString(),
                        Path.GetFileName(array[i]),
                        Path.DirectorySeparatorChar.ToString(),
                        "equivalences.txt"
                    }));
                    if (File.Exists(path))
                    {
                        string[] array2 = File.ReadAllText(path).Trim().Split(new char[]
                        {
                            ','
                        });
                        for (int j = 0; j < array2.Length; j++)
                        {
                            string text2 = null;
                            string a = array2[j];
                            if (array2[j].Contains("-"))
                            {
                                a = array2[j].Split(new char[]
                                {
                                    '-'
                                })[0];
                                text2 = array2[j].Split(new char[]
                                {
                                    '-'
                                })[1];
                            }
                            if (a == baseAcronym && (text2 == null || character.value.ToLower() == text2.ToLower()))
                            {
                                text = Path.GetFileName(array[i]).ToUpper();
                            }
                        }
                    }
                }
                result = text;
            }
            if (text == "DGL")
            {
                text = RoomSpecificScript.shouldGoRegionName;
                baseAcronym = RoomSpecificScript.shouldGoRegionName;
                string[] array = AssetManager.ListDirectory("World", true, false);
                for (int i = 0; i < array.Length; i++)
                {
                    string path = AssetManager.ResolveFilePath(string.Concat(new string[]
                    {
                        "World",
                        Path.DirectorySeparatorChar.ToString(),
                        Path.GetFileName(array[i]),
                        Path.DirectorySeparatorChar.ToString(),
                        "equivalences.txt"
                    }));
                    if (File.Exists(path))
                    {
                        string[] array2 = File.ReadAllText(path).Trim().Split(new char[]
                        {
                            ','
                        });
                        for (int j = 0; j < array2.Length; j++)
                        {
                            string text2 = null;
                            string a = array2[j];
                            if (array2[j].Contains("-"))
                            {
                                a = array2[j].Split(new char[]
                                {
                                    '-'
                                })[0];
                                text2 = array2[j].Split(new char[]
                                {
                                    '-'
                                })[1];
                            }
                            if (a == baseAcronym && (text2 == null || character.value.ToLower() == text2.ToLower()))
                            {
                                text = Path.GetFileName(array[i]).ToUpper();
                            }
                        }
                    }
                }
                result = text;
            }
            Plugin.Log("Get Proper Region Acronym (New): " + result);
            return result;
        }

        public static Region OverWorld_GetRegion_string(On.OverWorld.orig_GetRegion_string orig, OverWorld self, string rName)
        {
            if (rName == "DGL")
                rName = RoomSpecificScript.shouldGoRegionName;
            return orig(self, rName);
        }

        public static int ShouldPlayAnimation(GateKarmaGlyph self)
        {/*
            if (!ModManager.MSC || self.requirement != MoreSlugcatsEnums.GateRequirement.OELock)
            {
                return 0;
            }*/
            if (self.gate.mode != RegionGate.Mode.MiddleClosed || !self.gate.EnergyEnoughToOpen || self.gate.unlocked)// || self.gate.letThroughDir == self.side
            {
                return 0;
            }
            int num = self.gate.PlayersInZone();
            /*if (num <= 0 || num >= 3)
            {
                return 0;
            }*/
            self.gate.letThroughDir = (num == 1);
            if (self.gate.dontOpen || self.gate.MeetRequirement)
            {
                return 1;
            }
            return -1;
        }
    }
}
