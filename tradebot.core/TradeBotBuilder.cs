using System;
using System.Collections.Generic;
using System.IO;
using DotNetCore.CAP;
using Hangfire;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using tradebot.core.helper;

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
        private Dictionary<string, string> _dockerSecrets = new Dictionary<string, string>();
        private readonly string[] _args;
        public TradeBotBuilder(string[] args)
        {
            this._args = args;
        }

        public ITradeBot Build()
        {
            this._configureServicesDelegate(this._serviceCollection);
            this._serviceCollection.AddSingleton<IConfiguration>(this._configuration);
            this._serviceProvider = this._serviceCollection.BuildServiceProvider();

            this._options = new TradeBotOptions(this._configuration);

            var logger = this._serviceProvider.GetRequiredService<ILogger<TradeBot>>();
            var loggerFactory = this._serviceProvider.GetRequiredService<ILoggerFactory>();
            this._logger = logger;

            var tradeFlowAnalyzer = this.AnalyzeTradeFlow();
            this._options.TradeAccounts = tradeFlowAnalyzer.TradeAccounts;
            this._options.BuyAccount = tradeFlowAnalyzer.BuyAccount;
            this._options.SellAccount = tradeFlowAnalyzer.SellAccount;

            var emailHelper = this._serviceProvider.GetRequiredService<IEmailHelper>();

            GlobalConfiguration.Configuration.UseSqlServerStorage(this._configuration["Hangfire:ConnectionString"]);
            var hangfireServer = new BackgroundJobServer();

            var tradeBot = new TradeBot(
                            this._options,
                            logger,
                            loggerFactory,
                            emailHelper,
                            hangfireServer);

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
        public ITradeBotBuilder AddDockerSecret(string secretKey, string configKey)
        {
            this._dockerSecrets.Add(secretKey, configKey);

            return this;
        }
        private ITradeBotBuilder UseDockerSecrets()
        {
            var secretsPath = "/run/secrets/";
            foreach (var secret in this._dockerSecrets)
            {
                if (File.Exists($"{secretsPath}{secret.Key}"))
                {
                    _logger.LogDebug($"Found {secret.Key} in Docker Secrets");
                    this.UseSetting(secret.Value, File.ReadAllText($"{secretsPath}{secret.Key}"));
                }
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
                                _serviceProvider.GetRequiredService<ILogger<BittrexAccount>>());

            var binanceTradingFee = Decimal.Parse(_configuration["BinanceAccount:TradingFee"]);
            var binanceBitcoinTransferFee = Decimal.Parse(_configuration["BinanceAccount:BitcoinTransferFee"]);
            var binanceAccount = new BinanceAccount(
                                this._options.Coin,
                                binanceTradingFee,
                                binanceBitcoinTransferFee,
                                _configuration["BinanceAccount:API_KEY"],
                                _configuration["BinanceAccount:API_SECRET"],
                                _serviceProvider.GetRequiredService<ILogger<BinanceAccount>>());

            var tradeAccounts = new List<ITradeAccount>()
                                    .AddTradeAccount(binanceAccount)
                                    .AddTradeAccount(bittrexAccount);

            var tradeFlowAnalyzer = new TradeFlowAnalyzer(this._options.TradeFlow, tradeAccounts);

            return tradeFlowAnalyzer;
        }

        public ITradeBotBuilder UseCommandLine()
        {
            var app = new CommandLineApplication();
            app.HelpOption();

            var options = new Dictionary<string, CommandOption>();
            options.Add("Coin", app.Option("-c|--coin <COIN>", "Trade Coin, e.g. ADA | XVG", CommandOptionType.SingleValue));
            options.Add("ExpectedDelta", app.Option("-d|--delta <DELTA>", "Expected Delta, e.g. 0.00000010", CommandOptionType.SingleValue));
            options.Add("IsAutoTrading", app.Option("-auto|--isautotrading", "Auto Trader Mode On/Off", CommandOptionType.NoValue));
            options.Add("TradeFlow", app.Option("-f|--tradeflow <BuyAtBinanceSellAtBittrex>", "BuyAtBinanceSellAtBittrex | SellAtBinanceBuyAtBittrex | AutoSwitch", CommandOptionType.SingleValue));
            options.Add("FixQuantity", app.Option("-q|--quantity <QUANTITY>", "Quantity to trade", CommandOptionType.SingleValue));
            options.Add("PlusPointToWin", app.Option("-w|--win <PlusPointToWin>", "Plus Point To Win e.g. 0.00000003", CommandOptionType.SingleValue));
            options.Add("TestMode", app.Option("-t|--testmode", "Test Mode On/Off", CommandOptionType.NoValue));

            app.OnExecute(() =>
            {
                foreach (var option in options)
                {
                    if (option.Value.HasValue())
                    {
                        if (option.Value.OptionType == CommandOptionType.NoValue)
                            this.UseSetting(option.Key, "true");
                        else
                            this.UseSetting(option.Key, option.Value.Value());
                    }
                }
            });

            app.Execute(this._args);

            if (app.OptionHelp.HasValue())
                Environment.Exit(1);

            return this;
        }

        ITradeBotBuilder UseCap(ICapPublisher publisher)
        {
            this._options.CapPublisher = publisher;
            return this;
        }
    }
}