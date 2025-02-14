using HunterExpansion.CustomOracle;
using UnityEngine;

namespace HunterExpansion
{
    public class NSHSwarmerHooks
    {
        public static int colorCycle = 200;
        public static int colorCount = 0;
        public static bool colorReturn = false;

        public static int cycleSprite;
        public static float expand;
        public static float lastExpand;
        public static float getToExpand;
        public static float push;
        public static float lastPush;
        public static float getToPush;
        public static float white;
        public static float lastWhite;
        public static float getToWhite;

        public static void Init()
        {
            //On.NSHSwarmer.InitiateSprites += NSHSwarmer_InitiateSprites;
            On.NSHSwarmer.DrawSprites += NSHSwarmer_DrawSprites;
            On.NSHSwarmer.Update += NSHSwarmer_Update;
        }

        public static void NSHSwarmer_InitiateSprites(On.NSHSwarmer.orig_InitiateSprites orig, NSHSwarmer self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);
            /*
            if (self.room.game.StoryCharacter != null && self.room.game.StoryCharacter == Plugin.SlugName)
            {
                cycleSprite = sLeaser.sprites.Length;

                Array.Resize(ref sLeaser.sprites, cycleSprite + 2);

                //增加圆环
                for (int i = 0; i < 2; i++)
                {
                    sLeaser.sprites[cycleSprite + i] = new FSprite("Futile_White", true);
                    sLeaser.sprites[cycleSprite + i].shader = rCam.game.rainWorld.Shaders["VectorCircle"];
                    sLeaser.sprites[cycleSprite + i].color = new Color(0f, 1f, 0f);
                }

                self.AddToContainer(sLeaser, rCam, null);
            }*/
        }

        public static void NSHSwarmer_DrawSprites(On.NSHSwarmer.orig_DrawSprites orig, NSHSwarmer self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (!(self as UpdatableAndDeletable).slatedForDeletetion &&
                self.room != null &&
                self.room.game.StoryCharacter == Plugin.SlugName &&
                self.room.abstractRoom.name == "NSH_AI")
            {
                //变色
                for (int k = 0; k < sLeaser.sprites.Length; k++)
                {
                    sLeaser.sprites[k].color = NSHSwarmer_Color(self);
                }
                sLeaser.sprites[4].color = Color.Lerp(NSHSwarmer_Color(self), new Color(1f, 1f, 1f), 0.5f);
                //放大特效
                /*
                for (int j = self.holoShape.LinesCount - 1; j >= 0; j--)
                {
                    float multi = 3f;//放大倍率
                    sLeaser.sprites[6 + j].scale *= multi;//加粗？
                    sLeaser.sprites[6 + j].x += multi * (sLeaser.sprites[6 + j].x - sLeaser.sprites[0].x);//更加远离神经元中心
                    sLeaser.sprites[6 + j].y += multi * (sLeaser.sprites[6 + j].y - sLeaser.sprites[0].y);
                }*/

                /*
                if (self.room.oracleWantToSpawn != null && self.room.oracleWantToSpawn == NSHOracleRegistry.NSHOracle)
                {
                    for (int i = 0; i < self.room.physicalObjects.Length; i++)
                    {
                        for (int j = 0; j < self.room.physicalObjects[i].Count; j++)
                        {
                            if (self.room.physicalObjects[i][j] is Oracle)
                            {
                                //增加圆环
                                if (sLeaser.sprites[cycleSprite].isVisible != (self.room.physicalObjects[i][j] as Oracle).Consious)
                                {
                                    for (int k = 0; k < 2; k++)
                                    {
                                        sLeaser.sprites[cycleSprite + k].isVisible = (self.room.physicalObjects[i][j] as Oracle).Consious;
                                    }
                                }
                            }
                        }
                    }

                    
                    Vector2 vector = Vector2.Lerp(self.bodyChunks[1].lastPos, self.bodyChunks[1].pos, timeStacker);
                    vector = vector + Custom.DirVec(Vector2.Lerp(self.firstChunk.lastPos, self.firstChunk.pos, timeStacker), vector) * 20f;

                    for (int k = 0; k < 2; k++)
                    {
                        sLeaser.sprites[cycleSprite + k].x = vector.x - camPos.x;
                        sLeaser.sprites[cycleSprite + k].y = vector.y - camPos.y;
                        sLeaser.sprites[cycleSprite + k].scale = Radius((float)k, timeStacker) / 8f;
                        sLeaser.sprites[cycleSprite + k].color = NSHSwarmer_Color(self);
                    }
                    sLeaser.sprites[cycleSprite].alpha = Mathf.Lerp(3f / Radius(0f, timeStacker), 1f, Mathf.Lerp(lastWhite, white, timeStacker));
                    sLeaser.sprites[cycleSprite + 1].alpha = 3f / Radius(1f, timeStacker);
                }
            }
            else
            {
                for (int k = 0; k < 2; k++)
                {
                    sLeaser.sprites[cycleSprite + k].isVisible = false;
                }
            }
                */
            }
            orig(self, sLeaser, rCam, timeStacker, camPos);
        }

        public static void NSHSwarmer_Update(On.NSHSwarmer.orig_Update orig, NSHSwarmer self, bool eu)
        {
            orig(self, eu);
            //变色
            if (colorCount < 0.5f * colorCycle && !colorReturn)
            {
                colorCount++;
            }
            else if (colorCount >= 0.5f * colorCycle && !colorReturn)
            {
                colorReturn = true;
            }
            else if (colorCount > 0 && colorReturn)
            {
                colorCount--;
            }
            else if (colorCount <= 0 && colorReturn)
            {
                colorReturn = false;
            }
            /*
            //圆环
            lastExpand = expand;
            lastPush = push;
            lastWhite = white;
            expand = Custom.LerpAndTick(expand, getToExpand, 0.05f, 0.0125f);
            push = Custom.LerpAndTick(push, getToPush, 0.02f, 0.025f);
            white = Custom.LerpAndTick(white, getToWhite, 0.07f, 0.022727273f);
            bool flag = false;
            if (Random.value < 0.125f)
            {
                flag = (getToWhite < 1f);
                getToWhite = 1f;
            }
            else
            {
                getToWhite = 0f;
            }
            if (Random.value < 0.00625f || flag)
            {
                getToExpand = ((Random.value < 0.5f && !flag) ? 1f : Mathf.Lerp(0.8f, 2f, Mathf.Pow(Random.value, 1.5f)));
            }
            if (Random.value < 0.00625f || flag)
            {
                getToPush = ((Random.value < 0.5f && !flag) ? 0f : ((float)(-1 + Random.Range(0, Random.Range(1, 6)))));
            }*/
        }

        public static Color NSHSwarmer_Color(NSHSwarmer self)
        {
            Color newColor = Color.Lerp(self.myColor, NSHOracleColor.Purple, ((float)colorCount) / ((float)colorCycle));
            return newColor;
        }
        /*
        private static float Radius(float ring, float timeStacker)
        {
            return (3f + ring + Mathf.Lerp(lastPush, push, timeStacker) - 0.5f * NSHOracleMeetHunter.swarmerEnergy) * Mathf.Lerp(lastExpand, expand, timeStacker) * 10f;
            //return (3f + ring + Mathf.Lerp(lastPush, push, timeStacker) - 0.5f * owner.averageVoice) * Mathf.Lerp(lastExpand, expand, timeStacker) * 10f;
        }*/
    }
}
