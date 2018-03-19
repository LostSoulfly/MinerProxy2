/* MinerProxy2 programmed by LostSoulfly.
   GNU General Public License v3.0 */

using MinerProxy2.Network.Sockets;
using System;
using System.Collections.Generic;

namespace MinerProxy2.Miners
{
    public class Miner
    {
        public DateTime connectionStartTime;
        public DateTime lastCalculatedTime;
        public List<DateTime> shareSubmittedTimes;

        public long acceptedShares { get; set; }

        public TcpConnection connection { get; set; }

        public long hashrate { get; set; }

        public bool noRigName { get; set; }

        public long rejectedShares { get; set; }

        public long submittedShares { get; set; }

        public string workerIdentifier
        {
            get
            {
                if (workerName != null)
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

        public void PrintShares()
        {
            Serilog.Log.Information("{0}'s shares: {1}/{2}/{3}", workerIdentifier, submittedShares, acceptedShares, rejectedShares);
        }
    }
}