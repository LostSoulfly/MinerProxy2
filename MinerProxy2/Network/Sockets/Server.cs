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
        private Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private readonly List<TcpConnection> clientSockets = new List<TcpConnection>();
        private const int BUFFER_SIZE = 2048;
        private readonly byte[] buffer = new byte[BUFFER_SIZE];

        public event EventHandler<ClientDataReceivedArgs> OnClientDataReceived;

        public event EventHandler<ClientErrorArgs> OnClientError;

        public event EventHandler<ClientConnectedArgs> OnClientConnected;

        public event EventHandler<ClientDisonnectedArgs> OnClientDisconnected;

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

        public void Disconnect(TcpConnection connection)
        {
            try
            {
                connection.socket.Shutdown(SocketShutdown.Both);
                connection.socket.Close();
            }
            catch { } finally { clientSockets.Remove(connection); }
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
            OnClientConnected?.Invoke(this, new ClientConnectedArgs(tcpConnection));
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
                OnClientDisconnected?.Invoke(this, new ClientDisonnectedArgs(tcpConnection));
                Disconnect(tcpConnection);
                Log.Error(ex, "Server ReceiveCallback");
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(buffer, recBuf, received);

            OnClientDataReceived?.Invoke(this, new ClientDataReceivedArgs(recBuf, tcpConnection));

            try
            {
                current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, tcpConnection);
            }
            catch (Exception ex)
            {
                OnClientError?.Invoke(this, new ClientErrorArgs(ex, tcpConnection));
                Log.Error(ex, "BeginReceive Error");
                Disconnect(tcpConnection);
            }
        }

        public void BroadcastToMiners(byte[] data)
        {
            Log.Debug("Server instance broadcasting data to all miners..");
            
            foreach (TcpConnection connection in clientSockets)
            { 
                try
                { 
                    connection.socket.Send(data);
                }
                catch (Exception ex)
                {
                Log.Error(ex, "BroadcastToMiners");
                Disconnect(connection);
                //todo disconnect/remove client
            }
        }
            
        }

        public bool Send(byte[] data, TcpConnection connection)
        {
            try
            {
                connection.socket.Send(data);
                return true;
            }
            catch (Exception ex)
            {
                //Remove miner?
                Log.Error(ex, "Send");
                Disconnect(connection);
                return false;
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