using MinerProxy2.Interfaces;
using MinerProxy2.Miners;
using MinerProxy2.Network.Connections;
using MinerProxy2.Network.Sockets;
using Serilog;

namespace MinerProxy2.Network
{
    public class MinerServer
    {
        private readonly Server minerServer;
        public ICoinHandlerMiner _coinHandler;
        private readonly PoolClient _pool;
        private MinerManager _minerManager;

        private int port;

        public MinerServer(int port, PoolClient pool, MinerManager minerManager, ICoinHandlerMiner coinHandler)
        {
            Log.Information("MinerServer initialized: " + pool.poolEndPoint);

            this._minerManager = minerManager;

            _pool = pool;
            _coinHandler = coinHandler;
            coinHandler.SetMinerServer(this);
            coinHandler.SetPoolClient(_pool);


            this.port = port;
            minerServer = new Server();
            minerServer.OnClientDataReceived += MinerServer_OnClientDataReceived;
            minerServer.OnClientConnected += MinerServer_OnClientConnected;
            minerServer.OnClientDisconnected += MinerServer_OnClientDisconnected;
        }

        private void MinerServer_OnClientDisconnected(object sender, ClientDisonnectedArgs e)
        {
            Miner miner = _minerManager.GetMiner(e.connection);

            if (miner != null)
                _minerManager.RemoveMiner(miner);
        }

        public void ListenForMiners()
        {
            Log.Information("Starting MinerServer on " + port);
            minerServer.Start(port);
        }

        public void SendToPool(byte[] data)
        {
            Log.Debug("MinerServer SendToPool");
            _pool.SendToPool(data);
        }

        public void SendToMiner(byte[] data, TcpConnection connection)
        {
            minerServer.Send(data, connection);
        }

        public void BroadcastToMiners(byte[] data)
        {
            //Log.Debug("MinerServer broadcast");
            minerServer.BroadcastToMiners(data);
        }

        private void MinerServer_OnClientConnected(object sender, ClientConnectedArgs e)
        {
            Log.Information(e.connection.endPoint.ToString() + " has connected!");
            //_coinHandler.MinerConnected
        }

        private void MinerServer_OnClientDataReceived(object sender, ClientDataReceivedArgs e)
        {
            //Log.Information(Encoding.ASCII.GetString(e.Data));
            _coinHandler.MinerDataReceived(e.Data, e.connection);
        }
    }
}