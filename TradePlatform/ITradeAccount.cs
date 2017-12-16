using System.Threading.Tasks;

namespace tradebot.TradePlatform
{
    public interface ITradeAccount
    {
        Coin Bitcoin { get; set; }
        Coin TradeCoin { get; set; }
        Task UpdatePrices();
    }
}