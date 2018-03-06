using MinerProxy2.Network;
using MinerProxy2.Network.Sockets;
using System;

namespace MinerProxy2.Interfaces
{
    public interface ICoinHandlerMiner
    {
        void SetPool(PoolClient pool);

        void SetMinerServer(MinerServer minerServer);

        void SendToPool(byte[] data, TcpConnection connection);

        void BroadcastToMiners(byte[] data);

        void SendToMiner(byte[] data, TcpConnection connection);

        void SendToMiner(string data, TcpConnection connection);

        void MinerDataReceived(byte[] data, TcpConnection connection);

        void MinerDisconnected(TcpConnection connection);

        void MinerConnected(TcpConnection connection);

        void MinerError(Exception exception, TcpConnection connection);
    }
}