using MinerProxy2.Network.Sockets;
using System;

namespace MinerProxy2.Network.Connections
{
    public class ClientDataReceivedArgs : EventArgs
    {
        public ClientDataReceivedArgs(byte[] data, TcpConnection connection)
        {
            _data = data;
            _connection = connection;
        }

        private byte[] _data;
        private TcpConnection _connection;
        public byte[] Data { get { return _data; } }
        public TcpConnection connection { get { return _connection; } }
    }

    public class ClientConnectedArgs : EventArgs
    {
        public ClientConnectedArgs(TcpConnection connection)
        {
            _connection = connection;
        }

        private TcpConnection _connection;
        public TcpConnection connection { get { return _connection; } }
    }

    public class ClientDisonnectedArgs : EventArgs
    {
        public ClientDisonnectedArgs(TcpConnection connection)
        {
            _connection = connection;
        }

        private TcpConnection _connection;
        public TcpConnection connection { get { return _connection; } }
    }

    public class ClientErrorArgs : EventArgs
    {
        public ClientErrorArgs(Exception exception, TcpConnection connection)
        {
            _connection = connection;
            _exception = exception;
        }

        private TcpConnection _connection;
        private Exception _exception;
        public TcpConnection connection { get { return _connection; } }
        public Exception exception { get { return _exception; } }
    }
}