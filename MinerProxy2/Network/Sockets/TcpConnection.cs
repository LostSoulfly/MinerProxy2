using System.Net;
using System.Net.Sockets;

namespace MinerProxy2.Network.Sockets
{
    public class TcpConnection
    {
        public int uniqueId;
        public readonly IPEndPoint endPoint;
        public readonly Socket socket;

        public TcpConnection(IPEndPoint endPoint, Socket socket)
        {
            this.endPoint = endPoint;
            this.socket = socket;
        }
    }
}