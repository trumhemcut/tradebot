using System;
using Microsoft.Extensions.Configuration;
namespace tradebot.core
{
    public class TradeBotBuilder : ITradeBotBuilder
    {
        private IConfiguration _configuration;
        private IConfigurationBuilder _configurationBuilder;

        public TradeBotBuilder()
        {
            this._configuration = new ConfigurationBuilder().Build();
        }

        public ITradeBotBuilder Configure(Action<IConfigurationBuilder> configureDelegate)
        {
            configureDelegate(this._configurationBuilder);
            
            return this;
        }
    }
}