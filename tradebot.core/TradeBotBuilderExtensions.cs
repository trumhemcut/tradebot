using System;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace tradebot.core
{
    public static class TradeBotBuilderExtensions
    {
        public static List<ITradeAccount> AddTradeAccount(this List<ITradeAccount> tradeAccounts, ITradeAccount tradeAccount)
        {
            tradeAccounts.Add(tradeAccount);
            return tradeAccounts;
        }

        public static ITradeBotBuilder WithSellAccount(this ITradeBotBuilder tradeBotBuilder, ITradeAccount sellAccount)
        {
            tradeBotBuilder.SetSellAccount(sellAccount);

            return tradeBotBuilder;
        }

        public static ITradeBotBuilder WithBuyAccount(this ITradeBotBuilder tradeBotBuilder, ITradeAccount buyAccount)
        {
            tradeBotBuilder.SetBuyAccount(buyAccount);

            return tradeBotBuilder;
        }
    }
}