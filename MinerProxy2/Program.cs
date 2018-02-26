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

            //initialize client listener

            //don't initialize server connection until we have a client

            Console.ReadLine();
        }
    }
}
