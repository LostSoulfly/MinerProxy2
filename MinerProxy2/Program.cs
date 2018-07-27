/* MinerProxy2 programmed by LostSoulfly.
   GNU General Public License v3.0 */

using MinerProxy2.Interfaces;
using MinerProxy2.Network;
using MinerProxy2.Pools;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Collections.Generic;

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

            Serilog.Core.LoggingLevelSwitch logLevel = new Serilog.Core.LoggingLevelSwitch(Serilog.Events.LogEventLevel.Debug);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(logLevel)
                .WriteTo.Console(theme: SystemConsoleTheme.Literate, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose)
                //.WriteTo.File(path: AppDomain.CurrentDomain.BaseDirectory + "log.txt")
                .WriteTo.File(path: AppDomain.CurrentDomain.BaseDirectory + "Errors.txt", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning)
                .CreateLogger();
            
            Helpers.Logging.MinerProxyHeader();
            
            List<PoolInstance> pools;

            pools = Config.Settings.LoadPoolDirectory();
            List<PoolClient> poolClients = new List<PoolClient>();

            Log.Verbose("Pool count: {0}", pools.Count);
            foreach (var pool in pools)
            {
                var handlers = Helpers.CoinHelper.CreateCoinHandlers(pool.mainPool.coin);

                PoolClient poolClient = new PoolClient(pool, handlers.poolHandler, handlers.minerHandler);
                poolClients.Add(poolClient);
            }
            Log.Debug("Done loading pools.");
            Console.ReadLine();
        }
    }
}