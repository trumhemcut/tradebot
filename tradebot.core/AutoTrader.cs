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
        public bool _testMode;
        private readonly string _trans;
        private ILogger _logger;
        public AutoTrader(
            ITradeAccount buyAccount,
            ITradeAccount sellAccount,
            TradeInfo tradeInfo,
            bool testMode,
            string trans,
            ILogger<AutoTrader> logger)
        {
            this._buyAccount = buyAccount;
            this._sellAccount = sellAccount;
            this._tradeInfo = tradeInfo;
            this._testMode = testMode;
            this._trans = trans;
            this._logger = logger;
        }

        public async Task<TradeBotApiResult> Trade()
        {
#if DEBUG
            this._testMode = true;
#endif
            if (this._testMode)
            {
                this._tradeInfo.SellPrice += 0.00000900M;
                this._tradeInfo.BuyPrice -= 0.00000900M;
                this._tradeInfo.CoinQuantityAtBuy = 100;
                this._tradeInfo.CoinQuantityAtSell = 100;
            }

            TradeBotApiResult buyResult = null, sellResult = null;

            // Since we experienced many times that Binance throws issues usually.
            // We will stop this if Binance is not successful

            if (this._buyAccount is BinanceAccount)
            {
                buyResult = await this._buyAccount.Buy(
                                        this._trans,
                                        this._tradeInfo.CoinQuantityAtBuy,
                                        this._tradeInfo.BuyPrice);
                if (!buyResult.Success)
                    return buyResult;

                sellResult = await this._sellAccount.Sell(
                                        this._trans,
                                        this._tradeInfo.CoinQuantityAtSell,
                                        this._tradeInfo.SellPrice);
                return sellResult;
            }
            else
            {
                sellResult = await this._sellAccount.Sell(
                                        this._trans,
                                        this._tradeInfo.CoinQuantityAtSell,
                                        this._tradeInfo.SellPrice);
                if (!sellResult.Success)
                    return sellResult;

                buyResult = await this._buyAccount.Buy(
                                        this._trans,
                                        this._tradeInfo.CoinQuantityAtBuy,
                                        this._tradeInfo.BuyPrice);
                return buyResult;
            }
        }
    }
}