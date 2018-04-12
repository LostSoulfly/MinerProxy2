/* MinerProxy2 programmed by LostSoulfly.
   GNU General Public License v3.0 */

using System;
using System.Linq;

namespace MinerProxy2.Helpers
{
    public static class JsonHelper
    {
        
        public static bool DoesJsonObjectExist(dynamic json)
        {
            try
            {
                if (json is null)
                    return false;

                if (string.IsNullOrEmpty(Convert.ToString(json)))
                    return false;
            }
            catch { return false; }

            return true;
        }
        
    }
}