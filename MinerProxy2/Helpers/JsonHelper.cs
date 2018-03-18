using System;
using System.Collections.Generic;
using System.Linq;
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

        public static byte[] CheckForNewLine(byte[] data)
        {
            byte[] endCharacter = data.Skip(data.Length - 2).Take(2).ToArray();

            if (!(endCharacter.SequenceEqual(Environment.NewLine.GetBytes())))
            {
                data = data.Concat(Environment.NewLine.GetBytes()).ToArray();
            }

            return data;
        }

        public static string CheckForNewLine(string data)
        {
            //Yeah, this is crappy, but it was quick and easy
            //TODO rewrite without converting to bytes and back
            return CheckForNewLine(data.GetBytes()).GetString();
        }
    }
}
