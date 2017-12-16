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
                .AddJsonFile("appsettings.json", optional:true)
                .AddJsonFile("appsettings.dev.json", optional:true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            decimal expectedDelta = Decimal.Parse(Configuration["ExpectedDelta"]);
            int resumeAfterExpectedDelta = Int32.Parse(Configuration["ResumeAfterDelta"]);
            string emailTo = Configuration["Email:EmailTo"];
            
            var coin = "ADA";
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
                                        sellAccount);

            tradeBot.BitcoinTradingAmount = Decimal.Parse(Configuration["BitcoinTradingAmount"]);;

            tradeBot.Execute().Wait();
        }
    }
}
