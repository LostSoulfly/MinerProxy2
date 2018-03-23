/* MinerProxy2 programmed by LostSoulfly.
   GNU General Public License v3.0 */

using System;
using System.Linq;

namespace MinerProxy2.Helpers
{
    public static class JsonHelper
    {
        public static byte[] CheckForNewLine(byte[] data)
        {

            /*
            byte[] endCharacter = data.Skip(data.Length - 2).Take(2).ToArray();

            if (!(endCharacter.SequenceEqual("\n".GetBytes())))
            {
                data = data.Concat(Environment.NewLine.GetBytes()).ToArray();
            }
            */

            return CheckForNewLine(data.GetString()).GetBytes();
        }

        public static string CheckForNewLine(string data)
        {
            
            return data.TrimNewLine() + "\n";
        }

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