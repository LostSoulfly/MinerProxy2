/* MinerProxy2 programmed by LostSoulfly.
   GNU General Public License v3.0 */

using MinerProxy2.Network.Sockets;
using System;

namespace MinerProxy2.Network.Connections
{
    public class ClientConnectedArgs : EventArgs
    {
        private TcpConnection _connection;

        public TcpConnection connection { get { return _connection; } }

        public ClientConnectedArgs(TcpConnection connection)
        {
            _connection = connection;
        }
    }

    public class ClientDataReceivedArgs : EventArgs
    {
        private TcpConnection _connection;

        private byte[] _data;

        public TcpConnection connection { get { return _connection; } }

        public byte[] Data { get { return _data; } }

        public ClientDataReceivedArgs(byte[] data, TcpConnection connection)
        {
            _data = data;
            _connection = connection;
        }
    }

    public class ClientDisonnectedArgs : EventArgs
    {
        private TcpConnection _connection;

        public TcpConnection connection { get { return _connection; } }

        public ClientDisonnectedArgs(TcpConnection connection)
        {
            _connection = connection;
        }
    }

    public class ClientErrorArgs : EventArgs
    {
        private TcpConnection _connection;

        private Exception _exception;

        public TcpConnection connection { get { return _connection; } }

        public Exception exception { get { return _exception; } }

        public ClientErrorArgs(Exception exception, TcpConnection connection)
        {
            _connection = connection;
            _exception = exception;
        }
    }
}