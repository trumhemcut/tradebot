using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace tradebot.core
{
    // TradeInfo is very important class
    // since it will decide which action to follow
    // The other class should read data from it rather refer to other proprerties from TradeBot
    public class TradeInfoAnalyzer
    {
        public ITradeAccount SellAccount { get { return this._options.SellAccount; } }
        public ITradeAccount BuyAccount { get { return this._options.BuyAccount; } }
        public decimal BitcoinTradingAmount { get { return this._options.BitcoinTradingAmount; } }
        public List<ITradeAccount> TradeAccounts { get { return this._options.TradeAccounts; } }
        public TradeFlow TradeFlow { get { return this._options.TradeFlow; } }
        private TradeBotOptions _options;

        public TradeInfoAnalyzer(
            TradeBotOptions tradeOptions)
        {
            this._options = tradeOptions;
        }
        public async Task UpdateCoinPrices()
        {
            var updatePrices = new List<Task>();
            this.TradeAccounts.ForEach(acc => updatePrices.Add(acc.UpdatePrices()));
            await Task.WhenAll(updatePrices);

            if (this.TradeFlow == TradeFlow.AutoSwitch)
                AnalyzeFlowAgain();
        }

        private void AnalyzeFlowAgain()
        {
            var accHasMaxBidPrice = this.TradeAccounts.OrderByDescending(acc => acc.CurrentBidPrice)
                                                       .FirstOrDefault();
            var accHasMinAskPrice = this.TradeAccounts.OrderBy(acc => acc.CurrentAskPrice)
                                                       .FirstOrDefault();
            if (accHasMaxBidPrice != accHasMinAskPrice)
            {
                this._options.SellAccount = accHasMaxBidPrice;
                this._options.BuyAccount = accHasMinAskPrice;
            }
        }

        public async Task<bool> UpdateBalances()
        {
            var updateBalances = new List<Task<TradeBotApiResult>>();
            this.TradeAccounts.ForEach(acc => updateBalances.Add(acc.UpdateBalances()));
            await Task.WhenAll(updateBalances);

            return !updateBalances.Where(balanceResult => balanceResult.Result.Success == false)
                                  .Any();
        }
        public TradeInfo AnalyzeDataFixedMode(decimal quantity, decimal plusPointToWin)
        {
            var tradeInfo = new TradeInfo();
            tradeInfo.Message = "OK to buy";
            tradeInfo.Tradable = true;

            var deltaBidAsk = this.SellAccount.TradeCoin.CoinPrice.BidPrice -
                              this.BuyAccount.TradeCoin.CoinPrice.AskPrice;
            var deltaBidBid = this.SellAccount.TradeCoin.CoinPrice.BidPrice -
                              this.BuyAccount.TradeCoin.CoinPrice.BidPrice;

            var coinQtyAtBuy = quantity;//* (1 + this._buyAccount.TradingFee / 100);
            var coinQtyAtSell = quantity;

            var sellPrice = this.SellAccount.CurrentBidPrice - plusPointToWin;
            var buyPrice = this.BuyAccount.CurrentAskPrice + plusPointToWin;
            
            var bitcoinQuantityAtSell = sellPrice * coinQtyAtSell * (1 - this.SellAccount.TradingFee / 100);
            var bitcoinQuantityAtBuy = buyPrice * coinQtyAtBuy * (1 + this.BuyAccount.TradingFee / 100);

            // Check coin balances at both side to make sure it's ok to order
            if (bitcoinQuantityAtBuy >= this.BuyAccount.Bitcoin.Balance)
            {
                tradeInfo.Message = "Bitcoin is not enough to buy.";
                tradeInfo.Tradable = false;
            }

            if (this.SellAccount.TradeCoin.Balance < 0.01M / this.SellAccount.CurrentAskPrice)
            {
                tradeInfo.Message = $"{this.SellAccount.TradeCoin.Token} quantity is too low to set order.";
                tradeInfo.Tradable = false;
            }

            var profit = bitcoinQuantityAtSell - bitcoinQuantityAtBuy;
            if (profit <= 0)
            {
                tradeInfo.Message = $"Profit is too low: {Math.Round(profit, 8)}";
                tradeInfo.Tradable = false;
            }

            tradeInfo.DeltaBidAsk = deltaBidAsk;
            tradeInfo.DeltaBidBid = deltaBidBid;
            tradeInfo.BitcoinQuantityAtSell = bitcoinQuantityAtSell;
            tradeInfo.CoinQuantityAtSell = coinQtyAtSell;
            tradeInfo.BitcoinQuantityAtBuy = bitcoinQuantityAtBuy;
            tradeInfo.CoinQuantityAtBuy = coinQtyAtBuy;
            tradeInfo.CoinProfit = 0;
            tradeInfo.BitcoinProfit = bitcoinQuantityAtSell - bitcoinQuantityAtBuy;
            tradeInfo.SellPrice = sellPrice;
            tradeInfo.BuyPrice = buyPrice;

            return tradeInfo;
        }
        public TradeInfo AnalyzeDeltaFinegrainedMode()
        {
            var tradeInfo = new TradeInfo();
            tradeInfo.Message = "OK to buy";
            tradeInfo.Tradable = true;

            var deltaBidAsk = this.SellAccount.TradeCoin.CoinPrice.BidPrice -
                              this.BuyAccount.TradeCoin.CoinPrice.AskPrice;
            var deltaBidBid = this.SellAccount.TradeCoin.CoinPrice.BidPrice -
                              this.BuyAccount.TradeCoin.CoinPrice.BidPrice;

            // Decide Quantity to sell / buy
            // Sell & buy at the same quantity to be easily manage
            var coinQty = this.BuyAccount.CurrentAskQty < this.SellAccount.CurrentBidQty ?
                           this.BuyAccount.CurrentAskQty : this.SellAccount.CurrentBidQty;

            coinQty = coinQty < this.SellAccount.TradeCoin.Balance ?
                      coinQty : this.SellAccount.TradeCoin.Balance;

            var bitcoinQuantityAtSell = this.SellAccount.CurrentBidPrice * coinQty * (1 - this.SellAccount.TradingFee / 100);
            var bitcoinQuantityAtBuy = this.BuyAccount.CurrentAskPrice * coinQty * (1 - this.BuyAccount.TradingFee / 100);

            // Check coin balances at both side to make sure it's ok to order
            if (bitcoinQuantityAtBuy >= this.BuyAccount.Bitcoin.Balance)
            {
                tradeInfo.Message = "Bitcoin is not enough to buy.";
                tradeInfo.Tradable = false;
            }

            if (this.SellAccount.TradeCoin.Balance < 0.01M / this.SellAccount.CurrentAskPrice)
            {
                tradeInfo.Message = $"{this.SellAccount.TradeCoin.Token} quantity is too low to set order.";
                tradeInfo.Tradable = false;
            }

            tradeInfo.DeltaBidAsk = deltaBidAsk;
            tradeInfo.DeltaBidBid = deltaBidBid;
            tradeInfo.BitcoinQuantityAtSell = bitcoinQuantityAtSell;
            tradeInfo.CoinQuantityAtSell = coinQty;
            tradeInfo.BitcoinQuantityAtBuy = bitcoinQuantityAtBuy;
            tradeInfo.CoinQuantityAtBuy = coinQty;
            tradeInfo.CoinProfit = 0;
            tradeInfo.BitcoinProfit = bitcoinQuantityAtSell - bitcoinQuantityAtBuy;
            tradeInfo.SellPrice = this.SellAccount.CurrentBidPrice;
            tradeInfo.BuyPrice = this.BuyAccount.CurrentAskPrice;

            return tradeInfo;
        }
        public TradeInfo AnalyzeDeltaNormalMode()
        {
            var deltaBidAsk = this.SellAccount.TradeCoin.CoinPrice.BidPrice -
                              this.BuyAccount.TradeCoin.CoinPrice.AskPrice;
            var deltaBidBid = this.SellAccount.TradeCoin.CoinPrice.BidPrice -
                              this.BuyAccount.TradeCoin.CoinPrice.BidPrice;

            var bitcoinQuantityAtSell = (this.BitcoinTradingAmount + this.SellAccount.Bitcoin.TransferFee) *
                                      (1 + this.SellAccount.TradingFee / 100);
            var coinQuantityAtSell = bitcoinQuantityAtSell / this.SellAccount.TradeCoin.CoinPrice.BidPrice;

            var bitcoinQuantityAtBuy = this.BitcoinTradingAmount * (1 - this.BuyAccount.TradingFee / 100);
            var coinQuantityAtBuy = bitcoinQuantityAtBuy / this.BuyAccount.TradeCoin.CoinPrice.AskPrice;

            return new TradeInfo
            {
                DeltaBidAsk = deltaBidAsk,
                DeltaBidBid = deltaBidBid,
                BitcoinQuantityAtSell = bitcoinQuantityAtSell,
                CoinQuantityAtSell = coinQuantityAtSell,
                BitcoinQuantityAtBuy = bitcoinQuantityAtBuy,
                CoinQuantityAtBuy = coinQuantityAtBuy,
                CoinProfit = coinQuantityAtBuy - coinQuantityAtSell,
                BuyPrice = this.BuyAccount.TradeCoin.CoinPrice.AskPrice,
                SellPrice = this.SellAccount.TradeCoin.CoinPrice.BidPrice
            };
        }
    }
}