using System;
using System.Collections.Generic;
using System.Text;

namespace MinerProxy2.Network.Sockets
{
    public class ServerConnectedArgs : EventArgs
    {
        public ServerConnectedArgs()
        {
        }
    }

    public class ServerDisconnecteddArgs : EventArgs
    {
        public ServerDisconnecteddArgs()
        {
        }
    }

    public class ServerDataReceivedArgs : EventArgs
    {
        public ServerDataReceivedArgs(byte[] data)
        {
            _data = data;
        }
        private byte[] _data;
        public byte[] Data { get { return _data; } }
    }

    public class ServerErrorArgs : EventArgs
    {
        public ServerErrorArgs(string error)
        {
            _error = error;
        }
        private string _error;
        public string Error { get { return _error; } }
    }
}
