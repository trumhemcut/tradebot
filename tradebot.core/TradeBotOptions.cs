using System;
using Microsoft.Extensions.Configuration;

namespace tradebot.core
{
    public class TradeBotOptions
    {
        public decimal BitcoinTradingAmount { get; set; }
        public int ResumeAfterExpectedDelta { get; set; }
        public decimal ExpectedDelta { get; set; }
        public string EmailTo { get; set; }
        public bool IsAutoTrading { get; set; }
        public string MailApiKey { get; set; }
        public string Coin { get; set; }
        public TradeFlow TradeFlow { get; set; }
        public ITradeAccount BuyAccount { get; set; }
        public ITradeAccount SellAccount { get; set; }
        public TradeBotOptions() { }

        public TradeBotOptions(IConfiguration configuration)
        {
            this.IsAutoTrading = Boolean.Parse(configuration["IsAutoTrading"]);
            this.ExpectedDelta = Decimal.Parse(configuration["ExpectedDelta"]);
            this.EmailTo = configuration["Email:EmailTo"];
            this.ResumeAfterExpectedDelta = Int32.Parse(configuration["ResumeAfterDelta"]);
            this.BitcoinTradingAmount = Decimal.Parse(configuration["BitcoinTradingAmount"]);
            this.MailApiKey = configuration["Email:ApiKey"];
            this.MailApiKey = configuration["Email:ApiKey"];
            this.TradeFlow = (TradeFlow)Enum.Parse(typeof(TradeFlow), configuration["TradeFlow"]);
        }
    }
}