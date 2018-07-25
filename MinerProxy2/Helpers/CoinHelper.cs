using MinerProxy2.Coins.Ethereum;
using MinerProxy2.Coins.Monero;
using MinerProxy2.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace MinerProxy2.Helpers
{
    class CoinHelper
    {
        public static (ICoinHandlerMiner minerHandler, ICoinHandlerPool poolHandler) CreateCoinHandlers(string coin)
        {
            switch (coin.ToUpper())
            {
                case "ELLA":
                case "PIRL":
                case "META":
                case "ETP":
                case "ETH":
                case "UBQ":
                case "ETC":
                    return ((ICoinHandlerMiner)new EthereumMinerHandler(), (ICoinHandlerPool)new EthereumPoolHandler());

                case "XMR":
                    return ((ICoinHandlerMiner)new MoneroMinerHandler(), (ICoinHandlerPool)new MoneroPoolHandler());

                case "LBRY":
                    break;

                case "SIA":
                    break;

                case "HUSH":
                case "KMD":
                case "BTCZ":
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

            return ((ICoinHandlerMiner)new EthereumMinerHandler(), (ICoinHandlerPool)new MoneroPoolHandler());
        }
    }
}
