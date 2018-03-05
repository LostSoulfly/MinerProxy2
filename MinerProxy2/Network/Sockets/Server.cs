using MinerProxy2.Network.Connections;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MinerProxy2.Network.Sockets
{
    

    class Server
    {
        private readonly Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private readonly List<TcpConnection> clientSockets = new List<TcpConnection>();
        private const int BUFFER_SIZE = 2048;
        private readonly byte[] buffer = new byte[BUFFER_SIZE];
        
        public event EventHandler<DataReceivedArgs> RaiseDataReceived;

        public event EventHandler<ServerErrorArgs> RaiseServerError;
        
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
            } catch (Exception ex)
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
            
            clientSockets.Add(new TcpConnection(socket.RemoteEndPoint, socket);
            socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);
            Log.Information("Client connected, waiting for request...");
            serverSocket.BeginAccept(AcceptCallback, null);
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;
            int received;

            try
            {
                received = current.EndReceive(AR);
            }
            catch (SocketException ex)
            {
                Log.Information(ex, "Client forcefully disconnected");
                // Don't shutdown because the socket may be disposed and its disconnected anyway.
                current.Close();
                clientSockets.Remove(current);
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(buffer, recBuf, received);
            
            RaiseDataReceived?.Invoke(this, new DataReceivedArgs(recBuf, current));

            try
            {
                current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
            }
            catch ( Exception ex )  { Log.Error(ex, "BeginReceive Error"); }
        }

    }
}
