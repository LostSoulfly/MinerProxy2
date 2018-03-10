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

        public void BroadcastToMiners(byte[] data)
        {
            throw new NotImplementedException();
        }

        public void BroadcastToMiners(string data)
        {
            throw new NotImplementedException();
        }

        public void MinerConnected(TcpConnection connection)
        {
            throw new NotImplementedException();
        }

        public void MinerDataReceived(byte[] data, TcpConnection connection)
        {
            throw new NotImplementedException();
        }

        public void MinerDisconnected(TcpConnection connection)
        {
            throw new NotImplementedException();
        }

        public void MinerError(Exception exception, TcpConnection connection)
        {
            throw new NotImplementedException();
        }

        public void PrintMinerStats()
        {
            throw new NotImplementedException();
        }

        public void SendToMiner(byte[] data, TcpConnection connection)
        {
            throw new NotImplementedException();
        }

        public void SendToMiner(string data, TcpConnection connection)
        {
            throw new NotImplementedException();
        }

        public void SendToPool(byte[] data)
        {
            throw new NotImplementedException();
        }

        public void SetMinerManager(MinerManager minerManager)
        {
            throw new NotImplementedException();
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