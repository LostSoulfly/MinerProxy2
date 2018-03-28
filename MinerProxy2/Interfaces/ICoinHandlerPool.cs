/* MinerProxy2 programmed by LostSoulfly.
   GNU General Public License v3.0 */

using MinerProxy2.Miners;
using MinerProxy2.Network;
using MinerProxy2.Network.Sockets;
using System;

namespace MinerProxy2.Interfaces
{
    public interface ICoinHandlerPool
    {
        void BroadcastToMiners(byte[] data);

        void DoPoolGetWork(PoolClient poolClient);

        void DoSendHashrate(PoolClient poolClient);

        void DoPoolLogin(PoolClient poolClient);

        void PoolConnected(PoolClient poolClient);

        void PoolDataReceived(byte[] data, PoolClient poolClient);

        void PoolDisconnected(PoolClient poolClient);

        void PoolError(Exception exception, PoolClient poolClient);

        void SendToMiner(byte[] data, TcpConnection connection);

        void SendToPool(byte[] data);

        void SetMinerManager(MinerManager minerManager);

        void SetMinerServer(MinerServer minerServer);

        void SetPoolClient(PoolClient pool);
    }
}