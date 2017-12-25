using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace tradebot.core
{
    public class TradeBot : ITradeBot
    {
        private int _timeLeftToSendEmail;
        public ITradeAccount BuyAccount { get { return this._options.BuyAccount; } }
        public ITradeAccount SellAccount { get { return this._options.SellAccount; } }
        public string MailApiKey { get { return this._options.MailApiKey; } }
        public decimal BitcoinTradingAmount { get { return this._options.BitcoinTradingAmount; } }
        public int ResumeAfterExpectedDelta { get { return this._options.ResumeAfterExpectedDelta; } }
        public decimal ExpectedDelta { get { return this._options.ExpectedDelta; } }
        public string EmailTo { get { return this._options.EmailTo; } }
        public bool IsAutoTrading { get { return this._options.IsAutoTrading; } }
        public string Coin { get { return this._options.Coin; } }
        public TradeMode TradeMode { get { return this._options.TradeMode; } }
        private readonly TradeBotOptions _options;
        public TradeBot() { }
        public TradeBot(TradeBotOptions options)
        {
            this._timeLeftToSendEmail = 0;
            this._options = options;
        }
        public async Task Execute()
        {
            int errorCount = 0;
            while (true)
            {
                try
                {
                    await UpdateCoinPrices();
                    await UpdateBalances();

                    TradeInfo tradeInfo = null;

                    switch (this.TradeMode)
                    {
                        case TradeMode.FinegrainedTrade:
                            tradeInfo = new TradeInfoAnalyzer(this._options)
                                            .AnalyzeDeltaFinegrainedMode();
                            break;
                        case TradeMode.NormalTrade:
                        default:
                            tradeInfo = new TradeInfoAnalyzer(this._options)
                                            .AnalyzeDeltaNormalMode();
                            break;
                    }

                    var content = $"{Coin} - {this.BuyAccount.GetType().Name}: {this.BuyAccount.TradeCoin.CoinPrice.AskPrice} * " +
                                  $"{this.SellAccount.GetType().Name}: {this.SellAccount.TradeCoin.CoinPrice.BidPrice} * " +
                                  $"B-A: {tradeInfo.DeltaBidAsk} * " +
                                  $"BTC Profit: {Math.Round(tradeInfo.BitcoinProfit, 6)} * " +
                                  $"Coin Qt.: {Math.Round(tradeInfo.CoinQuantityAtSell)} * " +
                                  $"BTC Qt.: {Math.Round(tradeInfo.BitcoinQuantityAtBuy, 4)}";
                    Console.WriteLine(content);

                    // Check to send notification
                    if (tradeInfo.DeltaBidAsk >= this.ExpectedDelta)
                    {
                        if (IsAutoTrading)
                        {
                            Console.WriteLine("AutoTrader: ON");
                            if (!tradeInfo.Tradable)
                            {
                                Console.WriteLine($"Not tradable: {tradeInfo.Message}");
                                await EmailHelper.SendEmail(
                                    $"Not tradable: {tradeInfo.Message}",
                                    this.EmailTo,
                                    $"Not tradable: {tradeInfo.Message}",
                                    this.MailApiKey);
                            }
                            else
                            {
                                var autoTrader = new AutoTrader(
                                    sellAccount: SellAccount,
                                    buyAccount: BuyAccount,
                                    tradeInfo: tradeInfo
                                );
                                try
                                {
                                    await autoTrader.Trade();
                                }
                                catch (Exception ex)
                                {
                                    var message = ex.Message;
                                    if (ex.InnerException != null)
                                        message = message + "\n" + ex.InnerException.Message;

                                    await EmailHelper.SendEmail(
                                        $"Trade Error! Please check!!!",
                                        this.EmailTo,
                                        message,
                                        this.MailApiKey);
                                }

                                await WaitUntilOrdersAreMatched(tradeInfo);

                                await EmailHelper.SendEmail(
                                    $"Trade successfully, please check!!!",
                                    this.EmailTo,
                                    "Trade successfully :)",
                                    this.MailApiKey);
                            }
                        }
                        Console.Write("Time to buy ...");
                        Console.Write($"Send email in {_timeLeftToSendEmail}s...\n");
                        await SendMailIfTimePassed(tradeInfo, content);
                    }
                    else
                    {
                        Console.WriteLine($"Expected Delta: {ExpectedDelta}");
                    }

                    errorCount = 0;
                    this._timeLeftToSendEmail -= 2;
                    Thread.Sleep(2000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("we saw an error. Please try again!");
                    Console.WriteLine(ex.Message);
                    if (ex.InnerException != null)
                        Console.WriteLine(ex.InnerException.Message);

                    errorCount++;
                    if (errorCount > 100)
                    {
                        await EmailHelper.SendEmail(
                            $"[TradeBot] Program Error, Please double check",
                            this.EmailTo,
                            ex.Message,
                            this.MailApiKey);
                        Thread.Sleep(TimeSpan.FromMinutes(this.ResumeAfterExpectedDelta));
                    }
                    Thread.Sleep(2000);
                }
            }
        }

        public async Task WaitUntilOrdersAreMatched(TradeInfo tradeInfo)
        {
            var sellWasMatched = false;
            var buyWasMatched = false;

            Console.WriteLine("MATCH CHECKING ");

            while (true)
            {
                if (await this.SellAccount.IsOrderMatched() && !sellWasMatched)
                {
                    Console.WriteLine("");
                    Console.WriteLine("Sell order was matched.");
                    sellWasMatched = true;
                }

                if (await this.BuyAccount.IsOrderMatched() && !buyWasMatched)
                {
                    Console.WriteLine("");
                    Console.WriteLine("Buy order was matched.");
                    buyWasMatched = true;
                }
                if (sellWasMatched && buyWasMatched)
                {
                    Console.WriteLine("SUCCESSFUL! Sell & buy were matched.");
                    break;
                }
                Console.Write(".");

                Thread.Sleep(500);
            }
        }

        private async Task SendMailIfTimePassed(TradeInfo tradeInfo, string content)
        {
            if (this._timeLeftToSendEmail <= 0)
            {
                await EmailHelper.SendEmail(
                    $"[{Coin}], Delta = {tradeInfo.DeltaBidBid}, Profit = {tradeInfo.CoinProfit}, Buy Qt.={tradeInfo.CoinQuantityAtBuy}",
                    this.EmailTo,
                    content,
                    this.MailApiKey);
                this._timeLeftToSendEmail = 300;
            }
        }

        public async Task UpdateCoinPrices() =>
            await Task.WhenAll(this.BuyAccount.UpdatePrices(), this.SellAccount.UpdatePrices());

        public async Task UpdateBalances() =>
            await Task.WhenAll(this.BuyAccount.UpdateBalances(), this.SellAccount.UpdateBalances());
    }
}
