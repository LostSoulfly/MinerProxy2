using MinerProxy2.Coins;
using MinerProxy2.Interfaces;
using MinerProxy2.Network;
using MinerProxy2.Pools;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System;

namespace MinerProxy2
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //Initialize PoolManager
            //PoolManager poolManager = new PoolManager();

            //initialize settings
            //Pass PoolManager to settings for populating it



            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .WriteTo.File(path: AppDomain.CurrentDomain.BaseDirectory + "log.txt")
                .CreateLogger();

            Helpers.Logging.MinerProxyHeader();
            

            ICoinHandlerMiner coinHandler = (ICoinHandlerMiner)new EthereumMinerHandler();
            ICoinHandlerPool poolHandler = (ICoinHandlerPool)new EthereumPoolHandler();

            string server = "ubq.pool.sexy";
            int port = 9009;

            //"us1.ethermine.org", 4444

            PoolInstance poolInstance = new PoolInstance(server, port, 9000, "MProxy", "0x3Ff3CF71689C7f2f8F5c1b7Fc41e030009ff7332", "ETH");
            PoolClient pool = new PoolClient(poolInstance, poolHandler, coinHandler);

            Console.ReadLine();
        }
    }
}