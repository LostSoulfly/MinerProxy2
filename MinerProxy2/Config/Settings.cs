using System.Collections.Generic;

namespace MinerProxy2.Config
{
    public static class Settings
    {
        //load settings file

        public static PoolManager poolManager = new PoolManager();
        public static bool logging { get; set; }
        public static bool debug { get; set; }
        public static int donationPercent { get; set; }
        public static List<string> allowedIPAddresses = new List<string>();
    }
}