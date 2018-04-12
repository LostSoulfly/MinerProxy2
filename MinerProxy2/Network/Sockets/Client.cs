/* MinerProxy2 programmed by LostSoulfly.
   GNU General Public License v3.0 */

using MinerProxy2.Helpers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace MinerProxy2.Network.Sockets
{
    public class Client
    {
        private readonly object dataLock = new object();
        private byte[] buffer;
        private byte[] unusedBuffer;
        private int unusedBufferLength;
        private readonly object bufferLock = new object();
        private int BUFFER_SIZE = 4096;
        private bool clientConnected;
        private Socket clientSocket;
        public string host;
        private bool isDisconnecting;
        public int port;

        public event EventHandler<ServerConnectedArgs> OnServerConnected;

        public event EventHandler<ServerDataReceivedArgs> OnServerDataReceived;

        public event EventHandler<ServerDisonnectedArgs> OnServerDisconnected;

        public event EventHandler<ServerErrorArgs> OnServerError;

        public Client(string host, int port, int bufferSize = 4096)
        {
            this.BUFFER_SIZE = bufferSize;
            this.host = host;
            this.port = port;
            //initialize the buffer
            buffer = new byte[BUFFER_SIZE];
        }

        private void ConnectCallback(IAsyncResult ar)
        {

            Socket socket = (Socket)ar.AsyncState;
            try
            {
                // Complete the connection.
                socket.EndConnect(ar);
                clientConnected = true;
                Log.Verbose("Socket connected to {0}", socket.RemoteEndPoint.ToString());

                OnServerConnected?.Invoke(this, new ServerConnectedArgs(socket));

                socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);
            }
            catch (ObjectDisposedException exception)
            {
                Log.Verbose(exception, "ConnectCallback");
            }
            catch (SocketException socketException)
            {
                Log.Verbose("Pool connection error", socketException);
                OnServerError?.Invoke(this, new ServerErrorArgs(socketException, socket));
            }
        }

        private void Receive(Socket socket)
        {
            if (!clientConnected || isDisconnecting || !this.clientSocket.Connected)
                return;

            try
            {
                // Begin receiving the data from the remote device.
                socket.BeginReceive(buffer, 0, BUFFER_SIZE, 0,
                    new AsyncCallback(ReceiveCallback), socket);
            }
            catch (Exception exception)
            {
                if (!clientConnected || isDisconnecting || !this.clientSocket.Connected)
                {
                    Log.Error(exception, "Receive");
                    OnServerError?.Invoke(this, new ServerErrorArgs(exception, socket));
                }
            }
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            if (!clientConnected || isDisconnecting || !this.clientSocket.Connected)
                return;

            Socket socket = (Socket)AR.AsyncState;
            int received;
            try
            {
                received = socket.EndReceive(AR);
            }
            catch (ObjectDisposedException ex)
            {
                if (!clientConnected || isDisconnecting || !this.clientSocket.Connected)
                    return;

                Log.Debug(ex, "Client ObjectDisposed exception");
                OnServerDisconnected?.Invoke(this, new ServerDisonnectedArgs(socket));

                return;
            }
            catch (SocketException ex)
            {
                Log.Debug(ex, "Client forcefully disconnected");
                OnServerDisconnected?.Invoke(this, new ServerDisonnectedArgs(socket));

                return;
            }
            
            if (received == 0)
            {
                if (!clientConnected || isDisconnecting || !this.clientSocket.Connected)
                {
                    Log.Verbose("Pool receive buffer was empty; need to reconnect.. " + received);
                    OnServerDisconnected?.Invoke(this, new ServerDisonnectedArgs(socket));

                    return;
                }
                else { return; }
            }

            lock (bufferLock)
            {
                byte[] packet = new byte[received];
                Array.Copy(buffer, packet, packet.Length);

                List<byte[]> packets = Arrays.ProcessBuffer(ref unusedBuffer, ref unusedBufferLength, buffer, received, "\n".GetBytes());

                for (int i = 0; i < packets.Count; i++)
                {
                    OnServerDataReceived?.Invoke(this, new ServerDataReceivedArgs(packets[i], socket));
                }
            }

            try
            {
                if (!clientConnected || isDisconnecting || !this.clientSocket.Connected)
                    return;
                socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);
            }
            catch (ObjectDisposedException ex)
            {
                if (!clientConnected || isDisconnecting || !this.clientSocket.Connected)
                {
                    if (socket == null)
                        OnServerError?.Invoke(this, new ServerErrorArgs(ex, socket));
                    else
                        OnServerError?.Invoke(this, new ServerErrorArgs(ex, null));
                    Log.Error(ex, "Pool BeginReceive Error");
                }
                else { return; }
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                Log.Verbose("Sent {0} bytes to {1}.", bytesSent, client.RemoteEndPoint.ToString());
            }
            catch (ObjectDisposedException exception)
            {
                if (!clientConnected || isDisconnecting || !this.clientSocket.Connected)
                    return;

                Log.Error(exception, "Client SendCallback");
            }
        }

        public void Close()
        {
            isDisconnecting = true;
            clientConnected = false;
            Log.Verbose("Client Close()");
            try
            {
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
            catch { }

            isDisconnecting = false;
        }

        public void Connect()
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.BeginConnect(host, port,
                new AsyncCallback(ConnectCallback), clientSocket);
        }

        public void Connect(string host, int port)
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.host = host;
            this.port = port;
            clientSocket.BeginConnect(host, port,
                new AsyncCallback(ConnectCallback), clientSocket);
        }

        public void Reconnect()
        {
            if (!clientConnected || isDisconnecting || !this.clientSocket.Connected)
                return;

            Close();
            Log.Warning("Reconnecting to pool..");
            Connect();
        }

        public void SendToPool(byte[] data)
        {
            if (!clientConnected || isDisconnecting || !this.clientSocket.Connected)
                return;

            Log.Verbose("Client SendToPool: {0}", data.GetString());
            // Begin sending the data to the remote device.
            try
            {
                data = data.CheckForNewLine();

                this.clientSocket.BeginSend(data, 0, data.Length, SocketFlags.None,
                    new AsyncCallback(SendCallback), clientSocket);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "SendToPool");
            }
        }
    }
}