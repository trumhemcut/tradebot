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
    }
}