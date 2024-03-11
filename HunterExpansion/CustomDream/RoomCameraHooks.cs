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
    }
}
