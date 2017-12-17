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
        private int _timeLeftToSendEmail;
        public decimal BitcoinTradingAmount { get; set; }
        public ITradeAccount BuyAccount { get; set; }
        public ITradeAccount SellAccount { get; set; }
        public int ResumeAfterExpectedDelta { get; protected set; }
        public decimal ExpectedDelta { get; protected set; }
        public string Coin { get; protected set; }
        public string EmailTo { get; set; }
        private static TradeBot _tradebot;

        public TradeBot() => this.Coin = "ADA";
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
            this._timeLeftToSendEmail = 0;
        }
        public async Task Execute()
        {
            int errorCount = 0;
            while (true)
            {
                try
                {
                    await UpdateCoinPrices();
                    var deltaPrices = this.GetDelta();
                    var profit = this.CaculateProfit();

                    var content = $"Bittrex: {this.BuyAccount.TradeCoin.CoinPrice.BidPrice} * " +
                                      $"Binance: {this.SellAccount.TradeCoin.CoinPrice.BidPrice} * " +
                                      $"Bid-Bid: {deltaPrices.Item1} * " +
                                      $"Bid-Ask: {deltaPrices.Item2} * " +
                                      $"Profit: {Math.Round(profit.Item1)} * " +
                                      $"AmountToSell: {Math.Round(profit.Item2)}";
                    Console.WriteLine(content);

                    // Check to send notification
                    if (deltaPrices.Item1 >= this.ExpectedDelta)
                    {
                        Console.Write("Time to buy ...");
                        Console.Write($"Send email in {_timeLeftToSendEmail}s...");
                        await SendMailIfTimePassed(deltaPrices.Item1, profit, content);
                    }

                    errorCount = 0;
                    this._timeLeftToSendEmail -= 2;
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

        private async Task SendMailIfTimePassed(decimal delta, Tuple<decimal, decimal> profit, string content)
        {
            if (this._timeLeftToSendEmail <= 0)
            {
                await EmailHelper.SendEmail($"[TradeBot] Delta = {delta}, Profit = {profit.Item1}, AmountToBuy={profit.Item2}", this.EmailTo, content);
                this._timeLeftToSendEmail = 300;
            }
            
        }

        public Tuple<decimal, decimal> GetDelta()
        {
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

        public Tuple<decimal, decimal> CaculateProfit()
        {
            var bitcoinAmountAtSell = (this.BitcoinTradingAmount + this.SellAccount.Bitcoin.TransferFee) *
                                      (1 + this.SellAccount.TradingFee / 100);
            var coinAmountAtSell = bitcoinAmountAtSell / this.SellAccount.TradeCoin.CoinPrice.BidPrice;

            var bitcoinAmountAtBuy = this.BitcoinTradingAmount * (1 - this.BuyAccount.TradingFee / 100);
            var coinAmountAtBuy = bitcoinAmountAtBuy / this.BuyAccount.TradeCoin.CoinPrice.AskPrice;

            return new Tuple<decimal, decimal>(coinAmountAtBuy - coinAmountAtSell, coinAmountAtSell);
        }
    }
}
