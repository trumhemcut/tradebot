using System;

namespace tradebot.core
{
    public class TradeInfoAnalyzer
    {
        private readonly ITradeAccount _sellAccount;
        private readonly ITradeAccount _buyAccount;
        private readonly decimal _bitcoinTradingAmount;

        public TradeInfoAnalyzer(TradeBotOptions tradeOptions)
        {
            this._buyAccount = tradeOptions.BuyAccount;
            this._sellAccount = tradeOptions.SellAccount;
            this._bitcoinTradingAmount = tradeOptions.BitcoinTradingAmount;
        }
        public TradeInfo AnalyzeDeltaFinegrainedMode()
        {
            var tradeInfo = new TradeInfo();
            tradeInfo.Message = "OK to buy";
            tradeInfo.Tradable = true;

            var deltaBidAsk = this._sellAccount.TradeCoin.CoinPrice.BidPrice -
                              this._buyAccount.TradeCoin.CoinPrice.AskPrice;
            var deltaBidBid = this._sellAccount.TradeCoin.CoinPrice.BidPrice -
                              this._buyAccount.TradeCoin.CoinPrice.BidPrice;

            // Decide Quantity to sell / buy
            // Sell & buy at the same quantity to be easily manage
            var coinQty = this._buyAccount.CurrentAskQty < this._sellAccount.CurrentBidQty ?
                           this._buyAccount.CurrentAskQty : this._sellAccount.CurrentBidQty;

            coinQty = coinQty < this._sellAccount.TradeCoin.Balance ?
                      coinQty : this._sellAccount.TradeCoin.Balance;

            var bitcoinQuantityAtSell = this._sellAccount.CurrentBidPrice * coinQty * (1 - this._sellAccount.TradingFee / 100);
            var bitcoinQuantityAtBuy = this._buyAccount.CurrentAskPrice * coinQty * (1 - this._buyAccount.TradingFee / 100);

#if DEBUG
            this._buyAccount.Bitcoin.Balance = 1;
#endif
            // Check coin balances at both side to make sure it's ok to order
            if (bitcoinQuantityAtBuy >= this._buyAccount.Bitcoin.Balance)
            {
                tradeInfo.Message = "Bitcoin is not enough to buy.";
                tradeInfo.Tradable = false;
            }

            if (this._sellAccount.TradeCoin.Balance < 0.01M / this._sellAccount.CurrentAskPrice)
            {
                tradeInfo.Message = $"{this._sellAccount.TradeCoin.Token} quantity is too low to set order.";
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
            tradeInfo.SellPrice = _sellAccount.CurrentBidPrice;
            tradeInfo.BuyPrice = _buyAccount.CurrentAskPrice;

            return tradeInfo;
        }
        public TradeInfo AnalyzeDeltaNormalMode()
        {
            var deltaBidAsk = this._sellAccount.TradeCoin.CoinPrice.BidPrice -
                              this._buyAccount.TradeCoin.CoinPrice.AskPrice;
            var deltaBidBid = this._sellAccount.TradeCoin.CoinPrice.BidPrice -
                              this._buyAccount.TradeCoin.CoinPrice.BidPrice;

            var bitcoinQuantityAtSell = (this._bitcoinTradingAmount + this._sellAccount.Bitcoin.TransferFee) *
                                      (1 + this._sellAccount.TradingFee / 100);
            var coinQuantityAtSell = bitcoinQuantityAtSell / this._sellAccount.TradeCoin.CoinPrice.BidPrice;

            var bitcoinQuantityAtBuy = this._bitcoinTradingAmount * (1 - this._buyAccount.TradingFee / 100);
            var coinQuantityAtBuy = bitcoinQuantityAtBuy / this._buyAccount.TradeCoin.CoinPrice.AskPrice;

            return new TradeInfo
            {
                DeltaBidAsk = deltaBidAsk,
                DeltaBidBid = deltaBidBid,
                BitcoinQuantityAtSell = bitcoinQuantityAtSell,
                CoinQuantityAtSell = coinQuantityAtSell,
                BitcoinQuantityAtBuy = bitcoinQuantityAtBuy,
                CoinQuantityAtBuy = coinQuantityAtBuy,
                CoinProfit = coinQuantityAtBuy - coinQuantityAtSell,
                BuyPrice = this._buyAccount.TradeCoin.CoinPrice.AskPrice,
                SellPrice = this._sellAccount.TradeCoin.CoinPrice.BidPrice
            };
        }
    }
}