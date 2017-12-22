using System;
using Microsoft.Extensions.Configuration;
namespace tradebot.core
{
    public interface ITradeBotBuilder
    {
         ITradeBotBuilder Configure(Action<IConfigurationBuilder> configureDelegate);
    }
}