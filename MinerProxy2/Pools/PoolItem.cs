namespace MinerProxy2.Pools
{
    public class PoolItem
    {
        public string poolAddress { get; set; }
        public int poolPort { get; set; }
        public string coin { get; set; }
        public int maxMiners { get; set; }
        public int donationPercent { get; set; }
        public string poolWorkerName { get; set; }

        public PoolItem(string host, int port, string poolWorkerName, string coin, int maxMiners, int donation)
        {
            this.poolAddress = host;
            this.poolPort = port;
            this.coin = coin;
            this.maxMiners = maxMiners;
            this.donationPercent = donation;
            this.poolWorkerName = poolWorkerName;
        }
    }
}