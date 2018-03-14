using System;
using System.Collections.Generic;
using System.Text;

namespace MinerProxy2.Helpers
{
    public static class JsonHelper
    {
        public static bool DoesJsonObjectExist(dynamic json)
        {
            try
            {
                if (ReferenceEquals(null, json))
                    {
                    return false;
                }
            } catch { return false; }

            return true;
        }
    }
}
