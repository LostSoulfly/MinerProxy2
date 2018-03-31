/* MinerProxy2 programmed by LostSoulfly.
   GNU General Public License v3.0 */

using System;
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

        internal string poolEndPoint { get { return poolAddress + ":" + poolPort; } }

        public int poolPort { get; set; }

        public string poolWallet { get; set; }

        public string poolWorkerName { get; set; }

        public string poolPassword { get; set; }

        internal string poolHashrateId { get; set; }
        
        public PoolItem(string poolAddress, int poolPort, int localListenPort, string poolWorkerName, string poolWallet, string coin, int donationPercent)
        {
            this.poolAddress = poolAddress;
            this.poolPort = poolPort;
            this.localListenPort = localListenPort;
            this.coin = coin;
            //this.maxMiners = maxMiners;
            this.donationPercent = donationPercent;
            this.poolWorkerName = poolWorkerName;
            this.poolWallet = poolWallet;
            this.poolPassword = "x";

            //Generate a new ID to submit with hashrates. New ID each initialization
            long val = new Random().Next(0, int.MaxValue);
            this.poolHashrateId = String.Format("0x{0:X64}", val);
        }
    }
}