using MinerProxy2.Network.Connections;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace MinerProxy2.Network.Sockets
{
    public class Server
    {
        private readonly Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private readonly List<TcpConnection> clientSockets = new List<TcpConnection>();
        private const int BUFFER_SIZE = 2048;
        private readonly byte[] buffer = new byte[BUFFER_SIZE];

        public event EventHandler<ClientDataReceivedArgs> RaiseClientDataReceived;

        public event EventHandler<ClientErrorArgs> RaiseClientError;

        public event EventHandler<ClientConnectedArgs> RaiseClientConnected;

        public event EventHandler<ClientDisonnectedArgs> RaiseClientDisconnected;

        /// <summary>
        /// Begin listening for new clients on port specified.
        /// </summary>
        public bool Start(int port)
        {
            try
            {
                serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
                serverSocket.Listen(0);
                serverSocket.BeginAccept(AcceptCallback, null);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Server failed to start.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Close all connected client (we do not need to shutdown the server socket as its connections
        /// are already closed with the clients) and stop the server.
        /// </summary>
        public void Stop()
        {
            foreach (TcpConnection client in clientSockets)
            {
                client.socket.Shutdown(SocketShutdown.Both);
                client.socket.Close();
            }

            serverSocket.Close();
        }

        private void AcceptCallback(IAsyncResult AR)
        {
            Socket socket;

            try
            {
                socket = serverSocket.EndAccept(AR);
            }
            catch (ObjectDisposedException ex)
            {
                Log.Error(ex, "AcceptCallback Disposed Exception");
                return;
            }

            IPEndPoint endPoint = socket.RemoteEndPoint as IPEndPoint;
            TcpConnection tcpConnection = new TcpConnection(endPoint, socket);

            clientSockets.Add(tcpConnection);
            RaiseClientConnected?.Invoke(this, new ClientConnectedArgs(tcpConnection));
            socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, tcpConnection);
            //Log.Information("Client connected, waiting for request...");
            serverSocket.BeginAccept(AcceptCallback, null);
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            TcpConnection tcpConnection = (TcpConnection)AR.AsyncState;
            Socket current = tcpConnection.socket;
            //TcpConnection tcpConnection = GetTcpConnection(current);
            int received;

            try
            {
                received = current.EndReceive(AR);
            }
            catch (SocketException ex)
            {
                //Log.Information(ex, "Client forcefully disconnected");
                // Don't shutdown because the socket may be disposed and its disconnected anyway.
                RaiseClientDisconnected?.Invoke(this, new ClientDisonnectedArgs(tcpConnection));
                current.Close();
                clientSockets.Remove(tcpConnection);
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(buffer, recBuf, received);

            RaiseClientDataReceived?.Invoke(this, new ClientDataReceivedArgs(recBuf, tcpConnection));

            try
            {
                current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, tcpConnection);
            }
            catch (Exception ex)
            {
                RaiseClientError?.Invoke(this, new ClientErrorArgs(ex, tcpConnection));
                Log.Error(ex, "BeginReceive Error");
            }
        }
        
        public void BroadcastToMiners(byte[] data)
        {
            Log.Debug("Server instance broadcasting data to all miners..");
            foreach (TcpConnection connection in clientSockets)
            {
                connection.socket.Send(data);
            }
        }

        private TcpConnection GetTcpConnection(Socket socket)
        {
            var tcpConnection = clientSockets.Find(x => x.socket.RemoteEndPoint == socket.RemoteEndPoint);
            if (tcpConnection != null) { return tcpConnection; }

            return null;
        }
    }
}