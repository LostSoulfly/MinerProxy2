using System.Collections.Generic;

namespace MinerProxy2.Config.Pool
{
    public class PoolManager
    {
        public List<PoolItem> failoverPools = new List<PoolItem>();
        public PoolItem mainPool;
        public PoolItem currentPool;
        //failure attempts, then switch
        //retry main pool in seconds

        PoolManager(string host, int port, string coin, int maxMiners)
        {
            mainPool.poolAddress = host;
            mainPool.poolPort = port;
            mainPool.coin = coin;
            mainPool.maxMiners = maxMiners;
        }

        void AddFailoverPool(string host, int port)
        {
            failoverPools.Add(new PoolItem(host, port, mainPool.coin, mainPool.maxMiners));
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