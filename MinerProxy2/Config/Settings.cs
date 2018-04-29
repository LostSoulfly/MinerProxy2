/* MinerProxy2 programmed by LostSoulfly.
   GNU General Public License v3.0 */

using MinerProxy2.Pools;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;

namespace MinerProxy2.Config
{
    public static class Settings
    {
        public static int logLevel { get; set; }

        public static List<PoolInstance> LoadPoolDirectory()
        {
            List<PoolInstance> pools = new List<PoolInstance>();
            PoolInstance temp;

            string dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"pools\");
            Log.Information("Loading pools.. {0}", @"\pools\");

            if (!Directory.Exists(dirPath))
            {
                Log.Information("Pool directory doesn't exist; creating..");
                Directory.CreateDirectory(dirPath);
            }

            List<string> dir = new List<string>(Directory.EnumerateFiles(dirPath, "*.json"));

            if (dir.Count == 0)
            {
                Log.Information("No pools found; creating default..");
                PoolInstance etcPool = new PoolInstance("us1-etc.ethermine.org", 4444, 9000, "MProxyETC", "0x83D557A1E88C9E3BbAe51DFA7Bd12CF523B28b84", "ETC", 0);
                etcPool.AddFailoverPool(etcPool.mainPool.poolAddress, etcPool.mainPool.poolPort);
                etcPool.AddAllowedIPAddress("0.0.0.0");
                WritePoolToFile("Ethermine ETC.json", etcPool);
            }

            dir = new List<string>(Directory.EnumerateFiles(dirPath, "*.json"));

            foreach (var item in dir)
            {
                Log.Verbose("Attempting to load {0}..", Path.GetRelativePath(AppDomain.CurrentDomain.BaseDirectory, item));
                temp = LoadPoolFromFile(item);
                if (temp == null)
                {
                    Log.Debug("Skipping {0}", item);
                    continue;
                }
                Log.Information("Loaded {0} for {1} on port {2}", temp.mainPool.poolWorkerName, temp.mainPool.poolEndPoint, temp.localListenPort);
                pools.Add(temp);
            }

            return pools;
        }

        public static PoolInstance LoadPoolFromFile(string filename)
        {
            if (!File.Exists(filename))
            {
                Log.Warning("Unable to load {0}!", Path.GetRelativePath(AppDomain.CurrentDomain.BaseDirectory, filename));
                return null;
            }

            Log.Debug("Loading {0}..", Path.GetRelativePath(AppDomain.CurrentDomain.BaseDirectory, filename));
            try
            {
                PoolInstance pool = JsonConvert.DeserializeObject<PoolInstance>(File.ReadAllText(filename));

                return pool;
            }
            catch (Exception ex)
            {
                Log.Error("Unable to load {0}: {1}", filename, ex.Message);
                return null;
            }
        }

        public static bool WritePoolToFile(string filename, PoolInstance pool)
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + @"pools\", filename);
                Log.Debug("Writing {0} to {1}..", pool.mainPool.poolWorkerName, Path.GetRelativePath(AppDomain.CurrentDomain.BaseDirectory, path));
                File.WriteAllText(path, JsonConvert.SerializeObject(pool, Formatting.Indented));
                return true;
            }
            catch (Exception ex)
            {
                Log.Warning(string.Format("Save settings error: {0}", ex.Message));
                return false;
            }
        }
    }
}