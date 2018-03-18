using MinerProxy2.Helpers;
using MinerProxy2.Interfaces;
using MinerProxy2.Miners;
using MinerProxy2.Network.Connections;
using MinerProxy2.Network.Sockets;
using Serilog;
using System.Text;

namespace MinerProxy2.Network
{
    public class MinerServer
    {
        private readonly Server minerServer;
        public ICoinHandlerMiner _coinHandler;
        private readonly PoolClient _poolClient;
        private MinerManager _minerManager;

        private int port;

        public MinerServer(int port, PoolClient pool, MinerManager minerManager, ICoinHandlerMiner coinHandler)
        {
            Log.Information("MinerServer initialized: " + pool.poolEndPoint);

            this._minerManager = minerManager;

            _poolClient = pool;
            _coinHandler = coinHandler;
            _coinHandler.SetMinerServer(this);
            _coinHandler.SetPoolClient(_poolClient);


            this.port = port;
            minerServer = new Server();
            minerServer.OnClientDataReceived += MinerServer_OnClientDataReceived;
            minerServer.OnClientConnected += MinerServer_OnClientConnected;
            minerServer.OnClientDisconnected += MinerServer_OnClientDisconnected;
            minerServer.OnClientError += MinerServer_OnClientError;
        }

        private void MinerServer_OnClientError(object sender, ClientErrorArgs e)
        {
            _minerManager.RemoveMiner(_minerManager.GetMiner(e.connection));
            _poolClient.CheckPoolConnection();
        }

        private void MinerServer_OnClientDisconnected(object sender, ClientDisonnectedArgs e)
        {
            Miner miner = _minerManager.GetMiner(e.connection);

            if (miner != null)
                _minerManager.RemoveMiner(miner);

            _poolClient.CheckPoolConnection();
        }
        
        public void ListenForMiners()
        {
            Log.Information("Starting MinerServer on " + port);
            minerServer.Start(port);
        }

        public void SendToPool(byte[] data)
        {
            Log.Debug("MinerServer SendToPool");
            _poolClient.SendToPool(data);
        }

        public void SendToPool(string data)
        {
            Log.Debug("MinerServer SendToPool");
            _poolClient.SendToPool(data.GetBytes());
        }

        public void SendToMiner(byte[] data, TcpConnection connection)
        {
            minerServer.Send(data, connection);
        }

        public void SendToMiner(string data, TcpConnection connection)
        {
            minerServer.Send(data.GetBytes(), connection);
        }

        public void BroadcastToMiners(byte[] data)
        {
            //Log.Debug("MinerServer broadcast");
            minerServer.BroadcastToMiners(data);
        }

        public void BroadcastToMiners(string data)
        {
            //Log.Debug("MinerServer broadcast");
            minerServer.BroadcastToMiners(data.GetBytes());
        }

        private void MinerServer_OnClientConnected(object sender, ClientConnectedArgs e)
        {
            _poolClient.CheckPoolConnection();
            Log.Information("{0} has connected!" , e.connection.endPoint.ToString());
            //_coinHandler.MinerConnected
        }

        private void MinerServer_OnClientDataReceived(object sender, ClientDataReceivedArgs e)
        {
            //Log.Information(Encoding.ASCII.GetString(e.Data));
            _coinHandler.MinerDataReceived(e.Data, e.connection);
        }
    }
}