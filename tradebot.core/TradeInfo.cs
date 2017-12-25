namespace tradebot.core
{
    public class TradeInfo
    {
        public decimal DeltaBidBid { get; set; }
        public decimal DeltaBidAsk { get; set; }
        public decimal BitcoinQuantityAtSell { get; set; }
        public decimal CoinQuantityAtSell { get; set; }
        public decimal BitcoinQuantityAtBuy { get; set; }
        public decimal CoinQuantityAtBuy { get; set; }
        public decimal CoinProfit { get; set; }
        public decimal BitcoinProfit { get; set; }
        public decimal SellPrice { get; set; }
        public decimal BuyPrice { get; set; }
        public bool Tradable { get; set; }
        // In case we can't trade, what's the reason
        public string Message { get; set; }
    }
}