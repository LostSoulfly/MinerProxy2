/* MinerProxy2 programmed by LostSoulfly.
   GNU General Public License v3.0 */

using MinerProxy2.Helpers;
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
        private const int BUFFER_SIZE = 4096;
        //private readonly byte[] buffer = new byte[BUFFER_SIZE];
        private readonly List<TcpConnection> clientSockets = new List<TcpConnection>();
        private bool isDisconnecting;
        private bool serverListening;
        private Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public int GetNumberOfConnections { get { return clientSockets.Count; } }

        public event EventHandler<ClientConnectedArgs> OnClientConnected;

        public event EventHandler<ClientDataReceivedArgs> OnClientDataReceived;

        public event EventHandler<ClientDisonnectedArgs> OnClientDisconnected;

        public event EventHandler<ClientErrorArgs> OnClientError;

        private void AcceptCallback(IAsyncResult AR)
        {
            if (!serverListening || isDisconnecting)
                return;

            Socket socket;

            try
            {
                socket = serverSocket.EndAccept(AR);
            }
            catch (ObjectDisposedException ex)
            {
                //Log.Debug(ex, "AcceptCallback Disposed Exception");
                return;
            }
            catch (SocketException ex)
            {
                //Log.Debug(ex, "SocketException Server AcceptCallback");
                return;
            }

            IPEndPoint endPoint = socket.RemoteEndPoint as IPEndPoint;
            TcpConnection tcpConnection = new TcpConnection(endPoint, socket, BUFFER_SIZE);

            clientSockets.Add(tcpConnection);
            OnClientConnected?.Invoke(this, new ClientConnectedArgs(tcpConnection));
            try
            {
                socket.BeginReceive(tcpConnection.buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, tcpConnection);
                //Log.Verbose("New miner connected, waiting for request.");

            } catch (ObjectDisposedException ex)
            {
                //Log.Error("BeginReceive Server", ex);
                Disconnect(tcpConnection);
            }
            serverSocket.BeginAccept(AcceptCallback, null);
        }

        private TcpConnection GetTcpConnection(Socket socket)
        {
            var tcpConnection = clientSockets.Find(x => x.socket.RemoteEndPoint == socket.RemoteEndPoint);
            if (tcpConnection != null) { return tcpConnection; }

            return null;
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            if (!serverListening || isDisconnecting)
                return;

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
                    OnClientError?.Invoke(this, new ClientErrorArgs(ex, tcpConnection));

                Disconnect(tcpConnection);
                return;
            }
            catch (ObjectDisposedException ex)
            {
                //Log.Debug(ex, "ObjectDisposedException Server ReceiveCallBack");

                OnClientError?.Invoke(this, new ClientErrorArgs(ex, tcpConnection));
                Disconnect(tcpConnection);
                return;
            }

            if (received == 0)
            {
                OnClientDisconnected?.Invoke(this, new ClientDisonnectedArgs(tcpConnection));
                Disconnect(tcpConnection);
                return;
            }

            byte[] packet = new byte[received];
            Array.Copy(tcpConnection.buffer, packet, packet.Length);

            List<byte[]> packets = Arrays.ProcessBuffer(ref tcpConnection.unusedBuffer, ref tcpConnection.unusedBufferLength, packet, packet.Length, "\n".GetBytes());

            for (int i = 0; i < packets.Count; i++)
            {
                OnClientDataReceived?.Invoke(this, new ClientDataReceivedArgs(packets[i], tcpConnection));
            }
            
            try
            {
                current.BeginReceive(tcpConnection.buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, tcpConnection);
            }
            catch (ObjectDisposedException ex)
            {
                OnClientError?.Invoke(this, new ClientErrorArgs(ex, tcpConnection));
                Disconnect(tcpConnection);
                //Log.Debug(ex, "Server ObjectDisposed BeginReceive Error");
            }
            catch (SocketException ex)
            {
                Disconnect(tcpConnection);
                OnClientError?.Invoke(this, new ClientErrorArgs(ex, tcpConnection));
                //Log.Debug(ex, "Server SocketException BeginReceive Error");
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            if (!serverListening || isDisconnecting)
                return;

            TcpConnection client = (TcpConnection)ar.AsyncState;
            try
            {
                // Complete sending the data to the remote device.
                int bytesSent = client.socket.EndSend(ar);
                //Log.Verbose("Sent {0} bytes to {1}", bytesSent, client.endPoint);
            }
            catch (Exception exception)
            {
                Disconnect(client);
                //Log.Error(exception, "Server SendCallback");
            }
        }

        public void BroadcastToMiners(byte[] data)
        {
            Log.Verbose("Broadcasting to all miners: {0}", data.GetString());

            clientSockets.ForEach<TcpConnection>(s => this.Send(data, s));
        }

        public void Disconnect(TcpConnection connection)
        {
            try
            {
                Log.Verbose("Disconnecting {0}", connection.endPoint);
                connection.socket.Shutdown(SocketShutdown.Both);
                connection.socket.Close();
            }
            catch { }
            finally { clientSockets.Remove(connection); }
            isDisconnecting = false;
        }

        public bool Send(byte[] data, TcpConnection connection)
        {
            if (!serverListening || isDisconnecting)
                return false;

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
                Disconnect(connection);
                //Log.Debug(ex, "ObjectDisposedException Server Send");
                OnClientError?.Invoke(this, new ClientErrorArgs(ex, connection));
                return false;
            }
            catch (SocketException ex)
            {
                Disconnect(connection);
                //Log.Debug(ex, "SocketException Server Send");
                OnClientError?.Invoke(this, new ClientErrorArgs(ex, connection));
                return false;
            }
        }

        /// <summary>
        /// Begin listening for new clients on port specified.
        /// </summary>
        public bool Start(int port)
        {
            try
            {
                serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
                serverListening = true;
                isDisconnecting = false;
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
        /// Close all connected client (we do not need to shutdown the server socket as its
        /// connections are already closed with the clients) and stop the server.
        /// </summary>
        public void Stop()
        {
            //clientSockets.ForEach<TcpConnection>(Disconnect);
            serverListening = false;
            isDisconnecting = true;
            for (int i = clientSockets.Count - 1; i >= 0; i--)
            {
                Disconnect(clientSockets[i]);
            }

            serverSocket.Close();
            isDisconnecting = false;
        }
    }
}