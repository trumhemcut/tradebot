using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace tradebot.core
{
    public class TradeBotBuilder : ITradeBotBuilder
    {
        private IConfiguration _configuration;
        private IConfigurationBuilder _configurationBuilder;
        private TradeBotOptions _options;
        private ServiceCollection _serviceCollection = new ServiceCollection();
        private Action<ServiceCollection> _configureServicesDelegate;
        private ILogger _logger;
        private IServiceProvider _serviceProvider;
        public TradeBotBuilder()
        {
        }

        public ITradeBot Build()
        {
            this._configureServicesDelegate(this._serviceCollection);
            this._serviceProvider = this._serviceCollection.BuildServiceProvider();

            this._options = new TradeBotOptions(this._configuration);

            // Configure logging
            var logger = this._serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Bot");
            this._logger = logger;

            var tradeFlowAnalyzer = this.AnalyzeTradeFlow();
            this._options.BuyAccount = tradeFlowAnalyzer.BuyAccount;
            this._options.SellAccount = tradeFlowAnalyzer.SellAccount;

            var tradeBot = new TradeBot(_options, logger);

            return tradeBot;
        }

        public ITradeBotBuilder ConfigureServices(Action<IServiceCollection> serviceCollectionDelegate)
        {
            this._configureServicesDelegate = serviceCollectionDelegate;

            return this;
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
        public ITradeBotBuilder UseDockerSecrets()
        {
            var secrets = new Dictionary<string, string>();

            secrets.Add("Email.ApiKey", "Email:ApiKey");
            secrets.Add("BinanceAccount.API_KEY", "BinanceAccount:API_KEY");
            secrets.Add("BinanceAccount.API_SECRET", "BinanceAccount.API_SECRET");
            secrets.Add("BittrexAccount.API_KEY", "BittrexAccount.API_key");
            secrets.Add("BittrexAccount.API_SECRET", "BittrexAccount.API_SECRET");
            
            foreach (var secret in secrets)
            {
                this.UseDockerSecretKey(secret.Key, secret.Value);
            }

            return this;
        }

        private ITradeBotBuilder UseDockerSecretKey(string secretKey, string configKey)
        {
            var secretsPath = "/run/secrets/";

            if (File.Exists($"{secretsPath}{secretKey}"))
            {
                _logger.LogDebug($"Found {secretKey} in Docker Secrets");
                this.UseSetting(configKey, File.ReadAllText($"{secretsPath}{secretKey}"));
            }

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
                                _configuration["BittrexAccount:API_SECRET"],
                                _serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Bittrex"));

            var binanceTradingFee = Decimal.Parse(_configuration["BinanceAccount:TradingFee"]);
            var binanceBitcoinTransferFee = Decimal.Parse(_configuration["BinanceAccount:BitcoinTransferFee"]);
            var binanceAccount = new BinanceAccount(
                                this._options.Coin,
                                binanceTradingFee,
                                binanceBitcoinTransferFee,
                                _configuration["BinanceAccount:API_KEY"],
                                _configuration["BinanceAccount:API_SECRET"],
                                _serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Binance"));


            var tradeFlowAnalyzer = new TradeFlowAnalyzer(
                this._options.TradeFlow, binanceAccount, bittrexAccount
            );

            return tradeFlowAnalyzer;
        }
    }
}