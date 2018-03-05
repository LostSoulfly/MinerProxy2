using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using MinerProxy2.Network.Sockets;

namespace MinerProxy2.Interfaces
{
    public interface ICoinHandlerMinerServer
    {
        
        void SendToPool();
        void MinerDataReceived(byte[] data, TcpConnection socket);
        void MinerDisconnected(Socket socket);
        void MinerConnected(Socket socket);
        void MinerError(Exception exception, Socket socket);
    }
}
