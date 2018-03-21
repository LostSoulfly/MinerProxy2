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
            Log.Debug("Requesting first work from pool..");
            _pool.SendToPool("{\"worker\": \"\", \"jsonrpc\": \"2.0\", \"params\": [], \"id\": 3, \"method\": \"eth_getWork\"}\n");
        }

        public void DoPoolLogin(PoolClient poolClient)
        {
            Log.Verbose("Authorizing with pool {0}", poolClient.poolEndPoint);
            _pool.SendToPool("{\"worker\": \"" + "eth1.0" + "\", \"jsonrpc\": \"2.0\", \"params\": [\"" + _pool.poolWallet + "." + _pool.poolWorkerName + "\", \"x\"], \"id\": 2, \"method\": \"eth_submitLogin\"}\r\n");
        }

        public void PoolConnected(PoolClient poolClient)
        {
            Log.Information("{0} connected: ", poolClient.poolEndPoint);
        }

        public void PoolDataReceived(byte[] data, PoolClient poolClient)
        {
            Log.Verbose("Pool {0} sent: {1}", poolClient.poolEndPoint, data.GetString());

            string split = data.GetString();

            foreach (string s in split.Split('\r', '\n'))
            {
                if (s.Length <= 1)
                    continue;

                dynamic dyn = JsonConvert.DeserializeObject(s.CheckForNewLine());

                if (Helpers.JsonHelper.DoesJsonObjectExist(dyn.id))
                {
                    //Log.Information("dyn.id: " + dyn.id);
                    switch ((int)dyn.id)
                    {
                        case 0:
                            //Log.Debug("{0} sent new target", poolClient.poolEndPoint);
                            Log.Verbose("{0} sent new target: {1}", poolClient.poolEndPoint, s);
                            byte[] work = s.GetBytes();
                            _pool.currentPoolWork = work;
                            Log.Verbose("currentPoolWork length: {0}", _pool.currentPoolWork.Length);

                            _minerServer.BroadcastToMiners(work);
                            break;

                        case 1:
                            Log.Information("Case 1: " + s);
                            break;

                        case 2:
                            Log.Information("Authorized with {0}!", poolClient.poolEndPoint);
                            //_minerServer.BroadcastToMiners(Encoding.ASCII.GetBytes(s));
                            break;

                        case 3:
                            //Log.Debug("{0} sent new work.", _pool.poolEndPoint);
                            Log.Verbose("{0} sent new work: {1}", poolClient.poolEndPoint, s);

                            if (_pool.currentPoolWork != s.GetBytes())
                                _minerServer.BroadcastToMiners(s);

                            _pool.currentPoolWork = s.GetBytes();
                            break;

                        case int i when (i >= 10):
                        case 4:

                            //Doesn't detect rejected shares yet
                            Miner miner = _minerManager.GetNextShare(true);

                            if (miner != null)
                            {
                                _minerServer.SendToMiner(s, miner.connection);
                                _pool.acceptedSharesCount++;
                                Log.Information("{0}'s share was accepted! ({1})", miner.workerIdentifier, _minerManager.ResetMinerShareSubmittedTime(miner));

                                //miner.PrintShares();
                            }
                            break;

                        case 5:
                            Log.Debug("Case 5: " + s);
                            break;

                        case 6:
                            Log.Verbose("Hashrate accepted by {0}", poolClient.poolEndPoint);
                            _minerServer.BroadcastToMiners(s);

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