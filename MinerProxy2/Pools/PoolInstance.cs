/* MinerProxy2 programmed by LostSoulfly.
   GNU General Public License v3.0 */

using System;
using System.Collections.Generic;

namespace MinerProxy2.Pools
{
    public class PoolInstance
    {
        private PoolItem currentPool;
        internal int numberOfConnects;
        internal DateTime poolConnectedTime;
        internal long submittedSharesCount, acceptedSharesCount, rejectedSharesCount;
        public int protocol;
        public PoolItem mainPool;
        public List<PoolItem> failoverPools = new List<PoolItem>();
        //failure attempts, then switch
        //retry main pool in seconds
        
        public PoolInstance(string poolHost, int poolPort, int localListenPort, string poolWorkerName, string poolWallet, string coin)
        {
            mainPool = new PoolItem(poolHost, poolPort, localListenPort, poolWorkerName, poolWallet, coin, 1);
        }

        public void AddFailoverPool(string host, int port)
        {
            failoverPools.Add(new PoolItem(host, port, mainPool.localListenPort, mainPool.poolWorkerName, mainPool.poolWallet, mainPool.coin, mainPool.donationPercent));
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