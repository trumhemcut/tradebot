using tradebot.core;

namespace tradebot
{
    public class TradeFlowAnalyzer
    {
        public ITradeAccount SellAccount { get; set; }
        public ITradeAccount BuyAccount { get; set; }
        public TradeFlowAnalyzer(
            TradeFlow tradeFlow,
            ITradeAccount binanceAccount,
            ITradeAccount bittrexAccount)
        {
            AnalyzeTheTradeFlow(
                tradeFlow,
                binanceAccount,
                bittrexAccount);
        }

        private void AnalyzeTheTradeFlow(
           TradeFlow tradeFlow,
           ITradeAccount binanceAccount,
           ITradeAccount bittrexAccount)
        {
            switch (tradeFlow)
            {
                case TradeFlow.BuyAtBinanceSellAtBittrex:
                    this.SellAccount = bittrexAccount;
                    this.BuyAccount = binanceAccount;
                    break;
                case TradeFlow.SellAtBinanceBuyAtBittrex:
                    this.SellAccount = binanceAccount;
                    this.BuyAccount = bittrexAccount;
                    break;
                default:
                    break;
            }
        }
    }
}