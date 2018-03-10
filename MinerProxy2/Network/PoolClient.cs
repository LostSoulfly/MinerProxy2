using MinerProxy2.Coins.MinerHandler;
using MinerProxy2.Interfaces;
using MinerProxy2.Network.Sockets;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinerProxy2.Network
{
    public class PoolClient
    {
        private MinerServer minerServer;
        private Client poolClient;
        private ICoinHandlerPool poolHandler;
        private ICoinHandlerMiner coinHandler;
        private string host = "us1.ethermine.org";
        private int port = 4444;
        private List<byte[]> submittedShares = new List<byte[]>();
        private readonly object submittedShareLock = new object();
        public string poolEndPoint { get; }

        public PoolClient(string address, int port, ICoinHandlerPool pool)
        {
            poolHandler = pool;
            this.host = address;
            this.port = port;
            poolEndPoint = address + ":" + port;
        }

        public PoolClient()
        {
            poolClient = new Client();
            poolClient.OnServerConnected += PoolClient_OnServerConnected;
            poolClient.OnServerDataReceived += PoolClient_OnServerDataReceived;
            poolClient.Connect();

            coinHandler = (ICoinHandlerMiner)new EthereumMinerHandler();

            coinHandler.SetPool(this);
            minerServer = new MinerServer(9000, this, coinHandler);
        }

        private void PoolClient_OnServerDataReceived(object sender, ServerDataReceivedArgs e)
        {
            Log.Debug("Pool sent: " + Encoding.ASCII.GetString(e.Data));
            poolHandler.PoolDataReceived(e.Data, this);
        }
        
        private void PoolClient_OnServerConnected(object sender, ServerConnectedArgs e)
        {
            Log.Debug("Pool connected: " + e.socket.RemoteEndPoint.ToString());
            minerServer.ListenForMiners();
            //poolClient.SendToPool(Encoding.ASCII.GetBytes("{\"worker\": \"proxy\", \"jsonrpc\": \"2.0\", \"params\": [\"0x0c0ff71b06413865fe9fE9a4C40396c136a62980\", \"x\"], \"id\": 2, \"method\": \"eth_submitLogin\"}\r\n"));
        }

        public void SendToPool(byte[] data)
        {
            Log.Debug("PoolClient SendToPool");
            this.poolClient.SendToPool(data);
        }

        public bool HasShareBeenSubmitted(byte[] share)
        {
            bool submitted;

            lock (submittedShareLock)
            {
                //search the list to see if this share has been 
                submitted = submittedShares.Any(item => item == share);

                //If it wasn't found in the list, we add it
                if (!submitted)
                    submittedShares.Add(share);
            }

            return submitted;
        }

        public void ClearSubmittedShares()
        {
            lock (submittedShareLock) { submittedShares.Clear(); }
        }

    }
}