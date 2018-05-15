using System;
using System.Collections.Generic;
using System.Text;
using MinerProxy2.Helpers;

namespace MinerProxy2.Miners
{
    public class MinerStatsItem
    {
        //hashrate
        public FixedSizedQueue<HashrateItem> hashrateHistory = new FixedSizedQueue<HashrateItem>(1000);

        public DateTime lastHashrateAcceptedTime;

        public FixedSizedQueue<ShareItem> shareHistory = new FixedSizedQueue<ShareItem>(1000);

        public TimeSpan totalConnectedTime;

        public DateTime firstConnectTime;

        public DateTime lastConnectTime;

        public DateTime lastDisconnectTime;

        public long numberOfConnects;

        public long totalAcceptedShares;

        public long totalRejectedShares;

        public long totalSubmittedShares;

        public string workerName;

        public MinerStatsItem(string workerName)
        {
            this.workerName = workerName;
        }

        public void AddHashrate(long hashrate)
        {
            // We only want to store a new hashrate every ~10 seconds
            TimeSpan nextHashrate = TimeSpan.FromSeconds(10);

            if (lastHashrateAcceptedTime + nextHashrate >= DateTime.Now || lastHashrateAcceptedTime == DateTime.MinValue)
            {
                lastHashrateAcceptedTime = DateTime.Now;
                hashrateHistory.Enqueue(new HashrateItem(DateTime.Now, hashrate));
            }

        }

        public void AddShare(bool accepted)
        {
            shareHistory.Enqueue(new ShareItem(DateTime.Now, accepted));
        }
      
    }
}
