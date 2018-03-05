using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using MinerProxy2.Network.Sockets;

namespace MinerProxy2.Network.Connections
{
    public class DataReceivedArgs : EventArgs
    {
        public DataReceivedArgs(byte[] data, TcpConnection connection)
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
