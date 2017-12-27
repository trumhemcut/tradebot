using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace tradebot.core
{
    public interface ITradeBotBuilder
    {
        ITradeBotBuilder Configure(Action<IConfigurationBuilder> configureDelegate);
        ITradeBotBuilder ConfigureServices(Action<IServiceCollection> serviceCollectionDelegate);
        ITradeBot Build();
        ITradeBotBuilder UseSetting(string key, string value);
        ITradeBotBuilder AddDockerSecret(string secretKey, string configKey);
        ITradeBotBuilder SetSellAccount(ITradeAccount sellAccount);
        ITradeBotBuilder SetBuyAccount(ITradeAccount buyAccount);
    }
}