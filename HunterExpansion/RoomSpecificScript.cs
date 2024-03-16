using CustomSaveTx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HunterExpansion
{
    public static class RoomSpecificScript
    {
        public static void Init()
        {
            On.RoomSpecificScript.AddRoomSpecificScript += RoomSpecificScript_AddRoomSpecificScript;
            //On.Room.Loaded += Room_Loaded;
        }
        /*
        private static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
        {
            orig.Invoke(self);
            if (self.abstractRoom.name == "SI_A07")
            {
                RoomSpecificScript.AddRoomSpecificScript(self);
            }
        }
        */
        private static void RoomSpecificScript_AddRoomSpecificScript(On.RoomSpecificScript.orig_AddRoomSpecificScript orig, Room room)
        {
            orig.Invoke(room);
            AddRoomSpecificScript(room);
        }

        public static void AddRoomSpecificScript(Room room)
        {
            Plugin.Log("IsStorySession: {0}, saveStateNumber: {1}, room: {2}", new object[]
            {
                room.game.IsStorySession,
                room.game.GetStorySession.saveState.saveStateNumber,
                room.abstractRoom.name
            });
            string name = room.abstractRoom.name;
            if (name == "NSH_ROOF03")
            {
                room.AddObject(new NSH_ROOF03GradientGravity(room));
            }
            if (name == "NSH_E01")
            {
                room.AddObject(new NSH_E01GradientGravity(room));
            }
            /*
            if (name == "GATE_SB_OE" && room.game.IsStorySession && room.game.GetStorySession.saveState.saveStateNumber == Plugin.SlugName)
            {
                Plugin.Log("Try Play Endding");
                room.AddObject(new NSH_HunterEnding(room));
            }*/
        }

        public class NSH_ROOF03GradientGravity : UpdatableAndDeletable
        {
            public NSH_ROOF03GradientGravity(Room room)
            {
                this.room = room;
            }

            public override void Update(bool eu)
            {
                base.Update(eu);
                AbstractCreature firstAlivePlayer = this.room.game.FirstAlivePlayer;
                if (this.room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null && firstAlivePlayer.realizedCreature.room == this.room)
                {
                    float value = Vector2.Distance((firstAlivePlayer.realizedCreature as Player).firstChunk.pos, new Vector2(4160f, 200f));
                    this.room.gravity = Mathf.InverseLerp(0f, 3000f, value);
                }
            }
        }

        public class NSH_E01GradientGravity : UpdatableAndDeletable
        {
            public NSH_E01GradientGravity(Room room)
            {
                this.room = room;
            }

            public override void Update(bool eu)
            {
                base.Update(eu);
                AbstractCreature firstAlivePlayer = this.room.game.FirstAlivePlayer;
                if (this.room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null && firstAlivePlayer.realizedCreature.room == this.room)
                {
                    //房间中轴：x = 2529
                    if ((firstAlivePlayer.realizedCreature as Player).firstChunk.pos.x > 2529)
                    {
                        this.room.gravity = 0f;
                    }
                    else
                    {

                        float value = Vector2.Distance((firstAlivePlayer.realizedCreature as Player).firstChunk.pos, new Vector2(2529f, 329f));
                        this.room.gravity = 0.65f * Mathf.InverseLerp(0f, 2000f, value);
                    }
                }
            }
        }
    }
}
