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
        public static ITradeBotBuilder UseCommandLine(this ITradeBotBuilder tradeBotBuilder, string[] args)
        {
            var app = new CommandLineApplication();
            app.HelpOption();

            var options = new Dictionary<string, CommandOption>();
            options.Add("Coin", app.Option("-c|--coin <COIN>", "Trade Coin, e.g. ADA | XVG", CommandOptionType.SingleValue));
            options.Add("ExpectedDelta", app.Option("-d|--delta <DELTA>", "Expected Delta, e.g. 0.00000010", CommandOptionType.SingleValue));
            options.Add("IsAutoTrading", app.Option("-auto|--isautotrading", "Auto Trader Mode On/Off", CommandOptionType.NoValue));
            options.Add("TradeFlow", app.Option("-f|--tradeflow <BuyAtBinanceSellAtBittrex>", "BuyAtBinanceSellAtBittrex | SellAtBinanceBuyAtBittrex | AutoSwitch", CommandOptionType.SingleValue));
            options.Add("FixQuantity", app.Option("-q|--quantity <QUANTITY>", "Quantity to trade", CommandOptionType.SingleValue));
            options.Add("PlusPointToWin", app.Option("-w|--win <PlusPointToWin>", "Plus Point To Win e.g. 0.00000003", CommandOptionType.SingleValue));
            options.Add("TestMode", app.Option("-t|--testmode", "Test Mode On/Off", CommandOptionType.NoValue));

            app.OnExecute(() =>
            {
                foreach (var option in options)
                {
                    if (option.Value.HasValue())
                    {
                        if (option.Value.OptionType == CommandOptionType.NoValue)
                            tradeBotBuilder.UseSetting(option.Key, "true");
                        else
                            tradeBotBuilder.UseSetting(option.Key, option.Value.Value());
                    }
                }
            });

            app.Execute(args);

            if (app.OptionHelp.HasValue())
                Environment.Exit(1);

            return tradeBotBuilder;
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