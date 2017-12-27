using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace tradebot.core
{
    public class AutoTrader
    {
        public ITradeAccount BuyAccount { get; set; }
        public ITradeAccount SellAccount { get; set; }
        public TradeInfo TradeInfo { get; set; }
        public TradeInfo FinegrainedTradeInfo { get; set; }
        public decimal PlusPointToWin { get; set; }
        public bool TestMode { get; set; }
        private ILogger _logger;
        public AutoTrader(
            ITradeAccount buyAccount,
            ITradeAccount sellAccount,
            TradeInfo tradeInfo,
            decimal plusPointToWin,
            bool testMode,
            ILogger logger)
        {
            this.BuyAccount = buyAccount;
            this.SellAccount = sellAccount;
            this.TradeInfo = tradeInfo;
            this.PlusPointToWin = plusPointToWin;
            this.TestMode = testMode;
            this._logger = logger;
        }

        public async Task<bool> Trade()
        {
#if DEBUG
            this.TestMode = true;
#endif
            if (this.TestMode)
            {
                // this.PlusPointToWin = -0.00000900M;
                this.PlusPointToWin = -1.00000900M;
                this.TradeInfo.CoinQuantityAtBuy = 30;
                this.TradeInfo.CoinQuantityAtSell = 30;
            }

            var buyPrice = TradeInfo.BuyPrice + this.PlusPointToWin;
            var sellPrice = TradeInfo.SellPrice - this.PlusPointToWin;

            TradeBotApiResult buyResult = null, sellResult = null;

            // Since we experienced many times that Binance throws issues usually.
            // We will stop this if Binance is not successful

            if (this.BuyAccount is BinanceAccount)
            {
                buyResult = await this.BuyAccount.Buy(TradeInfo.CoinQuantityAtBuy, buyPrice);
                if (!buyResult.Success)
                    return false;

                sellResult = await this.SellAccount.Sell(TradeInfo.CoinQuantityAtSell, sellPrice);
            }
            else
            {
                sellResult = await this.SellAccount.Sell(TradeInfo.CoinQuantityAtSell, sellPrice);
                if (!sellResult.Success)
                    return false;
                
                buyResult = await this.BuyAccount.Buy(TradeInfo.CoinQuantityAtBuy, buyPrice);
            }

            return buyResult.Success && sellResult.Success;
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