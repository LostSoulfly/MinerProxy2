using MinerProxy2.Interfaces;
using MinerProxy2.Miners;
using MinerProxy2.Network;
using MinerProxy2.Network.Sockets;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Text;

namespace MinerProxy2.Coins
{
    internal class EthereumMinerHandler : ICoinHandlerMiner
    {
        private PoolClient _pool;
        private MinerServer _minerServer;
        private MinerManager _minerManager;

        public void BroadcastToMiners(byte[] data)
        {
            _minerServer.BroadcastToMiners(data);
        }

        public void BroadcastToMiners(string data)
        {
            _minerServer.BroadcastToMiners(Encoding.ASCII.GetBytes(data));
        }

        public void MinerConnected(TcpConnection connection)
        {
            Log.Information("Miner connected: " + connection.endPoint);
            if (_pool.currentWork != null)
                _minerServer.SendToMiner(_pool.currentWork, connection);
           //_minerManager.AddMiner();
        }

        public void MinerDataReceived(byte[] data, TcpConnection connection)
        {
            //process the data here, such as replacing the wallet and then submitting shares to _pool
            //Log.Information("Sending to pool from " + connection.endPoint + ": " + Encoding.ASCII.GetString(data));
            
            string test = Encoding.ASCII.GetString(data);

            foreach (string s in test.Split('\r', '\n'))
            {
                if (s.Length <= 1)
                    continue;

                dynamic dyn = JsonConvert.DeserializeObject(s);

                //Log.Information("Split: " + s);
                if (Helpers.JsonHelper.DoesJsonObjectExist(dyn.id))
                {
                    //Log.Information("dyn.id: " + dyn.id);
                    switch ((int)dyn.id)
                    {
                        case 0:
                            Log.Information("Case 0?");
                            break;

                        case 2:
                            Log.Information("Miner sending eth_submitLogin");

                            string worker = dyn.@params[0];

                            if (worker.Contains("."))
                                worker = worker.Split(".")[1];

                            _minerManager.AddMiner(new Miner(worker, connection));
                            _minerServer.SendToMiner(Encoding.ASCII.GetBytes("{\"id\":2,\"jsonrpc\":\"2.0\",\"result\":true}\r\n"), connection);
                            break;

                        case 3:
                            Log.Information("Client requested work."); // + Encoding.ASCII.GetString(_pool.currentWork));
                            if (_pool.currentWork.Length > 0)
                            {
                                _minerServer.SendToMiner(_pool.currentWork, connection);
                            } else
                            {
                                _pool.SendToPool(Encoding.ASCII.GetBytes(s));
                            }
                            break;

                        case 4:
                            Log.Information("Miner found a share: " + connection.endPoint);
                            _pool.SubmitShareToPool(Encoding.ASCII.GetBytes(s), _minerManager.GetMiner(connection));
                            break;

                        case 6:
                            Log.Information("Miner sending hashrate: " + connection.endPoint);
                            _pool.SendToPool(Encoding.ASCII.GetBytes(s));
                            break;

                        default:
                            Log.Information("Unhandled: " + s);
                            break;
                    }
                } /* else if (dyn.error != null && dyn.result == null)
            {
                Log.Error("Server sent Error: " + dyn.error.code + ": " + dyn.error.message);
            }
            */

            }

            //_pool.SendToPool(data);

            //Respond with getWork results with the currentWork from the pool
            //occasionally request new work from the server?

        }

        public void MinerDisconnected(TcpConnection connection)
        {
            Log.Information("Miner disconnected: " + connection.endPoint);
            //_minerManager.RemoveMiner(connection);
        }

        public void MinerError(Exception exception, TcpConnection connection)
        {
            Log.Error(exception, "Miner Error");
        }

        public void PrintMinerStats()
        {
            Log.Information("Miner Stats");
        }

        public void SendToMiner(byte[] data, TcpConnection connection)
        {
            _minerServer.SendToMiner(data, connection);
        }

        public void SendToMiner(string data, TcpConnection connection)
        {
            _minerServer.SendToMiner(Encoding.ASCII.GetBytes(data), connection);
        }

        public void SendToPool(byte[] data)
        {
            _pool.SendToPool(data);
        }

        public void SetMinerManager(MinerManager minerManager)
        {
            _minerManager = minerManager;
        }

        public void SetMinerServer(MinerServer minerServer)
        {
            _minerServer = minerServer;
        }

        public void SetPoolClient(PoolClient pool)
        {
            _pool = pool;
        }
    }
}