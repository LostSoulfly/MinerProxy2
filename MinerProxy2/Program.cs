/* MinerProxy2 programmed by LostSoulfly.
   GNU General Public License v3.0 */

using MinerProxy2.Interfaces;
using MinerProxy2.Miners;
using MinerProxy2.Network;
using MinerProxy2.Pools;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MinerProxy2
{
    internal class Program
    {

        private static List<PoolInstance> pools = new List<PoolInstance>();
        public static List<PoolClient> poolClients = new List<PoolClient>();

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

            pools = Config.Settings.LoadPoolDirectory();

            Log.Verbose("Pool count: {0}", pools.Count);
            foreach (var pool in pools)
            {
                var handlers = Helpers.CoinHelper.CreateCoinHandlers(pool.mainPool.coin);

                PoolClient poolClient = new PoolClient(pool, handlers.poolHandler, handlers.minerHandler);
                poolClients.Add(poolClient);
            }
            Log.Debug("Done loading pools.");
            string key;

            while (true)
            {
                if (Console.KeyAvailable)
                {
                    key = Console.ReadKey(true).Key.ToString();

                    switch (key)
                    {
                        case "M":
                            foreach (var pool in poolClients)
                            {
                                Log.Information($"Miner Stats for [{pool.poolWorkerName}] miners");
                                foreach (var stats in pool.minerManager.GetMinerStatsList())
                                {
                                    Log.Information($"Miner: {stats.workerName}");
                                    Log.Information($"Hashrate Average: {(stats.hashrateHistory.Average(HashrateItem => HashrateItem.hashrate)).ToString("#,##0,Mh/s").Replace(",", ".")}");
                                    Log.Information($"numberofConnects: {stats.numberOfConnects}");
                                    Log.Information($"firstConnectTime: {stats.firstConnectTime}");
                                    Log.Information($"lastConnectTime: {stats.lastConnectTime}");
                                    Log.Information($"lastDisconnectTime: {stats.lastDisconnectTime}");
                                    Log.Information($"totalConnectedTime: {stats.totalConnectedTime}");
                                    Log.Information($"totalAcceptedShares: {stats.totalAcceptedShares}");
                                    Log.Information($"totalRejectedShares: {stats.totalRejectedShares}");
                                    Log.Information($"totalSubmittedShares: {stats.totalSubmittedShares}");
                                }
                            }
                            
                            
                            break;

                        case "Q":
                            Log.Information("Shutting down MineProxy..");
                            foreach (var pool in poolClients)
                            {

                                pool.minerServer.StopListening();
                                pool.Stop();
                            }
                            System.Environment.Exit(0);
                            break;

                        default:
                            Log.Information(key);
                            break;

                    }
                }
            }
        }
    }
}