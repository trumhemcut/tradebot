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
            var tradable = false;

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

            var bitcoinQuantityAtSell = this._sellAccount.CurrentBidPrice * coinQty;
            var bitcoinQuantityAtBuy = coinQty * (1 - this._buyAccount.TradingFee / 100);

            // Check coin balances
            if ((bitcoinQuantityAtBuy > this._buyAccount.Bitcoin.Balance) ||
            (this._sellAccount.TradeCoin.Balance < 0.01M / this._sellAccount.CurrentAskPrice))
            {
                tradable = false;
                Console.WriteLine("Coin balances are not enough to trade.");
            }

            return new TradeInfo
            {
                DeltaBidAsk = deltaBidAsk,
                DeltaBidBid = deltaBidBid,
                BitcoinQuantityAtSell = bitcoinQuantityAtSell,
                CoinQuantityAtSell = coinQty,
                BitcoinQuantityAtBuy = bitcoinQuantityAtBuy,
                CoinQuantityAtBuy = coinQty,
                CoinProfit = 0,
                BitcoinProfit = bitcoinQuantityAtSell - bitcoinQuantityAtBuy,
                Tradable = tradable
            };
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
                CoinProfit = coinQuantityAtBuy - coinQuantityAtSell
            };
        }
    }
}