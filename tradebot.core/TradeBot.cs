using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using tradebot.core.helper;

namespace tradebot.core
{
    public class TradeBot : ITradeBot
    {
        private int _timeLeftToSendEmail;
        public ITradeAccount BuyAccount { get { return this._options.BuyAccount; } }
        public ITradeAccount SellAccount { get { return this._options.SellAccount; } }
        public int ResumeAfterExpectedDelta { get { return this._options.ResumeAfterExpectedDelta; } }
        public decimal ExpectedDelta { get { return this._options.ExpectedDelta; } }
        public bool IsAutoTrading { get { return this._options.IsAutoTrading; } }
        public string Coin { get { return this._options.Coin; } }
        public decimal PlusPointToWin { get { return this._options.PlusPointToWin; } }
        public TradeMode TradeMode { get { return this._options.TradeMode; } }
        public decimal FixedQuantity { get { return this._options.FixedQuantity; } }
        public bool TestMode { get { return this._options.InTestMode; } }
        private readonly TradeBotOptions _options;
        private readonly ILogger _logger;
        private readonly IEmailHelper _emailHelper;
        private readonly ILoggerFactory _loggerFactory;

        public TradeBot(
            TradeBotOptions options,
            ILogger<TradeBot> logger,
            ILoggerFactory loggerFactory,
            IEmailHelper emailHelper)
        {
            this._timeLeftToSendEmail = 0;
            this._options = options;
            this._logger = logger;
            this._emailHelper = emailHelper;

            this._logger.LogInformation("Bot is created successfully");
        }

        public async Task Execute()
        {
            int errorCount = 0;
            int transNumber = 0;
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
                        if (IsAutoTrading) await DoAutoTrading(transNumber, tradeInfo);

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
                    await Task.Delay(2000);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    if (ex.InnerException != null)
                        _logger.LogError(ex.InnerException.Message);

                    errorCount++;
                    if (errorCount > 100)
                    {
                        await this._emailHelper.SendEmail($"[TradeBot] Program Error, Please double check", ex.Message);
                        await Task.Delay(TimeSpan.FromMinutes(this.ResumeAfterExpectedDelta));
                    }
                    await Task.Delay(2000);
                }
            }
        }
        public async Task DoAutoTrading(int transNumber, TradeInfo tradeInfo)
        {
            _logger.LogInformation("AutoTrader: ON");
            if (!tradeInfo.Tradable)
            {
                _logger.LogWarning($"{this.Coin} - Not tradable: {tradeInfo.Message}");
                await this._emailHelper.SendEmail($"Not tradable: {tradeInfo.Message}", $"Not tradable: {tradeInfo.Message}");
            }
            else
            {
                try
                {
                    transNumber++;
                    var trans = $"{this.Coin}-{DateTime.Now.ToString("ddMMM")}-{transNumber}";
                    var autoTrader = new AutoTrader(
                        sellAccount: SellAccount,
                        buyAccount: BuyAccount,
                        tradeInfo: tradeInfo,
                        plusPointToWin: this.PlusPointToWin,
                        testMode: this.TestMode,
                        logger: this._loggerFactory.CreateLogger<AutoTrader>()
                    );
                    var tradeResult = await autoTrader.Trade();
                    if (tradeResult.Success)
                    {
                        await WaitUntilOrdersAreMatched(tradeInfo, trans);
                    }
                    else
                    {
                        await this._emailHelper.SendEmail(
                            $"{transNumber} - Trade error, please check!!!",
                            $"{tradeResult.ErrorMessage}");

                        _logger.LogCritical("Trading Error occurred! Exit for now...");
                        Environment.Exit(1);
                    }
                }
                catch (Exception ex)
                {
                    var message = ex.Message;
                    if (ex.InnerException != null)
                        message = message + "\n" + ex.InnerException.Message;

                    await this._emailHelper.SendEmail($"Trade Error! Please check!!!", message);
                }
            }
        }

        public async Task WaitUntilOrdersAreMatched(TradeInfo tradeInfo, string transNumber)
        {
            var sellWasMatched = false;
            var buyWasMatched = false;

            _logger.LogInformation($"{transNumber} - MATCH CHECKING");

            while (true)
            {
                if (await this.SellAccount.IsOrderMatched() && !sellWasMatched)
                {
                    Console.WriteLine("");
                    _logger.LogInformation($"[{transNumber}] - Sell order was matched.");
                    sellWasMatched = true;
                    await this._emailHelper.SendEmail(
                        $"[{DateTime.Now.ToString("dd/MM/yy hh:mm:ss")}] Sell order was matched",
                        $"SellPrice: {tradeInfo.SellPrice} - BuyPrice: {tradeInfo.BuyPrice}");
                }

                if (await this.BuyAccount.IsOrderMatched() && !buyWasMatched)
                {
                    Console.WriteLine("");
                    _logger.LogInformation($"[{transNumber}] - Buy order was matched.");
                    buyWasMatched = true;
                    await this._emailHelper.SendEmail(
                        $"[{transNumber}] - Buy order was matched",
                        $"SellPrice: {tradeInfo.SellPrice} - BuyPrice: {tradeInfo.BuyPrice}");
                }
                if (sellWasMatched && buyWasMatched)
                {
                    _logger.LogInformation($"[{transNumber}] - SUCCESSFUL! Sell & buy were matched.");
                    break;
                }
                Console.Write(".");

                await Task.Delay(300);
            }
        }

        private async Task SendMailIfTimePassed(TradeInfo tradeInfo, string content)
        {
            if (this._timeLeftToSendEmail <= 0)
            {
                var title = $"[{Coin}], Delta = {tradeInfo.DeltaBidBid}, Profit = {tradeInfo.CoinProfit}, Buy Qt.={tradeInfo.CoinQuantityAtBuy}";
                await this._emailHelper.SendEmail(title, content);
                this._timeLeftToSendEmail = 300;
            }
        }
    }
}