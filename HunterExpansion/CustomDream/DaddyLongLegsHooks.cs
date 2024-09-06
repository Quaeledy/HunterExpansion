using CustomDreamTx;
using HunterExpansion.CustomOracle;
using HunterExpansion.CustomSave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HunterExpansion
{
    public class DaddyLongLegsHooks
    {
        public static void Init()
        {
            On.DaddyLongLegs.Update += DaddyLongLegs_Update;
            On.DaddyGraphics.ApplyPalette += DaddyGraphics_ApplyPalette;
            On.DaddyGraphics.DaddyTubeGraphic.ApplyPalette += DaddyTubeGraphic_ApplyPalette;
            On.DaddyGraphics.DaddyDangleTube.ApplyPalette += DaddyDangleTube_ApplyPalette;
            On.DaddyGraphics.DaddyDeadLeg.ApplyPalette += DaddyDeadLeg_ApplyPalette;

            On.DaddyCorruption.Update += DaddyCorruption_Update;
            On.DaddyCorruption.CorruptionTube.TubeGraphic.ApplyPalette += CorruptionTube_ApplyPalette;
            On.DaddyCorruption.Bulb.ApplyPalette += Bulb_ApplyPalette;
        }
        #region 自由移动的腐化
        public static void DaddyLongLegs_Update(On.DaddyLongLegs.orig_Update orig, DaddyLongLegs self, bool eu)
        {
            orig(self, eu);
            DaddyLongLegs dc = self;
            bool flag = dc.room != null && dc.room.world.region != null && dc.room.world.region.name == "NSH";
            if (flag)
            {
                self.effectColor = new Color(0.57255f, 0.11373f, 0.22745f);
                self.eyeColor = self.effectColor;
            }
        }

        public static void DaddyGraphics_ApplyPalette(On.DaddyGraphics.orig_ApplyPalette orig, DaddyGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);
            PhysicalObject dc = self.owner;
            bool flag = dc.room != null && dc.room.world.region != null && dc.room.world.region.name == "NSH";
            if (flag)
            {
                for (int i = 0; i < self.daddy.bodyChunks.Length; i++)
                {
                    sLeaser.sprites[self.BodySprite(i)].color = Color.Lerp(PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.Red), Color.gray, 0.4f);
                }
            }
        }

        public static void DaddyTubeGraphic_ApplyPalette(On.DaddyGraphics.DaddyTubeGraphic.orig_ApplyPalette orig, DaddyGraphics.DaddyTubeGraphic self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);
            PhysicalObject dc = self.owner.owner;
            bool flag = dc.room != null && dc.room.world.region != null && dc.room.world.region.name == "NSH";
            if (flag)
            {
                Color color = Color.Lerp(PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.Red), Color.gray, 0.4f);
                Color EffectColor = new Color(0.57255f, 0.11373f, 0.22745f);
                for (int i = 0; i < (sLeaser.sprites[self.firstSprite] as TriangleMesh).vertices.Length; i++)
                {
                    float floatPos = Mathf.InverseLerp(0.3f, 1f, (float)i / (float)((sLeaser.sprites[self.firstSprite] as TriangleMesh).vertices.Length - 1));
                    (sLeaser.sprites[self.firstSprite] as TriangleMesh).verticeColors[i] = Color.Lerp(color, EffectColor, self.OnTubeEffectColorFac(floatPos));
                }
                int num = 0;
                for (int j = 0; j < self.bumps.Length; j++)
                {
                    sLeaser.sprites[self.firstSprite + 1 + j].color = Color.Lerp(color, EffectColor, self.OnTubeEffectColorFac(self.bumps[j].pos.y));
                    if (self.bumps[j].eyeSize > 0f)
                    {
                        sLeaser.sprites[self.firstSprite + 1 + self.bumps.Length + num].color = (self.owner.colorClass ? EffectColor : color);
                        num++;
                    }
                }
            }
        }

        public static void DaddyDangleTube_ApplyPalette(On.DaddyGraphics.DaddyDangleTube.orig_ApplyPalette orig, DaddyGraphics.DaddyDangleTube self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);
            PhysicalObject dc = self.owner.owner;
            bool flag = dc.room != null && dc.room.world.region != null && dc.room.world.region.name == "NSH";
            if (flag)
            {
                Color color = Color.Lerp(PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.Red), Color.gray, 0.4f);
                Color EffectColor = new Color(0.57255f, 0.11373f, 0.22745f);
                for (int i = 0; i < (sLeaser.sprites[self.firstSprite] as TriangleMesh).vertices.Length; i++)
                {
                    float floatPos = Mathf.InverseLerp(0.3f, 1f, (float)i / (float)((sLeaser.sprites[self.firstSprite] as TriangleMesh).vertices.Length - 1));
                    (sLeaser.sprites[self.firstSprite] as TriangleMesh).verticeColors[i] = Color.Lerp(color, EffectColor, self.OnTubeEffectColorFac(floatPos));
                }
                sLeaser.sprites[self.firstSprite].color = color;
                for (int j = 0; j < self.bumps.Length; j++)
                {
                    sLeaser.sprites[self.firstSprite + 1 + j].color = Color.Lerp(color, EffectColor, self.OnTubeEffectColorFac(self.bumps[j].pos.y));
                }
            }
        }

        public static void DaddyDeadLeg_ApplyPalette(On.DaddyGraphics.DaddyDeadLeg.orig_ApplyPalette orig, DaddyGraphics.DaddyDeadLeg self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);
            PhysicalObject dc = self.owner.owner;
            bool flag = dc.room != null && dc.room.world.region != null && dc.room.world.region.name == "NSH";
            if (flag)
            {
                Color color = Color.Lerp(PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.Red), Color.gray, 0.4f);
                Color EffectColor = new Color(0.57255f, 0.11373f, 0.22745f);
                for (int i = 0; i < (sLeaser.sprites[self.firstSprite] as TriangleMesh).vertices.Length; i++)
                {
                    float floatPos = Mathf.InverseLerp(0.3f, 1f, (float)i / (float)((sLeaser.sprites[self.firstSprite] as TriangleMesh).vertices.Length - 1));
                    (sLeaser.sprites[self.firstSprite] as TriangleMesh).verticeColors[i] = Color.Lerp(color, EffectColor, self.OnTubeEffectColorFac(floatPos));
                }
                int num = 0;
                for (int j = 0; j < self.bumps.Length; j++)
                {
                    sLeaser.sprites[self.firstSprite + 1 + j].color = Color.Lerp(color, EffectColor, self.OnTubeEffectColorFac(self.bumps[j].pos.y));
                    if (self.bumps[j].eyeSize > 0f)
                    {
                        sLeaser.sprites[self.firstSprite + 1 + self.bumps.Length + num].color = (self.owner.colorClass ? (EffectColor * Mathf.Lerp(0.5f, 0.2f, self.deadness)) : color);
                        num++;
                    }
                }
            }
        }
        #endregion

        #region 腐化墙
        public static void DaddyCorruption_Update(On.DaddyCorruption.orig_Update orig, DaddyCorruption self, bool eu)
        {
            orig(self, eu);
            DaddyCorruption dc = self;
            bool flag = dc.room != null && dc.room.world.region != null && dc.room.world.region.name == "NSH";
            if (flag)
            {
                self.effectColor = new Color(0.57255f, 0.11373f, 0.22745f);
                self.eyeColor = self.effectColor;
            }
        }

        public static void CorruptionTube_ApplyPalette(On.DaddyCorruption.CorruptionTube.TubeGraphic.orig_ApplyPalette orig, DaddyCorruption.CorruptionTube.TubeGraphic self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);
            DaddyCorruption dc = self.owner.owner;
            bool flag = dc.room != null && dc.room.world.region != null && dc.room.world.region.name == "NSH";
            if (flag)
            {
                Color color = Color.Lerp(PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.Red), Color.gray, 0.4f);
                Color effectColor = new Color(0.57255f, 0.11373f, 0.22745f); 
                for (int i = 0; i < (sLeaser.sprites[self.firstSprite] as TriangleMesh).vertices.Length; i++)
                {
                    float floatPos = (float)i / (float)((sLeaser.sprites[self.firstSprite] as TriangleMesh).vertices.Length - 1);
                    (sLeaser.sprites[self.firstSprite] as TriangleMesh).verticeColors[i] = Color.Lerp(color, effectColor, self.OnTubeEffectColorFac(floatPos));
                }
                int num = 0;
                for (int j = 0; j < self.bumps.Length; j++)
                {
                    sLeaser.sprites[self.firstSprite + 1 + j].color = Color.Lerp(color, effectColor, self.OnTubeEffectColorFac(self.bumps[j].pos.y));
                    if (self.bumps[j].eyeSize > 0f)
                    {
                        sLeaser.sprites[self.firstSprite + 1 + self.bumps.Length + num].color = (self.owner.owner.GWmode ? color : effectColor);
                        num++;
                    }
                }
            }
        }

        public static void Bulb_ApplyPalette(On.DaddyCorruption.Bulb.orig_ApplyPalette orig, DaddyCorruption.Bulb self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);

            DaddyCorruption dc = self.owner;
            bool flag = dc.room != null && dc.room.world.region != null && dc.room.world.region.name == "NSH";
            if (flag)
            {
                sLeaser.sprites[self.firstSprite].color = Color.Lerp(PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.Red), Color.gray, 0.4f);
            }
        }
        #endregion
    }
}
