using System;
using System.Net;
using System.Text;
using Serilog;
using MinerProxy2.Coins.MinerHandler;
using MinerProxy2.Network.Sockets;
using MinerProxy2.Interfaces;

namespace MinerProxy2.Network
{
    public class PoolClient
    {
        private MinerServer minerServer;
        private ICoinHandlerPool poolHandler;
        string host = "us1.ethermine.org";
        int port = 4444;
        
        
        public PoolClient(string address, int port, ICoinHandlerPool pool)
        {
            poolHandler = pool;
        }
        

        public PoolClient()
        {
            //start a MinerConnection instance as well
            //that listens for miners for this pool's Proxy port.

            //create a Coins class instance for a supported algo
            //Hook into the methods within it when data is passed from the server or miners
            //call methods in the Coins class to send work to miners

            ICoinHandlerMiner coinHandler = (ICoinHandlerMiner)new Ethereum();

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
