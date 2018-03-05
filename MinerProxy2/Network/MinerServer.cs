using System;
using System.Collections.Generic;
using System.Text;
using Serilog;
using MinerProxy2.Network.Sockets;
using MinerProxy2.Interfaces;
using MinerProxy2.Network.Connections;

namespace MinerProxy2.Network
{
    public class MinerServer
    {
        Server minerServer;
        private ICoinHandlerMinerServer _coinHandler;
        private readonly PoolClient _pool;

        public MinerServer(int port, PoolClient pool, ICoinHandlerMinerServer coinHandler)
        {
            Log.Information("Starting MinerServer on " + port);
            _pool = pool;
            _coinHandler = coinHandler;
            minerServer = new Server();
            minerServer.RaiseDataReceived += MinerServer_RaiseDataReceived;
            minerServer.Start(port);
        }

        private void MinerServer_RaiseDataReceived(object sender, DataReceivedArgs e)
        {
            Log.Information(Encoding.ASCII.GetString(e.Data));
            _coinHandler.MinerDataReceived(e.Data, e.connection);
        }
    }
}
