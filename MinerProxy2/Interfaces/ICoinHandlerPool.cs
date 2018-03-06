using MinerProxy2.Network;
using MinerProxy2.Network.Sockets;
using System;

namespace MinerProxy2.Interfaces
{
    public interface ICoinHandlerPool
    {
        void SetPool(PoolClient pool);

        void SetMinerServer(MinerServer minerServer);

        void BroadcastToMiners(byte[] data);

        void SendToMiner(byte[] data, TcpConnection connection);

        void SendToPool(byte[] data, TcpConnection connection);

        void MinerDataReceived(byte[] data, TcpConnection connection);

        void MinerDisconnected(TcpConnection socket);

        void MinerConnected(TcpConnection socket);

        void MinerError(Exception exception, TcpConnection connection);
    }
}