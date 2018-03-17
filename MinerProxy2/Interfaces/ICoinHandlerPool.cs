using MinerProxy2.Miners;
using MinerProxy2.Network;
using MinerProxy2.Network.Sockets;
using MinerProxy2.Pools;
using System;

namespace MinerProxy2.Interfaces
{
    public interface ICoinHandlerPool
    {
        void SetPoolClient(PoolClient pool);

        void SetMinerServer(MinerServer minerServer);

        void SetMinerManager(MinerManager minerManager);

        void SetPoolInfo(PoolInstance poolInfo);

        void BroadcastToMiners(byte[] data);

        void SendToMiner(byte[] data, TcpConnection connection);

        void SendToPool(byte[] data);

        void PoolDataReceived(byte[] data, PoolClient poolClient);

        void PoolDisconnected(PoolClient poolClient);

        void PoolConnected(PoolClient poolClient);

        void PoolError(Exception exception, PoolClient poolClient);

        void DoPoolLogin(PoolClient poolClient);
    }
}