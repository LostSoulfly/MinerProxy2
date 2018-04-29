using System;
using System.Collections.Generic;
using System.Text;

namespace MinerProxy2.Miners
{
    public class HashrateItem
    {
        public long hashrate;
        public DateTime hashrateTime;

        public HashrateItem(DateTime hashrateTime, long hashrate)
        {
            this.hashrateTime = hashrateTime;
            this.hashrate = hashrate;
        }
    }
}
