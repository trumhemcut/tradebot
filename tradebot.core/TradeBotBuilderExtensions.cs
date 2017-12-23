using System;

namespace tradebot.core
{
    public static class TradeBotBuilderExtensions
    {
        public static ITradeBotBuilder UseCommandLine(this ITradeBotBuilder tradeBotBuilder, string[] args)
        {
            // Example of CLI
            // dotnet run ADA 0.0000100

            if (args.Length > 0)
                tradeBotBuilder.UseSetting("Email:EmailTo", args[0]);

            if (args.Length > 1)
                tradeBotBuilder.UseSetting("ExpectedDelta", args[1]);

            if (args.Length > 2)
                tradeBotBuilder.UseSetting("IsAutoTrading", args[2]);

            if (args.Length > 3)
                tradeBotBuilder.UseSetting("TradeFlow", args[3]);

            return tradeBotBuilder;
        }

        public static ITradeBotBuilder WithSellAccount(this ITradeBotBuilder tradeBotBuilder, ITradeAccount sellAccount){
            tradeBotBuilder.SetSellAccount(sellAccount);

            return tradeBotBuilder;
        }

        public static ITradeBotBuilder WithBuyAccount(this ITradeBotBuilder tradeBotBuilder, ITradeAccount buyAccount){
            tradeBotBuilder.SetBuyAccount(buyAccount);

            return tradeBotBuilder;
        }

    }
}