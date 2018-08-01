using System;
using System.Collections.Generic;
using System.Text;
using MinerProxy2.Helpers;
using Serilog;

namespace MinerProxy2.Miners
{
    public class MinerStatsItem
    {
        //hashrate
        public FixedSizedQueue<HashrateItem> hashrateHistory = new FixedSizedQueue<HashrateItem>(1000);

        public DateTime? lastHashrateAcceptedTime;

        public FixedSizedQueue<ShareItem> shareHistory = new FixedSizedQueue<ShareItem>(1000);

        public TimeSpan? totalConnectedTime;

        public DateTime? firstConnectTime;

        public DateTime? lastConnectTime;

        public DateTime? lastDisconnectTime;

        public long numberOfConnects = 0;

        public long totalAcceptedShares = 0;

        public long totalRejectedShares = 0;

        public long totalSubmittedShares = 0;

        public string workerName = string.Empty;

        public MinerStatsItem(string workerName)
        {
            this.workerName = workerName;
        }

        public void AddHashrate(long hashrate)
        {
            // We only want to store a new hashrate every ~10 seconds
            TimeSpan nextHashrate = TimeSpan.FromSeconds(10);
            
            if (lastHashrateAcceptedTime == null || lastHashrateAcceptedTime + nextHashrate >= DateTime.Now)
            {
                lastHashrateAcceptedTime = DateTime.Now;
                hashrateHistory.Enqueue(new HashrateItem(DateTime.Now, hashrate));
            }

        }

        public void MinerConnected()
        {
            this.lastConnectTime = DateTime.Now;

            if (this.firstConnectTime == null)
                this.firstConnectTime = DateTime.Now;
            numberOfConnects++;
        }

        public void MinerDisconnected()
        {
            this.lastDisconnectTime = DateTime.Now;
        }

        public void AddShare(bool accepted)
        {
            Log.Verbose(string.Format("AddShare: {0} {1}", this.workerName, accepted));

            shareHistory.Enqueue(new ShareItem(DateTime.Now, accepted));

            if (accepted)
                this.totalAcceptedShares++;
            else
                this.totalRejectedShares++;
        }
      
        public void SubmittedShare()
        {
            this.totalSubmittedShares++;
        }

    }
}
