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

namespace MinerProxy2.Coins.PassThru
{
    class PassThruMinerHandler : ICoinHandlerMiner
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
            _minerManager.AddMiner(new Miner("passthru", connection));
        }

        public void MinerDataReceived(byte[] data, TcpConnection connection)
        {
            Log.Information("{0} sent: {1}", connection.endPoint, data.GetString());
            //SendToMiner(data, connection);

            

            SendToPool(data);
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
