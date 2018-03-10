using System.Collections.Generic;

namespace MinerProxy2.Miners
{
    public class MinerManager
    {
        public readonly object MinerManagerLock = new object();
        public List<Miner> minerList = new List<Miner>();

        public void AddMiner()
        {
        }

        public void RemoveMiner()
        {
        }

        public Miner GetMiner()
        {
            return null;
        }
    }
}