using MinerProxy2.Interfaces;
using MinerProxy2.Miners;
using MinerProxy2.Network;
using MinerProxy2.Network.Sockets;
using Serilog;
using System;
using System.Text;

namespace MinerProxy2.Coins.MinerHandler
{
    internal class EthereumMinerHandler : ICoinHandlerMiner
    {
        private PoolClient _pool;
        private MinerServer _minerServer;
        private MinerManager _minerManager;

        public void BroadcastToMiners(byte[] data)
        {
            _minerServer.BroadcastToMiners(data);
        }

        public void BroadcastToMiners(string data)
        {
            _minerServer.BroadcastToMiners(Encoding.ASCII.GetBytes(data));
        }

        public void MinerConnected(TcpConnection connection)
        {
            Log.Information("Miner connected: " + connection.endPoint);
           //_minerManager.AddMiner();
        }

        public void MinerDataReceived(byte[] data, TcpConnection connection)
        {
            //process the data here, such as replacing the wallet and then submitting shares to _pool
            Log.Information("Sending to pool from " + connection.endPoint + ": " + Encoding.ASCII.GetString(data));
            _pool.SendToPool(data);
        }

        public void MinerDisconnected(TcpConnection connection)
        {
            Log.Information("Miner disconnected: " + connection.endPoint);
            //_minerManager.RemoveMiner(connection);
        }

        public void MinerError(Exception exception, TcpConnection connection)
        {
            Log.Error(exception, "Miner Error");
        }

        public void PrintMinerStats()
        {
            Log.Information("Miner Stats");
        }

        public void SendToMiner(byte[] data, TcpConnection connection)
        {
            _minerServer.SendToMiner(data, connection);
        }

        public void SendToMiner(string data, TcpConnection connection)
        {
            _minerServer.SendToMiner(Encoding.ASCII.GetBytes(data), connection);
        }

        public void SendToPool(byte[] data)
        {
            _pool.SendToPool(data);
        }

        public void SetMinerManager(MinerManager minerManager)
        {
            _minerManager = minerManager;
        }

        public void SetMinerServer(MinerServer minerServer)
        {
            _minerServer = minerServer;
        }

        public void SetPool(PoolClient pool)
        {
            _pool = pool;
        }
    }
}