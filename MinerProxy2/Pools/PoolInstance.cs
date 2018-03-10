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

        PoolInstance(string host, int port, string coin, int maxMiners)
        {
            mainPool.poolAddress = host;
            mainPool.poolPort = port;
            mainPool.coin = coin;
            mainPool.maxMiners = maxMiners;
        }

        void AddFailoverPool(string host, int port)
        {
            failoverPools.Add(new PoolItem(host, port, mainPool.coin, mainPool.poolWorkerName, mainPool.maxMiners, mainPool.donationPercent));
        }

        PoolItem GetCurrentPool()
        {
            if (currentPool == null)
                currentPool = mainPool;

            return this.currentPool;

        }

        PoolItem GetFailoverPool()
        {
            //check which pool we're on, then get the next failover or return mainpool


            return currentPool;
        }

    }
}