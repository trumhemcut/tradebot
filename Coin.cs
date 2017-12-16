namespace tradebot
{
    public class Coin
    {
        public string Token { get; set; }
        public decimal Balance { get; set; }
        public decimal TransferFee { get; set; }
        public CoinPrice CoinPrice { get; set; }
        public string Address { get; set; }
    }
}