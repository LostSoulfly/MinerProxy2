/* MinerProxy2 programmed by LostSoulfly.
   GNU General Public License v3.0 */

using System.Net;
using System.Net.Sockets;

namespace MinerProxy2.Network.Sockets
{
    public class TcpConnection
    {
        public readonly IPEndPoint endPoint;
        public readonly Socket socket;
        public int uniqueId;

        public TcpConnection(IPEndPoint endPoint, Socket socket)
        {
            this.endPoint = endPoint;
            this.socket = socket;
        }
    }
}