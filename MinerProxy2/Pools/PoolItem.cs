namespace MinerProxy2.Pools
{
    public class PoolItem
    {
        public string poolAddress { get; set; }
        public int poolPort { get; set; }
        public int localListenPort { get; set; }
        public string coin { get; set; }
        //public int maxMiners { get; set; }
        public int donationPercent { get; set; }
        public string poolWorkerName { get; set; }
        public string poolWallet { get; set; }
        public string poolEndPoint { get { return poolAddress + ":" + poolPort; } }

        public PoolItem(string host, int port, int localListenPort, string poolWorkerName, string poolWallet, string coin, int donation)
        {
            this.poolAddress = host;
            this.poolPort = port;
            this.localListenPort = localListenPort;
            this.coin = coin;
            //this.maxMiners = maxMiners;
            this.donationPercent = donation;
            this.poolWorkerName = poolWorkerName;
            this.poolWallet = poolWallet;
        }
    }
}