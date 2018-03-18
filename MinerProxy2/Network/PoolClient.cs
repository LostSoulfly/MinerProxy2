using MinerProxy2.Interfaces;
using MinerProxy2.Miners;
using MinerProxy2.Network.Sockets;
using MinerProxy2.Pools;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace MinerProxy2.Network
{
    public class PoolClient
    {
        private Client poolClient;
        private MinerServer minerServer;
        private MinerManager minerManager = new MinerManager();
        private PoolInstance poolInstance;
        private ICoinHandlerPool poolHandler;
        private ICoinHandlerMiner coinHandler;
        private bool poolConnected;
        public string poolWallet { get { return poolInstance.GetCurrentPool().poolWallet; } }
        public string poolWorkerName { get { return poolInstance.GetCurrentPool().poolWorkerName; } }
        public string poolEndPoint { get { return poolInstance.GetCurrentPool().poolEndPoint; } }
        public long acceptedSharesCount { get { return poolInstance.acceptedSharesCount; } set { poolInstance.acceptedSharesCount = value; } }
        public long submittedSharesCount { get { return poolInstance.submittedSharesCount; } set { poolInstance.submittedSharesCount = value; } }
        public long rejectedSharesCount { get { return poolInstance.rejectedSharesCount; } set { poolInstance.rejectedSharesCount = value; } }
        public byte[] currentPoolWork = new byte[0];

        private Timer statsTimer;
        
        private List<byte[]> submittedSharesHistory = new List<byte[]>();
        private readonly object submittedShareLock = new object();

        public PoolClient(PoolInstance poolInstance, ICoinHandlerPool pool, ICoinHandlerMiner miner)
        {
            this.poolHandler = pool;
            this.coinHandler = miner;
            this.poolInstance = poolInstance;

            //Create a new instance of the MinerServer, which creates an instance of Network.Sockets.Server
            minerServer = new MinerServer(poolInstance.GetCurrentPool().localListenPort, this, minerManager, coinHandler);

            //Create a new instance of the Network.Sockets.Client
            poolClient = new Client(poolInstance.GetCurrentPool().poolAddress, poolInstance.GetCurrentPool().poolPort);

            //define pool events
            poolClient.OnServerConnected += PoolClient_OnServerConnected;
            poolClient.OnServerDataReceived += PoolClient_OnServerDataReceived;
            poolClient.OnServerDisconnected += PoolClient_OnServerDisconnected;
            poolClient.OnServerError += PoolClient_OnServerError;
            
            //setup coin miner handler
            coinHandler.SetMinerServer(minerServer);
            coinHandler.SetPoolClient(this);
            coinHandler.SetMinerManager(minerManager);
            
            //setup coin Pool handler
            poolHandler.SetMinerServer(minerServer);
            poolHandler.SetPoolClient(this);
            poolHandler.SetMinerManager(minerManager);

            //TODO: add option to wait for miner connections before connecting to pool
            poolClient.Connect();
            minerServer.ListenForMiners();
        }

        private void StartPoolStats()
        {
            statsTimer = new Timer(60000);
            statsTimer.AutoReset = true;
            
            statsTimer.Elapsed += delegate
            {
                TimeSpan time = poolInstance.poolConnectedTime - DateTime.Now;
                Log.Information("{0} uptime: {1}. Miners: {2} Total Shares: {3}/{4}/{5}",
                    this.poolEndPoint, time.ToString("hh\\:mm"), minerManager.ConnectedMiners, poolInstance.submittedSharesCount, poolInstance.acceptedSharesCount, poolInstance.rejectedSharesCount);
            };

            statsTimer.Start();

        }

        private void StopPoolStats()
        {
            statsTimer.Stop();
        }

        public void SubmitShareToPool(byte[] data, Miner miner)
        {
            Log.Debug("{0} submitting share.", miner.workerIdentifier);
            if (submittedSharesHistory.Any(item => item == data))
            {
                Log.Warning("Share already exists, not sending to pool.");
                return;
            }

            poolInstance.submittedSharesCount++;
            minerManager.AddSubmittedShare(miner);
            SendToPool(data);
        }

        private void PoolClient_OnServerError(object sender, ServerErrorArgs e)
        {
            Log.Error(e.exception, "Server error!");
            poolConnected = false;
            StopPoolStats();
        }

        private void PoolClient_OnServerDisconnected(object sender, ServerDisonnectedArgs e)
        {
            poolConnected = false;
            StopPoolStats();
            ClearSubmittedSharesHistory();
        }

        private void PoolClient_OnServerDataReceived(object sender, ServerDataReceivedArgs e)
        {
            //Log.Information(Encoding.ASCII.GetString(e.Data));
            poolHandler.PoolDataReceived(e.Data, this);
        }
        
        private void PoolClient_OnServerConnected(object sender, ServerConnectedArgs e)
        {
            Log.Verbose("Pool connected: {0}.", e.socket.RemoteEndPoint.ToString());
            poolInstance.poolConnectedTime = DateTime.Now;
            StartPoolStats();
            if (!poolConnected)
            {
                poolConnected = true;
                poolHandler.DoPoolLogin(this);
            }
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
                submitted = submittedSharesHistory.Any(item => item == share);

                //If it wasn't found in the list, we add it
                if (!submitted)
                    submittedSharesHistory.Add(share);
            }

            return submitted;
        }

        public void ClearSubmittedSharesHistory()
        {
            Log.Debug("Clearing submitted shares history.");
            lock (submittedShareLock) { submittedSharesHistory.Clear(); }
        }

    }
}