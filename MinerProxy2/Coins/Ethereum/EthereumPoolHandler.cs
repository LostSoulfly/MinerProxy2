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
using System.Linq;

namespace MinerProxy2.Coins
{
    public class EthereumPoolHandler : ICoinHandlerPool
    {
        private MinerManager _minerManager;
        private MinerServer _minerServer;
        private PoolClient _pool;

        public void BroadcastToMiners(byte[] data)
        {
            //This will probably be removed, I don't think it's necessary..
            _minerServer.BroadcastToMiners(data);
        }

        public void DoPoolGetWork(PoolClient poolClient)
        {
            //Log.Debug("Requesting work from pool..");
            //_pool.SendToPool("{\"worker\": \"\", \"jsonrpc\": \"2.0\", \"params\": [], \"id\": 3, \"method\": \"eth_getWork\"}\n");
            _pool.SendToPool("{\"id\":5,\"jsonrpc\":\"2.0\",\"method\":\"eth_getWork\",\"params\":[]}");
        }

        public void DoPoolLogin(PoolClient poolClient)
        {
            Log.Verbose("Authorizing with pool {0}", poolClient.poolEndPoint);

            //workername login
            //_pool.SendToPool("{\"worker\": \"" + _pool.poolWorkerName + "\", \"jsonrpc\": \"2.0\", \"params\": [\"" + _pool.poolWallet + "\", \"x\"], \"id\": 2, \"method\": \"eth_submitLogin\"}");

            //Phoenix uses id:1
            //poolClient.SendToPool("{\"id\":1,\"jsonrpc\":\"2.0\",\"method\":\"eth_submitLogin\",\"worker\":\"eth1.0\",\"params\":[\"0x83D557A1E88C9E3BbAe51DFA7Bd12CF523B28b84.lulz\"]}".CheckForNewLine());

            _pool.SendToPool(string.Format("{{\"worker\": \"eth1.0\", \"jsonrpc\": \"2.0\", \"params\": [\"{0}.{1}\", \"{2}\"], \"id\": 1, \"method\": \"eth_submitLogin\"}}", _pool.poolWallet, _pool.poolWorkerName, "x"));
        }

        public void DoSendHashrate(PoolClient poolClient)
        {
            string hashrateJson = string.Format("{{\"id\":6,\"jsonrpc\":\"2.0\",\"method\":\"eth_submitHashrate\",\"params\":[\"0x{0}\", \"{1}\"]}}",
                _minerManager.GetCurrentHashrateLong().ToString("X"), _pool.poolHashrateId);
            //Log.Debug("HashrateJson: " + hashrateJson);
            _pool.SendToPool(hashrateJson);

        }

        public void PoolConnected(PoolClient poolClient)
        {
            Log.Information("{0} connected: ", poolClient.poolEndPoint);
        }

