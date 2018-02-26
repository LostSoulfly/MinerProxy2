using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SimpleTCP.Server;
using SimpleTCP;

namespace MinerProxy2.Network
{
    class Server
    {
        readonly SimpleTcpClient server;

        string host = "us1.ethermine.org";
        int port = 4444;
        int bufferSize = 2048;
        int connectAttempts;

        Server(string address, int port, int bufferSize = 2048)
        {
            
        }

        Server()
        {

        }

        public void Connect()
        {

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
