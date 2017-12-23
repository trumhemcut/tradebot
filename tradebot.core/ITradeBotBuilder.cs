using System;
using Microsoft.Extensions.Configuration;
namespace tradebot.core
{
    public interface ITradeBotBuilder
    {
         ITradeBotBuilder Configure(Action<IConfigurationBuilder> configureDelegate);
         ITradeBot Build();
         ITradeBotBuilder UseSetting(string key, string value);
         ITradeBotBuilder SetSellAccount(ITradeAccount sellAccount);
        ITradeBotBuilder SetBuyAccount(ITradeAccount buyAccount);
    }
}