using System;

namespace tradebot.core
{
    public class CoinPrice
    {
        public decimal LastPrice { get; set; }
        public decimal BidPrice { get; set; }
        public decimal BidQuantity { get; set; }
        public decimal AskPrice { get; set; }
        public decimal AskQuantity { get; set; }
        public DateTime RetrivalTime { get; set; }
    }
}