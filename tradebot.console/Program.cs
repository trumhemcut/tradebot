using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using tradebot.core;

namespace tradebot.console
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }
        static void Main(string[] args)
        {
            var tradebot = CreateBot(args);
            tradebot.Execute().Wait();
        }

        public static ITradeBot CreateBot(string[] args)
        {
            var tradeBot = new TradeBotBuilder()
                            .Configure(config =>
                            {
                                config.SetBasePath(Directory.GetCurrentDirectory())
                                      .AddJsonFile("appsettings.json", optional: true)
                                      .AddJsonFile("appsettings.dev.json", optional: true)
                                      .AddEnvironmentVariables();
                            })
                            .ConfigureServices(services => {
                                services.AddSingleton<ILoggerFactory>(
                                    new LoggerFactory()
                                        .AddConsole()
                                        .AddDebug());
                            })
                            .UseCommandLine(args)
                            .Build();

            return tradeBot;
        }
    }
}
