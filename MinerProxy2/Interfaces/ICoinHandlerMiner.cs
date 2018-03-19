/* MinerProxy2 programmed by LostSoulfly.
   GNU General Public License v3.0 */

using MinerProxy2.Miners;
using MinerProxy2.Network;
using MinerProxy2.Network.Sockets;
using System;

namespace MinerProxy2.Interfaces
{
    public interface ICoinHandlerMiner
    {
        void BroadcastToMiners(byte[] data);

        void BroadcastToMiners(string data);

        void MinerConnected(TcpConnection connection);

        void MinerDataReceived(byte[] data, TcpConnection connection);

        void MinerDisconnected(TcpConnection connection);

        void MinerError(Exception exception, TcpConnection connection);

        void PrintMinerStats();

        void SendToMiner(byte[] data, TcpConnection connection);

        void SendToMiner(string data, TcpConnection connection);

        void SendToPool(byte[] data);

        void SetMinerManager(MinerManager minerManager);

        void SetMinerServer(MinerServer minerServer);

        void SetPoolClient(PoolClient pool);
    }
}