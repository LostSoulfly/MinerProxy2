using System;
using System.Net;
using System.Text;
using Serilog;
using MinerProxy2.Coins.MinerHandler;
using MinerProxy2.Network.Sockets;
using MinerProxy2.Interfaces;

namespace MinerProxy2.Network.Sockets
{
    public class PoolClient
    {
        readonly PoolClient poolClient;
        private MinerServer minerServer;

        public byte[] currentWork = Encoding.ASCII.GetBytes("\r\n");

        string host = "us1.ethermine.org";
        int port = 4444;
        
        /*
        public PoolClient(string address, int port, int bufferSize = 2048, Container container = null)
        {
            poolClient = new TcpClient();
        }
        */

        public PoolClient()
        {
            //start a MinerConnection instance as well
            //that listens for miners for this pool's Proxy port.

            //create a Coins class instance for a supported algo
            //Hook into the methods within it when data is passed from the server or miners
            //call methods in the Coins class to send work to miners

            ICoinHandlerMinerServer coinHandler = (ICoinHandlerMinerServer)new Ethereum();

            minerServer = new MinerServer(9000, this, coinHandler);
            
        }

        public void Connect()
        {
            Log.Information("Connecting to server..");
            
            //poolClient.Write("");
            //poolClient.Write("{\"worker\": \"proxy\", \"jsonrpc\": \"2.0\", \"params\": [\"0x0c0ff71b06413865fe9fE9a4C40396c136a62980\", \"x\"], \"id\": 2, \"method\": \"eth_submitLogin\"}\r\n");
            
        }
        
    }
}
