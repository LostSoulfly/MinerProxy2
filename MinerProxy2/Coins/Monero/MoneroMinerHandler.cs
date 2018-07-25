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
    internal class MoneroMinerHandler : ICoinHandlerMiner
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
            _minerServer.BroadcastToMiners(data);
        }

        public void MinerConnected(TcpConnection connection)
        {
            Log.Verbose("{0} connected.", connection.endPoint);
        }

        public void MinerDataReceived(byte[] data, TcpConnection connection)
        {
            Log.Verbose("{0} sent: {1}", connection.endPoint, data.GetString());

            Miner miner = _minerManager.GetMiner(connection);

            if (miner == null)
                Log.Verbose("{0}: Miner does not exist.", connection.endPoint);

            string minerData = data.GetString();
            int id = -999;
            string jsonMethod = "";

            if (minerData.Length <= 1)
                return;

            dynamic dyn = new object();

            try { dyn = JsonConvert.DeserializeObject(minerData.TrimNewLine()); } catch (Exception ex) { Log.Error(ex, "DeserializeObject Json error"); return; }

            if (Helpers.JsonHelper.DoesJsonObjectExist(dyn.id))
                id = (int)dyn.id;

            if (JsonHelper.DoesJsonObjectExist(dyn.method))
            {
                jsonMethod = dyn.method;

                switch (jsonMethod.ToLower())
                {
                    case "login":
                        string worker = dyn.@params.login;

                        if (worker.Contains("."))
                            worker = worker.Split(".")[1];
                        else if (!_pool.poolWallet.Contains(worker))    // if our wallet address isn't part of the worker address, probably a devfee?
                            worker = "DevFee";
                        else
                            worker = connection.endPoint.ToString();

                        miner = new Miner(worker, connection);

                        _minerManager.AddMiner(miner);
                        Log.Information("{0} has authenticated for [{1}]!", miner.workerIdentifier, _pool.poolWorkerName);
                        //SendToMiner("{\"id\":" + id + ",\"jsonrpc\":\"2.0\",\"error\":null,\"result\":{\"status\": \"OK\"}}", connection);

                        _minerManager.AddMinerID(miner, id);

                        if (_pool.currentPoolTarget.Length != 0)
                            SendToMiner(_pool.currentPoolWork.GetBytes(), connection);
                        return;

                    case "submit":

                        if (miner == null)
                        {
                            Log.Error("submit from non-existent miner!");
                            return;
                        }

                        Log.Verbose("{0} found a share!", miner.workerIdentifier);

                        _minerManager.AddMinerID(miner, id);

                        _pool.SubmitShareToPool(minerData.GetBytes(), miner);
                        return;

                    case "keepalived":
                        //{"id":2,"jsonrpc":"2.0","method":"keepalived","params":{"id":""}}
                        Log.Verbose($"Keepalive from {miner.workerIdentifier}");
                        SendToMiner($"{{\"id\":{id},\"jsonrpc\":\"2.0\",\"error\":null,\"result\":{{\"status\":\"KEEPALIVED\"}}}}", connection);
                        return;
                }
            }
        }

        public void MinerDisconnected(TcpConnection connection)
        {
            Miner miner = _minerManager.GetMiner(connection);

            if (miner != null)
            {
                Log.Information("{0} has disconnected for {1}", miner.workerIdentifier, _pool.poolEndPoint);
                _minerManager.RemoveMiner(miner);
            }
            else
            {
                Log.Information("Non-miner {0} has disconnected for {1}", connection.endPoint, _pool.poolEndPoint);
            }
        }

        public void MinerError(Exception exception, TcpConnection connection)
        {
            Log.Error(exception, "Miner Error");
            MinerDisconnected(connection);
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