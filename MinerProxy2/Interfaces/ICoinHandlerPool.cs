using MinerProxy2.Network.Sockets;
using System;
using System.Collections.Generic;
using System.Text;


namespace MinerProxy2.Interfaces
{
    public interface ICoinHandlerPool
    {
        bool SetupComplete();
        void InitializeHandler();
        void BroadcastToMiners(byte[] data);
        void SendToMiner(byte[] data, TcpConnection connection);
        void SendToPool(byte[] data, TcpConnection connection);
        void MinerDataReceived(byte[] data, TcpConnection connection);
        void MinerDisconnected(TcpConnection socket);
        void MinerConnected(TcpConnection socket);
        void MinerError(Exception exception, TcpConnection connection);

    }
}
