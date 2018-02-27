using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SimpleTCP.Server;
using SimpleTCP;

namespace MinerProxy2.Network
{
    class PoolConnection
    {
        readonly SimpleTcpClient serverConnection;

        string host = "us1.ethermine.org";
        int port = 4444;

        PoolConnection(string address, int port, int bufferSize = 2048)
        {
            
        }

        PoolConnection()
        {
            //start a MinerConnection instance as well
            //that listens for miners for this pool's Proxy port.

            //create a Coins class instance for a supported algo
            //Hook into the methods within it when data is passed from the server or miners
            //call methods in the Coins class to send work to miners
        }

        public void Connect()
        {
            serverConnection.Connect(host, port);
        }

        public void Disconnect()
        {

        }

        private void RequestLoop()
        {

        }

        public void SendData()
        {

        }

        private void ReceiveData()
        {

        }
    }
}
