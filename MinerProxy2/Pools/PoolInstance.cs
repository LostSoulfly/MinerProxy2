using System.Collections.Generic;

namespace MinerProxy2.Pools
{
    public class PoolInstance
    {
        public List<PoolItem> failoverPools = new List<PoolItem>();
        public readonly PoolItem mainPool;
        private PoolItem currentPool;
        public int localListenPort;
        public bool passwordAsWorkerName;
        public bool useDotBeforeWorkerName;
        //failure attempts, then switch
        //retry main pool in seconds

        public PoolInstance(string host, int port, string coin)
        {
            mainPool.poolAddress = host;
            mainPool.poolPort = port;
            mainPool.coin = coin;
        }

        public void AddFailoverPool(string host, int port)
        {
            failoverPools.Add(new PoolItem(host, port, mainPool.coin, mainPool.poolWorkerName, mainPool.donationPercent));
        }

        public PoolItem GetCurrentPool()
        {
            if (currentPool == null)
                currentPool = mainPool;

            return this.currentPool;
        }

        public PoolItem GetFailoverPool()
        {
            //check which pool we're on, then get the next failover or return mainpool

            return currentPool;
        }
    }
}