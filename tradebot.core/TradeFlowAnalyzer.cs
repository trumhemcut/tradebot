using System.Collections.Generic;
using System.Linq;
using tradebot.core;

namespace tradebot
{
    public class TradeFlowAnalyzer
    {
        public ITradeAccount SellAccount { get; set; }
        public ITradeAccount BuyAccount { get; set; }
        public List<ITradeAccount> TradeAccounts { get; set; }
        public TradeFlowAnalyzer(
            TradeFlow tradeFlow,
            List<ITradeAccount> tradeAccounts)
        {
            this.TradeAccounts = tradeAccounts;
            AnalyzeTheTradeFlow(
                tradeFlow,
                tradeAccounts);
        }

        private void AnalyzeTheTradeFlow(
           TradeFlow tradeFlow,
           List<ITradeAccount> tradeAccounts)
        {
            switch (tradeFlow)
            {
                case TradeFlow.BuyAtBinanceSellAtBittrex:
                    this.SellAccount = tradeAccounts.FirstOrDefault(acc => acc is BittrexAccount);
                    this.BuyAccount = tradeAccounts.FirstOrDefault(acc => acc is BinanceAccount); ;
                    break;
                case TradeFlow.SellAtBinanceBuyAtBittrex:
                    this.SellAccount = tradeAccounts.FirstOrDefault(acc => acc is BinanceAccount);
                    this.BuyAccount = tradeAccounts.FirstOrDefault(acc => acc is BittrexAccount); ;
                    break;
                case TradeFlow.AutoSwitch:
                    // We will decide about this at TradeInfo Analyzer step
                    this.SellAccount = null;
                    this.BuyAccount = null;
                    break;
                default:
                    break;
            }
        }
    }
}