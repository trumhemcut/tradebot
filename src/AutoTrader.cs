using System.Threading.Tasks;
using tradebot.TradePlatform;

namespace tradebot
{
    public class AutoTrader
    {
        public ITradeAccount BuyAccount { get; set; }
        public ITradeAccount SellAccount { get; set; }
        public decimal AmountToBuy { get; set; }
        public decimal PriceToBuy { get; set; }
        public decimal AmountToSell { get; set; }
        public decimal PriceToSell { get; set; }
        public AutoTrader(
            ITradeAccount buyAccount, 
            ITradeAccount sellAccount,
            decimal amountToBuy,
            decimal priceToBuy,
            decimal amountToSell,
            decimal priceToSell)
        {
            this.BuyAccount = buyAccount;
            this.BuyAccount = sellAccount;
        }
        public async Task Trade()
        {
            await Task.WhenAll( 
                this.BuyAccount.Buy(AmountToBuy, PriceToBuy),
                this.SellAccount.Sell(AmountToSell, PriceToSell)
            );
        }
    }
}