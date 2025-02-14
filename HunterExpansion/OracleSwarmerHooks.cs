using HunterExpansion.CustomOracle;
using HunterExpansion.CustomSave;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HunterExpansion
{
    public class OracleSwarmerHooks
    {
        public static ConditionalWeakTable<OracleSwarmer, NSHOracleSwarmer> OracleSwarmerData = new ConditionalWeakTable<OracleSwarmer, NSHOracleSwarmer>();

        public static void Init()
        {
            On.OracleSwarmer.ctor += OracleSwarmer_ctor;
            On.OracleSwarmer.Update += OracleSwarmer_Update;
            On.OracleSwarmer.BitByPlayer += OracleSwarmer_BitByPlayer;
            On.SLOracleSwarmer.BitByPlayer += SLOracleSwarmer_BitByPlayer;
        }

        public static void OracleSwarmer_ctor(On.OracleSwarmer.orig_ctor orig, OracleSwarmer self, AbstractPhysicalObject abstractPhysicalObject, World world)
        {
            orig(self, abstractPhysicalObject, world);

            OracleSwarmerData.Add(self, new NSHOracleSwarmer(self));

            if (OracleSwarmerData.TryGetValue(self, out var oracleSwarmer))
            {
                if (OracleSwarmerRegionSave.oracleSwarmerRegion.ContainsKey(self.abstractPhysicalObject.ID))
                {
                    oracleSwarmer.spawnRegion = OracleSwarmerRegionSave.oracleSwarmerRegion[self.abstractPhysicalObject.ID];
                }

                if (oracleSwarmer.spawnRegion == null)
                {
                    oracleSwarmer.spawnRegion = world.region.name;
                    oracleSwarmer.id = self.abstractPhysicalObject.ID;
                }
            }
        }

        public static void OracleSwarmer_Update(On.OracleSwarmer.orig_Update orig, OracleSwarmer self, bool eu)
        {
            orig(self, eu);
            if (OracleSwarmerData.TryGetValue(self, out var oracleSwarmer))
            {
                if (self.room.game.world != null &&
                    self.room.game.world.region.name != oracleSwarmer.spawnRegion &&
                    !OracleSwarmerRegionSave.oracleSwarmerRegion.ContainsKey(self.abstractPhysicalObject.ID))
                {
                    OracleSwarmerRegionSave.oracleSwarmerRegion.Add(oracleSwarmer.id, oracleSwarmer.spawnRegion);
                }
                if (self.slatedForDeletetion &&
                    OracleSwarmerRegionSave.oracleSwarmerRegion.ContainsKey(self.abstractPhysicalObject.ID))
                {
                    OracleSwarmerRegionSave.oracleSwarmerRegion.Remove(oracleSwarmer.id);
                }
            }
        }

        public static void OracleSwarmer_BitByPlayer(On.OracleSwarmer.orig_BitByPlayer orig, OracleSwarmer self, Creature.Grasp grasp, bool eu)
        {
            if (self.bites <= 2)
            {
                List<PhysicalObject>[] physicalObjects = self.room.physicalObjects;
                for (int i = 0; i < physicalObjects.Length; i++)
                {
                    for (int j = 0; j < physicalObjects[i].Count; j++)
                    {
                        PhysicalObject physicalObject = physicalObjects[i][j];
                        if (physicalObject is Oracle && (physicalObject as Oracle).ID == NSHOracleRegistry.NSHOracle)
                        {
                            if (OracleSwarmerHooks.OracleSwarmerData.TryGetValue(self, out var oracleSwarmer) &&
                                !oracleSwarmer.nshHasAlreadySpokenToSlugcat)
                            {
                                oracleSwarmer.nshHasAlreadySpokenToSlugcat = true;
                                NSHOracleBehaviour oracleBehavior = (physicalObject as Oracle).oracleBehavior as NSHOracleBehaviour;
                                if (!oracleBehavior.State.alreadyTalkedAboutItems.Contains(self.abstractPhysicalObject.ID) ||//如果NSH没有读过神经元，就默认是自己的神经元
                                    (self is SSOracleSwarmer && //注意，NSH的神经元需要比FP的神经元先判断
                                     oracleSwarmer.spawnRegion == "NSH"))
                                {
                                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Creature, are you intentionally provoking me?"), 20);
                                    oracleBehavior.State.InfluenceLike(-0.4f);
                                }
                                else if (self is SSOracleSwarmer &&
                                     oracleSwarmer.spawnRegion == "DM")
                                {
                                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("..."), 20);
                                    //oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Goodbye, you cruel creature."), 20);
                                    oracleBehavior.State.likesPlayer = -1;
                                    oracleBehavior.generateKillingIntent = true;
                                }
                                else if (self is SSOracleSwarmer ||
                                    (self is SSOracleSwarmer &&
                                     (oracleSwarmer.spawnRegion == "SS" || oracleSwarmer.spawnRegion == "RM" || oracleSwarmer.spawnRegion == "CL")))
                                {
                                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Okay, if you brought it to me just for this moment, feel free."), 20);
                                }
                            }
                            break;
                        }
                    }
                }
            }
            orig(self, grasp, eu);
        }

        public static void SLOracleSwarmer_BitByPlayer(On.SLOracleSwarmer.orig_BitByPlayer orig, SLOracleSwarmer self, Creature.Grasp grasp, bool eu)
        {
            if (self.bites <= 2)
            {
                List<PhysicalObject>[] physicalObjects = self.room.physicalObjects;
                for (int i = 0; i < physicalObjects.Length; i++)
                {
                    for (int j = 0; j < physicalObjects[i].Count; j++)
                    {
                        PhysicalObject physicalObject = physicalObjects[i][j];
                        if (physicalObject is Oracle && (physicalObject as Oracle).ID == NSHOracleRegistry.NSHOracle)
                        {
                            if (OracleSwarmerHooks.OracleSwarmerData.TryGetValue(self, out var oracleSwarmer) &&
                                !oracleSwarmer.nshHasAlreadySpokenToSlugcat)
                            {
                                oracleSwarmer.nshHasAlreadySpokenToSlugcat = true;
                                NSHOracleBehaviour oracleBehavior = (physicalObject as Oracle).oracleBehavior as NSHOracleBehaviour;
                                if (!oracleBehavior.State.alreadyTalkedAboutItems.Contains(self.abstractPhysicalObject.ID))//如果NSH没有读过神经元，就默认是自己的神经元
                                {
                                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Beast, are you intentionally provoking me?"), 20);
                                    oracleBehavior.State.InfluenceLike(-0.4f);
                                }
                                else if (self is SLOracleSwarmer)
                                {
                                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("..."), 20);
                                    oracleBehavior.dialogBox.NewMessage(oracleBehavior.Translate("Goodbye, you cruel creature."), 20);
                                    oracleBehavior.State.likesPlayer = -1;
                                    oracleBehavior.generateKillingIntent = true;
                                }
                            }

                            //让NSH在蛞蝓猫当面吃Moon神经元时不会晕倒
                            if (self.oracle != null && self.oracle.ID == NSHOracleRegistry.NSHOracle)
                            {
                                self.oracle = null;
                            }
                            break;
                        }
                    }
                }
            }
            orig(self, grasp, eu);
        }
    }

    public class NSHOracleSwarmer
    {
        public static WeakReference<OracleSwarmer> oracleSwarmerRef;
        public string spawnRegion;
        public EntityID id;
        public bool nshHasAlreadySpokenToSlugcat;

        public NSHOracleSwarmer(OracleSwarmer oracleSwarmer)
        {
            nshHasAlreadySpokenToSlugcat = false;
            oracleSwarmerRef = new WeakReference<OracleSwarmer>(oracleSwarmer);
        }
    }
}