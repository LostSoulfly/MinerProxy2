using System;
using System.Collections.Generic;
using System.Text;
using MinerProxy2.Network;
using MinerProxy2.Network.Sockets;
using System.Net;
using System.Net.Sockets;

namespace MinerProxy2.Network.Sockets
{
    public class ServerDataReceivedArgs : EventArgs
    {
        public ServerDataReceivedArgs(byte[] data, Socket socket)
        {
            _data = data;
            _socket = socket;
        }
        private byte[] _data;
        private Socket _socket;
        public byte[] Data { get { return _data; } }
        public Socket socket { get { return _socket; } }
    }

    public class ServerConnectedArgs : EventArgs
    {
        public ServerConnectedArgs(Socket socket)
        {
            _socket = socket;
        }
        private Socket _socket;
        public Socket socket { get { return _socket; } }
    }

    public class ServerDisonnectedArgs : EventArgs
    {
        public ServerDisonnectedArgs(Socket socket)
        {
            _socket = socket;
        }
        private Socket _socket;
        public Socket socket { get { return _socket; } }
    }

    public class ServerErrorArgs : EventArgs
    {
        public ServerErrorArgs(Exception exception, Socket socket)
        {
            _socket = socket;
            _exception = exception;
        }
        private Socket _socket;
        private Exception _exception;
        public Socket socket { get { return _socket; } }
        public Exception exception { get { return _exception; } }
    }
}
