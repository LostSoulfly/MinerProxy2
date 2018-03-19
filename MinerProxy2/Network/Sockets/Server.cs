using MinerProxy2.Helpers;
using MinerProxy2.Network.Connections;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

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

        public int GetNumberOfConnections { get { return clientSockets.Count; } }

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
            clientSockets.ForEach<TcpConnection>(Disconnect);

            serverSocket.Close();
        }

        public void Disconnect(TcpConnection connection)
        {
            try
            {
                Log.Verbose("Disconnecting {0}", connection.endPoint);
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
                Log.Debug(ex, "AcceptCallback Disposed Exception");
                return;
            }
            catch (SocketException ex)
            {
                Log.Debug(ex, "SocketException Server AcceptCallback");
                return;
            }

            IPEndPoint endPoint = socket.RemoteEndPoint as IPEndPoint;
            TcpConnection tcpConnection = new TcpConnection(endPoint, socket);

            clientSockets.Add(tcpConnection);
            OnClientConnected?.Invoke(this, new ClientConnectedArgs(tcpConnection));
            socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, tcpConnection);
            Log.Verbose("New miner connected, waiting for request.");
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
                if (ex.ErrorCode != 10054)
                    Log.Debug(ex, "Client forcefully disconnected {1}: {0}", tcpConnection.endPoint, ex.ErrorCode);

                OnClientError?.Invoke(this, new ClientErrorArgs(ex, tcpConnection));
                Disconnect(tcpConnection);
                return;
            }
            catch (ObjectDisposedException ex)
            {
                Log.Debug(ex, "ObjectDisposedException Server ReceiveCallBack");

                OnClientError?.Invoke(this, new ClientErrorArgs(ex, tcpConnection));
                Disconnect(tcpConnection);
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(buffer, recBuf, received);

            if (recBuf.Length == 0)
            {
                OnClientDisconnected?.Invoke(this, new ClientDisonnectedArgs(tcpConnection));
                Disconnect(tcpConnection);
                return;
            }
            
            OnClientDataReceived?.Invoke(this, new ClientDataReceivedArgs(recBuf, tcpConnection));

            try
            {
                current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, tcpConnection);
            }
            catch (ObjectDisposedException ex)
            {
                OnClientError?.Invoke(this, new ClientErrorArgs(ex, tcpConnection));
                Log.Debug(ex, "Server ObjectDisposed BeginReceive Error");
                Disconnect(tcpConnection);
            }
            catch (SocketException ex)
            {
                OnClientError?.Invoke(this, new ClientErrorArgs(ex, tcpConnection));
                Log.Debug(ex, "Server SocketException BeginReceive Error");
                Disconnect(tcpConnection);
            }
        }

        public void BroadcastToMiners(byte[] data)
        {
            Log.Verbose("Broadcasting to all miners: {0}", data.GetString());

            clientSockets.ForEach<TcpConnection>(s => this.Send(data, s));
            
        }

        public bool Send(byte[] data, TcpConnection connection)
        {
            Log.Verbose("Sending {0}: {1}", connection.endPoint, data.GetString());

            try
            {
                data = data.CheckForNewLine();

                connection.socket.BeginSend(data, 0, data.Length, SocketFlags.None,
                    new AsyncCallback(SendCallback), connection);
                return true;
            }
            catch (ObjectDisposedException ex)
            {
                Log.Debug(ex, "ObjectDisposedException Server Send");
                OnClientError?.Invoke(this, new ClientErrorArgs(ex, connection));
                Disconnect(connection);
                return false;
            }
            catch (SocketException ex)
            {
                Log.Debug(ex, "SocketException Server Send");
                OnClientError?.Invoke(this, new ClientErrorArgs(ex, connection));
                Disconnect(connection);
                return false;
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            TcpConnection client = (TcpConnection)ar.AsyncState;
            try
            {
                
                // Complete sending the data to the remote device.
                int bytesSent = client.socket.EndSend(ar);
                Log.Verbose("Sent {0} bytes to {1}", bytesSent, client.endPoint);
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Server SendCallback");
                Disconnect(client);
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