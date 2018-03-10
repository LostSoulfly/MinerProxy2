using System;

namespace MinerProxy2.Miners
{
    public class Miner
    {
        public long submittedShares { get; set; }
        public long acceptedShares { get; set; }
        public long rejectedShares { get; set; }
        public long hashrate { get; set; }
        public string endPoint { get; set; }
        public string connectionName { get; set; }
        public string workerName { get; set; }
        public bool connectionAlive { get; set; }
        public bool noRigName { get; set; }
        public DateTime connectionStartTime;
        public DateTime lastCalculatedTime;
    }
}