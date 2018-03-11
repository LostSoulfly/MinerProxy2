using MinerProxy2.Interfaces;
using MinerProxy2.Network;
using MinerProxy2.Network.Sockets;
using MinerProxy2.Pools;
using Serilog;
using System;
using System.Text;

namespace MinerProxy2.Coins
{
    public class EthereumPoolHandler : ICoinHandlerPool
    {
        private MinerServer _minerServer;
        private PoolClient _pool;
        private PoolManager _poolManager;
        private byte[] currentWork;

        public void BroadcastToMiners(byte[] data)
        {
            //This will probably be removed, I don't think it's necessary..
            _minerServer.BroadcastToMiners(data);
        }

        public void DoPoolLogin(PoolClient poolClient)
        {
            _pool.SendToPool(Encoding.ASCII.GetBytes("{\"worker\": \"" + "<<this pool instance name from _poolManager? Or poolInstance?>>" + "\", \"jsonrpc\": \"2.0\", \"params\": [\"0x0c0ff71b06413865fe9fE9a4C40396c136a62980\", \"x\"], \"id\": 2, \"method\": \"eth_submitLogin\"}\r\n"));

        }

        public void PoolConnected(PoolClient poolClient)
        {
            Log.Information("Pool connected: " + poolClient.poolEndPoint);
        }

        public void PoolDataReceived(byte[] data, PoolClient poolClient)
        {
            //process pool data here
            //such as pushing work to all miners
            //or letting miners know the share they submitted was accepted/rejected.
            Log.Information("Pool " + poolClient.poolEndPoint + " sent: " + Encoding.ASCII.GetString(data));

            //This is a simple recording of last data incase a new miner connects after the pool has connected
            //but the idea is to keep the current work to send to new miners going forward.
            currentWork = data;

            //This is just for testing with 1 miner.
            _minerServer.BroadcastToMiners(data);

        }

        public void PoolDisconnected(PoolClient poolClient)
        {
            Log.Information("Pool disconnected: " + poolClient.poolEndPoint);
        }

        public void PoolError(Exception exception, PoolClient poolClient)
        {
            Log.Error(exception, "Pool Error");
        }

        public void SendToMiner(byte[] data, TcpConnection connection)
        {
            _minerServer.SendToMiner(data, connection);
        }

        public void SendToPool(byte[] data)
        {
            _pool.SendToPool(data);
        }

        public void SetMinerServer(MinerServer minerServer)
        {
            _minerServer = minerServer;
        }

        public void SetPool(PoolClient pool)
        {
            _pool = pool;
        }

        public void SetPoolManager(PoolManager poolManager)
        {
            _poolManager = poolManager;
        }
    }
}