using System;
using System.Collections.Generic;
using System.Text;

namespace MinerProxy2.Miners
{
    public class ShareItem
    {
        public bool accepted;
        public DateTime shareTime;

        public ShareItem(DateTime hashrateTime, bool accepted)
        {
            this.shareTime = hashrateTime;
            this.accepted = accepted;
        }
    }
}
