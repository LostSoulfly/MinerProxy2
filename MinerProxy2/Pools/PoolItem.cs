/* MinerProxy2 programmed by LostSoulfly.
   GNU General Public License v3.0 */

using System.Collections.Generic;

namespace MinerProxy2.Pools
{
    public class PoolItem
    {
        public List<string> allowedIPAddresses = new List<string>();

        public string coin { get; set; }

        //public int maxMiners { get; set; }
        public int donationPercent { get; set; }

        public int localListenPort { get; set; }

        public string poolAddress { get; set; }

        public string poolEndPoint { get { return poolAddress + ":" + poolPort; } }

        public int poolPort { get; set; }

        public string poolWallet { get; set; }

        public string poolWorkerName { get; set; }

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