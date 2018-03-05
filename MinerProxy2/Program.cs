using System;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace MinerProxy2
{
    class Program
    {
        
        static void Main(string[] args)
        {
            
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .WriteTo.File(path: AppDomain.CurrentDomain.BaseDirectory + "log.txt")
                .CreateLogger();

            Helpers.Logging.MinerProxyHeader();

            //var container = new SimpleInjector.Container();
            //container.Register()

            //initialize settings

            //initialize an instance of PoolConnection for each proxy defined in settings
            //MinerConnection is initialized for each PoolConnection for that proxy
            //Each PoolConnection keeps track of its own miners, shares, and disseminating work updates.
            //keep a list of all proxies that we can reference and control from here.

            //failover support implemented in the PoolConnection class
            //Miners don't need to disconnect.

            //keep track of submitted work, all submits for the current work, and prevent sending to server
            //while returning a Success back to the miner, or optionally not sending anything at all.

            //don't initialize server connection until we have a client

            Network.PoolClient pool = new Network.PoolClient();
            Console.ReadLine();
            pool.Connect();

            Console.ReadLine();
        }
    }
}
