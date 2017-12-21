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
        public bool IsAutoTrading { get; set; }

        public TradeBot() => this.Coin = "ADA";
        public TradeBot(string coin,
                        decimal expectedDelta,
                        int resumeAfterExpectedData,
                        string emailTo,
                        ITradeAccount buyAccount,
                        ITradeAccount sellAccount,
                        bool isAutoTrading)
        {
            this.Coin = coin;
            this.ExpectedDelta = expectedDelta;
            this.ResumeAfterExpectedDelta = resumeAfterExpectedData;
            this.EmailTo = emailTo;
            this._timeLeftToSendEmail = 0;
            this.IsAutoTrading = true;
            this.BuyAccount = buyAccount;
            this.SellAccount = sellAccount;
        }
        public TradeInfo AnalyzeDelta()
        {
            var deltaBidAsk = this.SellAccount.TradeCoin.CoinPrice.BidPrice -
                              this.BuyAccount.TradeCoin.CoinPrice.AskPrice;
            var deltaBidBid = this.SellAccount.TradeCoin.CoinPrice.BidPrice -
                              this.BuyAccount.TradeCoin.CoinPrice.BidPrice;

            var bitcoinQuantityAtSell = (this.BitcoinTradingAmount + this.SellAccount.Bitcoin.TransferFee) *
                                      (1 + this.SellAccount.TradingFee / 100);
            var coinQuantityAtSell = bitcoinQuantityAtSell / this.SellAccount.TradeCoin.CoinPrice.BidPrice;

            var bitcoinQuantityAtBuy = this.BitcoinTradingAmount * (1 - this.BuyAccount.TradingFee / 100);
            var coinQuantityAtBuy = bitcoinQuantityAtBuy / this.BuyAccount.TradeCoin.CoinPrice.AskPrice;

            return new TradeInfo
            {
                DeltaBidAsk = deltaBidAsk,
                DeltaBidBid = deltaBidBid,
                BitcoinQuantityAtSell = bitcoinQuantityAtSell,
                CoinQuantityAtSell = coinQuantityAtSell,
                BitcoinQuantityAtBuy = bitcoinQuantityAtBuy,
                CoinQuantityAtBuy = coinQuantityAtBuy,
                ProfitQuantity = coinQuantityAtBuy - coinQuantityAtSell
            };
        }

        public async Task Execute()
        {
            int errorCount = 0;
            while (true)
            {
                try
                {
                    await UpdateCoinPrices();

                    var tradeInfo = AnalyzeDelta();

                    var content = $"Bittrex: {this.BuyAccount.TradeCoin.CoinPrice.BidPrice} * " +
                                      $"Binance: {this.SellAccount.TradeCoin.CoinPrice.BidPrice} * " +
                                      $"B-B: {tradeInfo.DeltaBidBid} * " +
                                      $"B-A: {tradeInfo.DeltaBidAsk} * " +
                                      $"Profit: {Math.Round(tradeInfo.ProfitQuantity)} * " +
                                      $"Sell Qt.: {Math.Round(tradeInfo.CoinQuantityAtSell)} * " +
                                      $"Buy Qt.: {Math.Round(tradeInfo.BitcoinQuantityAtBuy)}";
                    Console.WriteLine(content);

                    // Check to send notification
                    if (tradeInfo.DeltaBidBid >= this.ExpectedDelta)
                    {
                        Console.Write("Time to buy ...");
                        if (IsAutoTrading)
                        {
                            Console.WriteLine("AutoTrader is initializing...");
                            var autoTrader = new AutoTrader(
                                this.SellAccount,
                                this.BuyAccount,
                                tradeInfo
                            ).Trade();

                        }
                        Console.Write($"Send email in {_timeLeftToSendEmail}s...");
                        await SendMailIfTimePassed(tradeInfo, content);
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

        private async Task SendMailIfTimePassed(TradeInfo tradeInfo, string content)
        {
            if (this._timeLeftToSendEmail <= 0)
            {
                await EmailHelper.SendEmail($"[{Coin}], Delta = {tradeInfo.DeltaBidBid}, Profit = {tradeInfo.ProfitQuantity}, Buy Qt.={tradeInfo.CoinQuantityAtBuy}", this.EmailTo, content);
                this._timeLeftToSendEmail = 300;
            }
        }

        public async Task UpdateCoinPrices()
        {
            await Task.WhenAll(this.BuyAccount.UpdatePrices(), this.SellAccount.UpdatePrices());
        }
    }
}
