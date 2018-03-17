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
        private MinerServer minerServer;
        private Client poolClient;
        private MinerManager minerManager;
        private PoolInstance poolInstance;
        private ICoinHandlerPool poolHandler;
        private ICoinHandlerMiner coinHandler;
        private string host;
        private int port;
        private bool poolConnected;

        private Timer statsTimer;

        //public string poolWallet { get { return poolInstance.GetCurrentPool().poolWallet; } }
        //public string poolWorkerName { get { return poolInstance.GetCurrentPool().poolWorkerName; } }
        public byte[] currentWork = new byte[0];
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
            this.minerManager = new MinerManager();

            minerServer = new MinerServer(poolInstance.GetCurrentPool().localListenPort, this, minerManager, coinHandler);

            poolClient = new Client(poolInstance.GetCurrentPool().poolAddress, poolInstance.GetCurrentPool().poolPort);
            poolClient.OnServerConnected += PoolClient_OnServerConnected;
            poolClient.OnServerDataReceived += PoolClient_OnServerDataReceived;
            poolClient.OnServerDisconnected += PoolClient_OnServerDisconnected;
            poolClient.OnServerError += PoolClient_OnServerError;
            poolClient.Connect();

            coinHandler.SetMinerServer(minerServer);
            coinHandler.SetPoolClient(this);
            coinHandler.SetMinerManager(minerManager);

            poolHandler.SetMinerServer(minerServer);
            poolHandler.SetPoolClient(this);
            poolHandler.SetMinerManager(minerManager);

            poolHandler.SetPoolInfo(poolInstance);

            minerServer.ListenForMiners();
        }

        private void StartPoolStats()
        {
            statsTimer = new Timer(30000);
            statsTimer.AutoReset = true;
            
            statsTimer.Elapsed += delegate
            {
                TimeSpan time = poolInstance.poolConnectedTime - DateTime.Now;
                Log.Information("Server connection uptime: " + time.ToString("hh\\:mm\\:ss"));
            };

            statsTimer.Start();

        }

        private void StopPoolStats()
        {
            statsTimer.Stop();
        }

        public void SubmitShareToPool(byte[] data, Miner miner)
        {
           
            Log.Verbose("Miner submitting share: " + miner.connection.endPoint);
            if (submittedShares.Any(item => item == data))
            {
                Log.Warning("Share already exists, not sending to pool.");
                return;
            }


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
            ClearSubmittedShares();
        }

        private void PoolClient_OnServerDataReceived(object sender, ServerDataReceivedArgs e)
        {
            //Log.Information(Encoding.ASCII.GetString(e.Data));
            poolHandler.PoolDataReceived(e.Data, this);
        }
        
        private void PoolClient_OnServerConnected(object sender, ServerConnectedArgs e)
        {
            Log.Information("Pool connected: {0}.", e.socket.RemoteEndPoint.ToString());
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