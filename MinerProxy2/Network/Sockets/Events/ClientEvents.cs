/* MinerProxy2 programmed by LostSoulfly.
   GNU General Public License v3.0 */

using System;
using System.Net.Sockets;

namespace MinerProxy2.Network.Sockets
{
    public class ServerConnectedArgs : EventArgs
    {
        private Socket _socket;

        public Socket socket { get { return _socket; } }

        public ServerConnectedArgs(Socket socket)
        {
            _socket = socket;
        }
    }

    public class ServerDataReceivedArgs : EventArgs
    {
        private byte[] _data;

        private Socket _socket;

        public byte[] Data { get { return _data; } }

        public Socket socket { get { return _socket; } }

        public ServerDataReceivedArgs(byte[] data, Socket socket)
        {
            _data = data;
            _socket = socket;
        }
    }

    public class ServerDisonnectedArgs : EventArgs
    {
        private Socket _socket;

        public Socket socket { get { return _socket; } }

        public ServerDisonnectedArgs(Socket socket)
        {
            _socket = socket;
        }
    }

    public class ServerErrorArgs : EventArgs
    {
        private Exception _exception;

        private Socket _socket;

        public Exception exception { get { return _exception; } }

        public Socket socket { get { return _socket; } }

        public ServerErrorArgs(Exception exception, Socket socket)
        {
            _socket = socket;
            _exception = exception;
        }
    }
}