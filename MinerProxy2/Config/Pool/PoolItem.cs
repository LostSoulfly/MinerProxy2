namespace MinerProxy2.Config.Pool
{
    public class PoolItem
    {
        public string poolAddress { get; set; }
        public int poolPort { get; set; }
        public string coin { get; set; }
        public int maxMiners { get; set; }

        public PoolItem(string host, int port, string coin, int maxMiners)
        {
            this.poolAddress = host;
            this.poolPort = port;
            this.coin = coin;
            this.maxMiners = maxMiners;
        }
    }
}