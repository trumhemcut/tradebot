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
        public decimal BitcoinTradingAmount { get; set; }
        public ITradeAccount BuyAccount { get; set; }
        public ITradeAccount SellAccount { get; set; }
        public int ResumeAfterExpectedDelta { get; protected set; }
        public decimal ExpectedDelta { get; protected set; }
        public string Coin { get; protected set; }
        public string EmailTo { get; set; }
        private static TradeBot _tradebot;
        public static TradeBot Instance
        {
            get
            {
                if (_tradebot == null)
                    _tradebot = new TradeBot();

                return _tradebot;
            }
        }
        public TradeBot()
        {
            this.Coin = "ADA";
        }
        public TradeBot(string coin,
                        decimal expectedDelta,
                        int resumeAfterExpectedData,
                        string emailTo)
        {
            this.Coin = coin;
            this.ExpectedDelta = expectedDelta;
            this.ResumeAfterExpectedDelta = resumeAfterExpectedData;
            this.EmailTo = emailTo;
        }
        public async Task Execute()
        {
            int errorCount = 0;
            while (true)
            {
                try
                {
                    var deltaPrices = await this.GetDelta();
                    Console.WriteLine($"Bittrex Price: {deltaPrices.Item1} - " +
                                      $"Binance Price: {deltaPrices.Item2} - " +
                                      $"Bid-Bid: {deltaPrices.Item1} - " +
                                      $"Bid-Ask: {deltaPrices.Item2}");

                    // Check to send notification
                    if (deltaPrices.Item1 >= this.ExpectedDelta)
                    {
                        var profit = this.CaculateProfit();
                        Console.WriteLine("Time to buy ...");
                        await EmailHelper.SendEmail($"[TradeBot] Delta = {deltaPrices.Item1}, Profit = {profit}", this.EmailTo, "Buy di pa");

                        Thread.Sleep(TimeSpan.FromMinutes(this.ResumeAfterExpectedDelta));
                    }

                    errorCount = 0;
                    Thread.Sleep(2000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("we saw an error. Please try again!");
                    Console.WriteLine(ex.Message);
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


        public async Task<Tuple<decimal, decimal>> GetDelta(){
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
            var bitcoinAmountAtSell = (this.BitcoinTradingAmount + this.SellAccount.Bitcoin.TradingFee) *
                                      (this.SellAccount.TradeCoin.TradingFee / 100);
            var coinAmountAtSell = bitcoinAmountAtSell / this.SellAccount.TradeCoin.CoinPrice.BidPrice;

            var cointAmountAtBuy = (this.BitcoinTradingAmount - (1 - this.BuyAccount.TradeCoin.TradingFee / 100)) *
                                   this.SellAccount.TradeCoin.CoinPrice.AskPrice;

            return cointAmountAtBuy - coinAmountAtSell;
        }
    }
}
