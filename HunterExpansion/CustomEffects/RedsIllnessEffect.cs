using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HunterExpansion.CustomEffects
{
    public class RedsIllnessEffect : CosmeticSprite
    {
        public float TotFade(float timeStacker)
        {
            return Mathf.Lerp(this.lastFade, this.fade, timeStacker) * Mathf.Lerp(this.lastViableFade, this.viableFade, timeStacker);
        }

        public RedsIllnessEffect(Player player, Room room)
        {
            this.player = player;
            this.room = room;
            this.rotDir = ((Random.value < 0.5f) ? -1f : 1f);
        }

        public static bool CanShowPlayer(Player player)
        {
            return !player.inShortcut && player.room != null && player.room.ViewedByAnyCamera(player.firstChunk.pos, 100f);//&& !player.dead 
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (this.room == null)
            {
                return;
            }
            this.lastFade = this.fade;
            this.lastViableFade = this.viableFade;
            this.lastRot = this.rot;
            this.sin += 1f / Mathf.Lerp(120f, 30f, this.fluc3);
            this.fluc = Custom.LerpAndTick(this.fluc, this.fluc1, 0.02f, 0.016666668f);
            this.fluc1 = Custom.LerpAndTick(this.fluc1, this.fluc2, 0.02f, 0.016666668f);
            this.fluc2 = Custom.LerpAndTick(this.fluc2, this.fluc3, 0.02f, 0.016666668f);
            if (Mathf.Abs(this.fluc2 - this.fluc3) < 0.01f)
            {
                this.fluc3 = Random.value;
            }
            this.fade = Mathf.Pow(1f * (0.85f + 0.15f * Mathf.Sin(this.sin * 3.1415927f * 2f)), Mathf.Lerp(1.5f, 0.5f, this.fluc));
            this.rot += this.rotDir * this.fade * (0.5f + 0.5f * this.fluc) * 7f * (0.1f + 0.9f * Mathf.InverseLerp(1f, 4f, Vector2.Distance(this.player.firstChunk.lastLastPos, this.player.firstChunk.pos)));
            if (!RedsIllness.RedsIllnessEffect.CanShowPlayer(this.player) || this.player.room != this.room)
            {
                this.viableFade = Mathf.Max(0f, this.viableFade - 0.033333335f);
                /*
                if (this.viableFade <= 0f && this.lastViableFade <= 0f)
                {
                    this.Destroy();
                }*/
                /*
                if (CustomDreamRx.currentActivateNormalDream != null && CustomDreamRx.currentActivateNormalDream.activateDreamID == HunterExpansion.CustomDream.HunterDreamRegistry.HunterExpansion_4 && CustomDreamRx.currentActivateNormalDream.dreamFinished)
                {
                    this.Destroy();
                }*/
            }
            else
            {
                this.viableFade = Mathf.Min(1f, this.viableFade + 0.033333335f);
                this.pos = (this.room.game.Players[0].realizedCreature.firstChunk.pos * 2f + this.room.game.Players[0].realizedCreature.bodyChunks[1].pos) / 3f;
            }
            if (this.fade == 0f && this.lastFade > 0f)
            {
                this.rotDir = ((Random.value < 0.5f) ? -1f : 1f);
            }
            if (this.soundLoop == null && this.fade > 0f)
            {
                this.soundLoop = new DisembodiedDynamicSoundLoop(this);
                this.soundLoop.sound = SoundID.Reds_Illness_LOOP;
                this.soundLoop.VolumeGroup = 1;
                return;
            }
            if (this.soundLoop != null)
            {
                this.soundLoop.Update();
                this.soundLoop.Volume = Custom.LerpAndTick(this.soundLoop.Volume, Mathf.Pow((this.fade + 1f) / 2f, 0.5f), 0.06f, 0.14285715f);
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            if (this.soundLoop != null && this.soundLoop.emitter != null)
            {
                this.soundLoop.emitter.slatedForDeletetion = true;
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            base.InitiateSprites(sLeaser, rCam);
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("Futile_White", true);
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["RedsIllness"];
            this.AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("GrabShaders"));
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            float num = this.TotFade(timeStacker);
            if (num == 0f)
            {
                sLeaser.sprites[0].isVisible = false;
                return;
            }
            sLeaser.sprites[0].isVisible = true;
            sLeaser.sprites[0].x = Mathf.Clamp(Mathf.Lerp(this.lastPos.x, this.pos.x, timeStacker) - camPos.x, 0f, rCam.sSize.x);
            sLeaser.sprites[0].y = Mathf.Clamp(Mathf.Lerp(this.lastPos.y, this.pos.y, timeStacker) - camPos.y, 0f, rCam.sSize.y);
            sLeaser.sprites[0].rotation = Mathf.Lerp(this.lastRot, this.rot, timeStacker);
            sLeaser.sprites[0].scaleX = (rCam.sSize.x * (6f - 3f * num) + 2f) / 16f;
            sLeaser.sprites[0].scaleY = (rCam.sSize.x * (6f - 3f * num) + 2f) / 16f;
            sLeaser.sprites[0].color = new Color(num, num, 0f, 0f);
        }

        Player player;
        public float fade;
        public float lastFade;
        public float viableFade;
        public float lastViableFade;
        private float rot;
        private float lastRot;
        private float rotDir;
        private float sin;
        public float fluc;
        public float fluc1;
        public float fluc2;
        public float fluc3;
        public DisembodiedDynamicSoundLoop soundLoop;
    }
}
