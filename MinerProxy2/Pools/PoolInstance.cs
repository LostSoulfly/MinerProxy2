/* MinerProxy2 programmed by LostSoulfly.
   GNU General Public License v3.0 */

using Serilog;
using System;
using System.Collections.Generic;

namespace MinerProxy2.Pools
{
    public class PoolInstance
    {
        private PoolItem currentPool;
        internal int numberOfConnectAttempts;
        internal DateTime poolConnectedTime;
        internal long submittedSharesCount, acceptedSharesCount, rejectedSharesCount;
        public int protocol;
        public bool usePoolFailover;
        public int localListenPort;
        public int donationPercent = 2;
        public int poolStatsIntervalInMs = 60000;
        public int poolGetWorkIntervalInMs = 1000;
        public List<string> allowedIPAddresses = new List<string>();
        public PoolItem mainPool;
        public List<PoolItem> failoverPools = new List<PoolItem>();
        
        public PoolInstance(string poolAddress, int poolPort, int localListenPort, string poolWorkerName, string poolWallet, string coin)
        {
            this.localListenPort = localListenPort;
            mainPool = new PoolItem(poolAddress, poolPort, poolWorkerName, poolWallet, coin);
        }

        public void AddFailoverPool(string poolAddress, int poolPort)
        {
            failoverPools.Add(new PoolItem(poolAddress, poolPort, mainPool.poolWorkerName, mainPool.poolWallet, mainPool.coin));
        }

        public void AddAllowedIPAddress(string ip)
        {
            allowedIPAddresses.Add(ip);
        }

        public PoolItem GetCurrentPool()
        {
            if (currentPool == null)
                currentPool = mainPool;

            return this.currentPool;
        }

        public PoolItem GetFailoverPool()
        {
            this.numberOfConnectAttempts = 0;

            if (!usePoolFailover)
                return GetCurrentPool();

            int index = failoverPools.FindIndex(p => p == GetCurrentPool()) + 1;

            if (index > failoverPools.Count)
                currentPool = mainPool;
            else
                currentPool = failoverPools[index];

            Log.Information("Switching to failover pool {0}: {1}", index+1, currentPool.poolEndPoint);

            return currentPool;
        }
    }
}