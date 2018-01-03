using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using tradebot.core;
using tradebot.core.helper;

namespace tradebot.console
{
    class Program
    {
        static void Main(string[] args)
        {
            // Workaround Ctrl+C on Docker
            Console.CancelKeyPress += (_, __) => { Environment.Exit(1); };

            // Create bot
            var tradebot = CreateBot(args);
            tradebot.Execute().Wait();
        }

        public static ITradeBot CreateBot(string[] args)
        {
            var tradeBot = new TradeBotBuilder(args)
                            .Configure(config =>
                            {
                                config.SetBasePath(Directory.GetCurrentDirectory())
                                      .AddJsonFile("appsettings.json", optional: true)
                                      .AddJsonFile("appsettings.dev.json", optional: true)
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
            return tradeBot;
        }
    }
}
