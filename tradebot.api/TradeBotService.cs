using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using tradebot.core;
using tradebot.core.helper;

namespace tradebot.api
{
    public class TradeBotService : HostedService
    {
        private ITradeBot _tradeBot;
        public TradeBotService(ITradeBotBuilder tradeBotBuilder, ICapPublisher publisher)
        {
            this._tradeBot = tradeBotBuilder.Configure(config =>
                            {
                                config.SetBasePath(Directory.GetCurrentDirectory())
                                      .AddJsonFile("botsettings.json", optional: true)
                                      .AddJsonFile("botsettings.dev.json", optional: true)
                                      .AddEnvironmentVariables();
                            })
                            .ConfigureServices(services =>
                            {
                                services.AddLogging((loggerFactory) =>
                                                     loggerFactory
                                                        .AddConsole()
                                                        .AddDebug()
                                                        .SetMinimumLevel(LogLevel.Debug));
                                services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
                                services.AddSingleton<IEmailHelper, EmailHelper>();
                            })
                            .UseCommandLine()
                            .AddDockerSecret("Email.ApiKey", "Email:ApiKey")
                            .AddDockerSecret("BinanceAccount.API_KEY", "BinanceAccount:API_KEY")
                            .AddDockerSecret("BinanceAccount.API_SECRET", "BinanceAccount.API_SECRET")
                            .AddDockerSecret("BittrexAccount.API_KEY", "BittrexAccount.API_key")
                            .AddDockerSecret("BittrexAccount.API_SECRET", "BittrexAccount.API_SECRET")
                            .Build();
        }

        protected override Task ExecuteAsync(System.Threading.CancellationToken cancellationToken)
        {
            return this._tradeBot.Execute();
        }
    }
}