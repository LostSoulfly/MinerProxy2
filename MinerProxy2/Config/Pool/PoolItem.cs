namespace MinerProxy2.Config.Pool
{
    internal class PoolItem
    {
        public string poolAddress { get; set; }
        public int poolPort { get; set; }
        public string coin { get; set; }

        public PoolItem(string host, int port, string coin)
        {
            this.poolAddress = host;
            this.poolPort = port;
            this.coin = coin;
        }
    }
}