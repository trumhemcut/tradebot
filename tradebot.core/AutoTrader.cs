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
            plusPointToWin = -0.00000230M;
#endif

            try
            {
                var buyPrice = TradeInfo.BuyPrice + plusPointToWin;
                var sellPrice = TradeInfo.SellPrice - plusPointToWin;

                var buyTask = this.BuyAccount.Buy(TradeInfo.CoinQuantityAtBuy, buyPrice);
                var sellTask = this.SellAccount.Sell(TradeInfo.CoinQuantityAtSell, sellPrice);

                await Task.WhenAll(buyTask, sellTask);

                Console.Write($"Buy {TradeInfo.CoinQuantityAtBuy}, price: {buyPrice}");
                if (buyTask.Result.Success)
                    Console.Write("...OK!");
                else
                    Console.WriteLine(buyTask.Result.ErrorMessage);

                Console.WriteLine("");
                Console.WriteLine("");

                Console.Write($"Sell {TradeInfo.CoinQuantityAtBuy}, price: {sellPrice}");
                if (sellTask.Result.Success)
                    Console.Write("...OK!");
                else
                    Console.WriteLine(sellTask.Result.ErrorMessage);
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