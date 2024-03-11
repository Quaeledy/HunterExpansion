using System;
using UnityEngine;
using MoreSlugcats;
using Random = UnityEngine.Random;
using HunterExpansion.CustomOracle;

namespace HunterExpansion.CustomEffects
{
    public class OraclePanicDisplay : UpdatableAndDeletable
    {
        public Oracle oracle;
        public int timer;
        private int[] timings;
        public bool gravOn;
        public OracleChatLabel chatLabel;

        public OraclePanicDisplay(Oracle oracle)
        {
            this.oracle = oracle;
            this.timings = new int[]
            {
                120,
                200,
                320,
                520
            };
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            this.timer++;
            if (this.timer == 1)
            {
                this.gravOn = true;
                if (this.oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.DarkenLights) == null)
                {
                    this.oracle.room.roomSettings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.DarkenLights, 0f, false));
                }
                if (this.oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Darkness) == null)
                {
                    this.oracle.room.roomSettings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.Darkness, 0f, false));
                }
                if (this.oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Contrast) == null)
                {
                    this.oracle.room.roomSettings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.Contrast, 0f, false));
                }
                this.oracle.room.PlaySound(SoundID.Broken_Anti_Gravity_Switch_Off, 0f, 1f, 1f);
            }
            if (this.timer < this.timings[0])
            {
                float t = (float)this.timer / (float)this.timings[0];
                this.oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.DarkenLights).amount = Mathf.Lerp(0f, 1f, t);
                this.oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Darkness).amount = Mathf.Lerp(0f, 0.4f, t);
                this.oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Contrast).amount = Mathf.Lerp(0f, 0.3f, t);
            }
            if (this.timer == this.timings[0])
            {
                this.oracle.arm.isActive = false;
                this.oracle.setGravity(0.9f);
                this.oracle.stun = 9999;
                switch (Random.Range(0, 4))
                {
                    case 0:
                        oracle.room.PlaySound(NSHOracleSoundID.NSH_AI_Break_1, 0f, 0.5f, 1.25f);
                        break;
                    case 1:
                        oracle.room.PlaySound(NSHOracleSoundID.NSH_AI_Break_2, 0f, 0.5f, 1.25f);
                        break;
                    case 2:
                        oracle.room.PlaySound(NSHOracleSoundID.NSH_AI_Break_3, 0f, 0.5f, 1.25f);
                        break;
                    case 3:
                        oracle.room.PlaySound(NSHOracleSoundID.NSH_AI_Break_4, 0f, 0.5f, 1.25f);
                        break;
                }
                for (int i = 0; i < this.oracle.room.game.cameras.Length; i++)
                {
                    if (this.oracle.room.game.cameras[i].room == this.oracle.room && !this.oracle.room.game.cameras[i].AboutToSwitchRoom)
                    {
                        this.oracle.room.game.cameras[i].ScreenMovement(null, Vector2.zero, 15f);
                    }
                }
            }
            if (this.timer == (this.timings[1] + this.timings[2]) / 2)
            {
                this.oracle.arm.isActive = false;
                this.oracle.room.PlaySound((Random.value < 0.5f) ? NSHOracleSoundID.NSH_AI_Attack_1 : NSHOracleSoundID.NSH_AI_Attack_2, 0f, 0.5f, 1f);
                this.chatLabel = new OracleChatLabel(this.oracle.oracleBehavior);
                this.chatLabel.pos = new Vector2(485f, 360f);
                this.chatLabel.NewPhrase(99);
                this.oracle.setGravity(0.9f);
                this.oracle.stun = 9999;
                this.oracle.room.AddObject(this.chatLabel);
            }
            /*
            if (this.timer > (this.timings[1] + this.timings[2]) / 2 && this.timer < this.timings[2])
            {
                //加个倒地
                (this.oracle.oracleBehavior as NSHOracleBehaviour).getToWorking = 0f;
                this.oracle.firstChunk.vel += 1.5f * Vector2.down;
            }*/
            if (this.timer > this.timings[1] && this.timer < this.timings[2] && this.timer % 16 == 0)
            {
                this.oracle.room.ScreenMovement(null, new Vector2(0f, 0f), 2.5f);
                for (int j = 0; j < 6; j++)
                {
                    if (Random.value < 0.5f)
                    {
                        this.oracle.room.AddObject(new OraclePanicDisplay.PanicIcon(new Vector2((float)Random.Range(230, 740), (float)Random.Range(100, 620))));
                    }
                }
            }
            if (this.timer >= this.timings[2] && this.timer <= this.timings[3])
            {
                this.oracle.room.ScreenMovement(null, new Vector2(0f, 0f), 1f);
            }
            if (this.timer == this.timings[3])
            {
                this.chatLabel.Destroy();
                this.oracle.room.PlaySound(SoundID.Broken_Anti_Gravity_Switch_On, 0f, 1f, 1f);
                this.gravOn = false;
            }
            if (this.timer > this.timings[3])
            {
                float t2 = (float)(this.timer - this.timings[3]) / (float)this.timings[0];
                this.oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.DarkenLights).amount = Mathf.Lerp(1f, 0f, t2);
                this.oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Darkness).amount = Mathf.Lerp(0.4f, 0f, t2);
                this.oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Contrast).amount = Mathf.Lerp(0.3f, 0f, t2);
            }
            if (this.timer == this.timings[3] + this.timings[0])
            {
                this.oracle.setGravity(0f);
                this.oracle.arm.isActive = true;
                this.oracle.stun = 0;
                this.Destroy();
            }
        }

        public class PanicIcon : CosmeticSprite
        {
            public PanicIcon(Vector2 position)
            {
                this.pos = position;
            }

            public override void Update(bool eu)
            {
                this.circleScale = Mathf.Lerp(this.circleScale, 1f, 0.1f);
                if (this.circleScale > 0.98f)
                {
                    this.timer++;
                }
                if (this.timer == 160)
                {
                    this.Destroy();
                }
                base.Update(eu);
            }

            public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                sLeaser.sprites = new FSprite[3];
                for (int i = 0; i < 2; i++)
                {
                    sLeaser.sprites[i] = new FSprite("Futile_White", true);
                    sLeaser.sprites[i].shader = rCam.room.game.rainWorld.Shaders["VectorCircle"];
                    sLeaser.sprites[i].scale = 0f;
                }
                sLeaser.sprites[1].color = new Color(0.003921569f, 0f, 0f);
                sLeaser.sprites[0].color = new Color(0f, 0f, 0f);
                sLeaser.sprites[2] = new FSprite("miscDangerSymbol", true);
                sLeaser.sprites[2].isVisible = false;
                sLeaser.sprites[2].color = new Color(0f, 0f, 0f);
                this.AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("BackgroundShortcuts"));
            }

            public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                bool isVisible = true;
                if (this.timer > 130 && this.timer % 8 < 4)
                {
                    isVisible = false;
                }
                for (int i = 0; i < 2; i++)
                {
                    sLeaser.sprites[i].x = this.pos.x - camPos.x;
                    sLeaser.sprites[i].y = this.pos.y - camPos.y;
                    sLeaser.sprites[i].scale = this.circleScale * 4f * ((i == 0) ? 1f : 0.9f);
                    sLeaser.sprites[i].isVisible = isVisible;
                }
                sLeaser.sprites[2].x = this.pos.x - camPos.x;
                sLeaser.sprites[2].y = this.pos.y - camPos.y;
                if (this.circleScale > 0.98f)
                {
                    sLeaser.sprites[2].isVisible = isVisible;
                }
                base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            }

            public int timer;
            public float circleScale;
        }
    }
}
