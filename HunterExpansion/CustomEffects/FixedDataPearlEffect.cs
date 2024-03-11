using HunterExpansion.CustomSave;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HunterExpansion.CustomEffects
{
    internal class FixedDataPearlEffect : CosmeticSprite
    {
        public static void HooksOn()
        {
            On.DataPearl.InitiateSprites += DataPearl_InitiateSprites;
        }

        private static void DataPearl_InitiateSprites(On.DataPearl.orig_InitiateSprites orig, DataPearl self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig.Invoke(self, sLeaser, rCam);
            if(self.AbstractPearl.dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.Red_stomach && PearlFixedSave.pearlFixed)
            {
                self.room.AddObject(new FixedDataPearlEffect(self, self.room));
            }
        }

        static float dotMatrixGridWidth = 9f;

        static Color FrontColor = Color.green;
        static Color BackColor = Color.green * 0.7f;

        static float MovementUnstableRecoverVel = 0.015f;
        static float MovementToUnstableFactor = 0.03f;
        static float MaxUnstableBlinkProbability = 1f;

        static int SmallLozengeCount = 6;


        DataPearl bindPearl;
        FLabel testLabel;

        Mesh3D insideLozengeMesh;
        Mesh3DRenderer insideLozengeMeshRenderer;

        Mesh3D dotMatrixMesh;
        Mesh3DRenderer dotMatrixMeshRenderer;

        List<Mesh3D> smallLozengeMeshes = new List<Mesh3D>();
        List<Mesh3DRenderer> smallLozengeMeshRenderers = new List<Mesh3DRenderer>();

        AnimationTimeLine timeLine;

        int totalSprites = 0;

        float movementUnstableFactor;

        bool blink;
        bool lastBlink;
        int noBlinkCounter;

        bool NeedDeletion => bindPearl.slatedForDeletetion || room != bindPearl.room || !PearlFixedSave.pearlFixed;

        public FixedDataPearlEffect(DataPearl bindPearl, Room room)
        {
            this.bindPearl = bindPearl;
            pos = bindPearl.firstChunk.pos;
            lastPos = pos;

            CreateInsideLozengeMesh();
            CreateSmallLozengeMeshes();
            timeLine = new AnimationTimeLine(this);
        }

        void CreateInsideLozengeMesh()
        {
            Mesh3D.TriangleFacet[] facets = new Mesh3D.TriangleFacet[]
            {
                new Mesh3D.TriangleFacet(0,1,2),
                new Mesh3D.TriangleFacet(0,2,3),
                new Mesh3D.TriangleFacet(0,3,4),
                new Mesh3D.TriangleFacet(0,4,1),


                new Mesh3D.TriangleFacet(5,1,2),
                new Mesh3D.TriangleFacet(5,2,6),

                new Mesh3D.TriangleFacet(6,2,3),
                new Mesh3D.TriangleFacet(6,3,7),

                new Mesh3D.TriangleFacet(7,3,4),
                new Mesh3D.TriangleFacet(7,4,8),

                new Mesh3D.TriangleFacet(8,4,1),
                new Mesh3D.TriangleFacet(8,1,5),


                new Mesh3D.TriangleFacet(9,5,6),
                new Mesh3D.TriangleFacet(9,6,7),
                new Mesh3D.TriangleFacet(9,7,8),
                new Mesh3D.TriangleFacet(9,8,5),
            };

            insideLozengeMesh = new Mesh3D();
            insideLozengeMesh.SetFacet(facets);

            insideLozengeMesh.SetVertice(0, Vector3.up * 16f);
            insideLozengeMesh.SetVertice(9, Vector3.down * 16f);

            insideLozengeMesh.SetVertice(1, Vector3.left * 6f);
            insideLozengeMesh.SetVertice(2, Vector3.forward * 6f);
            insideLozengeMesh.SetVertice(3, Vector3.right * 6f);
            insideLozengeMesh.SetVertice(4, Vector3.back * 6f);

            for(int i = 5;i <= 8; i++)
            {
                insideLozengeMesh.SetVertice(i, insideLozengeMesh.GetOrigVertice(i - 4));
            }

            insideLozengeMeshRenderer = new Mesh3DFrameRenderer(insideLozengeMesh, totalSprites, 1f);
            totalSprites += insideLozengeMeshRenderer.totalSprites;
            insideLozengeMeshRenderer.shader = "Hologram";

            insideLozengeMeshRenderer.SetVerticeColor(FrontColor, true);
            insideLozengeMeshRenderer.SetVerticeColor(BackColor, false);
        }

        void CreateSmallLozengeMeshes()
        {
            for(int i = 0;i < SmallLozengeCount; i++)
            {
                Mesh3D.TriangleFacet[] facets = new Mesh3D.TriangleFacet[]
                {
                    new Mesh3D.TriangleFacet(0,1,2),
                    new Mesh3D.TriangleFacet(3,1,2),
                };
                var smallLozengeMesh = new Mesh3D();
                smallLozengeMesh.SetFacet(facets);

                smallLozengeMesh.SetVertice(0, Vector3.up * 6f);
                smallLozengeMesh.SetVertice(1, Vector3.left * 3f);
                smallLozengeMesh.SetVertice(2, Vector3.right * 3f);
                smallLozengeMesh.SetVertice(3, Vector3.down * 6f);

                var smallLozengeMeshRenderer = new Mesh3DFrameRenderer(smallLozengeMesh, totalSprites, 1f) { autoCaculateZ = false };
                totalSprites += smallLozengeMeshRenderer.totalSprites;
                smallLozengeMeshRenderer.shader = "Hologram";

                smallLozengeMeshRenderer.SetVerticeColor(FrontColor, true);
                smallLozengeMeshRenderer.SetVerticeColor(BackColor, false);

                smallLozengeMesh.scale = 0f;
                smallLozengeMesh.globalRotation = new Vector3(0f, Random.value * 120f, Random.value * 120f - 60f);
                smallLozengeMeshRenderer.maxZ = 40f;
                smallLozengeMeshRenderer.minZ = -40f;

                smallLozengeMeshes.Add(smallLozengeMesh);
                smallLozengeMeshRenderers.Add(smallLozengeMeshRenderer);
            }
        }

        void CreateDotMatrixMesh()
        {
            List<Mesh3D.TriangleFacet> facets = new List<Mesh3D.TriangleFacet>();//6x6x6
            for(int i = 0;i < 216;i += 3)
            {
                facets.Add(new Mesh3D.TriangleFacet(i, i + 1, i + 2));
            }
            dotMatrixMesh = new Mesh3D();
            dotMatrixMesh.SetFacet(facets.ToArray());
            
            for(int z = 0;z < 6; z++)
            {
                for(int y = 0;y < 6;y++)
                {
                    for(int x = 0;x < 6; x++)
                    {
                        dotMatrixMesh.SetVertice(z + y * 6 + x * 36, (new Vector3(3 - x, 3 - y, 3 - z) - Vector3.one * 0.5f) * dotMatrixGridWidth);
                    }
                }
            }
            dotMatrixMeshRenderer = new Mesh3DDotMatrixRenderer(dotMatrixMesh, totalSprites, 1f);
            totalSprites += dotMatrixMeshRenderer.totalSprites;
            dotMatrixMeshRenderer.shader = "Hologram";

            dotMatrixMeshRenderer.SetVerticeColor(FrontColor, true);
            dotMatrixMeshRenderer.SetVerticeColor(BackColor, false);

            dotMatrixMeshRenderer.rotateCenter = dotMatrixMesh.origVertices.Last();
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            base.InitiateSprites(sLeaser, rCam);

            //testLabel = new FLabel(Custom.GetFont(), "");
            sLeaser.sprites = new FSprite[totalSprites];
            insideLozengeMeshRenderer.InitSprites(sLeaser, rCam);
            for(int i = 0;i < smallLozengeMeshRenderers.Count;i++)
            {
                smallLozengeMeshRenderers[i].InitSprites(sLeaser, rCam);
            }
            //dotMatrixMeshRenderer.InitSprites(sLeaser, rCam);
            AddToContainer(sLeaser, rCam, null);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (newContatiner == null)
                newContatiner = rCam.ReturnFContainer("Foreground");

            if(testLabel != null)
            {
                newContatiner.RemoveChild(testLabel);
                newContatiner.AddChild(testLabel);
            }

            foreach (var fSprite in sLeaser.sprites)
            {
                fSprite.RemoveFromContainer();
                newContatiner.AddChild(fSprite);
            }

            insideLozengeMeshRenderer.AddToContainer(sLeaser, newContatiner);
            foreach(var renderer in smallLozengeMeshRenderers)
                renderer.AddToContainer(sLeaser, newContatiner);
        }

        public override void Update(bool eu)
        {
            if (slatedForDeletetion)
                return;

            if (NeedDeletion)
                Destroy();

            base.Update(eu);
            pos = bindPearl.firstChunk.pos;

            if (movementUnstableFactor > 0f) 
                movementUnstableFactor -= MovementUnstableRecoverVel;
            movementUnstableFactor += (pos - lastPos).magnitude * MovementToUnstableFactor;
            if (movementUnstableFactor > 1f)
                movementUnstableFactor = 1f;

            lastBlink = blink;
            if (Random.value < MaxUnstableBlinkProbability * movementUnstableFactor)
                blink = true;
            else
                blink = false;

            if (movementUnstableFactor <= 0f && noBlinkCounter < 40)
                noBlinkCounter++;
            else if (movementUnstableFactor > 0f)
                noBlinkCounter = 0;

            if (blink != lastBlink)
            {
                if (blink)
                {
                    insideLozengeMeshRenderer.SetVerticeColor(FrontColor * 0.1f, true);
                    insideLozengeMeshRenderer.SetVerticeColor(BackColor * 0.1f, false);
                    for(int i = 0; i < smallLozengeMeshRenderers.Count; i++)
                    {
                        smallLozengeMeshRenderers[i].SetVerticeColor(FrontColor * 0.1f, true);
                        smallLozengeMeshRenderers[i].SetVerticeColor(BackColor * 0.1f, false);
                    }
                }
                else
                {
                    insideLozengeMeshRenderer.SetVerticeColor(FrontColor, true);
                    insideLozengeMeshRenderer.SetVerticeColor(BackColor, false);
                    for (int i = 0; i < smallLozengeMeshRenderers.Count; i++)
                    {
                        smallLozengeMeshRenderers[i].SetVerticeColor(FrontColor, true);
                        smallLozengeMeshRenderers[i].SetVerticeColor(BackColor, false);
                    }
                }
            }

            if (noBlinkCounter == 40)
                timeLine.enable = true;

            if (movementUnstableFactor > 0.9f)
            {
                timeLine.enable = false;
            }
            else
            {
                insideLozengeMesh.localRotation = new Vector3(0f, (insideLozengeMesh.localRotation.y + 2f) % 360f, 0f);
                insideLozengeMesh.Update();
                insideLozengeMeshRenderer.Update();

                for(int i = 0;i < smallLozengeMeshes.Count;i++)
                {
                    smallLozengeMeshes[i].localRotation = new Vector3(0f, (smallLozengeMeshes[i].localRotation.y + i + 1f) % 360f, 0f);
                    smallLozengeMeshes[i].globalRotation = new Vector3(smallLozengeMeshes[i].globalRotation.x, (smallLozengeMeshes[i].globalRotation.y - 2f - i) % 360f, Mathf.Sin(Time.time * 0.2f * (i/3f + 1f)) * 30f);
                    smallLozengeMeshes[i].Update();
                }
            }

            timeLine.Update();
            if (testLabel != null)
            {
                testLabel.text = $"BlinkProb : {MaxUnstableBlinkProbability * movementUnstableFactor}";
            }
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            if (slatedForDeletetion)
                return;

            Vector2 smoothPos = Vector2.Lerp(lastPos, pos, timeStacker);
            insideLozengeMeshRenderer.DrawSprites(sLeaser, rCam, timeStacker, camPos, smoothPos);

            for(int i = 0;i < smallLozengeMeshRenderers.Count;i++)
            {
                smallLozengeMeshRenderers[i].DrawSprites(sLeaser, rCam, timeStacker, camPos, smoothPos);
            }

            if(testLabel != null)
            {
                testLabel.SetPosition(pos - camPos + new Vector2(50f, -50f));
            }
        }

        class AnimationTimeLine
        {
            FixedDataPearlEffect effect;
            bool _enable;
            public bool enable
            {
                get => _enable;
                set
                {
                    _enable = value;
                    if (!value)
                    {
                        counter = 0;
                        foreach(var track in tracks)
                            track.Update(counter);
                    }
                }
            }

            int counter;
            int maxCounter;

            public List<ITrack> tracks = new List<ITrack>();

            public AnimationTimeLine(FixedDataPearlEffect effect, int maxCounter = 820)
            {
                this.effect = effect;
                this.maxCounter = maxCounter;

                InitInsideLozengeAnimation();
                InitSmallLozengesAnimation();
            }

            void InitInsideLozengeAnimation()
            {
                var insideLozengeHeightTrack = new Track<float>
                (
                    this,
                    (t) =>
                    {
                        for (int i = 0; i <= 4; i++)
                        {
                            this.effect.insideLozengeMesh.SetAnimatedVertice(i, this.effect.insideLozengeMesh.GetOrigVertice(i) + Vector3.up * t);
                        }
                        for (int i = 5; i <= 9; i++)
                        {
                            this.effect.insideLozengeMesh.SetAnimatedVertice(i, this.effect.insideLozengeMesh.GetOrigVertice(i) + Vector3.down * t);
                        }
                    },
                    (current, next, t) =>
                    {

                        return Mathf.Lerp(current, next, t);
                    },
                    0f
                )
                {
                    useEaseFunction = true
                };
                insideLozengeHeightTrack.AddKeyFrame(60, 15f);
                insideLozengeHeightTrack.AddKeyFrame(580, 15f);
                insideLozengeHeightTrack.AddKeyFrame(620, 0f);
                insideLozengeHeightTrack.AddKeyFrame(820, 0f);
                tracks.Add(insideLozengeHeightTrack);
            }

            void InitSmallLozengesAnimation()
            {
                for(int i = 0;i < effect.smallLozengeMeshes.Count;i++)
                {
                    int localIndex = i;
                    var scaleTrack = new Track<float>(
                        this,
                        (t) => effect.smallLozengeMeshes[localIndex].scale = t,
                        (current, next, t) => Mathf.Lerp(current, next, t),
                        0f
                    )
                    {
                        useEaseFunction = true
                    };

                    scaleTrack.AddKeyFrame(60 + i * 10, 0f);
                    scaleTrack.AddKeyFrame(60 + i * 10 + 40, 1f);
                    scaleTrack.AddKeyFrame(580 - i * 5, 1f);
                    scaleTrack.AddKeyFrame(580 - i * 5 + 30, 0f);
                    scaleTrack.AddKeyFrame(820, 0f);
                    tracks.Add(scaleTrack);

                    var positionTrack = new Track<float>(
                        this,
                        (t) => effect.smallLozengeMeshes[localIndex].position = new Vector3(t, 0f, 0f),
                        (current, next, t) => Mathf.Lerp(current, next, t),
                        0f
                    )
                    {
                        useEaseFunction = true
                    };

                    positionTrack.AddKeyFrame(60 + i * 10, 0f);
                    positionTrack.AddKeyFrame(60 + i * 10 + 40, 25f + i * 2);
                    positionTrack.AddKeyFrame(580 - i * 5, 25f + i * 2);
                    positionTrack.AddKeyFrame(580 - i * 5 + 30, 0f);
                    scaleTrack.AddKeyFrame(820, 0f);
                    tracks.Add(positionTrack);
                }
            }


            public void Update()
            {
                if (!enable)
                    return;
                foreach(var track in tracks)
                {
                    track.Update(counter);
                }
                counter++;
                if (counter >= maxCounter)
                    counter -= maxCounter;
            }


            public class Track<T> : ITrack
            {
                public AnimationTimeLine timeLine;

                public Action<T> SetValue;
                public Func<T,T,float,T> LerpValue;

                public T defaultValue;

                public List<KeyValuePair<int, T>> keyFrames = new List<KeyValuePair<int, T>>();
                public bool useEaseFunction;

                public Track(AnimationTimeLine timeLine, Action<T> setValue, Func<T, T, float, T> lerpValue, T defaultValue)
                {
                    this.timeLine = timeLine;

                    SetValue = setValue;
                    LerpValue = lerpValue;
                    keyFrames.Add(new KeyValuePair<int, T>(0, defaultValue));
                }

                public void AddKeyFrame(int counter, T value)
                {
                    keyFrames.Add(new KeyValuePair<int, T>(counter, value));
                }

                public void Update(int counter)
                {
                    KeyValuePair<int, T> currentFrame = keyFrames[0];
                    KeyValuePair<int, T> nextFrame = keyFrames[0];

                    for(int i = 0;i < keyFrames.Count - 1; i++)
                    {
                        if (keyFrames[i].Key <= counter && keyFrames[i + 1].Key >= counter)
                        {
                            currentFrame = keyFrames[i];
                            nextFrame = keyFrames[i + 1];
                        }
                    }

                    if(counter >= keyFrames.Last().Key)
                    {
                        currentFrame = nextFrame = keyFrames.Last();
                    }

                    if(currentFrame.Key == nextFrame.Key)
                    {
                        SetValue.Invoke(nextFrame.Value);
                    }
                    else
                    {
                        float tInStage = (counter - currentFrame.Key) / (float)(nextFrame.Key - currentFrame.Key);
                        if (useEaseFunction) tInStage = EaseInOutCubic(tInStage);

                        SetValue.Invoke(LerpValue(currentFrame.Value, nextFrame.Value, tInStage));
                    }
                }
            }

            public interface ITrack
            {
                void Update(int counter);
            }

            static float EaseInOutCubic(float x) 
            {
                return x< 0.5 ? 4 * x* x* x : 1 - Mathf.Pow(-2 * x + 2, 3) / 2;
            }
        }
    }
}
