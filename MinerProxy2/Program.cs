/* MinerProxy2 programmed by LostSoulfly.
   GNU General Public License v3.0 */

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

            string server = "us1.ethermine.org";
            int port = 4444;

            PoolInstance ethPoolInstance = new PoolInstance(server, port, 9000, "MProxy", "0x3Ff3CF71689C7f2f8F5c1b7Fc41e030009ff7332", "ETH");
            PoolClient ethereumPool = new PoolClient(ethPoolInstance, poolHandler, coinHandler);

            // ---- second proxy

            ICoinHandlerMiner coinHandler2 = (ICoinHandlerMiner)new EthereumMinerHandler();
            ICoinHandlerPool poolHandler2 = (ICoinHandlerPool)new EthereumPoolHandler();
            string server2 = "us1-etc.ethermine.org";
            int port2 = 4444;
            PoolInstance ethPoolInstance2 = new PoolInstance(server2, port2, 9001, "MProxy", "0x83D557A1E88C9E3BbAe51DFA7Bd12CF523B28b84", "ETH");
            PoolClient ethereumPool2 = new PoolClient(ethPoolInstance2, poolHandler2, coinHandler2);

            Console.ReadLine();
        }
    }
}