namespace tradebot
{
    public class Coin
    {
        public string Token { get; set; }
        public decimal Balance { get; set; }
        public decimal TradingFee { get; set; }
        public CoinPrice CoinPrice { get; set; }
    }
}