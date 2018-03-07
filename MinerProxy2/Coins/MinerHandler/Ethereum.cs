using MinerProxy2.Interfaces;
using MinerProxy2.Network;
using MinerProxy2.Network.Sockets;
using Serilog;
using System;
using System.Text;

namespace MinerProxy2.Coins.MinerHandler
{
    internal class Ethereum : ICoinHandlerMiner
    {
        private PoolClient _pool;
        private MinerServer _minerServer;

        public Ethereum()
        {
            Log.Information("Ethereum MinerHandler Initialized");
        }

        public void BroadcastToMiners(byte[] data)
        {
            throw new NotImplementedException();
        }

        public void MinerConnected(TcpConnection connection)
        {
            throw new NotImplementedException();
        }

        public void MinerDataReceived(byte[] data, TcpConnection connection)
        {
            Log.Debug(connection.endPoint.ToString() + ": " + Encoding.ASCII.GetString(data));
            _pool.SendToPool(data);
        }

        public void MinerDisconnected(TcpConnection connection)
        {
            Log.Information("Miner disconnected: " + connection.endPoint.ToString());
        }

        public void MinerError(Exception exception, TcpConnection connection)
        {
            throw new NotImplementedException();
        }

        public void SendToMiner(byte[] data, TcpConnection connection)
        {
            connection.socket.Send(data);
        }

        public void SendToMiner(string data, TcpConnection connection)
        {
            connection.socket.Send(Encoding.ASCII.GetBytes(data));
        }

        public void SendToPool(byte[] data, TcpConnection connection)
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