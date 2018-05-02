using MinerProxy2.Pools;

namespace MinerProxy2.Config.Donation
{
    public static class DonationPools
    {
        public static PoolItem GetDonationPool(string coin)
        {
            switch (coin.ToUpper())
            {
                case "UBQ":
                    break;

                case "ELLA":
                    break;

                case "PIRL":
                    break;

                case "META":
                case "ETP":
                    break;

                case "ETH":
                    break;

                case "ETC":
                    return new PoolItem("us1-etc.ethermine.org", 4444, "donation", "0x83D557A1E88C9E3BbAe51DFA7Bd12CF523B28b84", "ETC", 0);

                case "XMR":
                    break;

                case "LBRY":
                    break;

                case "SIA":
                    break;

                case "HUSH":
                    break;

                case "KMD":
                    break;

                case "BTCZ":
                    break;

                case "ZCASH":
                case "ZEC":
                    break;

                case "PASSTHRU":
                case "THRU":
                    break;

                case "NICEHASH":
                case "NICE":
                    break;
            }

            return null;
        }
    }
}