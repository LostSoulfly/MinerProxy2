using System;
using System.Collections.Generic;
using System.Text;

namespace MinerProxy2.Config.Pool
{
    class PoolItem
    {
        public string   poolAddress { get; set; }
        public int      poolPort { get; set; }
        public string   coin { get; set; }

        public PoolItem(string host, int port, string coin)
        {
            this.poolAddress = host;
            this.poolPort = port;
            this.coin = coin;
        }
    }
}
