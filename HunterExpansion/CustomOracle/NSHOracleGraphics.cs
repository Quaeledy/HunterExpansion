using CustomOracleTx;
using RWCustom;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HunterExpansion.CustomOracle
{
    public class NSHOracleGraphics : CustomOracleGraphic
    {
        public GownCover[] cover;
        public int coverSprite;

        public GownRibbon[] ribbon;
        public int ribbonSprite;

        public int signSprite;

        public NSHOracleGraphics(PhysicalObject ow) : base(ow)
        {
            callBaseApplyPalette = false;
            callBaseInitiateSprites = false;

            Random.State state = Random.state;
            Random.InitState(42);
            totalSprites = 0;
            armJointGraphics = new ArmJointGraphics[oracle.arm.joints.Length];

            for (int i = 0; i < oracle.arm.joints.Length; i++)
            {
                armJointGraphics[i] = new ArmJointGraphics(this, oracle.arm.joints[i], totalSprites);
                totalSprites += armJointGraphics[i].totalSprites;
            }


            firstUmbilicalSprite = totalSprites;
            umbCord = new UbilicalCord(this, totalSprites);
            totalSprites += umbCord.totalSprites;


            firstBodyChunkSprite = totalSprites;
            totalSprites += 2;
            neckSprite = totalSprites;
            totalSprites++;
            firstFootSprite = totalSprites;
            totalSprites += 4;

            //光环
            halo = new Halo(this, totalSprites);
            totalSprites += halo.totalSprites;

            //袍子
            gown = new Gown(this);
            robeSprite = totalSprites;
            totalSprites++;


            firstHandSprite = totalSprites;
            totalSprites += 4;
            head = new GenericBodyPart(this, 5f, 0.5f, 0.995f, oracle.firstChunk);
            firstHeadSprite = totalSprites;
            totalSprites += 10;
            fadeSprite = totalSprites;
            totalSprites++;

            killSprite = totalSprites;
            totalSprites++;

            hands = new GenericBodyPart[2];

            for (int j = 0; j < 2; j++)
            {
                hands[j] = new GenericBodyPart(this, 2f, 0.5f, 0.98f, oracle.firstChunk);
            }
            feet = new GenericBodyPart[2];
            for (int k = 0; k < 2; k++)
            {
                feet[k] = new GenericBodyPart(this, 2f, 0.5f, 0.98f, oracle.firstChunk);
            }
            knees = new Vector2[2, 2];
            for (int l = 0; l < 2; l++)
            {
                for (int m = 0; m < 2; m++)
                {
                    knees[l, m] = oracle.firstChunk.pos;
                }
            }
            firstArmBaseSprite = totalSprites;
            armBase = new ArmBase(this, firstArmBaseSprite);
            totalSprites += armBase.totalSprites;

            //披肩
            cover = new GownCover[1];
            for (int i = 0; i < 1; i++)
                cover[i] = new GownCover(this);
            coverSprite = totalSprites;
            totalSprites += 1;

            //飘带
            ribbon = new GownRibbon[2];
            for (int i = 0; i < 2; i++)
                ribbon[i] = new GownRibbon(this);
            ribbonSprite = totalSprites;
            totalSprites += 2;

            //可爱标志
            signSprite = totalSprites;
            totalSprites += 1;

            voiceFreqSamples = new float[64];
            Random.state = state;

            //这一条是为了将玩家图层移动到迭代器图层
            this.internalContainerObjects = new List<GraphicsModule.ObjectHeldInInternalContainer>();
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            base.AddToContainer(sLeaser, rCam, newContatiner);
            if (newContatiner == null)
            {
                newContatiner = rCam.ReturnFContainer("Midground");
            }
            newContatiner.AddChild(sLeaser.containers[0]);
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            base.ApplyPalette(sLeaser, rCam, palette);

            for (int i = 0; i < armJointGraphics.Length; i++)
            {
                armJointGraphics[i].ApplyPalette(sLeaser, rCam, palette);
                armJointGraphics[i].metalColor = palette.blackColor;
            }
            Color color = NSHOracleColor.Green;

            for (int j = 0; j < base.owner.bodyChunks.Length; j++)
            {
                sLeaser.sprites[firstBodyChunkSprite + j].color = NSHOracleColor.Violet;//color;
            }
            sLeaser.sprites[neckSprite].color = NSHOracleColor.Pink;//color;
            sLeaser.sprites[HeadSprite].color = color;
            sLeaser.sprites[ChinSprite].color = color;

            for (int k = 0; k < 2; k++)
            {
                sLeaser.sprites[EyeSprite(k)].color = NSHOracleRegistry.isCorrupted ? NSHOracleColor.CorruptedRed : NSHOracleColor.DarkGreen;
            }

            for (int k = 0; k < 2; k++)
            {
                if (this.armJointGraphics.Length == 0)
                {
                    sLeaser.sprites[this.PhoneSprite(k, 0)].color = this.GenericJointBaseColor();
                    sLeaser.sprites[this.PhoneSprite(k, 1)].color = this.GenericJointHighLightColor();
                    sLeaser.sprites[this.PhoneSprite(k, 2)].color = this.GenericJointHighLightColor();
                }
                else
                {
                    sLeaser.sprites[this.PhoneSprite(k, 0)].color = this.armJointGraphics[0].BaseColor(default(Vector2));
                    sLeaser.sprites[this.PhoneSprite(k, 1)].color = this.armJointGraphics[0].HighLightColor(default(Vector2));
                    sLeaser.sprites[this.PhoneSprite(k, 2)].color = this.armJointGraphics[0].HighLightColor(default(Vector2));
                }

                sLeaser.sprites[HandSprite(k, 0)].color = color;
                if (gown != null)
                {
                    for (int l = 0; l < 4; l++)
                    {
                        Color handColor = Gown_Color(gown, (float)l / 7f);
                        (sLeaser.sprites[HandSprite(k, 1)] as TriangleMesh).verticeColors[l * 4] = handColor;
                        (sLeaser.sprites[HandSprite(k, 1)] as TriangleMesh).verticeColors[l * 4 + 1] = handColor;
                        (sLeaser.sprites[HandSprite(k, 1)] as TriangleMesh).verticeColors[l * 4 + 2] = handColor;
                        (sLeaser.sprites[HandSprite(k, 1)] as TriangleMesh).verticeColors[l * 4 + 3] = handColor;
                    }
                    for (int l = 4; l < 7; l++)
                    {
                        Color handColor = Gown_Color(gown, (float)l / 7f);
                        (sLeaser.sprites[HandSprite(k, 1)] as TriangleMesh).verticeColors[l * 4] = handColor;
                        (sLeaser.sprites[HandSprite(k, 1)] as TriangleMesh).verticeColors[l * 4 + 1] = handColor;
                        (sLeaser.sprites[HandSprite(k, 1)] as TriangleMesh).verticeColors[l * 4 + 2] = handColor;
                        (sLeaser.sprites[HandSprite(k, 1)] as TriangleMesh).verticeColors[l * 4 + 3] = handColor;
                    }
                }
                else
                {
                    sLeaser.sprites[HandSprite(k, 1)].color = color;
                }
                sLeaser.sprites[FootSprite(k, 0)].color = color;
                sLeaser.sprites[FootSprite(k, 1)].color = color;
            }
            if (umbCord != null)
            {
                umbCord.ApplyPalette(sLeaser, rCam, palette);
                sLeaser.sprites[firstUmbilicalSprite].color = palette.blackColor;
            }
            else if (discUmbCord != null)
            {
                discUmbCord.ApplyPalette(sLeaser, rCam, palette);
            }
            if (armBase != null)
            {
                armBase.ApplyPalette(sLeaser, rCam, palette);
            }

            sLeaser.sprites[signSprite].color = NSHOracleColor.LightGreen;
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[totalSprites];

            sLeaser.containers = new FContainer[]
            {
                new FContainer()
            };

            for (int i = 0; i < base.owner.bodyChunks.Length; i++)
            {
                sLeaser.sprites[firstBodyChunkSprite + i] = new FSprite("Circle20", true);
                sLeaser.sprites[firstBodyChunkSprite + i].scale = base.owner.bodyChunks[i].rad / 10f;
                sLeaser.sprites[firstBodyChunkSprite + i].color = new Color(1f, (i == 0) ? 0.5f : 0f, (i == 0) ? 0.5f : 0f);
            }

            for (int j = 0; j < armJointGraphics.Length; j++)
            {
                armJointGraphics[j].InitiateSprites(sLeaser, rCam);
            }

            if (gown != null)
            {
                if (NSHOracleRegistry.isCorrupted)
                {
                    sLeaser.sprites[robeSprite] = TriangleMesh.MakeGridMesh("MoonCloakTex", gown.divs - 1);
                }
                else
                {
                    sLeaser.sprites[robeSprite] = TriangleMesh.MakeGridMesh("Futile_White", gown.divs - 1);
                }
                for (int i = 0; i < gown.divs; i++)
                {
                    for (int j = 0; j < gown.divs; j++)
                    {
                        (sLeaser.sprites[robeSprite] as TriangleMesh).verticeColors[j * gown.divs + i] = gown.Color((float)i / (float)(gown.divs - 1));
                    }
                }
                /*
                gown.InitiateSprite(robeSprite, sLeaser, rCam);
                if (NSHOracleRegistry.isCorrupted)
                    sLeaser.sprites[robeSprite].element = Futile.atlasManager.GetElementWithName("MoonCloakTex");*/
            }

            if (halo != null)
            {
                halo.InitiateSprites(sLeaser, rCam);
            }

            if (armBase != null)
            {
                armBase.InitiateSprites(sLeaser, rCam);
            }
            sLeaser.sprites[neckSprite] = new FSprite("pixel", true);
            sLeaser.sprites[neckSprite].scaleX = 3f;
            sLeaser.sprites[neckSprite].anchorY = 0f;
            sLeaser.sprites[HeadSprite] = new FSprite("Circle20", true);
            sLeaser.sprites[ChinSprite] = new FSprite("Circle20", true);
            for (int k = 0; k < 2; k++)
            {
                sLeaser.sprites[EyeSprite(k)] = new FSprite("pixel", true);

                sLeaser.sprites[PhoneSprite(k, 0)] = new FSprite("Circle20", true);
                sLeaser.sprites[PhoneSprite(k, 1)] = new FSprite("Circle20", true);
                sLeaser.sprites[PhoneSprite(k, 2)] = new FSprite("LizardScaleA1", true);
                sLeaser.sprites[PhoneSprite(k, 2)].anchorY = 0f;
                sLeaser.sprites[PhoneSprite(k, 2)].scaleY = 0f;//耳机长度
                sLeaser.sprites[PhoneSprite(k, 2)].scaleX = ((k == 0) ? -1f : 1f) * 0.75f;

                sLeaser.sprites[HandSprite(k, 0)] = new FSprite("haloGlyph-1", true);
                sLeaser.sprites[HandSprite(k, 1)] = TriangleMesh.MakeLongMesh(7, false, true);
                sLeaser.sprites[FootSprite(k, 0)] = new FSprite("haloGlyph-1", true);
                sLeaser.sprites[FootSprite(k, 1)] = TriangleMesh.MakeLongMesh(7, false, true);
            }

            if (umbCord != null)
            {
                umbCord.InitiateSprites(sLeaser, rCam);
            }
            else if (discUmbCord != null)
            {
                discUmbCord.InitiateSprites(sLeaser, rCam);
            }

            sLeaser.sprites[HeadSprite].scaleX = head.rad / 9f;
            sLeaser.sprites[HeadSprite].scaleY = head.rad / 11f;
            sLeaser.sprites[ChinSprite].scale = head.rad / 15f;
            //光晕
            sLeaser.sprites[fadeSprite] = new FSprite("Futile_White", true);
            sLeaser.sprites[fadeSprite].scale = 12.5f;
            sLeaser.sprites[fadeSprite].color = Color.Lerp(NSHOracleColor.Green, Color.black, 0.3f);
            sLeaser.sprites[fadeSprite].shader = rCam.game.rainWorld.Shaders["FlatLightBehindTerrain"];
            sLeaser.sprites[fadeSprite].alpha = 0.5f;

            sLeaser.sprites[killSprite] = new FSprite("Futile_White", true);
            sLeaser.sprites[killSprite].isVisible = false;
            sLeaser.sprites[killSprite].shader = rCam.game.rainWorld.Shaders["FlatLight"];

            for (int i = 0; i < cover.Length; i++)
                cover[i].InitiateSprites(coverSprite + i, sLeaser, rCam);
            for (int i = 0; i < ribbon.Length; i++)
                ribbon[i].InitiateSprites(ribbonSprite + i, sLeaser, rCam);

            sLeaser.sprites[signSprite] = new FSprite("NSHSign", true);

            base.InitiateSprites(sLeaser, rCam);

            for (int i = 0; i < cover.Length; i++)
            {
                rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[coverSprite + i]);
                sLeaser.sprites[coverSprite + i].MoveBehindOtherNode(sLeaser.sprites[HeadSprite]);
            }
            for (int i = 0; i < ribbon.Length; i++)
            {
                rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[ribbonSprite + i]);
                sLeaser.sprites[ribbonSprite + i].MoveBehindOtherNode(sLeaser.sprites[FootSprite(0, 1)]);
            }
            rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[signSprite]);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

            for (int i = 0; i < cover.Length; i++)
                cover[i].DrawSprites(i, robeSprite, coverSprite + i, sLeaser, rCam, timeStacker, camPos);
            for (int i = 0; i < ribbon.Length; i++)
                ribbon[i].DrawSprites(i, ribbonSprite + i, sLeaser, rCam, timeStacker, camPos);

            //可爱标志
            Vector2 vector = Vector2.Lerp(base.owner.firstChunk.lastPos, base.owner.firstChunk.pos, timeStacker);
            Vector2 vector3 = Vector2.Lerp(this.head.lastPos, this.head.pos, timeStacker);
            Vector2 vector4 = Custom.DirVec(vector3, vector);
            Vector2 a3 = Custom.PerpendicularVector(vector4);
            Vector2 vector5 = this.RelativeLookDir(timeStacker);
            Vector2 vector16 = vector3 + a3 * vector5.x * 2.5f + vector4 * (-2f - vector5.y * 1.5f);
            sLeaser.sprites[signSprite].x = vector16.x - camPos.x;
            sLeaser.sprites[signSprite].y = vector16.y - camPos.y;
            sLeaser.sprites[signSprite].rotation = 45f + Custom.AimFromOneVectorToAnother(vector16, vector3 - vector4 * 10f);//因为是正方形所以要加45°
            sLeaser.sprites[signSprite].scaleX = 0.65f * Mathf.Lerp(0.8f, 0.6f, Mathf.Abs(vector5.x));
            sLeaser.sprites[signSprite].scaleY = 0.65f * Custom.LerpMap(vector5.y, 0f, 1f, 0.8f, 0.2f);
        }

        public override void Update()
        {
            base.Update();
            lightsource.color = Color.Lerp(NSHOracleColor.LightGreen, Color.white, 0.4f);
            if (oracle.room.abstractRoom.name == "HR_AI")
            {
                lightsource.alpha = 0f;
            }
            if (NSHOracleRegistry.isCorrupted)
            {
                lightsource.color = NSHOracleColor.CorruptedRed;
            }

            for (int i = 0; i < cover.Length; i++)
                cover[i].Update();
            for (int i = 0; i < ribbon.Length; i++)
                ribbon[i].Update(i);
        }

        #region 继承的颜色
        public override Color ArmJoint_HighLightColor(ArmJointGraphics armJointGraphics, Vector2 pos)
        {
            //return NSHOracleColor.DarkGrey;
            float h = 0.75f;
            if (armJointGraphics.owner.owner.room.abstractRoom.name == "HR_AI" || NSHOracleRegistry.isCorrupted)
                h = 0.025f;
            return Color.Lerp(Custom.HSL2RGB(h, Mathf.Lerp(0.5f, 0.1f, Mathf.Pow(1f, 0.5f)), Mathf.Lerp(0.15f, 0.85f - 0.65f * this.oracle.room.Darkness(pos), Mathf.Pow(1f, 0.45f))), new Color(0f, 0f, 0.15f), Mathf.Pow(Mathf.InverseLerp(0.45f, -0.05f, 1f), 0.9f) * 0.4f);
        }

        public override Color ArmJoint_BaseColor(ArmJointGraphics armJointGraphics, Vector2 pos)
        {
            //return NSHOracleColor.VeryDarkGrey;
            float h = 0.75f;
            if (armJointGraphics.owner.owner.room.abstractRoom.name == "HR_AI" || NSHOracleRegistry.isCorrupted)
                h = 0.025f;
            return Color.Lerp(Custom.HSL2RGB(h, Mathf.Lerp(0.4f, 0.1f, Mathf.Pow(1f, 0.5f)), Mathf.Lerp(0.05f, 0.7f - 0.5f * this.oracle.room.Darkness(pos), Mathf.Pow(1f, 0.45f))), new Color(0f, 0f, 0.1f), Mathf.Pow(Mathf.InverseLerp(0.45f, -0.05f, 1f), 0.9f) * 0.5f);
        }

        public override Color UbilicalCord_WireCol_1(UbilicalCord ubilicalCord)
        {
            return NSHOracleColor.Rose;
        }

        public override Color UbilicalCord_WireCol_2(UbilicalCord ubilicalCord)
        {
            return NSHOracleColor.Blue;
        }

        public override Color Gown_Color(Gown gown, float f)
        {
            if (f <= 0.1f)
                return NSHOracleColor.Violet;
            else if (f == 1f)
                return NSHOracleColor.DarkViolet;
            else
                return Color.Lerp(NSHOracleColor.Violet, NSHOracleColor.DarkViolet, f + 0.1f);
        }
        #endregion

        public class GownCover
        {
            public OracleGraphics owner;

            public int divs = 11;
            public float sleeveWidth = 0.8f;

            public Vector2[] collarPos;
            public Vector2[] leftSleevePos;
            public Vector2[] rightSleevePos;
            public Vector2[] midColPos;

            public GownCover(OracleGraphics owner)
            {
                this.owner = owner;

                collarPos = new Vector2[divs];
                leftSleevePos = new Vector2[10];
                rightSleevePos = new Vector2[10];

                midColPos = new Vector2[divs];
            }

            public Color Color(int y)
            {
                return NSHOracleColor.Pink;
            }

            public void InitiateSprites(int sprite, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                sLeaser.sprites[sprite] = TriangleMesh.MakeGridMesh(NSHOracleRegistry.isCorrupted ? "MoonCloakTex" : "Futile_White", divs - 1);

                for (int x = 0; x < divs; x++)
                {
                    for (int y = 0; y < divs; y++)
                    {
                        (sLeaser.sprites[sprite] as TriangleMesh).verticeColors[y * divs + x] = Color(y);
                    }
                }
            }

            public void Update()
            {
                for (int x = 0; x < divs; x++)
                {
                    Vector2 delta = owner.gown.clothPoints[x, 0, 0] - owner.gown.clothPoints[0, 0, 0];
                    collarPos[x] = owner.gown.clothPoints[0, 0, 0] + delta / 2.5f;
                }

                for (int y = 0; y < divs; y++)
                {
                    Vector2 delta = owner.gown.clothPoints[5, y, 0] - owner.gown.clothPoints[5, 0, 0];
                    midColPos[y] = owner.gown.clothPoints[5, 0, 0] + delta / 6f;
                }
            }

            public void DrawSprites(int i, int robeSprite, int sprite, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                Vector2 smoothedBodyPos = Vector2.Lerp(owner.owner.firstChunk.lastPos, owner.owner.firstChunk.pos, timeStacker);
                Vector2 bodyDir = Custom.DirVec(Vector2.Lerp(owner.owner.bodyChunks[1].lastPos, owner.owner.bodyChunks[1].pos, timeStacker), smoothedBodyPos);
                Vector2 perpBodyDir = Custom.PerpendicularVector(bodyDir);

                for (int k = 0; k < 2; k++)
                {
                    Vector2 smoothedHandPos = Vector2.Lerp(smoothedBodyPos, Vector2.Lerp(owner.hands[k].lastPos, owner.hands[k].pos, timeStacker), 0.8f);
                    Vector2 shoulderPos = smoothedBodyPos + perpBodyDir * 1.5f * ((k == 1) ? -1f : 1f);
                    Vector2 cB = smoothedHandPos + Custom.DirVec(smoothedHandPos, shoulderPos) * 3f + bodyDir;
                    Vector2 cA = shoulderPos + perpBodyDir * 3f * ((k == 1) ? -1f : 1f);

                    Vector2 vector14 = shoulderPos - perpBodyDir * 2f * ((k == 1) ? -1f : 1f);


                    for (int m = 0; m < 5; m++)
                    {
                        float f = (float)m / 6f;
                        Vector2 sleevePosOnBezier = Custom.Bezier(shoulderPos, cA, smoothedHandPos, cB, f);
                        Vector2 vector16 = Custom.DirVec(vector14, sleevePosOnBezier);
                        Vector2 vector17 = Custom.PerpendicularVector(vector16) * ((k == 0) ? -1f : 1f);
                        float num6 = Vector2.Distance(vector14, sleevePosOnBezier);

                        Vector2 posA = sleevePosOnBezier - vector16 * num6 * 0.3f + vector17 * sleeveWidth - camPos;
                        Vector2 posB = sleevePosOnBezier + vector17 * sleeveWidth - camPos;

                        if (k == 0)
                        {
                            leftSleevePos[m * 2] = posA;
                            leftSleevePos[m * 2 + 1] = posB;
                        }
                        else
                        {
                            rightSleevePos[m * 2] = posA;
                            rightSleevePos[m * 2 + 1] = posB;
                        }
                        vector14 = sleevePosOnBezier;
                    }
                }

                TriangleMesh gown = sLeaser.sprites[robeSprite] as TriangleMesh;
                TriangleMesh cover = sLeaser.sprites[sprite] as TriangleMesh;

                //衣领（披肩上侧）
                for (int x = 0; x < divs; x++)//draw collar
                {
                    cover.MoveVertice(0 * divs + x, collarPos[x] - camPos + bodyDir * 5f);
                }

                //披肩两侧
                int half = (int)((divs - 1) / 2);
                for (int k = 0; k < 2; k++)//玩家视角，左0右1
                {
                    Vector2 smoothedHandPos = Vector2.Lerp(owner.hands[k].lastPos, owner.hands[k].pos, timeStacker);
                    Vector2 fromBodyToHand = Custom.DirVec(smoothedBodyPos, smoothedHandPos);//手的方向
                    float angleBetweenHandAndBody = Vector2.SignedAngle(-bodyDir, fromBodyToHand);//手与身体夹角（手贴在身体两侧时为0，举在头顶为±180）
                    float angleScale = Mathf.Abs(angleBetweenHandAndBody);
                    float scale = (angleScale >= 45) ? Mathf.Lerp(1f, 0.75f, (angleScale - 45) / 100) : 1f;
                    for (int y = 1; y < divs; y++)
                    {
                        for (int x = 0; x < divs; x++)//draw collar
                        {
                            if (x >= half && k == 1)//玩家视角的右侧
                            {
                                cover.MoveVertice(y * divs + 0, leftSleevePos[y - 1] + bodyDir * 10f * scale);// + leftExpand
                                cover.MoveVertice(y * divs + divs - 1, rightSleevePos[y - 1] + bodyDir * 10f * scale);// + rightExpand
                            }
                            else if (x < half && k == 0)//玩家视角的左侧
                            {
                                cover.MoveVertice(y * divs + 0, leftSleevePos[y - 1] + bodyDir * 10f * scale);// + leftExpand
                                cover.MoveVertice(y * divs + divs - 1, rightSleevePos[y - 1] + bodyDir * 10f * scale);// + rightExpand
                            }
                        }
                    }
                }
                /*
                //披肩两侧
                for (int y = 1; y < divs; y++)//draw left and right
                {
                    cover.MoveVertice(y * divs + 0,         leftSleevePos[y - 1] + bodyDir * 10f);// + leftExpand
                    cover.MoveVertice(y * divs + divs - 1, rightSleevePos[y - 1] + bodyDir * 10f);// + rightExpand
                }*/

                //披肩中间
                for (int y = 1; y < divs; y++)//draw mid
                {
                    cover.MoveVertice(y * divs + 5, midColPos[y] - camPos);
                }

                for (int x = 1; x < 5; x++)
                {
                    for (int y = 1; y < divs; y++)
                    {
                        Vector2 left = cover.vertices[y * divs + 0];
                        Vector2 mid = cover.vertices[y * divs + 5];
                        float t = Mathf.InverseLerp(0f, 5f, x);

                        cover.MoveVertice(y * divs + x, Vector2.Lerp(left, mid, t));
                    }
                }

                for (int x = 6; x < divs - 1; x++)
                {
                    for (int y = 1; y < divs; y++)
                    {
                        Vector2 right = cover.vertices[y * divs + divs - 1];
                        Vector2 mid = cover.vertices[y * divs + 5];
                        float t = Mathf.InverseLerp(5, divs, x);

                        cover.MoveVertice(y * divs + x, Vector2.Lerp(mid, right, t));
                    }
                }
                //当手抬高时披肩高度变小
                for (int k = 0; k < 2; k++)//玩家视角，左0右1
                {
                    Vector2 smoothedHandPos = Vector2.Lerp(owner.hands[k].lastPos, owner.hands[k].pos, timeStacker);
                    Vector2 fromBodyToHand = Custom.DirVec(smoothedBodyPos, smoothedHandPos);//手的方向
                    float angleBetweenHandAndBody = Vector2.SignedAngle(-bodyDir, fromBodyToHand);//手与身体夹角（手贴在身体两侧时为0，举在头顶为±180）
                    float angleScale = Mathf.Abs(angleBetweenHandAndBody);
                    float scale = (angleScale >= 60) ? Mathf.Lerp(0f, 0.8f, (angleScale - 60) / 120) : 0f;
                    for (int y = 1; y < divs; y++)
                    {
                        for (int x = 0; x < divs; x++)//draw collar
                        {
                            if (x >= half && k == 1)//玩家视角的右侧
                                cover.MoveVertice(y * divs + x, Vector2.Lerp(cover.vertices[y * divs + x], cover.vertices[0 * divs + x], scale));
                            else if (x < half && k == 0)//玩家视角的左侧
                                cover.MoveVertice(y * divs + x, Vector2.Lerp(cover.vertices[y * divs + x], cover.vertices[0 * divs + x], scale));
                        }
                    }
                }
            }
        }

        public class GownRibbon
        {
            public OracleGraphics owner;
            public TailSegment[] ribbon;
            public static int divs = 8;
            public int timeAdd;
            public readonly float width = 0.35f;//飘带宽度
            public readonly float spacing = 6f;//飘带间距
            public readonly float maxLength = 9f;

            public GownRibbon(OracleGraphics owner)
            {
                this.owner = owner;

                ribbon = new TailSegment[divs];
                ribbon[0] = new TailSegment(owner, 5f, 4f, null, 0.85f, 1f, 3f, true);
                for (int j = 1; j < divs; j++)
                    ribbon[j] = new TailSegment(owner, 5f, 7f, ribbon[j - 1], 0.55f, 1f, 0.5f, true);
            }

            public Color Color(int y)
            {
                return UnityEngine.Color.Lerp(NSHOracleColor.Pink, NSHOracleColor.Purple, (float)y / (float)((divs - 1) * 4 + 3));
            }

            public void InitiateSprites(int sprite, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                string tex = NSHOracleRegistry.isCorrupted ? "NSHRibbonTex" : "Futile_White";
                TriangleMesh mesh = MakeLongMesh(divs, false, true, tex, true);
                sLeaser.sprites[sprite] = mesh;
                for (int j = 0; j < mesh.verticeColors.Length; j++)
                {
                    (sLeaser.sprites[sprite] as TriangleMesh).verticeColors[j] = Color(j);
                }
            }

            public void Update(int i)
            {
                timeAdd++;
                if (timeAdd > 100)
                    timeAdd = 0;

                Vector2 smoothedBodyPos = owner.owner.firstChunk.lastPos;
                Vector2 bodyDir = Custom.DirVec(owner.owner.bodyChunks[1].lastPos, smoothedBodyPos);
                Vector2 perpBodyDir = Custom.PerpendicularVector(bodyDir);

                //通过身体角度判断移动
                var moveDeg = Mathf.Clamp(Custom.AimFromOneVectorToAnother(Vector2.zero, bodyDir), -22.5f, 22.5f);
                //实际偏移
                var nowSpacing = spacing * (Mathf.Abs(moveDeg) > 15 ? 0.3f : 1f);

                var rootPos = smoothedBodyPos + (i == 0 ? -1 : 1) * perpBodyDir.normalized * nowSpacing + bodyDir * 5f;

                var num3 = 1f - Mathf.Clamp((Mathf.Abs(Mathf.Lerp(owner.owner.bodyChunks[1].vel.x, owner.owner.bodyChunks[0].vel.x, 0.35f)) - 1f) * 0.5f, 0f, 1f);

                Vector2 vector2 = rootPos;
                Vector2 pos = rootPos;
                float num9 = 28f;

                ribbon[0].connectedPoint = new Vector2?(rootPos);
                for (int k = 0; k < divs; k++)
                {
                    ribbon[k].Update();
                    ribbon[k].vel *= Mathf.Lerp(0.75f, 0.9f, num3 * (1f - owner.owner.bodyChunks[1].submersion));//水中减少速度

                    TailSegment tailSegment = ribbon[k];
                    tailSegment.vel.y = tailSegment.vel.y - Mathf.Lerp(0.1f, 0.5f, num3) * (1f - owner.owner.bodyChunks[1].submersion) * owner.owner.EffectiveRoomGravity;
                    num3 = (num3 * 10f + 1f) / 11f;

                    //超出长度限位
                    if (!Custom.DistLess(ribbon[k].pos, rootPos, maxLength * (k + 1)))
                    {
                        ribbon[k].pos = rootPos + Custom.DirVec(rootPos, ribbon[k].pos) * maxLength * (k + 1);
                    }

                    Vector2 perp = Custom.PerpendicularVector(rootPos, ribbon[k].pos);

                    if (k == 0)
                    {
                        ribbon[k].vel += (perp * (Random.value - 0.5f) / Vector2.Distance(vector2, ribbon[k].pos) * 3f *
                                         (Random.value > 0.95f || Random.value < 0.05f ? 1f : 0f)) *
                                         (1 - owner.owner.EffectiveRoomGravity);
                    }
                    if (k > 0)
                    {
                        ribbon[k].vel += (Vector2.Dot(Custom.DirVec(ribbon[k].lastPos, ribbon[k - 1].lastPos), perp.normalized) * perp.normalized *
                                         (Vector2.SignedAngle(Custom.DirVec(ribbon[k].lastPos, ribbon[k - 1].lastPos), perp.normalized) > 0 ? 1f : -1f)) *
                                         0.3f * Mathf.Pow(((float)(divs - k)) / (float)divs, 1.5f) * (1 - owner.owner.EffectiveRoomGravity);
                    }
                    num9 *= 0.25f;
                    vector2 = pos;
                    pos = ribbon[k].pos;
                }
                /*
                    if (k == 1)
                    {
                        ribbon[k].vel += (perp * (Random.value - 0.5f) / Vector2.Distance(vector2, ribbon[k].pos) * 5f *
                                         (Random.value > 0.98f || Random.value < 0.02f ? 1f : 0f)) *
                                         (1 - owner.owner.EffectiveRoomGravity);
                    }
                    if (k >= 1)
                    {
                        ribbon[k].vel += (Vector2.Dot(Custom.DirVec(ribbon[k].lastPos, ribbon[k - 1].lastPos), perp.normalized) * perp.normalized *
                                         (Vector2.SignedAngle(Custom.DirVec(ribbon[k].lastPos, ribbon[k - 1].lastPos), perp.normalized) > 0 ? 1f : -1f)) *
                                         2.5f * Mathf.Pow(((float)(divs - k)) / (float)divs, 2f) * (1 - owner.owner.EffectiveRoomGravity);//0.5f * Mathf.Pow(((float)(divs - k)) / (float)divs, 1.5f)
                        ribbon[k].vel += (Custom.DirVec(vector2, ribbon[k].pos).normalized) / Custom.Dist(vector2, ribbon[k].pos) * 
                                         1f * (1f - Mathf.Pow(((float)(divs - k)) / (float)divs, 1.2f)) * (1 - owner.owner.EffectiveRoomGravity);//0.01f * Mathf.Pow(((float)(divs - k)) / (float)divs, 1.5f)
                    }*/
            }

            public void DrawSprites(int i, int sprite, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                Vector2 smoothedBodyPos = Vector2.Lerp(owner.owner.firstChunk.lastPos, owner.owner.firstChunk.pos, timeStacker);
                Vector2 bodyDir = Custom.DirVec(Vector2.Lerp(owner.owner.bodyChunks[1].lastPos, owner.owner.bodyChunks[1].pos, timeStacker), smoothedBodyPos);
                Vector2 perpBodyDir = Custom.PerpendicularVector(bodyDir);

                //身体位置
                Vector2 drawPos1 = Vector2.Lerp(owner.owner.bodyChunks[0].lastPos, owner.owner.bodyChunks[0].pos, timeStacker);
                //臀部位置
                Vector2 drawPos2 = Vector2.Lerp(owner.owner.bodyChunks[1].lastPos, owner.owner.bodyChunks[1].pos, timeStacker);
                //身体至臀部方向的向量
                Vector2 dif = (drawPos1 - drawPos2).normalized;
                //身体旋转角度
                float bodyRotation = Mathf.Atan2(dif.x, dif.y);

                //通过身体角度判断移动
                var moveDeg = Mathf.Clamp(Custom.AimFromOneVectorToAnother(Vector2.zero, (drawPos2 - drawPos1).normalized), -22.5f, 22.5f);

                //实际偏移
                var nowSpacing = spacing * (Mathf.Abs(moveDeg) > 10 ? 0.3f : 1f);

                //实际显示
                var dir = Custom.DirVec(owner.owner.bodyChunks[0].pos, owner.owner.bodyChunks[1].pos).normalized;
                var rootPos = owner.owner.bodyChunks[0].pos + 5f * dif + (i == 0 ? -1 : 1) * Custom.PerpendicularVector(dir).normalized * nowSpacing + dir * -0.2f;

                var lastDir = Custom.DirVec(owner.owner.bodyChunks[0].lastPos, owner.owner.bodyChunks[1].lastPos).normalized;
                Vector2 vector2 = Vector2.Lerp(Vector2.Lerp(owner.owner.bodyChunks[1].lastPos, owner.owner.bodyChunks[0].lastPos, 0.35f) + (i == 0 ? -1 : 1) * Custom.PerpendicularVector(lastDir).normalized * nowSpacing + lastDir * 5f, rootPos, timeStacker);
                Vector2 vector4 = (vector2 * 3f + rootPos) / 4f;

                float d2 = 6f;

                bool OutLength = false;

                TriangleMesh ribbonMesh = sLeaser.sprites[sprite] as TriangleMesh;

                for (int j = 0; j < divs; j++)
                {
                    Vector2 vector5 = Vector2.Lerp(ribbon[j].lastPos, ribbon[j].pos, timeStacker);
                    Vector2 normalized = (vector5 - vector4).normalized;
                    Vector2 widthDir = Custom.PerpendicularVector(normalized);
                    float d3 = Vector2.Distance(vector5, vector4) / 5f;

                    if (j == 0)
                    {
                        d3 = 0f;
                    }

                    if (j != 0 && !Custom.DistLess(ribbonMesh.vertices[j * 4], ribbonMesh.vertices[j * 4 - 4], 40))
                        OutLength = true;

                    //设置坐标
                    ribbonMesh.MoveVertice(j * 4, vector4 - widthDir * d2 * width + normalized * d3 - camPos);
                    ribbonMesh.MoveVertice(j * 4 + 1, vector4 + widthDir * d2 * width + normalized * d3 - camPos);

                    if (j < divs - 1)
                    {
                        ribbonMesh.MoveVertice(j * 4 + 2, vector5 - widthDir * ribbon[j].StretchedRad * width - normalized * d3 - camPos);
                        ribbonMesh.MoveVertice(j * 4 + 3, vector5 + widthDir * ribbon[j].StretchedRad * width - normalized * d3 - camPos);
                    }
                    else
                    {
                        ribbonMesh.MoveVertice(j * 4 + 2, vector5 - camPos);
                        ribbonMesh.MoveVertice(j * 4 + 3, vector5 - camPos);
                    }
                    //d2 = ribbon[i * 4 + j].StretchedRad;
                    d2 = ribbon[j].StretchedRad;
                    vector4 = vector5;
                }

                if ((OutLength && sLeaser.sprites[sprite].isVisible))
                    sLeaser.sprites[sprite].isVisible = false;
                else if (!OutLength && !sLeaser.sprites[sprite].isVisible)
                    sLeaser.sprites[sprite].isVisible = true;
            }
        }

        public static TriangleMesh MakeLongMesh(int segments, bool pointyTip, bool customColor, string texture, bool atlasedImage)
        {
            TriangleMesh.Triangle[] array = new TriangleMesh.Triangle[(segments - 1) * 4 + (pointyTip ? 1 : 2)];
            for (int i = 0; i < segments - 1; i++)
            {
                int num = i * 4;
                for (int j = 0; j < 4; j++)
                {
                    array[num + j] = new TriangleMesh.Triangle(num + j, num + j + 1, num + j + 2);
                }
            }
            array[(segments - 1) * 4] = new TriangleMesh.Triangle((segments - 1) * 4, (segments - 1) * 4 + 1, (segments - 1) * 4 + 2);
            if (!pointyTip)
            {
                array[(segments - 1) * 4 + 1] = new TriangleMesh.Triangle((segments - 1) * 4 + 1, (segments - 1) * 4 + 2, (segments - 1) * 4 + 3);
            }
            TriangleMesh triangleMesh = new TriangleMesh(texture, array, customColor, atlasedImage);
            float num2 = 1f / (float)((segments - 1) * 2 + 1);
            for (int k = 0; k < triangleMesh.UVvertices.Length; k++)
            {
                triangleMesh.UVvertices[k].x = ((k % 2 == 0) ? 0f : 1f);
                triangleMesh.UVvertices[k].y = (float)(k / 2) * num2;
            }
            if (pointyTip)
            {
                triangleMesh.UVvertices[triangleMesh.UVvertices.Length - 1].x = 0.5f;
            }
            return triangleMesh;
        }
    }
}
