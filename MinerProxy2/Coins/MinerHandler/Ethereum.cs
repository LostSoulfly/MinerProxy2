using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using MinerProxy2.Interfaces;
using MinerProxy2.Network;
using MinerProxy2.Network.Sockets;
using Serilog;

namespace MinerProxy2.Coins.MinerHandler
{
    class Ethereum : ICoinHandlerMiner
    {
        PoolClient poolClient;
        MinerServer minerServer;
        //Need a way to inject the reference for the POOL and MinerServers into each CoinHandler!

        public Ethereum(PoolClient pool, MinerServer miner)
        {
            Log.Information("Ethereum MinerHandler Initialized");
        }

        public void MinerConnected(TcpConnection socket)
        {
            throw new NotImplementedException();
        }

        public void MinerDataReceived(byte[] data, TcpConnection connection)
        {
            Log.Information(connection.endPoint.ToString() + " REC DATA: " + Encoding.ASCII.GetString(data));
        }

        public void MinerDisconnected(TcpConnection socket)
        {
            throw new NotImplementedException();
        }

        public void MinerError(Exception exception, TcpConnection connection)
        {
            throw new NotImplementedException();
        }

        public void SendToPool(byte[] data, TcpConnection connection)
        {
            throw new NotImplementedException();
        }
    }
}
