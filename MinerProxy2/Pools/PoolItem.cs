/* MinerProxy2 programmed by LostSoulfly.
   GNU General Public License v3.0 */

using System;
using System.Collections.Generic;

namespace MinerProxy2.Pools
{
    public class PoolItem
    {
        public string coin { get; set; }
        
        public string poolAddress { get; set; }

        internal string poolEndPoint { get { return poolAddress + ":" + poolPort; } }

        public int poolPort { get; set; }

        public string poolWallet { get; set; }

        public string poolWorkerName { get; set; }

        public string poolPassword { get; set; }

        internal string poolHashrateId { get; set; }
        
        public PoolItem(string poolAddress, int poolPort, string poolWorkerName, string poolWallet, string coin)
        {
            this.poolAddress = poolAddress;
            this.poolPort = poolPort;
            this.coin = coin;
            //this.maxMiners = maxMiners;
            this.poolWorkerName = poolWorkerName;
            this.poolWallet = poolWallet;
            this.poolPassword = "x";

            //Generate a new ID to submit with hashrates. New ID each initialization
            long val = new Random().Next(0, int.MaxValue);
            this.poolHashrateId = String.Format("0x{0:X64}", val);
        }
    }
}