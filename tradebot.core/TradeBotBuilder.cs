using System;
using System.IO;
using Microsoft.Extensions.Configuration;
namespace tradebot.core
{
    public class TradeBotBuilder : ITradeBotBuilder
    {
        private IConfiguration _configuration;
        private IConfigurationBuilder _configurationBuilder;
        private TradeBotOptions _options;

        public TradeBotBuilder()
        {
        }

        public ITradeBot Build()
        {
            this._options = new TradeBotOptions(this._configuration);

            var tradeFlowAnalyzer = this.AnalyzeTradeFlow();
            this._options.BuyAccount = tradeFlowAnalyzer.BuyAccount;
            this._options.SellAccount = tradeFlowAnalyzer.SellAccount;

            var tradeBot = new TradeBot(_options);
            return tradeBot;
        }

        public ITradeBotBuilder Configure(Action<IConfigurationBuilder> configureDelegate)
        {
            this._configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory());

            configureDelegate(_configurationBuilder);
            this._configuration = this._configurationBuilder.Build();

            return this;
        }

        public ITradeBotBuilder UseSetting(string key, string value)
        {
            this._configuration[key] = value;

            return this;
        }

        public ITradeBotBuilder SetBuyAccount(ITradeAccount buyAccount)
        {
            this._options.BuyAccount = buyAccount;

            return this;
        }

        public ITradeBotBuilder SetSellAccount(ITradeAccount sellAccount)
        {
            this._options.SellAccount = sellAccount;

            return this;
        }

        private TradeFlowAnalyzer AnalyzeTradeFlow()
        {
            var bittrexTradingFee = Decimal.Parse(_configuration["BittrexAccount:TradingFee"]);
            var bittrexBitcoinTransferFee = Decimal.Parse(_configuration["BittrexAccount:BitcoinTransferFee"]);
            var bittrexAccount = new BittrexAccount(
                                this._options.Coin,
                                bittrexTradingFee,
                                bittrexBitcoinTransferFee,
                                _configuration["BittrexAccount:API_KEY"],
                                _configuration["BittrexAccount:API_SECRET"]);

            var binanceTradingFee = Decimal.Parse(_configuration["BinanceAccount:TradingFee"]);
            var binanceBitcoinTransferFee = Decimal.Parse(_configuration["BinanceAccount:BitcoinTransferFee"]);
            var binanceAccount = new BinanceAccount(
                                this._options.Coin,
                                binanceTradingFee,
                                binanceBitcoinTransferFee,
                                _configuration["BinanceAccount:API_KEY"],
                                _configuration["BinanceAccount:API_SECRET"]);


            var tradeFlowAnalyzer = new TradeFlowAnalyzer(
                this._options.TradeFlow, binanceAccount, bittrexAccount
            );

            return tradeFlowAnalyzer;
        }
    }
}