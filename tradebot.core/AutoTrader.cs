using System;
using System.Threading.Tasks;

namespace tradebot.core
{
    public class AutoTrader
    {
        public ITradeAccount BuyAccount { get; set; }
        public ITradeAccount SellAccount { get; set; }
        public TradeInfo TradeInfo { get; set; }
        public TradeInfo FinegrainedTradeInfo { get; set; }
        public AutoTrader(
            ITradeAccount buyAccount,
            ITradeAccount sellAccount,
            TradeInfo tradeInfo)
        {
            this.BuyAccount = buyAccount;
            this.SellAccount = sellAccount;
            this.TradeInfo = tradeInfo;
        }

        public async Task Trade()
        {
            // TODO: REMOVE this line on production
            // Why 13? Well, I like this number :)
            var plusPointToWin = -0.00000013M;

            try
            {
                var buyPrice = BuyAccount.TradeCoin.CoinPrice.AskPrice + plusPointToWin;
                
                Console.WriteLine($"Buy {TradeInfo.CoinQuantityAtBuy}, price: {buyPrice}");
                var buyResult = await this.BuyAccount.Buy(
                    TradeInfo.CoinQuantityAtBuy,
                    buyPrice);
                
                if (buyResult.Success) Console.WriteLine("BUY ORDER SET");
                    
                var sellPrice = SellAccount.TradeCoin.CoinPrice.BidPrice - plusPointToWin;
                Console.WriteLine($"Sell {TradeInfo.CoinQuantityAtBuy}, price: {sellPrice}");
                await this.SellAccount.Sell(
                    TradeInfo.CoinQuantityAtSell,
                    sellPrice);
                if (buyResult.Success) Console.WriteLine("SELL ORDER SET");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Set order error: {ex.Message}");
            }
        }

        public void PrintDashboard()
        {
            Console.WriteLine(@"+--------------------------------------+
                                | SELL              | BUY              |
                                |-------------------|------------------|
                                | 0.00002608 BTC    | 0.00002508 BTC   |
                                | 18,000 ADA        | 17,000 ADA       |
                                |-------------------|------------------|
                                | Profit:                              |
                                +--------------------------------------+
            ");
        }
    }
}