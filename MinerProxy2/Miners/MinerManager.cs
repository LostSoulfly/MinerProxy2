/* MinerProxy2 programmed by LostSoulfly.
   GNU General Public License v3.0 */

using MinerProxy2.Helpers;
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
        public int ConnectedMiners { get { return minerList.Count; } }

        public void AddMiner(Miner miner)
        {
            lock (MinerManagerLock)
            {
                Log.Verbose("Adding new miner {0} ", miner.workerIdentifier);
                Miner existing = GetMiner(miner.connection);

                if (existing == null)
                {
                    minerList.Add(miner);
                }
            }
        }

        public void AddSubmittedShare(Miner miner)
        {
            //Log.Debug("shareSubmittedtimes current count (before new share): {0}", miner.shareSubmittedTimes.Count);
            miner.shareSubmittedTimes.Add(DateTime.Now);
            
            miner.submittedShares++;
        }

        public long GetAcceptedShareTotal()
        {
            long acceptedShares = 0;
            minerList.ForEach<Miner>(m => acceptedShares += m.acceptedShares);

            return acceptedShares;
        }

        public string GetCurrentHashrateReadable()
        {
            long total = 0;
            minerList.ForEach<Miner>(m => total += m.hashrate);
            return total.ToString("#,##0,Mh/s").Replace(",", ".");
        }

        public long GetCurrentHashrateLong()
        {
            long total = 0;
            minerList.ForEach<Miner>(m => total += m.hashrate);
            return total;
        }

        public Miner GetMiner(TcpConnection connection)
        {
            Miner miner = minerList.Find(item => item.connection == connection);

            if (miner != null)
                Log.Verbose("GetMiner {0} -> {1}", connection.endPoint, miner.workerIdentifier);

            return miner;
        }

        public Miner GetNextShare(bool accepted)
        {
            Miner miner = minerList.OrderBy(m => m.shareSubmittedTimes.DefaultIfEmpty(DateTime.MaxValue).FirstOrDefault()).First();
            Log.Verbose("GetNextShare: {0} ({1})!", miner.workerIdentifier, accepted ? "Accepted" : "Rejected");

            if (accepted)
            {
                miner.acceptedShares++;
            }
            else
                miner.rejectedShares++;

            return miner;
        }

        public long GetRejectedShareTotal()
        {
            long rejectedShares = 0;
            minerList.ForEach<Miner>(m => rejectedShares += m.rejectedShares);

            return rejectedShares;
        }

        public long GetSubmittedShareTotal()
        {
            long submittedShares = 0;
            minerList.ForEach<Miner>(m => submittedShares += m.submittedShares);

            return submittedShares;
        }

        /*
        public void AddMinerId(Miner miner, int id)
        {
            if (miner.minerIdList.Count > 0
                && miner.minerIdList.Exists(item => item == id))
                    return;

            Log.Debug("[{0}] adding ID {1}.", miner.workerIdentifier, id);
            miner.minerIdList.Add(id);
        }

        public List<int> GetMinerIds(Miner miner)
        {
            return miner.minerIdList;
        }
        */

        public void AddMinerID(Miner miner, int id)
        {
            miner.minerID = id;
        }

        public void RemoveMiner(Miner miner)
        {
            lock (MinerManagerLock)
            {
                Log.Debug("Removing {0}", miner.workerIdentifier);
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

        public string ResetMinerShareSubmittedTime(Miner miner)
        {
            string ts = miner.shareSubmittedTimes.First().ToReadableTime();
            Log.Verbose("Resetting {0} last submit time. ({1})", miner.workerIdentifier, ts);
            miner.shareSubmittedTimes.Remove(miner.shareSubmittedTimes.First());
            return ts;
        }

        public void UpdateMinerHashrate(long hashrate, Miner miner)
        {
            miner.hashrate = hashrate;
        }
    }
}