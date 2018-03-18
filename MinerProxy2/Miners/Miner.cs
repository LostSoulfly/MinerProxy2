using MinerProxy2.Network.Sockets;
using System;
using System.Collections.Generic;

namespace MinerProxy2.Miners
{
    public class Miner
    {
        public long submittedShares { get; set; }
        public long acceptedShares { get; set; }
        public long rejectedShares { get; set; }
        public long hashrate { get; set; }
        public string workerName { get; set; }
        //public bool connectionAlive { get; set; }
        public bool noRigName { get; set; }
        public string workerIdentifier { get
            {
                if (workerName != null)
                    return workerName;

                return connection.endPoint.ToString();
            } }
        //public byte[] currentWork { get; set; }
        public TcpConnection connection { get; set; }
        public DateTime connectionStartTime;
        public DateTime lastCalculatedTime;
        public List<DateTime> shareSubmittedTimes;

        public Miner(string workerName, TcpConnection connection)
        {
            this.connection = connection;
            this.workerName = workerName;
            //connectionAlive = true;
            shareSubmittedTimes = new List<DateTime>();
            connectionStartTime = DateTime.Now;
        }

    }
}