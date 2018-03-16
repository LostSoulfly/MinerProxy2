using MinerProxy2.Network.Sockets;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MinerProxy2.Miners
{
    public class MinerManager
    {
        public readonly object MinerManagerLock = new object();
        public List<Miner> minerList = new List<Miner>();

        public void AddMiner(Miner miner)
        {
            lock (MinerManagerLock)
            {
                Miner existing = GetMiner(miner.connection);

                if (existing == null)
                    minerList.Add(miner);
            }
        }


        public void RemoveMiner(Miner miner)
        {
            lock (MinerManagerLock)
            {
                try
                {
                    minerList.Remove(miner);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "RemoveMiner");
                }
            }
        }

        public Miner GetMiner(TcpConnection connection)
        {
            return minerList.Find(item => item.connection == connection);
        }

        public Miner GetOldestMinerShareSubmitted()
        {
            Miner miner = minerList.OrderBy(m => m.shareSubmittedTime).First();
            return miner;
        }
    }
}