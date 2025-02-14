using MoreSlugcats;
using UnityEngine;

namespace HunterExpansion
{
    public class OverseerHooks
    {
        public static void Init()
        {
            On.OverseerAbstractAI.ctor += OverseerAbstractAI_ctor;
            On.MoreSlugcats.Inspector.InitiateGraphicsModule += Inspector_InitiateGraphicsModule;
        }

        public static void OverseerAbstractAI_ctor(On.OverseerAbstractAI.orig_ctor orig, OverseerAbstractAI self, World world, AbstractCreature parent)
        {
            orig(self, world, parent);
            if (world.region != null && world.region.name == "NSH" && self.ownerIterator != 2 && self.ownerIterator != 3)
            {
                if (Random.value < 0.01f && ModManager.MSC)//1%的概率生成SRS的监视者
                {
                    self.ownerIterator = 3;
                }
                else//99%的概率生成NSH的监视者
                {
                    self.ownerIterator = 2;
                }
            }
        }

        public static void Inspector_InitiateGraphicsModule(On.MoreSlugcats.Inspector.orig_InitiateGraphicsModule orig, Inspector self)
        {
            if (self.ownerIterator == -1 && self.room.game.IsStorySession && self.room.world.region != null && self.room.world.region.name == "NSH")
            {
                self.ownerIterator = 2;
            }
            orig(self);
        }
        /*
        public static void OverseerGraphics_DrawSprites(On.OverseerGraphics.orig_DrawSprites orig, OverseerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (self.owner.room.world.region.name == "NSH" && Random.value > 0.01f)
            {
                sLeaser.sprites[self.GlowSprite].color = new Color(0f, 1f, 0f);
                sLeaser.sprites[self.BulbSprite].color = self.ColorOfSegment(0f, timeStacker);
                sLeaser.sprites[self.BkgBulbSprite].color = self.ColorOfSegment(0f, timeStacker);
                sLeaser.sprites[self.WhiteSprite].color = Color.Lerp(self.ColorOfSegment(0.75f, timeStacker), new Color(0f, 0f, 1f), 0.5f);
            }
            else if (self.owner.room.world.region.name == "NSH" && ModManager.MSC)
            {
                sLeaser.sprites[self.GlowSprite].color = new Color(1f, 0.2f, 0f);
                sLeaser.sprites[self.BulbSprite].color = self.ColorOfSegment(0f, timeStacker);
                sLeaser.sprites[self.BkgBulbSprite].color = self.ColorOfSegment(0f, timeStacker);
                sLeaser.sprites[self.WhiteSprite].color = Color.Lerp(self.ColorOfSegment(0.75f, timeStacker), new Color(0f, 0f, 1f), 0.5f);
            }
        }
    */
    }
}
