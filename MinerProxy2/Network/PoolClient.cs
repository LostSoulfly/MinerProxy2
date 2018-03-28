/* MinerProxy2 programmed by LostSoulfly.
   GNU General Public License v3.0 */

using MinerProxy2.Helpers;
using MinerProxy2.Interfaces;
using MinerProxy2.Miners;
using MinerProxy2.Network.Sockets;
using MinerProxy2.Pools;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace MinerProxy2.Network
{
    public class PoolClient
    {
        private readonly object submittedShareLock = new object();
        private ICoinHandlerMiner coinHandler;
        private MinerManager minerManager = new MinerManager();
        private MinerServer minerServer;
        private Client poolClient;
        private bool poolConnected;
        private ICoinHandlerPool poolHandler;
        private PoolInstance poolInstance;
        private Timer statsTimer;
        private Timer getWorkTimer;
        private List<byte[]> submittedSharesHistory = new List<byte[]>();

        public byte[] currentPoolWork = new byte[0];

        public byte[] currentPoolTarget = new byte[0];

        public long acceptedSharesCount { get { return poolInstance.acceptedSharesCount; } set { poolInstance.acceptedSharesCount = value; } }

        public string poolEndPoint { get { return poolInstance.GetCurrentPool().poolEndPoint; } }

        public string poolWallet { get { return poolInstance.GetCurrentPool().poolWallet; } }

        public string poolWorkerName { get { return poolInstance.GetCurrentPool().poolWorkerName; } }

        public string poolHashrateId { get { return poolInstance.GetCurrentPool().poolHashrateId; } }

        public long rejectedSharesCount { get { return poolInstance.rejectedSharesCount; } set { poolInstance.rejectedSharesCount = value; } }

        public long submittedSharesCount { get { return poolInstance.submittedSharesCount; } set { poolInstance.submittedSharesCount = value; } }

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

            //this.Start();
            minerServer.ListenForMiners();

            Log.Information("{0} waiting for miners..", poolEndPoint);
        }

        private void PoolClient_OnServerConnected(object sender, ServerConnectedArgs e)
        {
            Log.Verbose("Pool connected: {0}.", e.socket.RemoteEndPoint.ToString());
            poolInstance.poolConnectedTime = DateTime.Now;
            StartPoolStats();
            StartGetWorkTimer();
            if (!poolConnected)
            {
                poolConnected = true;
                poolHandler.DoPoolLogin(this);
                poolHandler.DoPoolGetWork(this);
            }
        }

        private void PoolClient_OnServerDataReceived(object sender, ServerDataReceivedArgs e)
        {
            //Log.Information(Encoding.ASCII.GetString(e.Data));
            poolHandler.PoolDataReceived(e.Data, this);
        }

        private void PoolClient_OnServerDisconnected(object sender, ServerDisonnectedArgs e)
        {
            poolConnected = false;
            Stop();
            CheckPoolConnection();
        }

        private void PoolClient_OnServerError(object sender, ServerErrorArgs e)
        {
            Log.Error(e.exception, "Server error!");
            poolConnected = false;
            Stop();
            CheckPoolConnection();
        }

        private void StartPoolStats()
        {
            statsTimer = new Timer(60000);
            statsTimer.AutoReset = true;

            statsTimer.Elapsed += delegate
            {
                TimeSpan time = poolInstance.poolConnectedTime - DateTime.Now;
                Log.Debug("Current hashrate: {0}", minerManager.GetCurrentHashrateReadable());
                Log.Information("[{0}] uptime: {1}. Miners: {2} Shares: {3}/{4}/{5}",
                    this.poolWorkerName, time.ToString("hh\\:mm"), minerManager.ConnectedMiners, poolInstance.submittedSharesCount, poolInstance.acceptedSharesCount, poolInstance.rejectedSharesCount);
                minerManager.minerList.ForEach<Miner>(m => m.PrintShares());
            };

            statsTimer.Start();
        }

        private void StartGetWorkTimer()
        {

            int tickRate = 500;//(5000 / minerServer.GetNumberOfConnections);

            //if (tickRate < 500) tickRate = 500;

            getWorkTimer = new Timer(tickRate);
            getWorkTimer.AutoReset = true;

            getWorkTimer.Elapsed += delegate
            {
                //Log.Debug("Requesting work from pool..");
                poolHandler.DoPoolGetWork(this);
            };

            getWorkTimer.Start();
        }

        private void StopGetWorkTimer()
        {
            if (getWorkTimer == null)
                return;

            if (getWorkTimer.Enabled)
                getWorkTimer.Stop();
        }

        private void StopPoolStats()
        {
            if (statsTimer == null)
                return;

            if (statsTimer.Enabled)
                statsTimer.Stop();
        }

        public bool CheckPoolConnection()
        {
            Log.Debug("{0} number of connections: {1}", poolWorkerName, minerServer.GetNumberOfConnections);

            if (poolConnected && minerServer.GetNumberOfConnections == 0)
            {
                Stop();
                Log.Information("Waiting for miners before reconnecting to {0}..", poolEndPoint);
                return false;
            }

            if (minerServer.GetNumberOfConnections == 0)
            {
                return false;
            }

            if (poolConnected)
                return true;

            Start();
            return true;
        }

        public void ClearSubmittedSharesHistory()
        {
            Log.Debug("Clearing submitted shares history.");
            lock (submittedShareLock) { submittedSharesHistory.Clear(); }
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

        public void SendToPool(byte[] data)
        {
            //Log.Debug("PoolClient SendToPool");
            this.poolClient.SendToPool(data);
        }

        public void SendToPool(string data)
        {
            //Log.Debug("PoolClient SendToPool");
            this.poolClient.SendToPool(data.GetBytes());
        }

        public void Start()
        {
            Log.Information("Connecting to {0}.", this.poolEndPoint);
            poolClient.Connect();
        }

        public void Stop()
        {
            StopPoolStats();
            StopGetWorkTimer();

            if (poolConnected)
            {
                Log.Information("Disconnecting from {0}.", this.poolEndPoint);
                poolConnected = false;
                currentPoolWork = new byte[0];
                currentPoolTarget = new byte[0];
                poolClient.Close();
                return;
            }

            ClearSubmittedSharesHistory();
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
    }
}