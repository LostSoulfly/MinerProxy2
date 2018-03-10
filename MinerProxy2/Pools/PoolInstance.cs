using System.Collections.Generic;

namespace MinerProxy2.Pools
{
    public class PoolInstance
    {
        public List<PoolItem> failoverPools = new List<PoolItem>();
        public readonly PoolItem mainPool;
        private PoolItem currentPool;
        public bool passwordAsWorkerName;
        public bool useDotBeforeWorkerName;
        //failure attempts, then switch
        //retry main pool in seconds

        private PoolInstance(string host, int port, string coin, int maxMiners)
        {
            mainPool.poolAddress = host;
            mainPool.poolPort = port;
            mainPool.coin = coin;
            mainPool.maxMiners = maxMiners;
        }

        private void AddFailoverPool(string host, int port)
        {
            failoverPools.Add(new PoolItem(host, port, mainPool.coin, mainPool.poolWorkerName, mainPool.maxMiners, mainPool.donationPercent));
        }

        private PoolItem GetCurrentPool()
        {
            if (currentPool == null)
                currentPool = mainPool;

            return this.currentPool;
        }

        private PoolItem GetFailoverPool()
        {
            //check which pool we're on, then get the next failover or return mainpool

            return currentPool;
        }
    }
}