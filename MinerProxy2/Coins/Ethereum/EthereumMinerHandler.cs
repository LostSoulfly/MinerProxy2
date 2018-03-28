/* MinerProxy2 programmed by LostSoulfly.
   GNU General Public License v3.0 */

using MinerProxy2.Helpers;
using MinerProxy2.Interfaces;
using MinerProxy2.Miners;
using MinerProxy2.Network;
using MinerProxy2.Network.Sockets;
using Newtonsoft.Json;
using Serilog;
using System;

namespace MinerProxy2.Coins
{
    internal class EthereumMinerHandler : ICoinHandlerMiner
    {
        private MinerManager _minerManager;
        private MinerServer _minerServer;
        private PoolClient _pool;

        public void BroadcastToMiners(byte[] data)
        {
            _minerServer.BroadcastToMiners(data);
        }

        public void BroadcastToMiners(string data)
        {
            _minerServer.BroadcastToMiners(data.GetBytes());
        }

        public void MinerConnected(TcpConnection connection)
        {
            Log.Verbose("{0} connected.", connection.endPoint);
            if (_pool.currentPoolWork != null)
                _minerServer.SendToMiner(_pool.currentPoolWork, connection);
            //_minerManager.AddMiner();
        }

        public void MinerDataReceived(byte[] data, TcpConnection connection)
        {
            Log.Verbose("{0} sent: {1}", connection.endPoint, data.GetString());

            Miner miner = _minerManager.GetMiner(connection);
            
            string test = data.GetString();
            int id = -999;
            string jsonMethod = "";

            foreach (string s in test.Split('\r', '\n'))
            {
                if (s.Length <= 1)
                    continue;

                dynamic dyn = new object();

                try { dyn = JsonConvert.DeserializeObject(s.TrimNewLine()); } catch (Exception ex) { Log.Error(ex, "DeserializeObject Json error"); return; }

                if (Helpers.JsonHelper.DoesJsonObjectExist(dyn.id))
                    id = (int)dyn.id;

                if (JsonHelper.DoesJsonObjectExist(dyn.method))
                { 
                    jsonMethod = dyn.method;
                    
                    if (miner == null && jsonMethod.ToLower().Contains("login"))
                    {
                        Log.Verbose("{0}: Miner does not exist; disconnecting..", connection.endPoint);
                        _minerServer.DisconnectConnection(connection);
                    }

                    switch (jsonMethod.ToLower())
                    {
                        case "eth_getwork":
                            Log.Verbose("{0} requested work.", miner.workerIdentifier); // + Encoding.ASCII.GetString(_pool.currentWork));


                            if (_pool.currentPoolWork.Length > 0)
                                _minerServer.SendToMiner(_pool.currentPoolWork, connection);
                            else
                                _pool.SendToPool(s.GetBytes());

                            break;


                        case "eth_submitwork":
                            Log.Information("{0} found a share!", miner.workerIdentifier);
                            _pool.SubmitShareToPool(s.GetBytes(), _minerManager.GetMiner(connection));
                            break;

                        case "eth_submithashrate":
                            string hash = dyn.@params[0];
                            long hashrate = Convert.ToInt64(hash, 16);
                            _minerManager.UpdateMinerHashrate(hashrate, miner);
                            Log.Verbose("{0} sent hashrate: {1}", miner.workerIdentifier, hashrate.ToString("#,##0,Mh/s").Replace(",", "."));
                            break;

                        case "eth_login": // DevFee only?
                        case "eth_submitlogin":

                            string worker = dyn.@params[0];

                            if (worker.Contains("."))
                                worker = worker.Split(".")[1];
                            else if (worker != _pool.poolWallet)
                                worker = "DevFee";
                            else
                                worker = connection.endPoint.ToString();

                            miner = new Miner(worker, connection);

                            _minerManager.AddMiner(miner);
                            Log.Debug("{0} has authenticated for {1}!", miner.workerIdentifier, _pool.poolEndPoint);
                            _minerServer.SendToMiner("{\"id\":" + id + ",\"jsonrpc\":\"2.0\",\"result\":true}", connection);
                            //_minerManager.AddMinerId(miner, id);

                            break;

                        default:
                            Log.Warning("MinerHandler Method Unhandled {0} ", s);
                            _pool.SendToPool(s.GetBytes());
                            break;
                    }

                    continue;
                }

                if (id > -999)
                {
                    switch (id)
                    {

                        default:
                            Log.Warning("MinerHandler id Unhandled {0} ", s);
                            _pool.SendToPool(s.GetBytes());
                            break;
                    }
                }
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

        //private void SendWorkWithId(byte data, )

        public void SendToMiner(byte[] data, TcpConnection connection)
        {
            _minerServer.SendToMiner(data, connection);
        }

        public void SendToMiner(string data, TcpConnection connection)
        {
            _minerServer.SendToMiner(data.GetBytes(), connection);
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