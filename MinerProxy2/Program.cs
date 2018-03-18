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

            Serilog.Core.LoggingLevelSwitch logLevel = new Serilog.Core.LoggingLevelSwitch(Serilog.Events.LogEventLevel.Verbose);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(logLevel)
                .WriteTo.Console(theme: AnsiConsoleTheme.Code, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug)
                //.WriteTo.File(path: AppDomain.CurrentDomain.BaseDirectory + "log.txt")
                .WriteTo.File(path: AppDomain.CurrentDomain.BaseDirectory + "verbose.txt", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose)
                .CreateLogger();

            Helpers.Logging.MinerProxyHeader();
            
            ICoinHandlerMiner coinHandler = (ICoinHandlerMiner)new EthereumMinerHandler();
            ICoinHandlerPool poolHandler = (ICoinHandlerPool)new EthereumPoolHandler();

            string server = "us1.ethermine.org"; //"ubq.pool.sexy";
            int port = 4444; //9009;
            

            PoolInstance ethPoolInstance = new PoolInstance(server, port, 9000, "MProxy", "0x3Ff3CF71689C7f2f8F5c1b7Fc41e030009ff7332", "ETH");
            PoolClient ethereumPool = new PoolClient(ethPoolInstance, poolHandler, coinHandler);

            Console.ReadLine();
        }
    }
}