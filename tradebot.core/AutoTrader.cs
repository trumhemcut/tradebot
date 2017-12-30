using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace tradebot.core
{
    public class AutoTrader
    {
        private readonly ITradeAccount _buyAccount;
        private readonly ITradeAccount _sellAccount;
        private readonly TradeInfo _tradeInfo;
        public decimal PlusPointToWin { get; set; }
        public bool TestMode { get; set; }
        private ILogger _logger;
        public AutoTrader(
            ITradeAccount buyAccount,
            ITradeAccount sellAccount,
            TradeInfo tradeInfo,
            decimal plusPointToWin,
            bool testMode,
            ILogger<AutoTrader> logger)
        {
            this._buyAccount = buyAccount;
            this._sellAccount = sellAccount;
            this._tradeInfo = tradeInfo;
            this.PlusPointToWin = plusPointToWin;
            this.TestMode = testMode;
            this._logger = logger;
        }

        public async Task<TradeBotApiResult> Trade()
        {
#if DEBUG
            this.TestMode = true;
#endif
            if (this.TestMode)
            {
                // this.PlusPointToWin = -0.00000900M;
                this.PlusPointToWin = -1.00000900M;
                this._tradeInfo.CoinQuantityAtBuy = 30;
                this._tradeInfo.CoinQuantityAtSell = 30;
            }

            var buyPrice = this._tradeInfo.BuyPrice + this.PlusPointToWin;
            var sellPrice = this._tradeInfo.SellPrice - this.PlusPointToWin;

            TradeBotApiResult buyResult = null, sellResult = null;

            // Since we experienced many times that Binance throws issues usually.
            // We will stop this if Binance is not successful

            if (this._buyAccount is BinanceAccount)
            {
                buyResult = await this._buyAccount.Buy(this._tradeInfo.CoinQuantityAtBuy, buyPrice);
                if (!buyResult.Success)
                    return buyResult;

                sellResult = await this._sellAccount.Sell(this._tradeInfo.CoinQuantityAtSell, sellPrice);
                return sellResult;
            }
            else
            {
                sellResult = await this._sellAccount.Sell(this._tradeInfo.CoinQuantityAtSell, sellPrice);
                if (!sellResult.Success)
                    return sellResult;

                buyResult = await this._buyAccount.Buy(this._tradeInfo.CoinQuantityAtBuy, buyPrice);
                return buyResult;
            }
        }
    }
}