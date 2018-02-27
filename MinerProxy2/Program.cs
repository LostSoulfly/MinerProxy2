using System;

namespace MinerProxy2
{
    class Program
    {
        
        static void Main(string[] args)
        {
            Helpers.Logger.MinerProxyHeader();
            Helpers.Logger.LogToConsole("Hello world", color: ConsoleColor.Green);

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

            Console.ReadLine();
        }
    }
}