        public void PoolDataReceived(byte[] data, PoolClient poolClient)
        {
            //Log.Debug("Pool {0} sent: {1}", poolClient.poolEndPoint, data.GetString());
            string work;

            string s = data.GetString();
            
            dynamic dyn = JsonConvert.DeserializeObject(s.TrimNewLine());

            if (Helpers.JsonHelper.DoesJsonObjectExist(dyn.id))
            {
                //This still needs to be converted to dyn.method
                switch ((int)dyn.id)
                {
                        
                    case 0:

                        work = dyn.result[0] + dyn.result[1] + dyn.result[2] + dyn.result[3];
                        if (_pool.currentPoolWork != work)
                        {
                            Log.Debug("[{0}] sent new target", poolClient.poolWorkerName);

                            lock (_minerManager.MinerManagerLock)
                            {
                                foreach (Miner m in _minerManager.GetMinerList())
                                {
                                    //Log.Debug("Modifying getWork ID {0} to ID {1}", (int)dyn.id, m.minerID);
                                    dyn.id = m.minerID;
                                    _minerServer.SendToMiner(JsonConvert.SerializeObject(dyn), m.connection);
                                }
                            }
                            _pool.currentPoolTarget = work;
                        }
                            
                        //_minerServer.BroadcastToMiners();
                        break;
                        

                    case 1:
                    case 2:

                        if (JsonHelper.DoesJsonObjectExist(dyn.error) && !JsonHelper.DoesJsonObjectExist(dyn.result))
                        {
                            Log.Fatal("Server error for {0}: {1} {2}", poolClient.poolEndPoint, Convert.ToString(dyn.error.code), Convert.ToString(dyn.error.message));
                            _pool.Stop();
                            _minerServer.StopListening();
                            return;
                        }
                        else if (JsonHelper.DoesJsonObjectExist(dyn.result))
                        {
                            if (dyn.result == false)
                            {
                                Log.Fatal("Server error2 for {0}: {1} {2}", poolClient.poolEndPoint, Convert.ToString(dyn.error.code), Convert.ToString(dyn.error.message));
                                _pool.Stop();
                                _minerServer.StopListening();
                                return;
                            }
                        }

                        Log.Information("Authorized with {0}!", poolClient.poolEndPoint);
                        break;
                            
                    case 5:
                    case 3:
                        //Log.Debug("{0} sent new work.", _pool.poolEndPoint);

                        if (JsonHelper.DoesJsonObjectExist(dyn.error))
                        {
                            Log.Error("Oops");
                            return;
                        }

                        work = dyn.result[0] + dyn.result[1] + dyn.result[2] + dyn.result[3];

                        if (_pool.currentPoolWork != work)
                        {
                            Log.Debug("[{0}] sent new work", poolClient.poolWorkerName);

                            _pool.ClearSubmittedSharesHistory();

                            lock (_minerManager.MinerManagerLock)
                            {
                                foreach (Miner m in _minerManager.GetMinerList())
                                {
                                    //Log.Debug("Modifying getWork ID {0} to ID {1} for {3}", (int)dyn.id, m.minerID, m.workerIdentifier);
                                    dyn.id = m.minerID;
                                    _minerServer.SendToMiner(JsonConvert.SerializeObject(dyn), m.connection);
                                }
                            }

                            _pool.currentPoolWork = work;
                            _pool.currentPoolWorkDynamic = dyn;
                        }

                            
                        break;

                    case int i when (i >= 7 && i != 999):
                    case 4:

                        bool result = false;

                        if (JsonHelper.DoesJsonObjectExist(dyn.result))
                            result = dyn.result;

                        Miner miner = _minerManager.GetNextShare(result);

                        if (miner != null)
                        {
                            _minerServer.SendToMiner(s, miner.connection);
                            _pool.acceptedSharesCount++;
                            Log.Information("{0}'s share was {1}! ({2})", miner.workerIdentifier, result ? "accepted" : "rejected", _minerManager.ResetMinerShareSubmittedTime(miner));

                            //miner.PrintShares();
                        }
                        break;
                            
                    case 6:
                        Log.Verbose("Hashrate accepted by {0}", poolClient.poolEndPoint);
                        //_minerServer.BroadcastToMiners(s);

                        break;

                    case 999:

                        if (JsonHelper.DoesJsonObjectExist(dyn.error) && !JsonHelper.DoesJsonObjectExist(dyn.result))
                        {
                            Log.Fatal("Server error for {0}: {1}", poolClient.poolEndPoint, Convert.ToString(dyn.error));
                            _pool.Stop();
                            _minerServer.StopListening();
                            return;
                        }
                        else if (JsonHelper.DoesJsonObjectExist(dyn.result))
                        {
                            if (dyn.result == false) //no dyn.error.code
                            {
                                Log.Fatal("Server error2 for {0}: {1}", poolClient.poolEndPoint, Convert.ToString(dyn.error));
                                _pool.Stop();
                                _minerServer.StopListening();
                                return;
                            }
                        }
                        break;

                    default:
                        Log.Warning("EthPoolHandler Unhandled: {0}", s);
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
            Log.Warning("Pool {0} disconnected.", poolClient.poolEndPoint);
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

        public void SetMinerManager(MinerManager minerManager)
        {
            this._minerManager = minerManager;
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