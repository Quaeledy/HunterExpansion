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
using static UnityEngine.RectTransform;

namespace HunterExpansion.CustomOracle
{
    public class NSHPearlRegistry : CustomOraclePearlTx
    {
        public static AbstractPhysicalObject.AbstractObjectType NSHPearl = new AbstractPhysicalObject.AbstractObjectType("NSHPearl", true);
        public static DataPearl.AbstractDataPearl.DataPearlType DataPearl_NSH = new DataPearl.AbstractDataPearl.DataPearlType("NSHPearl", true);
        public static Conversation.ID pearlConvID_NSH = new Conversation.ID("NSHPearl", true);
        public NSHPearlRegistry() : base(NSHPearl, DataPearl_NSH, pearlConvID_NSH)
        {
        }

        public override CustomOrbitableOraclePearl RealizeDataPearl(AbstractPhysicalObject abstractPhysicalObject, World world)
        {
            return new NSHPearl(abstractPhysicalObject, world);
        }

        public override void LoadSLPearlConversation(SLOracleBehaviorHasMark.MoonConversation self, SlugcatStats.Name saveFile, bool oneRandomLine, int randomSeed)
        {
            int extralingerfactor = self.interfaceOwner.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.Chinese ? 1 : 0;
            //开场白
            int i = Random.Range(0, 100000);
            NSHConversation.LoadEventsFromFile(self, 201, null, true, i);
            /*
            switch (Random.Range(0, 2))
            {
                case 0:
                    self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("You would like me to read this?"), 40 * extralingerfactor));
                    break;
                case 1:
                    self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Would you like me to read this pearl?"), 45 * extralingerfactor));
                    break;
                default:
                    self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Let's see... A pearl..."), 20 * extralingerfactor));
                    break;
            }*/
            //正式内容
            int j = (self.myBehavior is SLOracleBehaviorHasMark && (self.myBehavior as SLOracleBehaviorHasMark).holdingObject != null) ? (self.myBehavior as SLOracleBehaviorHasMark).holdingObject.abstractPhysicalObject.ID.RandomSeed : Random.Range(0, 100000);
            if (ModManager.MSC && self.myBehavior.oracle.room.game.IsMoonHeartActive())
            {
                NSHConversation.LoadEventsFromFile(self, 202, "MoonHeartActive", true, j);
            }
            else
            {
                NSHConversation.LoadEventsFromFile(self, 202, null, true, j);//注意，矛大师线应该去改DM迭代器的对话，而不是SL的
            }/*
            switch (Random.Range(0, 5))
            {
                case 0:
                    if (ModManager.MSC && self.myBehavior.oracle.room.game.IsMoonHeartActive())
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Its data compiling style is very familiar... I think it comes from No Significant Harassement."), 100 * extralingerfactor));
                    else
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Its data compiling style is very familiar... I think it comes from a member of the local group."), 100 * extralingerfactor));
                    self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("There isn't much left inside. I think this is a part of the unstructured grid of a physical field, but I lack information to further interpret it."), 140 * extralingerfactor));
                    break;

                case 1:
                    self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("This is a calculation process used to carefully encrypt and store special data, possibly some kind of key."), 110 * extralingerfactor));
                    self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Without context, I don't know its purpose."), 50 * extralingerfactor));
                    break;

                case 2:
                    self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Strange... This data clearly comes from an iterator. But there are several small errors inside, even though they have been carefully corrected, traces can still be seen."), 180 * extralingerfactor));
                    self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("We rarely make mistakes in calculations, so its creator was certainly not paying attention."), 90 * extralingerfactor));
                    break;

                case 3:
                    self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("This is a log of a working memory, but I can only interpret a few words."), 60 * extralingerfactor));
                    self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("It is obvious that the compiler used some method to compress his information, in order to store it in a short period of time and perform more operations."), 150 * extralingerfactor));
                    self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("This iterator used to handle an urgent matter, which overwhelmed them, but their calculations were still well-organized."), 120 * extralingerfactor));
                    if (ModManager.MSC && self.myBehavior.oracle.room.game.IsMoonHeartActive())
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("A faintly familiar feeling. If I were to speculate on my own, little creature, I would guess that it comes from the region of No Significant Harassment."), 160 * extralingerfactor));
                    else
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Ah, this makes me feel familiar..."), 160 * extralingerfactor));
                    break;

                case 4:
                    self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("This is a biome log, and its owner has observed and recorded the evolution of nearly a hundred species of organisms within their region."), 150 * extralingerfactor));
                    self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("..."), 20 * extralingerfactor));
                    if (ModManager.MSC && self.myBehavior.oracle.room.game.IsMoonHeartActive())
                    {
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("I saw a species unique to No Significant Harassment's region."), 80 * extralingerfactor));
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("I hope you didn't steal it, little creature. The time span of the edits is dramatic, and he persisted in updating its data until a long time ago."), 150 * extralingerfactor));
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("But for a long time recently, he seems to have forgotten it."), 60 * extralingerfactor));
                    }
                    else
                    {
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("I hope you didn't steal it, little creature. The time span of the edits is dramatic, and its owner persisted in updating its data until a long time ago."), 150 * extralingerfactor));
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("But for a long time recently, the writer seems to have forgotten it."), 60 * extralingerfactor));
                    }
                    break;

                default:
                    self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("This is a special qualia... it's a dying experience."), 60 * extralingerfactor));
                    self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("We all know that the near death experience of the ancients was not accompanied by fear, so obviously, this data is extracted from a simple creature."), 140 * extralingerfactor));
                    self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("I can distinguish the primordial desire for survival - perhaps it's from the brain of a squidcada."), 100 * extralingerfactor));
                    self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("I find it difficult to imagine whether it will have any practical use."), 60 * extralingerfactor));
                    break;
            }*/
        }
    }

