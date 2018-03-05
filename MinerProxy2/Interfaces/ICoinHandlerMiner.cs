using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using MinerProxy2.Network.Sockets;

namespace MinerProxy2.Interfaces
{
    public interface ICoinHandlerMiner
    {
        void SendToPool(byte[] data, TcpConnection connection);
        void MinerDataReceived(byte[] data, TcpConnection connection);
        void MinerDisconnected(TcpConnection socket);
        void MinerConnected(TcpConnection socket);
        void MinerError(Exception exception, TcpConnection connection);
    }
}
