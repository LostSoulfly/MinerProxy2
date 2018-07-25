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
            switch (_pool.poolProtocol)
            {
                case 0:
                    _pool.SendToPool(string.Format("{{\"id\":1,\"jsonrpc\":\"2.0\",\"method\":\"login\",\"params\":{{\"login\":\"{0}\",\"pass\":\"{1}\",\"agent\":\"XMRig/2.6.3 (Windows NT 10.0; Win64; x64) libuv/1.20.3 msvc/2017\",\"algo\":[\"cn/1\",\"cn/0\",\"cn/xtl\",\"cn/msr\",\"cn\"]}}}}", _pool.poolWallet, _pool.poolPassword));
                    break;

                case 1:
                    _pool.SendToPool(string.Format("{{\"id\":1,\"jsonrpc\":\"2.0\",\"method\":\"login\",\"params\":{{\"login\":\"{0}.{1}\",\"pass\":\"{2}\",\"agent\":\"XMRig/2.6.3 (Windows NT 10.0; Win64; x64) libuv/1.20.3 msvc/2017\",\"algo\":[\"cn/1\",\"cn/0\",\"cn/xtl\",\"cn/msr\",\"cn\"]}}}}", _pool.poolWallet, _pool.poolWorkerName, _pool.poolPassword));

                    break;

                case 2:
                    _pool.SendToPool(string.Format("{{\"id\":1,\"jsonrpc\":\"2.0\",\"method\":\"login\",\"params\":{{\"login\":\"{0}+{1}\",\"pass\":\"{2}\",\"agent\":\"XMRig/2.6.3 (Windows NT 10.0; Win64; x64) libuv/1.20.3 msvc/2017\",\"algo\":[\"cn/1\",\"cn/0\",\"cn/xtl\",\"cn/msr\",\"cn\"]}}}}", _pool.poolWallet, _pool.poolWorkerName, _pool.poolPassword));

                    break;
            }
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

            string poolData = data.GetString();

            if (poolData.Length <= 1)
                return;

            dynamic dyn = JsonConvert.DeserializeObject(poolData.TrimNewLine());

            if (Helpers.JsonHelper.DoesJsonObjectExist(dyn.id))
                {
                switch ((int)dyn.id)
                {
                    case 1:
                        if (Helpers.JsonHelper.DoesJsonObjectExist(dyn.method))
                        {
                            string jsonMethod = dyn.method;
                            switch (jsonMethod.ToLower())
                            {
                                case "job":
                                    work = dyn.@params.blob + dyn.@params.job_id + dyn.@params.target + dyn.@params.id;
                                    if (_pool.currentPoolTarget != work)
                                    {
                                        Log.Debug("[{0}] sent new target", poolClient.poolWorkerName);

                                        lock (_minerManager.MinerManagerLock)
                                        {
                                            foreach (Miner m in _minerManager.GetMinerList())
                                            {
                                                if (m.connection != null)
                                                {
                                                    Log.Debug("Modifying work target ID {0} to ID {1}", (int)dyn.id, m.minerID);
                                                    dyn.id = m.minerID;
                                                    SendToMiner(JsonConvert.SerializeObject(dyn), m.connection);
                                                }
                                            }
                                        }

                                        _pool.currentPoolTarget = work;
                                        _pool.currentPoolWork = poolData;
                                    }
                                    return;

                                default:
                                    return;
                            }
                        }
                        else
                        {

                            work = dyn.result.job.blob + dyn.result.job.job_id + dyn.result.job.target + dyn.result.job.id;
                            if (_pool.currentPoolTarget != work)
                            {
                                Log.Debug("[{0}] sent new target2", poolClient.poolWorkerName);

                                lock (_minerManager.MinerManagerLock)
                                {
                                    foreach (Miner m in _minerManager.GetMinerList())
                                    {
                                        if (m.connection != null)
                                        {
                                            Log.Debug("Modifying work target ID {0} to ID {1}", (int)dyn.id, m.minerID);
                                            dyn.id = m.minerID;
                                            _minerServer.SendToMiner(JsonConvert.SerializeObject(dyn), m.connection);
                                        }
                                    }
                                }

                                _pool.currentPoolTarget = work;
                                _pool.currentPoolWork = poolData;
                                return;
                            }
                        }
                        break;

                    default:
                        Log.Debug("switch defaulted");
                        break;
                }

                Log.Verbose("exit3");

                if (Helpers.JsonHelper.DoesJsonObjectExist(dyn.result.status))
                {
                    string status = dyn.result.status;
                    switch (status.ToUpper())
                    {
                        case "OK":
                            Log.Debug("[{0}] returned OK", poolClient.poolWorkerName);
                            bool result = true;

                            //result = dyn.result.status == "OK";

                            Miner miner = _minerManager.GetNextShare(result);

                            if (miner != null)
                            {
                                _minerServer.SendToMiner(poolData, miner.connection);

                                if (result)
                                    _pool.acceptedSharesCount++;
                                else
                                    _pool.rejectedSharesCount++;

                                Log.Information("[{0}] {1}'s share was {2}! ({3})", _pool.poolWorkerName, miner.workerIdentifier, result ? "accepted" : "rejected", _minerManager.ResetMinerShareSubmittedTime(miner));

                                if (!result)
                                    Log.Debug("Pool: " + poolData);
                            }
                            break;
                        default:
                            Log.Verbose("result default");
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