using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using System;
using UnityEngine;

namespace HunterExpansion
{
    public class RegionGateHooks
    {
        public static void InitIL()
        {
            //IL.RegionGateGraphics.Update += RegionGateGraphics_UpdateIL;
        }

        public static void Init()
        {
            On.RegionGate.ChangeDoorStatus += RegionGate_ChangeDoorStatus;
            On.RegionGate.DetectZone += RegionGate_DetectZone;
            On.RegionGate.AllPlayersThroughToOtherSide += RegionGate_AllPlayersThroughToOtherSide;
            On.RegionGate.customKarmaGateRequirements += RegionGate_customKarmaGateRequirements;
            On.RegionGateGraphics.DoorGraphic.ctor += DoorGraphic_ctor;
            On.GateKarmaGlyph.ctor += GateKarmaGlyph_ctor;
        }

        private static void RegionGateGraphics_UpdateIL(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);
                //移动图像位置
                if (c.TryGotoNext(MoveType.After,
                    (i) => i.Match(OpCodes.Ldarg_0),
                    (i) => i.MatchLdfld<RegionGateGraphics>("smoke"),
                    (i) => i.MatchLdloc(9)))//这里已经找到了pos2
                {
                    Plugin.Log("RegionGateGraphics_UpdateIL MatchFind!");
                    c.Emit(OpCodes.Ldarg_1);//找到RegionGate gate
                    c.EmitDelegate<Func<Vector2, RegionGate, Vector2>>((pos2, gate) =>
                    {
                        if (gate.room.abstractRoom.name == "GATE_SB_NSH")
                        {
                            Vector2 newPos = new Vector2(14f + (gate.letThroughDir ? 19f : 28f) * 20f, 47f) + //10+4, 30+17
                                             new Vector2(Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 15f, Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 10f);
                            return newPos;
                        }
                        else
                        {
                            return pos2;
                        }
                    });
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static void RegionGate_ChangeDoorStatus(On.RegionGate.orig_ChangeDoorStatus orig, RegionGate self, int door, bool open)
        {
            if (self.room.abstractRoom.name == "GATE_SB_NSH")
            {
                int num = 18 + door * 9;//14+4
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 25; j <= 33; j++)//8+17，16+17
                    {
                        self.room.GetTile(num + i, j).Terrain = (open ? Room.Tile.TerrainType.Air : Room.Tile.TerrainType.Solid);
                    }
                }
                return;
            }
            orig(self, door, open);
        }

        private static int RegionGate_DetectZone(On.RegionGate.orig_DetectZone orig, RegionGate self, AbstractCreature crit)
        {
            int result = orig(self, crit);
            if (self.room.abstractRoom.name == "GATE_SB_NSH")
            {
                int middleX = 28;
                int middleY = 28;
                bool flag = crit.pos.y > middleY - 8 && crit.pos.y < middleY + 8;
                if (crit.pos.room != self.room.abstractRoom.index)
                {
                    result = -1;
                }
                else if (crit.pos.x < middleX - 8 && flag)
                {
                    result = 0;
                }
                else if (crit.pos.x < middleX && flag)
                {
                    result = 1;
                }
                else if (crit.pos.x < middleX + 8 && flag)
                {
                    result = 2;
                }
                else
                {
                    result = 3;
                }
            }
            return result;
        }

        private static bool RegionGate_AllPlayersThroughToOtherSide(On.RegionGate.orig_AllPlayersThroughToOtherSide orig, RegionGate self)
        {
            bool result = orig(self);
            if (self.room.abstractRoom.name == "GATE_SB_NSH")
            {
                int middle = 28;
                for (int i = 0; i < self.room.game.Players.Count; i++)
                {
                    if (self.room.game.Players[i].pos.room == self.room.abstractRoom.index &&
                        (!self.letThroughDir || self.room.game.Players[i].pos.x < middle + 3) &&
                        (self.letThroughDir || self.room.game.Players[i].pos.x > middle - 4))
                    {
                        return false;
                    }
                }
            }
            return result;
        }

        public static void DoorGraphic_ctor(On.RegionGateGraphics.DoorGraphic.orig_ctor orig, RegionGateGraphics.DoorGraphic self, RegionGateGraphics rgGraphics, RegionGate.Door door)
        {
            orig(self, rgGraphics, door);
            if (door.gate.room.abstractRoom.name == "GATE_SB_NSH")
            {
                self.posZ = new Vector2((19f + 9f * (float)door.number) * 20f, 680f);//x： 15 + 4， y：340 + 340
                self.rustleLoop = new StaticSoundLoop(SoundID.Gate_Clamps_Moving_LOOP, self.posZ, rgGraphics.gate.room, 0f, 1f);
                self.screwTurnLoop = new StaticSoundLoop((rgGraphics.gate is WaterGate) ? SoundID.Gate_Water_Screw_Turning_LOOP : SoundID.Gate_Electric_Screw_Turning_LOOP, self.posZ, rgGraphics.gate.room, 0f, 1f);
                self.Reset();
            }
        }

        public static void GateKarmaGlyph_ctor(On.GateKarmaGlyph.orig_ctor orig, GateKarmaGlyph self, bool side, RegionGate gate, RegionGate.GateRequirement requirement)
        {
            orig(self, side, gate, requirement);
            if (gate.room.abstractRoom.name == "GATE_SB_NSH")
            {
                self.pos = gate.room.MiddleOfTile(side ? 32 : 23, 31);//y：14+17
                self.lastPos = self.pos;
            }
        }

        public static void RegionGate_customKarmaGateRequirements(On.RegionGate.orig_customKarmaGateRequirements orig, RegionGate self)
        {
            orig(self);
            if (ModManager.MSC && self.room.abstractRoom.name == "GATE_NSH_DGL")
            {
                if (!HunterExpansionEnums.GateRequirement.customNSHGateRequirements(self))
                {
                    self.karmaRequirements[0] = HunterExpansionEnums.GateRequirement.NSHLock;
                    self.karmaRequirements[1] = HunterExpansionEnums.GateRequirement.NSHLock;
                }
                if (ModManager.MMF && MMF.cfgDisableGateKarma.Value)
                {
                    int num;
                    if (int.TryParse(self.karmaRequirements[0].value, out num))
                    {
                        self.karmaRequirements[0] = RegionGate.GateRequirement.OneKarma;
                    }
                    int num2;
                    if (int.TryParse(self.karmaRequirements[1].value, out num2))
                    {
                        self.karmaRequirements[1] = RegionGate.GateRequirement.OneKarma;
                    }
                }
            }
        }
    }
}
