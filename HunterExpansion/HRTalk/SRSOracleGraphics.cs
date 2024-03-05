using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomOracleTx;
using UnityEngine;
using Random = UnityEngine.Random;
using RWCustom;
using static CustomOracleTx.CustomOracleBehaviour;
using CustomDreamTx;
using HunterExpansion.CustomDream;
using System.Diagnostics;

namespace HunterExpansion.CustomOracle
{
    public class SRSOracleGraphics : CustomOracleGraphic
    {
        public int signSprite;

        public SRSOracleGraphics(PhysicalObject ow) : base(ow)
        {
            callBaseApplyPalette = false;
            callBaseInitiateSprites = false;

            Random.State state = Random.state;
            Random.InitState(81);
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

            //可爱标志
            signSprite = totalSprites;
            totalSprites += 1;

            voiceFreqSamples = new float[64];
            Random.state = state;
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
            Color color = SRSOracleColor.Yellow;

            for (int j = 0; j < base.owner.bodyChunks.Length; j++)
            {
                sLeaser.sprites[firstBodyChunkSprite + j].color = color;
            }
            sLeaser.sprites[neckSprite].color = color;
            sLeaser.sprites[HeadSprite].color = color;
            sLeaser.sprites[ChinSprite].color = color;

            for (int k = 0; k < 2; k++)
            {
                sLeaser.sprites[EyeSprite(k)].color = new Color(0.02f, 0f, 0f);
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
                        (sLeaser.sprites[HandSprite(k, 1)] as TriangleMesh).verticeColors[l * 4]     = handColor;
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

            sLeaser.sprites[signSprite].color = SRSOracleColor.OrangeRed;
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
                gown.InitiateSprite(robeSprite, sLeaser, rCam);
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
                sLeaser.sprites[PhoneSprite(k, 2)].scaleY = 0.8f;//耳机长度
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
            sLeaser.sprites[fadeSprite].color = Color.Lerp(SRSOracleColor.OrangeRed, Color.black, 0.3f);
            sLeaser.sprites[fadeSprite].shader = rCam.game.rainWorld.Shaders["FlatLightBehindTerrain"];
            sLeaser.sprites[fadeSprite].alpha = 0.5f;

            sLeaser.sprites[killSprite] = new FSprite("Futile_White", true);
            sLeaser.sprites[killSprite].isVisible = false;
            sLeaser.sprites[killSprite].shader = rCam.game.rainWorld.Shaders["FlatLight"];

            sLeaser.sprites[signSprite] = new FSprite("SRSSign", true);

            base.InitiateSprites(sLeaser, rCam);

            rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[signSprite]);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

            //可爱标志
            Vector2 vector = Vector2.Lerp(base.owner.firstChunk.lastPos, base.owner.firstChunk.pos, timeStacker);
            Vector2 vector3 = Vector2.Lerp(this.head.lastPos, this.head.pos, timeStacker);
            Vector2 vector4 = Custom.DirVec(vector3, vector);
            Vector2 a3 = Custom.PerpendicularVector(vector4);
            Vector2 vector5 = this.RelativeLookDir(timeStacker);
            Vector2 vector16 = vector3 + a3 * vector5.x * 2.5f + vector4 * (-2f - vector5.y * 1.5f);
            sLeaser.sprites[signSprite].x = vector16.x - camPos.x;
            sLeaser.sprites[signSprite].y = vector16.y - camPos.y;
            sLeaser.sprites[signSprite].rotation = Custom.AimFromOneVectorToAnother(vector16, vector3 - vector4 * 10f);
            sLeaser.sprites[signSprite].scaleX = Mathf.Lerp(0.8f, 0.6f, Mathf.Abs(vector5.x));
            sLeaser.sprites[signSprite].scaleY = Custom.LerpMap(vector5.y, 0f, 1f, 0.8f, 0.2f);
        }

        public override void Update()
        {
            base.Update();
            lightsource.color = Color.Lerp(SRSOracleColor.LightYellow, Color.white, 0.3f);
            if (oracle.room.abstractRoom.name == "HR_AI")
            {
                lightsource.alpha = 0f;
            }
        }

        #region 继承的颜色
        public override Color ArmJoint_HighLightColor(ArmJointGraphics armJointGraphics, Vector2 pos)
        {
            //return SRSOracleColor.DarkGrey;
            float h = 0.75f;
            if (armJointGraphics.owner.owner.room.abstractRoom.name == "HR_AI")
                h = 0.025f;
            return Color.Lerp(Custom.HSL2RGB(h, Mathf.Lerp(0.5f, 0.1f, Mathf.Pow(1f, 0.5f)), Mathf.Lerp(0.15f, 0.85f - 0.65f * this.oracle.room.Darkness(pos), Mathf.Pow(1f, 0.45f))), new Color(0f, 0f, 0.15f), Mathf.Pow(Mathf.InverseLerp(0.45f, -0.05f, 1f), 0.9f) * 0.4f);
        }

        public override Color ArmJoint_BaseColor(ArmJointGraphics armJointGraphics, Vector2 pos)
        {
            //return SRSOracleColor.VeryDarkGrey;
            float h = 0.75f;
            if (armJointGraphics.owner.owner.room.abstractRoom.name == "HR_AI")
                h = 0.025f;
            return Color.Lerp(Custom.HSL2RGB(h, Mathf.Lerp(0.4f, 0.1f, Mathf.Pow(1f, 0.5f)), Mathf.Lerp(0.05f, 0.7f - 0.5f * this.oracle.room.Darkness(pos), Mathf.Pow(1f, 0.45f))), new Color(0f, 0f, 0.1f), Mathf.Pow(Mathf.InverseLerp(0.45f, -0.05f, 1f), 0.9f) * 0.5f);
        }

        public override Color UbilicalCord_WireCol_1(UbilicalCord ubilicalCord)
        {
            return SRSOracleColor.Rose;
        }

        public override Color UbilicalCord_WireCol_2(UbilicalCord ubilicalCord)
        {
            return SRSOracleColor.Blue;
        }

        public override Color Gown_Color(Gown gown, float f)
        {
            if (f <= 0.1f)
                return SRSOracleColor.Violet;
            else if(f == 1f)
                return SRSOracleColor.Brown;
            else
                return Color.Lerp(SRSOracleColor.Violet, SRSOracleColor.Brown, f + 0.1f);
        }
        #endregion
    }
}
