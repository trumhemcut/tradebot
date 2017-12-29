using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace tradebot.core
{
    public class TradeBot : ITradeBot
    {
        private int _timeLeftToSendEmail;
        public ITradeAccount BuyAccount { get { return this._options.BuyAccount; } }
        public ITradeAccount SellAccount { get { return this._options.SellAccount; } }
        public List<ITradeAccount> TradeAccounts { get { return this._options.TradeAccounts; } }
        public string MailApiKey { get { return this._options.MailApiKey; } }
        public decimal BitcoinTradingAmount { get { return this._options.BitcoinTradingAmount; } }
        public int ResumeAfterExpectedDelta { get { return this._options.ResumeAfterExpectedDelta; } }
        public decimal ExpectedDelta { get { return this._options.ExpectedDelta; } }
        public string EmailTo { get { return this._options.EmailTo; } }
        public bool IsAutoTrading { get { return this._options.IsAutoTrading; } }
        public string Coin { get { return this._options.Coin; } }
        public decimal PlusPointToWin { get { return this._options.PlusPointToWin; } }
        public TradeMode TradeMode { get { return this._options.TradeMode; } }
        public decimal FixedQuantity { get { return this._options.FixedQuantity; } }
        public bool TestMode { get { return this._options.InTestMode; } }
        private readonly TradeBotOptions _options;
        private readonly ILogger _logger;
        
        public TradeBot() { }

        public TradeBot(TradeBotOptions options, ILogger logger)
        {
            this._timeLeftToSendEmail = 0;
            this._options = options;
            this._logger = logger;

            this._logger.LogInformation("Bot is created successfully");
        }
        public async Task Execute()
        {
            int errorCount = 0;
            while (true)
            {
                try
                {
                    TradeInfo tradeInfo = null;
                    var tradeInfoAnalyzer = new TradeInfoAnalyzer(this._options);
                    await tradeInfoAnalyzer.UpdateCoinPrices();
                    if (!await tradeInfoAnalyzer.UpdateBalances())
                        continue;

                    switch (this.TradeMode)
                    {
                        case TradeMode.FinegrainedTrade:
                            tradeInfo = tradeInfoAnalyzer.AnalyzeDeltaFinegrainedMode();
                            break;
                        case TradeMode.FixedMode:
                            tradeInfo = tradeInfoAnalyzer.AnalyzeDataFixedMode(this.FixedQuantity);
                            break;
                        case TradeMode.NormalTrade:
                        default:
                            tradeInfo = tradeInfoAnalyzer.AnalyzeDeltaNormalMode();
                            break;
                    }

                    var content = $"{Coin} - {this.BuyAccount.GetType().Name}: {this.BuyAccount.TradeCoin.CoinPrice.AskPrice} * " +
                                  $"{this.SellAccount.GetType().Name}: {this.SellAccount.TradeCoin.CoinPrice.BidPrice} * " +
                                  $"B-A: {tradeInfo.DeltaBidAsk} * " +
                                  $"BTC Profit: {Math.Round(tradeInfo.BitcoinProfit, 6)} * " +
                                  $"Coin Qt.: {Math.Round(tradeInfo.CoinQuantityAtSell)} * " +
                                  $"BTC Qt.: {Math.Round(tradeInfo.BitcoinQuantityAtBuy, 4)}";
                    _logger.LogInformation(content);

                    // Check to send notification
                    if (tradeInfo.DeltaBidAsk >= this.ExpectedDelta)
                    {
                        if (IsAutoTrading)
                        {
                            _logger.LogInformation("AutoTrader: ON");
                            if (!tradeInfo.Tradable)
                            {
                                _logger.LogWarning($"Not tradable: {tradeInfo.Message}");
                                await EmailHelper.SendEmail(
                                    $"Not tradable: {tradeInfo.Message}",
                                    this.EmailTo,
                                    $"Not tradable: {tradeInfo.Message}",
                                    this.MailApiKey);
                            }
                            else
                            {
                                try
                                {
                                    var autoTrader = new AutoTrader(
                                        sellAccount: SellAccount,
                                        buyAccount: BuyAccount,
                                        tradeInfo: tradeInfo,
                                        plusPointToWin: this.PlusPointToWin,
                                        testMode: this.TestMode,
                                        logger: _logger
                                    );
                                    if (await autoTrader.Trade())
                                    {
                                        await WaitUntilOrdersAreMatched(tradeInfo);

                                        await EmailHelper.SendEmail(
                                            $"Trade successfully, please check!!!",
                                            this.EmailTo,
                                            "Trade successfully :)",
                                            this.MailApiKey);
                                    }
                                    else
                                    {
                                        await EmailHelper.SendEmail(
                                            $"Trade error, please check!!!",
                                            this.EmailTo,
                                            "Trade error :(",
                                            this.MailApiKey);

                                        _logger.LogCritical("Trading Error occurred! Exit for now...");
                                        Environment.Exit(1);
                                    }
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
                            }

                            //TODO: Temporarily run only once
                            //Console.WriteLine("Exit after trading successfully");
                            //return;
                        }
                        _logger.LogInformation("Time to buy ...");
                        _logger.LogInformation($"Send email in {_timeLeftToSendEmail}s...\n");
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
                    _logger.LogError(ex.Message);
                    if (ex.InnerException != null)
                        _logger.LogError(ex.InnerException.Message);

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

            _logger.LogInformation("MATCH CHECKING");
            await EmailHelper.SendEmail(
                $"[{DateTime.Now.ToString("dd/MM/yy hh:mm:ss")}] Checking matching...",
                this.EmailTo,
                $"SellPrice: {tradeInfo.SellPrice} - BuyPrice: {tradeInfo.BuyPrice}",
                this.MailApiKey);

            while (true)
            {
                if (await this.SellAccount.IsOrderMatched() && !sellWasMatched)
                {
                    Console.WriteLine("");
                    _logger.LogInformation("Sell order was matched.");
                    sellWasMatched = true;
                    await EmailHelper.SendEmail(
                        $"[{DateTime.Now.ToString("dd/MM/yy hh:mm:ss")}] Sell order was matched",
                        this.EmailTo,
                        $"SellPrice: {tradeInfo.SellPrice} - BuyPrice: {tradeInfo.BuyPrice}",
                        this.MailApiKey);
                }

                if (await this.BuyAccount.IsOrderMatched() && !buyWasMatched)
                {
                    Console.WriteLine("");
                    _logger.LogInformation("Buy order was matched.");
                    buyWasMatched = true;
                    await EmailHelper.SendEmail(
                        $"[{DateTime.Now.ToString("dd/MM/yy hh:mm:ss")}] Buy order was matched",
                        this.EmailTo,
                        $"SellPrice: {tradeInfo.SellPrice} - BuyPrice: {tradeInfo.BuyPrice}",
                        this.MailApiKey);
                }
                if (sellWasMatched && buyWasMatched)
                {
                    _logger.LogInformation("SUCCESSFUL! Sell & buy were matched.");
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
    }
}
