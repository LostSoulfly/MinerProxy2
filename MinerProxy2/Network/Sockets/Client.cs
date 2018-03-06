using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Serilog;
using MinerProxy2.Network;
using MinerProxy2.Network.Sockets;
using MinerProxy2.Network.Connections;

namespace MinerProxy2.Network.Sockets
{
    public class Client
    {
        private readonly Socket clientSocket = new Socket
               (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private const int BUFFER_SIZE = 2048;

        public event EventHandler<ServerDataReceivedArgs> RaiseServerDataReceived;
        public event EventHandler<ServerErrorArgs> RaiseServerError;
        public event EventHandler<ServerConnectedArgs> RaiseServerConnected;
        public event EventHandler<ServerDisonnectedArgs> RaiseServerDisconnected;


        //todo https://docs.microsoft.com/en-us/dotnet/framework/network-programming/asynchronous-client-socket-example

        public void Connect(string host, int port)
        {
            int attempts = 0;

            while (!clientSocket.Connected)
            {
                try
                {
                    attempts++;
                    // Change IPAddress.Loopback to a remote IP to connect to a remote host.
                    clientSocket.Connect(host, port);
                    System.Threading.Thread.Sleep(100);
                }
                catch (SocketException exception)
                {
                    Log.Fatal(exception, "Client Connect");
                }
            }
            RaiseServerConnected?.Invoke(this, new ServerConnectedArgs(clientSocket));
            RequestLoop();
        }

        public void RequestLoop()
        {
            
            while (true)
            {
                var buffer = new byte[BUFFER_SIZE];
                int received = clientSocket.Receive(buffer, SocketFlags.None);
                if (received == 0) return;
                var data = new byte[received];
                Array.Copy(buffer, data, received);

                RaiseServerDataReceived?.Invoke(this, new ServerDataReceivedArgs(data, clientSocket));

                Log.Debug("Pool sent: " + Encoding.ASCII.GetString(data));
                System.Threading.Thread.Sleep(10);
            }
        }

        /// <summary>
        /// Close socket
        /// </summary>
        public void Disconnect()
        {
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }
        
        /// <summary>
        /// Sends a string to the server with ASCII encoding.
        /// </summary>
        public void SendString(string text)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(text);
            clientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        public void Send(byte[] data)
        {
            clientSocket.Send(data, 0, data.Length, SocketFlags.None);
        }
        
    }
}
