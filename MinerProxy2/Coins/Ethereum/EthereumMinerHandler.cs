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
            Log.Information("{0} connected.", connection.endPoint);
            if (_pool.currentPoolWork != null)
                _minerServer.SendToMiner(_pool.currentPoolWork, connection);
           //_minerManager.AddMiner();
        }

        public void MinerDataReceived(byte[] data, TcpConnection connection)
        {
            Miner miner = _minerManager.GetMiner(connection);

            if (miner == null)
                Log.Verbose("{0}: Miner does not exist.", connection.endPoint);

            string test = Encoding.ASCII.GetString(data);

            foreach (string s in test.Split('\r', '\n'))
            {
                if (s.Length <= 1)
                    continue;

                dynamic dyn = JsonConvert.DeserializeObject(s);
                
                if (Helpers.JsonHelper.DoesJsonObjectExist(dyn.id))
                {
                    //Log.Information("dyn.id: " + dyn.id);
                    switch ((int)dyn.id)
                    {
                        case 0:
                            Log.Information("Case 0?");
                            break;

                        case 2:

                            string worker = dyn.@params[0];

                            if (worker.Contains("."))
                                worker = worker.Split(".")[1];

                            miner = new Miner(worker, connection);

                            _minerManager.AddMiner(miner);
                            Log.Debug("{0} has authenticated!", miner.workerIdentifier);
                            _minerServer.SendToMiner(Encoding.ASCII.GetBytes("{\"id\":2,\"jsonrpc\":\"2.0\",\"result\":true}\r\n"), connection);
                            break;

                        case 3:
                            Log.Verbose("{0} requested work.", miner.workerIdentifier); // + Encoding.ASCII.GetString(_pool.currentWork));
                            if (_pool.currentPoolWork.Length > 0)
                            {
                                _minerServer.SendToMiner(_pool.currentPoolWork, connection);
                            } else
                            {
                                _pool.SendToPool(Encoding.ASCII.GetBytes(s));
                            }
                            break;

                        case 10: //claymore id 10
                        case 4:
                            Log.Information("{0} found a share!", miner.workerIdentifier);
                            _pool.SubmitShareToPool(Encoding.ASCII.GetBytes(s), _minerManager.GetMiner(connection));
                            break;

                        case 6:
                            Log.Verbose("{0} sending hashrate.", miner.workerIdentifier);
                            _pool.SendToPool(Encoding.ASCII.GetBytes(s));
                            break;

                        default:
                            Log.Warning("MinerHandler Unhandled {0} ", s);
                            break;
                    }
                } /* else if (dyn.error != null && dyn.result == null)
            {
                Log.Error("Server sent Error: " + dyn.error.code + ": " + dyn.error.message);
            }
            */

            }
            
        }

        public void MinerDisconnected(TcpConnection connection)
        {
            Miner miner = _minerManager.GetMiner(connection);
            Log.Information("{0} disconnected.", miner.workerIdentifier);
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