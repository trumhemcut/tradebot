using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using tradebot.TradePlatform;

namespace tradebot
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.dev.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            var tradeFlow = TradeFlow.SellAtBinanceBuyAtBittrex;
            if (args.Length > 3)
                tradeFlow = (TradeFlow)Enum.Parse(typeof(TradeFlow), args[3]);
            else
                tradeFlow = (TradeFlow)Enum.Parse(typeof(TradeFlow), Configuration["TradeFlow"]);

            var isAutoTrading = false;
            if (args.Length > 2)
                isAutoTrading = Boolean.Parse(args[2]);
            else
                isAutoTrading = Boolean.Parse(Configuration["IsAutoTrading"]);

            decimal expectedDelta = 0;
            if (args.Length > 1)
                expectedDelta = Decimal.Parse(args[1]);
            else
                expectedDelta = Decimal.Parse(Configuration["ExpectedDelta"]);

            int resumeAfterExpectedDelta = Int32.Parse(Configuration["ResumeAfterDelta"]);
            string emailTo = Configuration["Email:EmailTo"];

            var coin = "ADA";
            if (args.Length > 0)
            {
                coin = args[0];
                if (!"ADA|XLM|XRP".Contains(coin))
                    throw new Exception("Coin is not supported!");
            }

            var bittrexTradingFee = Decimal.Parse(Configuration["BittrexAccount:TradingFee"]);
            var bittrexBitcoinTransferFee = Decimal.Parse(Configuration["BittrexAccount:BitcoinTransferFee"]);
            var bittrexAccount = new BittrexAccount(
                                coin,
                                bittrexTradingFee,
                                bittrexBitcoinTransferFee);

            var binanceTradingFee = Decimal.Parse(Configuration["BinanceAccount:TradingFee"]);
            var binanceBitcoinTransferFee = Decimal.Parse(Configuration["BinanceAccount:BitcoinTransferFee"]);
            var binanceAccount = new BinanceAccount(
                                coin,
                                binanceTradingFee,
                                binanceBitcoinTransferFee);

            

            var tradeFlowAnalyzer = new TradeFlowAnalyzer(
                tradeFlow, binanceAccount, bittrexAccount
            );

            var tradeBot = new TradeBot(coin,
                            expectedDelta,
                            resumeAfterExpectedDelta,
                            emailTo,
                            tradeFlowAnalyzer.BuyAccount,
                            tradeFlowAnalyzer.SellAccount,
                            isAutoTrading);

            tradeBot.BitcoinTradingAmount = Decimal.Parse(Configuration["BitcoinTradingAmount"]);

            tradeBot.Execute().Wait();
        }
    }
}
