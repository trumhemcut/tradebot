using System;

namespace tradebot.core
{
    public class CoinPrice
    {
        public decimal LastPrice { get; set; }
        public decimal BidPrice { get; set; }
        public decimal AskPrice { get; set; }
        public DateTime RetrivalTime { get; set; }
    }
}