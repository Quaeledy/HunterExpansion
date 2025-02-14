using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HunterExpansion.CustomEffects
{
    public class OracleSwarmerResync : CosmeticSprite
    {
        public OracleSwarmerResync(Oracle oracle, PhysicalObject obj, bool isContinuous, int interval, int lifeTime)
        {
            this.oracle = oracle;
            this.obj = obj;
            timer = 0;
            timings = new int[]
            {
                5,
                lifeTime
            };
            this.isContinuous = isContinuous;
            this.interval = interval;
            this.lifeTime = lifeTime;
            connections = new Connection[20];
            for (int j = 0; j < connections.Length; j++)
            {
                connections[j] = new Connection(this, new Vector2(oracle.room.PixelWidth / 2f, oracle.room.PixelHeight / 2f) + Custom.RNV() * Mathf.Lerp(300f, 500f, Random.value));
            }
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            for (int i = 0; i < connections.Length; i++)
            {
                connections[i].lastLightUp = connections[i].lightUp;
                connections[i].lightUp *= 0.9f;
                if (Vector2.Distance(connections[i].stuckAt, connections[i].handle) > 100f)
                {
                    connections[i].handle = new Vector2((connections[i].stuckAt.x + connections[i].handle.x) / 2f, (connections[i].stuckAt.y + connections[i].handle.y) / 2f);
                }
            }
            timer++;

            //瞬间闪电
            if (!isContinuous)
            {
                if (timer == 1)
                {
                    oracle.suppressConnectionFires = true;
                    connections[lightUpIndex].lightUp = 1f;
                    if (obj != null)
                    {
                        connections[lightUpIndex].stuckAt = obj.firstChunk.pos;
                    }
                    oracle.room.PlaySound(SoundID.SS_AI_Halo_Connection_Light_Up, 0f, 1f * (1f - oracle.noiseSuppress), 1f);
                    lightUpIndex++;
                    if (lightUpIndex >= connections.Length)
                    {
                        lightUpIndex = 0;
                    }
                }
                //只电一下就销毁
                if (timer > 20)
                {
                    Destroy();
                }
            }
            //连续间隔闪电，但没测试过
            else
            {
                if (timer == 1)
                {
                    if (oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.DarkenLights) == null)
                    {
                        oracle.room.roomSettings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.DarkenLights, 0f, false));
                    }
                    if (oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Darkness) == null)
                    {
                        oracle.room.roomSettings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.Darkness, 0f, false));
                    }
                    if (oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Contrast) == null)
                    {
                        oracle.room.roomSettings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.Contrast, 0f, false));
                    }
                    oracle.suppressConnectionFires = true;
                }
                if (timer < timings[0])
                {
                    float t = timer / (float)timings[0];
                    oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.DarkenLights).amount = Mathf.Lerp(0f, 0.35f, t);
                    oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Darkness).amount = Mathf.Lerp(0f, 0.2f, t);
                    oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Contrast).amount = Mathf.Lerp(0f, 0.05f, t);
                }
                if (timer % interval == 0)
                {
                    connections[lightUpIndex].lightUp = 1f;
                    if (obj != null)
                    {
                        connections[lightUpIndex].stuckAt = obj.firstChunk.pos;
                    }
                    oracle.room.PlaySound(SoundID.SS_AI_Halo_Connection_Light_Up, 0f, 1f * (1f - oracle.noiseSuppress), 1f);
                    lightUpIndex++;
                    if (lightUpIndex >= connections.Length)
                    {
                        lightUpIndex = 0;
                    }
                }
                if (timer > timings[timings.Length - 1])
                {
                    float t2 = (timer - timings[timings.Length - 1]) / (float)timings[0];
                    oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.DarkenLights).amount = Mathf.Lerp(0.35f, 0f, t2);
                    oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Darkness).amount = Mathf.Lerp(0.2f, 0f, t2);
                    oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Contrast).amount = Mathf.Lerp(0.05f, 0f, t2);
                }
                if (lifeTime != 0 && timer > lifeTime)
                {
                    Destroy();
                }
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[connections.Length];
            for (int j = 0; j < connections.Length; j++)
            {
                sLeaser.sprites[j] = TriangleMesh.MakeLongMesh(10, false, false);
                sLeaser.sprites[j].color = new Color(0f, 0f, 0f); // NSHSwarmerHooks.NSHSwarmer_Color(obj);
            }
            AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("BackgroundShortcuts"));
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            //int num = 9;
            for (int j = 0; j < connections.Length; j++)
            {
                if (connections[j].lastLightUp > 0.05f || connections[j].lightUp > 0.05f)
                {
                    Vector2 vector2 = connections[j].stuckAt;
                    float d = 2f * Mathf.Lerp(connections[j].lastLightUp, connections[j].lightUp, timeStacker);
                    for (int k = 0; k < 10; k++)
                    {
                        float f = k / 9f;
                        Vector2 pos = oracle.firstChunk.pos;
                        Vector2 a = Custom.DirVec(pos, connections[j].stuckAt);
                        Vector2 vector3 = Custom.Bezier(connections[j].stuckAt, connections[j].handle, pos, pos + a * 20f, f);
                        Vector2 vector4 = Custom.DirVec(vector2, vector3);
                        Vector2 a2 = Custom.PerpendicularVector(vector4);
                        float d2 = Vector2.Distance(vector2, vector3);

                        (sLeaser.sprites[j] as TriangleMesh).MoveVertice(k * 4, vector3 - vector4 * d2 * 0.3f - a2 * d - camPos);
                        (sLeaser.sprites[j] as TriangleMesh).MoveVertice(k * 4 + 1, vector3 - vector4 * d2 * 0.3f + a2 * d - camPos);
                        (sLeaser.sprites[j] as TriangleMesh).MoveVertice(k * 4 + 2, vector3 - a2 * d - camPos);
                        (sLeaser.sprites[j] as TriangleMesh).MoveVertice(k * 4 + 3, vector3 + a2 * d - camPos);
                        vector2 = vector3;
                    }
                }
            }
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        public Oracle oracle;
        public PhysicalObject obj;
        public int timer;
        private int[] timings;
        public float objSlider;
        public Connection[] connections;
        public int lightUpIndex;
        public bool isContinuous;
        public int interval;
        public int lifeTime;

        public class Connection
        {
            public Connection(OracleSwarmerResync parent, Vector2 stuckAt)
            {
                this.parent = parent;
                Vector2 vector = stuckAt;
                vector.x = Mathf.Clamp(vector.x, parent.oracle.arm.cornerPositions[0].x, parent.oracle.arm.cornerPositions[1].x);
                vector.y = Mathf.Clamp(vector.y, parent.oracle.arm.cornerPositions[2].y, parent.oracle.arm.cornerPositions[1].y);
                this.stuckAt = Vector2.Lerp(stuckAt, vector, 0.5f);
                handle = stuckAt + Custom.RNV() * Mathf.Lerp(400f, 700f, Random.value);
            }

            public OracleSwarmerResync parent;
            public Vector2 stuckAt;
            public Vector2 handle;
            public float lightUp;
            public float lastLightUp;
        }
    }
}
