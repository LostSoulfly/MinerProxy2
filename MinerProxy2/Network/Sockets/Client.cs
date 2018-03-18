using MinerProxy2.Helpers;
using Serilog;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace MinerProxy2.Network.Sockets
{
    public class Client
    {
        private Socket clientSocket;

        private int BUFFER_SIZE = 2048;
        private byte[] buffer;

        private string host;
        private int port;
        private bool clientConnected;
        private bool isDisconnecting;

        public event EventHandler<ServerDataReceivedArgs> OnServerDataReceived;

        public event EventHandler<ServerErrorArgs> OnServerError;

        public event EventHandler<ServerConnectedArgs> OnServerConnected;

        public event EventHandler<ServerDisonnectedArgs> OnServerDisconnected;

        private readonly object dataLock = new object();

        public Client(string host, int port, int bufferSize = 2048)
        {
            this.host = host;
            this.port = port;
            this.BUFFER_SIZE = bufferSize;
            
            //initialize the buffer
            buffer = new byte[BUFFER_SIZE];
        }

        public void Connect()
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            clientSocket.BeginConnect(host, port,
                new AsyncCallback(ConnectCallback), clientSocket);
        }

        public void Close()
        {
            try
            {
                isDisconnecting = true;
                clientConnected = false;
                Log.Verbose("Client Close()");
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            } catch { }

            isDisconnecting = false;
        }

        public void Reconnect()
        {
            if (isDisconnecting)
                return;

            Close();
            Log.Warning("Reconnecting to pool..");
            Connect();
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket socket = (Socket)ar.AsyncState;

                // Complete the connection.
                socket.EndConnect(ar);
                clientConnected = true;
                Log.Verbose("Socket connected to {0}", socket.RemoteEndPoint.ToString());

                OnServerConnected?.Invoke(this, new ServerConnectedArgs(socket));

                socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);
            }
            catch (ObjectDisposedException exception)
            {
                Log.Error(exception, "ConnectCallback");
            }
        }
        
        public void SendToPool(byte[] data)
        {
            if (!clientConnected)
                return;

            Log.Verbose("Client SendToPool: {0}", data.GetString());
            // Begin sending the data to the remote device.
            try
            {
                data = data.CheckForNewLine();

                this.clientSocket.BeginSend(data, 0, data.Length, SocketFlags.None,
                    new AsyncCallback(SendCallback), clientSocket);
            } catch (Exception ex)
            {
                Log.Error(ex, "SendToPool");
                Reconnect();
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
                Log.Error(exception, "Client SendCallback");
                Reconnect();
            }
        }

        private void Receive(Socket socket)
        {
            if (isDisconnecting)
                return;

            try
            {
                // Begin receiving the data from the remote device.
                    socket.BeginReceive(buffer, 0, BUFFER_SIZE, 0,
                        new AsyncCallback(ReceiveCallback), socket);
            }
            catch (Exception exception)
            {
                if (!isDisconnecting)
                {
                    Log.Error(exception, "Receive");
                    OnServerError?.Invoke(this, new ServerErrorArgs(exception, socket));
                    Reconnect();
                }
            }
        }

        private void ReceiveCallback(IAsyncResult AR)
        {

            if (isDisconnecting)
                return;

            Socket socket = (Socket)AR.AsyncState;
            int received;
            try
            {
                received = socket.EndReceive(AR);
            }
            catch (ObjectDisposedException ex)
            {
                //Log.Error(ex, "Client forcefully disconnected");
                // Don't shutdown because the socket may be disposed and its disconnected anyway.
                OnServerDisconnected?.Invoke(this, new ServerDisonnectedArgs(socket));
                Reconnect();
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(buffer, recBuf, received);

            if (recBuf.Length == 0)
            {
                if (!isDisconnecting)
                {
                    Log.Error("Pool receive buffer was empty; need to reconnect.. " + received);
                    Reconnect();
                    return;
                } else { return; }
            }

            OnServerDataReceived?.Invoke(this, new ServerDataReceivedArgs(recBuf, socket));

            try
            {
                socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);
            }
            catch (ObjectDisposedException ex)
            {
                if (!isDisconnecting)
                {
                    OnServerError?.Invoke(this, new ServerErrorArgs(ex, socket));
                    Log.Error(ex, "Pool BeginReceive Error");
                    Reconnect();
                } else { return; }
            }
        }
    }
}