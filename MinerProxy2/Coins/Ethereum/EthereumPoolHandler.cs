using MinerProxy2.Interfaces;
using MinerProxy2.Network;
using MinerProxy2.Network.Sockets;
using MinerProxy2.Pools;
using Serilog;
using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MinerProxy2.Coins
{
    public class EthereumPoolHandler : ICoinHandlerPool
    {
        private MinerServer _minerServer;
        private PoolClient _pool;
        private PoolManager _poolManager;
        private byte[] currentWork;

        public void BroadcastToMiners(byte[] data)
        {
            //This will probably be removed, I don't think it's necessary..
            _minerServer.BroadcastToMiners(data);
        }

        public void DoPoolLogin(PoolClient poolClient)
        {
            _pool.SendToPool(Encoding.ASCII.GetBytes("{\"worker\": \"" + "eth1.0" + "\", \"jsonrpc\": \"2.0\", \"params\": [\"" + poolClient.poolWallet + "." + poolClient.poolWorkerName + "\", \"x\"], \"id\": 2, \"method\": \"eth_submitLogin\"}\r\n"));

        }

        public void PoolConnected(PoolClient poolClient)
        {
            Log.Information("Pool connected: " + poolClient.poolEndPoint);
        }

        public void PoolDataReceived(byte[] data, PoolClient poolClient)
        {
            //process pool data here
            //such as pushing work to all miners
            //or letting miners know the share they submitted was accepted/rejected.
            Log.Information("Pool " + poolClient.poolEndPoint + " sent: " + Encoding.ASCII.GetString(data));
            
            //This is just for testing with 1 miner.
            //_minerServer.BroadcastToMiners(data);

            dynamic dyn = JsonConvert.DeserializeObject(Encoding.ASCII.GetString(data));

            Log.Information(Encoding.ASCII.GetString(data));
            if (dyn.id != null)
            {
                switch (dyn.id)
                {
                    case 0:
                        Log.Information("Case 0");
                        break;

                    case 1:
                        Log.Information("Case 1");
                        break;

                    case 2:
                        Log.Information("Case 2");
                        break;

                    case 3:
                        Log.Information("Server sent eth_getWork");
                        currentWork = data;
                        _minerServer.BroadcastToMiners(data);
                        return;
                        break;

                    case 4:
                        Log.Information("Case 4");
                        break;

                    case 5:
                        Log.Information("Case 5");
                        break;

                    default:
                        Log.Information("Unhandled.");
                        break;
                }
            } /* else if (dyn.error != null && dyn.result == null)
            {
                Log.Error("Server sent Error: " + dyn.error.code + ": " + dyn.error.message);
            }
            */

        }

        public void PoolDisconnected(PoolClient poolClient)
        {
            Log.Information("Pool disconnected: " + poolClient.poolEndPoint);
        }

        public void PoolError(Exception exception, PoolClient poolClient)
        {
            Log.Error(exception, "Pool Error");
        }

        public void SendToMiner(byte[] data, TcpConnection connection)
        {
            _minerServer.SendToMiner(data, connection);
        }

        public void SendToPool(byte[] data)
        {
            _pool.SendToPool(data);
        }

        public void SetMinerServer(MinerServer minerServer)
        {
            _minerServer = minerServer;
        }

        public void SetPool(PoolClient pool)
        {
            _pool = pool;
        }

        public void SetPoolManager(PoolManager poolManager)
        {
            _poolManager = poolManager;
        }
    }
}