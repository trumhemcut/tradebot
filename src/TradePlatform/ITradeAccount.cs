using System.Threading.Tasks;

namespace tradebot.TradePlatform
{
    public interface ITradeAccount
    {
        decimal TradingFee { get; set; }
        Coin Bitcoin { get; set; }
        Coin TradeCoin { get; set; }
        Task UpdatePrices();
        Task Buy(decimal quantity, decimal price);
        Task Sell(decimal quantity, decimal price);
    }
}