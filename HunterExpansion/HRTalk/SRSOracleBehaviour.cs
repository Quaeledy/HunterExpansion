using CustomOracleTx;
using UnityEngine;

namespace HunterExpansion.CustomOracle
{
    public class SRSOracleBehaviour : CustomOracleBehaviour
    {
        private CustomSubBehaviour subBehavior;

        public override int NotWorkingPalette => 25;
        public override int GetWorkingPalette => 39119;//39119 //26

        public override Vector2 GetToDir => Vector2.up;

        public SRSOracleBehaviour(Oracle oracle) : base(oracle)
        {
            this.action = CustomOracleBehaviour.CustomAction.General_Idle;
            this.oracle.health = 1f;
        }

        public override void Update(bool eu)
        {
            if (player != null && player.room != null && player.room.world.region != null && player.room.world.region.name == "HR" && !(subBehavior is SRSOracleRubicon))
                NewAction(SRSOracleBehaviorAction.Rubicon_Init);

            if (this.oracle.health == 0f)
            {
                this.getToWorking = 0f;
            }
            base.Update(eu);
        }

        #region 继承方法
        public override void SeePlayer()
        {
            base.SeePlayer();
            Plugin.Log("Oracle see player");
        }

        public override void NewAction(CustomAction nextAction)
        {
            Plugin.Log(string.Concat(new string[]
            {
                "new action: ",
                nextAction.ToString(),
                " (from ",
                action.ToString(),
                ")"
            }));

            if (nextAction == action) return;

            CustomSubBehaviour.CustomSubBehaviourID customSubBehaviourID = null;

            if (SRSOracleRubicon.SubBehaviourIsMeetSaint(nextAction))
            {
                customSubBehaviourID = SRSOracleBehaviorSubBehavID.Rubicon;
            }
            else
                customSubBehaviourID = CustomSubBehaviour.CustomSubBehaviourID.General;

            currSubBehavior.NewAction(action, nextAction);

            if (customSubBehaviourID != CustomSubBehaviour.CustomSubBehaviourID.General && customSubBehaviourID != currSubBehavior.ID)
            {
                //CustomSubBehaviour subBehavior = null;
                for (int i = 0; i < allSubBehaviors.Count; i++)
                {
                    if (allSubBehaviors[i].ID == customSubBehaviourID)
                    {
                        subBehavior = allSubBehaviors[i];
                        break;
                    }
                }
                if (subBehavior == null)
                {
                    if (customSubBehaviourID == SRSOracleBehaviorSubBehavID.Rubicon)
                    {
                        subBehavior = new SRSOracleRubicon(this);
                    }
                    allSubBehaviors.Add(subBehavior);
                }
                subBehavior.Activate(action, nextAction);
                currSubBehavior.Deactivate();
                Plugin.Log("Switching subbehavior to: " + subBehavior.ID.ToString() + " from: " + this.currSubBehavior.ID.ToString());
                currSubBehavior = subBehavior;
            }
            inActionCounter = 0;
            action = nextAction;
        }
        #endregion

        public void LockShortcuts()
        {
            if (this.oracle.room.lockedShortcuts.Count == 0)
            {
                for (int i = 0; i < this.oracle.room.shortcutsIndex.Length; i++)
                {
                    this.oracle.room.lockedShortcuts.Add(this.oracle.room.shortcutsIndex[i]);
                }
            }
        }
    }
}
