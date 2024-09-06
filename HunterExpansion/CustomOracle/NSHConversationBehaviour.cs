using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CustomOracleTx.CustomOracleBehaviour;

namespace HunterExpansion.CustomOracle
{
    public class NSHConversationBehaviour : CustomConversationBehaviour
    {
        public new NSHOracleBehaviour owner;

        public NSHConversationBehaviour(NSHOracleBehaviour owner) : base(owner, NSHOracleBehaviorSubBehavID.MeetHunter, NSHConversationID.Hunter_Talk0)
        {
            this.owner = owner;
        }

        //用于计算时间状态
        public virtual int GetPlayerEncountersState()
        {
            return 0;
        }
    }
}
