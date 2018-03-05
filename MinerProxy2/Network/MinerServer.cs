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
        private ICoinHandlerMiner _coinHandler;
        private readonly PoolClient _pool;

        public MinerServer(int port, PoolClient pool, ICoinHandlerMiner coinHandler)
        {
            Log.Information("Starting MinerServer on " + port);
            _pool = pool;
            _coinHandler = coinHandler;
            //_coinHandler.InitializeHandler();

            minerServer = new Server();
            minerServer.RaiseDataReceived += MinerServer_RaiseDataReceived;
            minerServer.RaiseClientConnected += MinerServer_RaiseClientConnected;
            minerServer.Start(port);
        }

        private void MinerServer_RaiseClientConnected(object sender, ClientConnectedArgs e)
        {
            Log.Information(e.connection.endPoint.ToString() + " has connected!");
            //_coinHandler.MinerConnected
        }

        private void MinerServer_RaiseDataReceived(object sender, DataReceivedArgs e)
        {
            //Log.Information(Encoding.ASCII.GetString(e.Data));
            _coinHandler.MinerDataReceived(e.Data, e.connection);
        }
    }
}
