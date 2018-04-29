/* MinerProxy2 programmed by LostSoulfly.
   GNU General Public License v3.0 */

using MinerProxy2.Network.Sockets;
using System;
using System.Collections.Generic;

namespace MinerProxy2.Miners
{
    public class Miner
    {
        public DateTime connectionDisconnectTime;
        public DateTime connectionStartTime;
        public Queue<HashrateItem> hashrateHistory = new Queue<HashrateItem>(1000);

        public bool minerConnected;

        public int minerID;

        public int numberOfConnects;

        //public TimeSpan totalTimeConnected;
        //public DateTime lastCalculatedTime;
        public List<DateTime> shareSubmittedTimes;

        public long acceptedShares { get; set; }

        public TcpConnection connection { get; set; }

        public long hashrate
        {
            get
            {
                return hashrate;
            }
            set
            {
                hashrate = value;
                hashrateHistory.Enqueue(new HashrateItem(DateTime.Now, value));
            }
        }

        public bool noRigName { get; set; }

        public long rejectedShares { get; set; }

        public long submittedShares { get; set; }

        public string workerIdentifier
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(workerName))
                    return workerName;

                return connection.endPoint.ToString();
            }
        }

        public string workerName { get; set; }

        public Miner(string workerName, TcpConnection connection)
        {
            this.connection = connection;
            this.workerName = workerName;
            //connectionAlive = true;
            shareSubmittedTimes = new List<DateTime>();
            connectionStartTime = DateTime.Now;
        }

        public void PrintShares(string prefix)
        {
            //Serilog.Log.Information(string.Format("{0, -10} {1, 6} {2, 6} {3, 6} {4, 11}", workerIdentifier, submittedShares, acceptedShares, rejectedShares, hashrate.ToString("#,##0,Mh/s").Replace(",", ".")));

            Serilog.Log.Information("[{0}] ({1}) {2}'s shares: {3}/{4}/{5} ({6})", prefix, workerIdentifier, this.minerConnected ? "Online" : "Offline", submittedShares, acceptedShares, rejectedShares, hashrate.ToString("#,##0,Mh/s").Replace(",", "."));
        }
    }
}