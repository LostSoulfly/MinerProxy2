using System;
using MinerProxy2.Interfaces;
using MinerProxy2.Network;
using MinerProxy2.Network.Sockets;
using MinerProxy2.Pools;

namespace MinerProxy2.Coins
{
    internal class EthereumPoolHandler : ICoinHandlerPool
    {
        public void BroadcastToMiners(byte[] data)
        {
            throw new NotImplementedException();
        }

        public void PoolConnected(PoolClient poolClient)
        {
            throw new NotImplementedException();
        }

        public void PoolDataReceived(byte[] data, PoolClient poolClient)
        {
            throw new NotImplementedException();
        }

        public void PoolDisconnected(PoolClient poolClient)
        {
            throw new NotImplementedException();
        }

        public void PoolError(Exception exception, PoolClient poolClient)
        {
            throw new NotImplementedException();
        }

        public void SendToMiner(byte[] data, TcpConnection connection)
        {
            throw new NotImplementedException();
        }

        public void SendToPool(byte[] data)
        {
            throw new NotImplementedException();
        }

        public void SetMinerServer(MinerServer minerServer)
        {
            throw new NotImplementedException();
        }

        public void SetPool(PoolClient pool)
        {
            throw new NotImplementedException();
        }

        public void SetPoolManager(PoolManager poolManager)
        {
            throw new NotImplementedException();
        }
    }
}