using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using tradebot.TradePlatform;

namespace tradebot
{
    public class TradeBot
    {
        private readonly TimeSpan _timeLeftToSendEmail;
        public decimal BitcoinTradingAmount { get; set; }
        public ITradeAccount BuyAccount { get; set; }
        public ITradeAccount SellAccount { get; set; }
        public int ResumeAfterExpectedDelta { get; protected set; }
        public decimal ExpectedDelta { get; protected set; }
        public string Coin { get; protected set; }
        public string EmailTo { get; set; }
        private static TradeBot _tradebot;

        public TradeBot()
        {
            this.Coin = "ADA";
        }
        public TradeBot(string coin,
                        decimal expectedDelta,
                        int resumeAfterExpectedData,
                        string emailTo,
                        ITradeAccount buyAccount,
                        ITradeAccount sellAccount)
        {
            this.Coin = coin;
            this.ExpectedDelta = expectedDelta;
            this.ResumeAfterExpectedDelta = resumeAfterExpectedData;
            this.EmailTo = emailTo;
            this.SellAccount = sellAccount;
            this.BuyAccount = buyAccount;
        }
        public async Task Execute()
        {
            int errorCount = 0;
            while (true)
            {
                try
                {
                    var deltaPrices = await this.GetDelta();
                    var profit = this.CaculateProfit();
                    Console.WriteLine($"Bittrex: {deltaPrices.Item1} - " +
                                      $"Binance: {deltaPrices.Item2} - " +
                                      $"Bid-Bid: {deltaPrices.Item1} - " +
                                      $"Bid-Ask: {deltaPrices.Item2} - " +
                                      $"Profit: {profit}");

                    // Check to send notification
                    if (deltaPrices.Item1 >= this.ExpectedDelta)
                    {
                        Console.WriteLine("Time to buy ...");
                        await SendMailIfTimePassed(deltaPrices.Item1, profit);
                    }

                    errorCount = 0;
                    Thread.Sleep(2000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("we saw an error. Please try again!");
                    if (ex.InnerException != null)
                        Console.WriteLine(ex.InnerException.Message);
                    errorCount++;
                    if (errorCount > 100)
                    {
                        await EmailHelper.SendEmail($"[TradeBot] Program Error, Please double check", this.EmailTo, ex.Message);
                        Thread.Sleep(TimeSpan.FromMinutes(this.ResumeAfterExpectedDelta));
                    }
                    Thread.Sleep(2000);
                }
            }
        }

        private async Task SendMailIfTimePassed(decimal delta, decimal profit)
        {
            await EmailHelper.SendEmail($"[TradeBot] Delta = {delta}, Profit = {profit}", this.EmailTo, "Buy di pa");
        }

        public async Task<Tuple<decimal, decimal>> GetDelta()
        {
            await UpdateCoinPrices();

            var deltaBidAsk = this.SellAccount.TradeCoin.CoinPrice.BidPrice -
                              this.BuyAccount.TradeCoin.CoinPrice.AskPrice;
            var deltaBidBid = this.SellAccount.TradeCoin.CoinPrice.BidPrice -
                              this.BuyAccount.TradeCoin.CoinPrice.BidPrice;

            return new Tuple<decimal, decimal>(deltaBidBid, deltaBidAsk);
        }

        public async Task UpdateCoinPrices()
        {
            await Task.WhenAll(this.BuyAccount.UpdatePrices(), this.SellAccount.UpdatePrices());
        }

        public decimal CaculateProfit()
        {
            var bitcoinAmountAtSell = (this.BitcoinTradingAmount + this.SellAccount.TradingFee) *
                                      (this.SellAccount.TradingFee / 100);
            var coinAmountAtSell = bitcoinAmountAtSell / this.SellAccount.TradeCoin.CoinPrice.BidPrice;

            var cointAmountAtBuy = (this.BitcoinTradingAmount - (1 - this.BuyAccount.TradingFee / 100)) *
                                   this.SellAccount.TradeCoin.CoinPrice.AskPrice;

            return cointAmountAtBuy - coinAmountAtSell;
        }
    }
}
