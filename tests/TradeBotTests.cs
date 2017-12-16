using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using tradebot;
using tradebot.TradePlatform;
using Xunit;

namespace tests
{
    public class TradeBotTests
    {
        public static IConfigurationRoot Configuration { get; set; }
        private readonly TradeBot tradeBot;
        public TradeBotTests()
        {
            decimal expectedDelta = 0.00000040M;
            int resumeAfterExpectedDelta = 5;
            string emailTo = "trumhemcut@hotmail.com";

            var coin = "ADA";
            var buyAccountTradingFee = 0.25M;
            var buyAccountBitcoinTransferFee = 0.0005M;
            var buyAccount = new BittrexAccount(
                                coin,
                                buyAccountTradingFee,
                                buyAccountBitcoinTransferFee);

            var sellAccountTradingFee = 0.1M;
            var sellAccountBitcoinTransferFee = 0.0005M;
            var sellAccount = new BinanceAccount(
                                coin,
                                sellAccountTradingFee,
                                sellAccountBitcoinTransferFee);

            this.tradeBot = new TradeBot(coin,
                                        expectedDelta,
                                        resumeAfterExpectedDelta,
                                        emailTo,
                                        buyAccount,
                                        sellAccount);

            tradeBot.BitcoinTradingAmount = 0.5M;
        }

        [Fact]
        public void CaculateProfit_ShouldReturnCorrectPrice()
        {
            // Given
            this.tradeBot.BuyAccount.TradeCoin.CoinPrice = new CoinPrice
            {
                AskPrice = 0.00001348M,
                BidPrice = 0.00001348M,
                LastPrice = 0.00001348M,
                RetrivalTime = DateTime.Now
            };

            this.tradeBot.SellAccount.TradeCoin.CoinPrice = new CoinPrice
            {
                AskPrice = 0.00001375M,
                BidPrice = 0.00001375M,
                LastPrice = 0.00001375M,
                RetrivalTime = DateTime.Now
            };
            
            // When
            var profit = this.tradeBot.CaculateProfit();

            // Then
            Assert.True(Math.Floor(profit) == 562);
        }
    }
}
