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
                Log.Information("Adding Miner " + miner.connection.endPoint);
                Miner existing = GetMiner(miner.connection);

                if (existing == null)
                {
                    minerList.Add(miner);
                }
            }
        }

        public void RemoveMiner(Miner miner)
        {
            lock (MinerManagerLock)
            {
                Log.Information("Removing Miner " + miner.connection.endPoint);
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

        public long GetSubmittedShareTotal()
        {
            long submittedShares = 0;
            foreach (Miner m in minerList)
            {
                submittedShares += m.acceptedShares;
            }

            return submittedShares;
        }

        public long GetRejectedShareTotal()
        {
            long rejectedShares = 0;
            foreach (Miner m in minerList)
            {
                rejectedShares += m.rejectedShares;
            }

            return rejectedShares;
        }

        public void AddSubmittedShare(Miner miner)
        {
            miner.shareSubmittedTimes.Add(DateTime.Now);
            miner.submittedShares++;
            Log.Information("Miner " + miner.connection.endPoint + " shares: {0}/{1}/{2}", miner.submittedShares, miner.acceptedShares, miner.rejectedShares);
        }

        public long GetAverageHashrate()
        {
            //average the hashrate of all connected miners
            return 0;
        }

        public void UpdateMinerHashrate(long hashrate, Miner miner)
        {
            miner.hashrate = hashrate;
        }

        public Miner GetMiner(TcpConnection connection)
        {
            Log.Debug("GetMiner Miner " + connection.endPoint);
            return minerList.Find(item => item.connection == connection);
        }

        public Miner GetNextShare(bool accepted)
        {
            Miner miner = minerList.OrderBy(m => m.shareSubmittedTimes.DefaultIfEmpty(DateTime.MaxValue).FirstOrDefault()).First();
            Log.Verbose("GetNextShare: {0}: {1}!", miner.connection.endPoint, accepted ? "Accepted" : "Rejected");

            if (accepted)
                miner.acceptedShares++;
            else
                miner.rejectedShares++;

            Log.Information("Miner " + miner.connection.endPoint + " shares: {0}/{1}/{2}", miner.submittedShares, miner.acceptedShares, miner.rejectedShares);

            return miner;
        }
        
        public void ResetMinerShareSubmittedTime(Miner miner)
        {
            Log.Debug("Resetting miner last submit time: {0} ({1})", miner.connection.endPoint, miner.shareSubmittedTimes.First().ToShortTimeString());
            miner.shareSubmittedTimes.Remove(miner.shareSubmittedTimes.First());
        }
    }
}