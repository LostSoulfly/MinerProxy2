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
        private List<Miner> minerList = new List<Miner>();
        public readonly object MinerManagerLock = new object();
        public int ConnectedMiners { get { return minerList.Count; } }

        public void AddMiner(Miner miner)
        {
            lock (MinerManagerLock)
            {
                Log.Verbose("Adding new miner {0} ", miner.workerIdentifier);
                Miner existing = GetMiner(miner.connection);

                if (existing == null)
                    existing = GetMiner(miner.workerName);

                if (existing == null)
                    minerList.Add(miner);

                miner.numberOfConnects++;
                miner.minerConnected = true;
            }
        }

        public void AddMinerID(Miner miner, int id)
        {
            miner.minerID = id;
        }

        public void AddSubmittedShare(Miner miner)
        {
            //Log.Debug("shareSubmittedtimes current count (before new share): {0}", miner.shareSubmittedTimes.Count);
            miner.shareSubmittedTimes.Add(DateTime.Now);

            miner.submittedShares++;
        }

        public long GetAcceptedShareTotal()
        {
            long acceptedShares = 0; try
            {
                GetMinerList().ForEach<Miner>(m => acceptedShares += m.acceptedShares);
            }
            catch { }

            return acceptedShares;
        }

        public long GetCurrentHashrateLong()
        {
            long total = 0;
            try
            {
                GetMinerList().ForEach<Miner>(m => total += m.hashrate);
            }
            catch { }
            return total;
        }

        public string GetCurrentHashrateReadable()
        {
            long total = 0;
            try
            {
                GetMinerList().ForEach<Miner>(m => total += m.hashrate);
            }
            catch { }

            if (total == 0)
                return "";

            return total.ToString("#,##0,Mh/s").Replace(",", ".");
        }

        public Miner GetMiner(TcpConnection connection)
        {
            Miner miner;
            try
            {
                miner = GetMinerList().First(item => item.connection.socket == connection.socket);
            }
            catch (Exception ex)
            {
                //Log.Error("GetMiner", ex);
                return null;
            }

            Log.Verbose("GetMiner {0} -> {1}", connection.endPoint, miner.workerIdentifier);

            return miner;
        }

        public Miner GetMiner(string workerName)
        {
            Miner miner;
            try
            {
                miner = GetMinerList().First(item => item.workerName == workerName);
            }
            catch (Exception ex)
            {
                //Log.Error("GetMiner", ex);
                return null;
            }

            Log.Debug("GetMiner by workerName {0} -> {1} ({2})", miner.connection.endPoint, miner.workerIdentifier, miner.minerConnected ? "was connected" : "was not connected");

            return miner;
        }

        public List<Miner> GetMinerList()
        {
            return minerList.ToList();
        }

        public Miner GetNextShare(bool accepted)
        {
            Miner miner;

            try
            {
                miner = GetMinerList().OrderBy(m => m.shareSubmittedTimes.DefaultIfEmpty(DateTime.MaxValue).FirstOrDefault()).First();
                Log.Verbose("GetNextShare: {0} ({1})!", miner.workerIdentifier, accepted ? "Accepted" : "Rejected");

                if (accepted)
                    miner.acceptedShares++;
                else
                    miner.rejectedShares++;

                return miner;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "GetNextshare");
                return null;
            }
        }

        public long GetRejectedShareTotal()
        {
            long rejectedShares = 0;
            try
            {
                GetMinerList().ForEach<Miner>(m => rejectedShares += m.rejectedShares);
            }
            catch { }

            return rejectedShares;
        }

        public long GetSubmittedShareTotal()
        {
            long submittedShares = 0;
            try
            {
                GetMinerList().ForEach<Miner>(m => submittedShares += m.submittedShares);
            }
            catch { }

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

        public void MinerOffline(Miner miner)
        {
            lock (MinerManagerLock)
            {
                miner.connectionDisconnectTime = DateTime.Now;
                miner.connection = null;
                miner.minerConnected = false;
                //TimeSpan timeSpan;
                //timeSpan = miner.connectionStartTime - miner.connectionDisconnectTime;
                //miner.totalTimeConnected += timeSpan;
            }
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
            string ts = string.Empty;
            try
            {
                ts = miner.shareSubmittedTimes.First().ToReadableTime();
                Log.Verbose("Resetting {0} last submit time. ({1})", miner.workerIdentifier, ts);
                miner.shareSubmittedTimes.Remove(miner.shareSubmittedTimes.First());
            }
            catch (Exception ex)
            {
                Log.Error("ResetMinerShareSubmittedTime", ex);
                ts = "error!";
            }
            return ts;
        }

        public void UpdateMinerHashrate(long hashrate, Miner miner)
        {
            miner.hashrate = hashrate;
        }
    }
}