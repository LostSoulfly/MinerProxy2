using MinerProxy2.Interfaces;
using MinerProxy2.Miners;
using MinerProxy2.Network.Sockets;
using MinerProxy2.Pools;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinerProxy2.Network
{
    public class PoolClient
    {
        private MinerServer minerServer;
        private Client poolClient;
        private MinerManager minerManager;
        private PoolInstance poolInstance;
        private ICoinHandlerPool poolHandler;
        private ICoinHandlerMiner coinHandler;
        private string host;
        private int port;
        private bool poolConnected;
        //public string poolWallet { get { return poolInstance.GetCurrentPool().poolWallet; } }
        //public string poolWorkerName { get { return poolInstance.GetCurrentPool().poolWorkerName; } }
        public byte[] currentWork;
        private List<byte[]> submittedShares = new List<byte[]>();
        private readonly object submittedShareLock = new object();
        public string poolEndPoint { get; }

        public PoolClient(PoolInstance poolInstance, ICoinHandlerPool pool, ICoinHandlerMiner miner)
        {
            this.poolHandler = pool;
            this.coinHandler = miner;
            this.poolInstance = poolInstance;
            this.host = poolInstance.mainPool.poolAddress;
            this.port = poolInstance.mainPool.poolPort;
            this.poolEndPoint = this.host + ":" + this.port;


            minerServer = new MinerServer(poolInstance.GetCurrentPool().localListenPort, this, coinHandler);

            poolClient = new Client(poolInstance.GetCurrentPool().poolAddress, poolInstance.GetCurrentPool().poolPort);
            poolClient.OnServerConnected += PoolClient_OnServerConnected;
            poolClient.OnServerDataReceived += PoolClient_OnServerDataReceived;
            poolClient.Connect();

            coinHandler.SetMinerServer(minerServer);
            coinHandler.SetPoolClient(this);

            poolHandler.SetMinerServer(minerServer);
            poolHandler.SetPoolClient(this);

            poolHandler.SetPoolInfo(poolInstance);

            minerServer.ListenForMiners();
        }
        
        private void PoolClient_OnServerDataReceived(object sender, ServerDataReceivedArgs e)
        {
            Log.Debug("OnServerDataReceived: " + poolConnected);
            if (!poolConnected)
            {
                poolConnected = true;
                poolHandler.DoPoolLogin(this);
            }
            poolHandler.PoolDataReceived(e.Data, this);
        }
        
        private void PoolClient_OnServerConnected(object sender, ServerConnectedArgs e)
        {
            Log.Debug("Pool connected: " + e.socket.RemoteEndPoint.ToString());
        }

        public void SendToPool(byte[] data)
        {
            //Log.Debug("PoolClient SendToPool");
            this.poolClient.SendToPool(data);
        }

        public bool HasShareBeenSubmitted(byte[] share)
        {
            bool submitted;

            lock (submittedShareLock)
            {
                //search the list to see if this share has been 
                submitted = submittedShares.Any(item => item == share);

                //If it wasn't found in the list, we add it
                if (!submitted)
                    submittedShares.Add(share);
            }

            return submitted;
        }

        public void ClearSubmittedShares()
        {
            lock (submittedShareLock) { submittedShares.Clear(); }
        }

    }
}