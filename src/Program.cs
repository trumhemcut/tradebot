﻿using System;
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

            var buyAccountTradingFee = Decimal.Parse(Configuration["BuyAccount:TradingFee"]);
            var buyAccountBitcoinTransferFee = Decimal.Parse(Configuration["BuyAccount:BitcoinTransferFee"]);
            var buyAccount = new BittrexAccount(
                                coin,
                                buyAccountTradingFee,
                                buyAccountBitcoinTransferFee);

            var sellAccountTradingFee = Decimal.Parse(Configuration["SellAccount:TradingFee"]);
            var sellAccountBitcoinTransferFee = Decimal.Parse(Configuration["SellAccount:BitcoinTransferFee"]);
            var sellAccount = new BinanceAccount(
                                coin,
                                sellAccountTradingFee,
                                sellAccountBitcoinTransferFee);

            var tradeBot = new TradeBot(coin,
                                        expectedDelta,
                                        resumeAfterExpectedDelta,
                                        emailTo,
                                        buyAccount,
                                        sellAccount,
                                        isAutoTrading);

            tradeBot.BitcoinTradingAmount = Decimal.Parse(Configuration["BitcoinTradingAmount"]);

            tradeBot.Execute().Wait();
        }
    }
}
