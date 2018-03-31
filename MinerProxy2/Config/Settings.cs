/* MinerProxy2 programmed by LostSoulfly.
   GNU General Public License v3.0 */

using MinerProxy2.Pools;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MinerProxy2.Config
{
    public static class Settings
    {
        public static int logLevel { get; set; }

        public static PoolInstance LoadPoolFromFile(string filename)
        {
            if (!File.Exists(filename))
            {
                Log.Warning("Unable to load {0}!", filename);
                return null;
            }

            Log.Debug("Loading {0}..", filename);
            try
            {
                PoolInstance pool = JsonConvert.DeserializeObject<PoolInstance>(File.ReadAllText(filename));

                Log.Debug(pool.mainPool.coin);
                Log.Debug(pool.mainPool.poolAddress);

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
                Log.Debug("Writing {0} to {1}..", pool.mainPool.poolWorkerName, path);
                File.WriteAllText(path, JsonConvert.SerializeObject(pool, Formatting.Indented));
                return true;
            }
            catch (Exception ex)
            {
                Log.Warning(string.Format("Save settings error: {0}", ex.Message));
                return false;
            }
        }

        public static List<PoolInstance> LoadPoolDirectory()
        {
            List<PoolInstance> pools = new List<PoolInstance>();
            PoolInstance temp;

            string dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"pools\");
            Log.Information("Loading pools.. {0}", dirPath);

            if (!Directory.Exists(dirPath))
            {
                Log.Information("Pool directory doesn't exist; creating..");
                Directory.CreateDirectory(dirPath);
            }

            if (!Directory.EnumerateFileSystemEntries(dirPath).Any())
            { 
                Log.Information("Pool directory is empty; creating default pool json..");
                PoolInstance etcPool = new PoolInstance("us1-etc.ethermine.org", 4444, 9000, "MProxyETC", "0x83D557A1E88C9E3BbAe51DFA7Bd12CF523B28b84", "ETC");
                WritePoolToFile("Ethermine ETC.json", etcPool);
            }
            
            List<string> dirs = new List<string>(Directory.EnumerateFiles(dirPath, "*.json"));

            foreach (var item in dirs)
            {
                Log.Debug("Attempting to load {0}..", Path.GetFileName(item));
                temp = LoadPoolFromFile(item);
                if (temp == null)
                {
                    Log.Debug("Skipping {0}", item);
                    continue;
                }
                pools.Add(temp);
            }

            return pools;
        }
            
    }
}