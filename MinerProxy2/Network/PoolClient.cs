using MinerProxy2.Coins.MinerHandler;
using MinerProxy2.Interfaces;
using MinerProxy2.Network.Sockets;
using Serilog;
using System.Text;

namespace MinerProxy2.Network
{
    public class PoolClient
    {
        private MinerServer minerServer;
        private Client poolClient;
        private ICoinHandlerPool poolHandler;
        private string host = "us1.ethermine.org";
        private int port = 4444;

        public PoolClient(string address, int port, ICoinHandlerPool pool)
        {
            poolHandler = pool;
        }

        public PoolClient()
        {
            /*
            poolClient = new Client();
            poolClient.RaiseServerConnected += PoolClient_RaiseServerConnected;
            poolClient.RaiseServerDataReceived += PoolClient_RaiseServerDataReceived;
            poolClient.Connect(host, port);
            */

            ICoinHandlerMiner coinHandler = (ICoinHandlerMiner)new Ethereum();
            coinHandler.SetPool(this);
            minerServer = new MinerServer(9000, this, coinHandler);
        }

        private void PoolClient_RaiseServerDataReceived(object sender, ServerDataReceivedArgs e)
        {
            Log.Debug("Pool sent: " + Encoding.ASCII.GetString(e.Data));
        }

        private void PoolClient_RaiseServerConnected(object sender, ServerConnectedArgs e)
        {
            Log.Debug("Pool connected: " + e.socket.RemoteEndPoint.ToString());
        }

        public void Test()
        {
            Log.Information("poolClient Test.");
        }

        public void Connect()
        {
            Log.Information("Connecting to server..");

            //poolClient.Write("");
            //poolClient.Write("{\"worker\": \"proxy\", \"jsonrpc\": \"2.0\", \"params\": [\"0x0c0ff71b06413865fe9fE9a4C40396c136a62980\", \"x\"], \"id\": 2, \"method\": \"eth_submitLogin\"}\r\n");
        }
    }
}