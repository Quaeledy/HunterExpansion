using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MoreSlugcats;
using static CustomOracleTx.CustomOracleBehaviour;
using Random = UnityEngine.Random;
using HunterExpansion.CustomSave;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using RWCustom;

namespace HunterExpansion.CustomOracle
{
    public class SRSOracleRubicon : CustomConversationBehaviour
    {
        public bool noticedPlayer;
        public bool startedConversation;
        public int dissappearedTimer;
        public float ghostOut;
        public HRKarmaShrine shrineControl;
        public int finalGhostFade;

        public SRSOracleRubicon(SRSOracleBehaviour owner) : base(owner, SRSOracleBehaviorSubBehavID.Rubicon, Conversation.ID.None)
        {
            Plugin.Log("SRS Oracle load Rubicon Behaviour!");
            noticedPlayer = false;
            startedConversation = false;
            dissappearedTimer = 0;
            ghostOut = 0;
            finalGhostFade = 0;
        }

        public static bool SubBehaviourIsMeetSaint(CustomAction nextAction)
        {
            return nextAction == SRSOracleBehaviorAction.Rubicon_Init;
        }

        public override void Update()
        {
            base.Update();
            if (this.owner.oracle == null || this.owner.oracle.room == null)
            {
                return;
            }
            if (!this.noticedPlayer)
            {
                this.owner.inActionCounter = 0;
                if (base.player != null && this.owner.oracle.room.GetTilePosition(base.player.mainBodyChunk.pos).y < 35)
                {
                    this.owner.getToWorking = 0f;
                    this.noticedPlayer = true;
                }
            }
            DeathPersistentSaveData deathPersistentSaveData = null;
            if (this.owner.oracle.room.game.IsStorySession)
            {
                deathPersistentSaveData = (this.owner.oracle.room.game.session as StoryGameSession).saveState.deathPersistentSaveData;
            }
            if (deathPersistentSaveData == null)
            {
                return;
            }
            if (this.owner.conversation != null)
            {
                if (base.player != null && base.player.room == this.owner.oracle.room)
                {
                    base.movementBehavior = CustomMovementBehavior.Talk;
                }
                else
                {
                    base.movementBehavior = CustomMovementBehavior.Idle;
                }
            }
            if (base.inActionCounter > 15 && !this.startedConversation && this.owner.conversation == null)
            {
                if (Plugin.ripSRS)
                {
                    this.startedConversation = true;
                    return;
                }
                if (ModManager.MSC && this.oracle.room.world.name == "HR")
                {
                    this.owner.conversation.colorMode = true;
                }
            }
            if (this.owner.conversation != null && !this.owner.conversation.paused && base.player != null && base.player.room != this.owner.oracle.room)
            {
                this.owner.conversation.paused = true;
                this.owner.restartConversationAfterCurrentDialoge = true;
                this.owner.dialogBox.Interrupt(base.Translate("..."), 40);
                this.owner.dialogBox.currentColor = Color.white;
            }
            if ((this.owner.conversation == null || this.owner.conversation.slatedForDeletion) && this.startedConversation)
            {
                this.owner.getToWorking = 1f;
                if (this.dissappearedTimer % 400 == 0)
                {
                    float value = Random.value;
                    if ((double)value < 0.3)
                    {
                        base.movementBehavior = CustomMovementBehavior.Idle;
                    }
                    else if ((double)value > 0.7)
                    {
                        base.movementBehavior = CustomMovementBehavior.KeepDistance;
                    }
                    else
                    {
                        base.movementBehavior = CustomMovementBehavior.Investigate;
                    }
                }
                this.dissappearedTimer++;
            }
            //这是
            if (Plugin.ripSRS && (deathPersistentSaveData.ripMoon || deathPersistentSaveData.ripPebbles || RipNSHSave.ripNSH) && this.owner.oracle.arm != null)
            {
                float x = this.owner.oracle.arm.cornerPositions[0].x;
                float num = this.owner.oracle.arm.cornerPositions[1].x - x;
                float y = this.owner.oracle.arm.cornerPositions[2].y;
                float num2 = this.owner.oracle.arm.cornerPositions[0].y - y;
                float num3 = (this.owner.nextPos.x - x) / num;
                float num4 = (this.owner.baseIdeal.x - x) / num;
                this.owner.nextPos.y = Mathf.Max(this.owner.nextPos.y, y + num2 * num3 - 75f);
                this.owner.baseIdeal.y = Mathf.Max(this.owner.nextPos.y, y + num2 * num4 - 75f);
            }
        }

        public void Interrupt(string text, int delay)
        {
            if (this.owner.conversation != null)
            {
                this.owner.conversation.paused = true;
                this.owner.restartConversationAfterCurrentDialoge = true;
            }
            this.owner.dialogBox.Interrupt(text, delay);
        }
    }
}
