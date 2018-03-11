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
            PoolInstance poolInstance = new PoolInstance("us1.ethermine.org", 4444, "ETH");
            PoolClient pool = new PoolClient(poolInstance, "MProxy", poolHandler, coinHandler);

            Console.ReadLine();
        }
    }
}