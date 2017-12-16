using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace tradebot
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.dev.json")
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            decimal expectedDelta = Decimal.Parse(Configuration["ExpectedDelta"]);
            string emailTo = Configuration["Email:EmailTo"];

            TradeBot.Instance.Execute(expectedDelta, emailTo).Wait();
        }
    }
}
