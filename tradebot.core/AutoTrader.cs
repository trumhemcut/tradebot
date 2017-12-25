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
            var plusPointToWin = 0.00000003M;
#if DEBUG
            plusPointToWin = 0.00000003M; //-0.00000015M;
#endif

            try
            {
                var buyPrice = TradeInfo.BuyPrice + plusPointToWin;

                Console.Write($"Buy {TradeInfo.CoinQuantityAtBuy}, price: {buyPrice}");
                var buyResult = await this.BuyAccount.Buy(
                    TradeInfo.CoinQuantityAtBuy,
                    buyPrice);

                if (buyResult.Success)
                    Console.Write("...OK!");
                else
                    Console.WriteLine(buyResult.ErrorMessage);
                
                Console.WriteLine("");
                Console.WriteLine("");

                var sellPrice = TradeInfo.SellPrice - plusPointToWin;
                Console.Write($"Sell {TradeInfo.CoinQuantityAtBuy}, price: {sellPrice}");
                var sellResult = await this.SellAccount.Sell(
                    TradeInfo.CoinQuantityAtSell,
                    sellPrice);
                if (sellResult.Success)
                    Console.Write("...OK!");
                else
                    Console.WriteLine(sellResult.ErrorMessage);
                Console.WriteLine("");
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