    public class NSHPearl : CustomOrbitableOraclePearl
    {
        public bool swarmerMarblesSet = false;
        public PhysicalObject orbitObj_origin;
        public Vector2 ps_origin;
        public int circle_origin;
        public float dist_origin;
        public int color_origin;

        public NSHPearl(AbstractPhysicalObject abstractPhysicalObject, World world) : base(abstractPhysicalObject, world)
        {
        }

        public override void Update(bool eu)
        {
            /*
            //在魔方节点直接销毁
            if (this.oracle.room.abstractRoom.name == "HR_AI")
            {
                for (int k = oracle.pearlCounter; k >= 0; k--)
                {
                    if(this.marbleIndex == k)
                    {
                        this.Destroy();
                        return;
                    }
                }
            }*/
            //有神经元时改变轨迹
            NSHSwarmer swarmer = NSHOracleMeetHunter.nshSwarmer;
            if (swarmer != null && swarmer.room != null && this.NotCarried && !swarmerMarblesSet)
            {
                //记录一下原始数据
                orbitObj_origin = orbitObj;
                ps_origin = firstChunk.pos;
                circle_origin = orbitCircle;
                dist_origin = orbitDistance;
                color_origin = marbleColor;

                //改变数据
                int newCycle;
                float newDist;
                if (color == NSHOracleColor.Purple)
                {
                    newCycle = 0;
                    newDist = 60f;
                }
                else if (color == NSHOracleColor.Green)
                {
                    newCycle = 1;
                    newDist = 230f;
                }
                else
                {
                    newCycle = 2;
                    newDist = 275f;

                }
                if(this.oracle == null)
                {
                    if (this != null && this.room.abstractRoom.name == "NSH_AI")
                    {
                        List<PhysicalObject>[] physicalObjects = this.room.physicalObjects;
                        for (int i = 0; i < physicalObjects.Length; i++)
                        {
                            for (int j = 0; j < physicalObjects[i].Count; j++)
                            {
                                PhysicalObject physicalObject = physicalObjects[i][j];
                                if (physicalObject is Oracle)
                                {
                                    if ((physicalObject as Oracle).ID == NSHOracleRegistry.NSHOracle)
                                    {
                                        this.oracle = physicalObject as Oracle;
                                    }
                                }
                            }
                        }
                    }
                }
                NSHOracleRegistry.ChangeMarble(this, oracle, swarmer, ps_origin, newCycle, newDist, color_origin);
                this.oracle.marbleOrbiting = true;
                swarmerMarblesSet = true;
            }
            else if ((swarmer == null || swarmer.room == null) && swarmerMarblesSet)
            {
                //恢复原始数据（注意位置不能设成原始位置）
                NSHOracleRegistry.ChangeMarble(this, oracle, orbitObj_origin, firstChunk.pos, circle_origin, dist_origin, color_origin);
                this.oracle.marbleOrbiting = false;
                swarmerMarblesSet = false;
            }
            float angle = this.orbitAngle;
            float dist = this.orbitDistance;
            Vector2 pos = base.firstChunk.pos;
            float orbitFlattenAngle = this.orbitFlattenAngle;
            float orbitFlattenFac = this.orbitFlattenFac;

            //this.orbitFlattenAngle = 0.9f;
            //this.orbitFlattenFac = 0.99f;

            base.Update(eu);
            
            if (swarmer != null && swarmer.room != null && this.oracle != null)
            {
                //如果重力偏低，且没被抓住，且迭代器存在
                if (this.room.gravity < 1f && this.NotCarried && this.oracle != null)
                {
                    //尽可能抵消原方法的计算
                    int listCount = 1;
                    CustomOracleTx.CustomOracleTx.CustomOralceEX customOralceEX;
                    if (CustomOracleTx.CustomOracleTx.oracleEx.TryGetValue(this.oracle, out customOralceEX))
                    {
                        listCount = customOralceEX.customMarbles.Count;
                    }
                    float num = angle;
                    float num5 = (float)this.marbleIndex / (float)listCount;
                    num = 360f * num5 + (float)this.oracle.behaviorTime * 0.1f;
                    Vector2 vector = new Vector2(this.oracle.room.PixelWidth / 2f, this.oracle.room.PixelHeight / 2f) + Custom.DegToVec(num) * 275f;

                    base.firstChunk.vel /= Custom.LerpMap(base.firstChunk.vel.magnitude, 1f, 6f, 0.999f, 0.9f);
                    base.firstChunk.vel -= Vector2.ClampMagnitude(vector - base.firstChunk.pos, 100f) / 100f * 0.4f * (1f - this.room.gravity);

                    this.orbitFlattenAngle = orbitFlattenAngle;
                    this.orbitFlattenFac = orbitFlattenFac;

                    //以下是新方法
                    int seekNum = 0;
                    int findNum = 0;
                    //遍历物品找到珍珠（43颗）
                    for (int i = 0; i < oracle.room.physicalObjects.Length; i++)
                    {
                        for (int j = 0; j < oracle.room.physicalObjects[i].Count; j++)
                        {
                            if (oracle.room.physicalObjects[i][j] is NSHPearl)
                            {
                                NSHPearl pearl = oracle.room.physicalObjects[i][j] as NSHPearl;
                                if (this == pearl)
                                {
                                    findNum = seekNum;
                                }
                                if (this.orbitCircle == pearl.orbitCircle)
                                {
                                    seekNum++;
                                }
                            }
                        }
                    }
                    angle = (float)findNum * (360f / (float)seekNum) + (float)this.oracle.behaviorTime * 0.5f * ((this.orbitCircle % 2 == 0) ? 1f : -1f);
                    //期望位置
                    pos = this.orbitObj.firstChunk.pos + Custom.DegToVec(angle) * dist;
                    base.firstChunk.vel *= Custom.LerpMap(base.firstChunk.vel.magnitude, 1f, 6f, 0.999f, 0.9f);
                    base.firstChunk.vel += Vector2.ClampMagnitude(pos - base.firstChunk.pos, 100f) / 100f * 0.4f * (1f - this.room.gravity);
                }
            }
        }

        public override void DataPearlApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            int num = Random.Range(0, 3);
            if (rCam.room.world.game.IsStorySession)
            {
                num = (abstractPhysicalObject as CustomOrbitableOraclePearl.AbstractCustomOraclePearl).color;
            }

            Color color = NSHOracleColor.Green;
            switch (num)
            {
                default:
                case 0:
                    color = NSHOracleColor.Purple;
                    break;
                case 1:
                    color = NSHOracleColor.LightGrey;
                    break;
                case 2:
                    color = NSHOracleColor.Green;
                    break;
                case 3:
                    color = NSHOracleColor.Orange;
                    break;
            }
            darkness = 0f;
            this.color = color;
        }
    }
}
