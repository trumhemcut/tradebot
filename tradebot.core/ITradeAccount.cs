using System.Threading.Tasks;

namespace tradebot.core
{
    public interface ITradeAccount
    {
        decimal TradingFee { get; set; }
        Coin Bitcoin { get; set; }
        Coin TradeCoin { get; set; }
        Task UpdatePrices();
        Task<object> Buy(decimal quantity, decimal price);
        Task<object> Sell(decimal quantity, decimal price);
    }
}