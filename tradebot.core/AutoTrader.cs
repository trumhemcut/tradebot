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
            // plusPointToWin = -0.00000230M;
            plusPointToWin = 0.00000003M;
#endif

            var buyPrice = TradeInfo.BuyPrice + plusPointToWin;
            var sellPrice = TradeInfo.SellPrice - plusPointToWin;

            TradeBotApiResult buyResult = null, sellResult = null;

            // Since we experienced many times that Binance throws issues usually.
            // We will stop this if Binance is not successful
            
            if (this.BuyAccount is BinanceAccount)
            {
                buyResult = await this.BuyAccount.Buy(TradeInfo.CoinQuantityAtBuy, buyPrice);
                if (!buyResult.Success)
                {
                    Console.WriteLine($"Buy Order ERROR! {buyResult.ErrorMessage}, Sell Order was ignored");
                    return;
                }
                sellResult = await this.SellAccount.Sell(TradeInfo.CoinQuantityAtSell, sellPrice);
            }
            else
            {
                sellResult = await this.SellAccount.Sell(TradeInfo.CoinQuantityAtSell, sellPrice);
                if (!sellResult.Success)
                {
                    Console.WriteLine($"Sell Order ERROR! {buyResult.ErrorMessage}, Buy Order was ignored");
                    return;
                }
                buyResult = await this.BuyAccount.Buy(TradeInfo.CoinQuantityAtBuy, buyPrice);
            }

            Console.Write($"Buy {TradeInfo.CoinQuantityAtBuy}, price: {buyPrice}");
            if (buyResult.Success)
                Console.Write("...OK!");
            else
                Console.WriteLine(buyResult.ErrorMessage);

            Console.WriteLine("");
            Console.WriteLine("");

            Console.Write($"Sell {TradeInfo.CoinQuantityAtBuy}, price: {sellPrice}");
            if (sellResult.Success)
                Console.Write("...OK!");
            else
                Console.WriteLine(sellResult.ErrorMessage);
            Console.WriteLine("");

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