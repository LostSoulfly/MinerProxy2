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

namespace MinerProxy2.Coins.Monero
{
    public class MoneroPoolHandler : ICoinHandlerPool
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
            //afaik in cryptonightv7 the miner does not request work
        }

        public void DoPoolLogin(PoolClient poolClient)
        {
            Log.Verbose("Authorizing with pool {0}", poolClient.poolEndPoint);

            _pool.SendToPool(string.Format("{{\"method\":\"login\",\"params\":{{\"login\":\"{0}\",\"pass\":\"{1}\",\"rigid\":\"\",\"agent\":\"xmr-stak/2.3.0/8f845f2/master/win/nvidia-amd-cpu/aeon-cryptonight-monero/0\"}},\"id\":1}}", _pool.poolWallet, _pool.poolWorkerName));
        }

        public void DoSendHashrate(PoolClient poolClient)
        {

        }

        public void PoolConnected(PoolClient poolClient)
        {
            Log.Information("{0} connected: ", poolClient.poolEndPoint);
        }

        public void PoolDataReceived(byte[] data, PoolClient poolClient)
        {
            Log.Verbose("Pool {0} sent: {1}", poolClient.poolEndPoint, data.GetString());

            string work;

            string split = data.GetString();

            foreach (string s in split.Split('\r', '\n'))
            {
                if (s.Length <= 1)
                    continue;

                dynamic dyn = JsonConvert.DeserializeObject(s.CheckForNewLine());

                /*if (Helpers.JsonHelper.DoesJsonObjectExist(dyn.method))
                {
                    switch ((string)dyn.method)
                    {
                        case "job":
                            work = dyn.@params[0] + dyn.@params[1] + dyn.@params[2] + dyn.@params[3];
                            if (_pool.currentPoolWork != work)
                            {
                                Log.Debug("[{0}] sent new target", poolClient.poolWorkerName);

                                foreach (Miner m in _minerManager.minerList)
                                {
                                    //Log.Debug("Modifying getWork ID {0} to ID {1}", (int)dyn.id, m.minerID);
                                    dyn.id = m.minerID;
                                    _minerServer.SendToMiner(JsonConvert.SerializeObject(dyn), m.connection);
                                }

                                _pool.currentPoolTarget = work;
                            }
                            break;

                        default:
                            break;
                    }
                }else */if (Helpers.JsonHelper.DoesJsonObjectExist(dyn.id))
                {
                    switch ((int)dyn.id)
                    {
                        case 1:
                            switch (dyn.method)
                            {
                                case "job":
                                    work = dyn.@params.blob + dyn.@params.job_id + dyn.@params.target + dyn.@params.id;
                                    if (_pool.currentPoolWork != work)
                                    {
                                        Log.Debug("[{0}] sent new target", poolClient.poolWorkerName);

                                        foreach (Miner m in _minerManager.GetMinerList())
                                        {
                                            //Log.Debug("Modifying getWork ID {0} to ID {1}", (int)dyn.id, m.minerID);
                                            dyn.id = m.minerID;
                                            _minerServer.SendToMiner(JsonConvert.SerializeObject(dyn), m.connection);
                                        }

                                        _pool.currentPoolTarget = work;
                                    }
                                    break;
                                default:
                                    break;
                            }
                            switch (dyn.result.status)
                            {
                                case "OK":
                                    Log.Debug("[{0}] returned OK", poolClient.poolWorkerName);
                                    break;
                            }
                            break;

                        default:
                            break;
                    }

                }
            }
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
