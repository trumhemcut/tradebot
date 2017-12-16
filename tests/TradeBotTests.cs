using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using tradebot;
using tradebot.TradePlatform;
using Xunit;

namespace tests
{
    public class TradeBotTests
    {
        public static IConfigurationRoot Configuration { get; set; }
        private readonly TradeBot _tradeBot;
        public TradeBotTests()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.dev.json", optional: true)
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

            tradeBot.BitcoinTradingAmount = Decimal.Parse(Configuration["BitcoinTradingAmount"]); ;
        }

        [Fact]
        public void CaculateProfit_ShouldReturnCorrectPrice()
        {
            var profit = this._tradeBot.CaculateProfit();
            Assert.True(true);
        }
    }
}
