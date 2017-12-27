using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Binance.Net;
using Binance.Net.Objects;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace tradebot.core
{
    public class BinanceAccount : ITradeAccount
    {
        private long _currentOrderId;
        public decimal TradingFee { get; set; }
        public Coin Bitcoin { get; set; }
        public Coin TradeCoin { get; set; }

        public decimal CurrentAskPrice { get { return this.TradeCoin.CoinPrice.AskPrice; } }
        public decimal CurrentBidPrice { get { return this.TradeCoin.CoinPrice.BidPrice; } }
        public decimal CurrentBidQty { get { return this.TradeCoin.CoinPrice.BidQuantity; } }
        public decimal CurrentAskQty { get { return this.TradeCoin.CoinPrice.AskQuantity; } }
        private ILogger _logger;

        public BinanceAccount(string coin,
                              decimal tradingFee,
                              decimal bitcoinTransferFee,
                              string apiKey,
                              string apiSecret,
                              ILogger logger)
        {
            BinanceDefaults.SetDefaultApiCredentials(apiKey, apiSecret);

            // Setup coin
            this.TradeCoin = new Coin { Token = coin };
            this.Bitcoin = new Coin { Token = "BTC", TransferFee = bitcoinTransferFee };
            this.TradingFee = tradingFee;
            this._logger = logger;
        }

        // Reference to https://www.binance.com/restapipub.html#user-content-market-data-endpoints
        public async Task UpdatePrices()
        {
            using (var client = new HttpClient())
            {
                string result = await client.GetStringAsync($"https://api.binance.com/api/v1/depth?symbol={this.TradeCoin.Token}BTC&limit=5");
                dynamic d = JsonConvert.DeserializeObject(result);

                this.TradeCoin.CoinPrice = new CoinPrice
                {
                    LastPrice = d.bids[0][0],
                    BidPrice = d.bids[0][0],
                    BidQuantity = d.bids[0][1],
                    AskPrice = d.asks[0][0],
                    AskQuantity = d.asks[0][1],
                    RetrivalTime = DateTime.Now
                };
            }
        }
        public async Task<TradeBotApiResult> UpdateBalances()
        {
            using (var binanceClient = new BinanceClient())
            {
                var accountInfo = await binanceClient.GetAccountInfoAsync();
                if (accountInfo.Success)
                {
                    this.Bitcoin.Balance = accountInfo.Data.Balances
                        .Where(balance => balance.Asset.Equals(this.Bitcoin.Token))
                        .Single()
                        .Total;

                    this.TradeCoin.Balance = accountInfo.Data.Balances
                        .Where(balance => balance.Asset.Equals(this.TradeCoin.Token))
                        .Single()
                        .Total;
                    return new TradeBotApiResult { Success = true };
                }
                else
                {
                    _logger.LogError(accountInfo.Error.Message);
                    return new TradeBotApiResult { Success = false, ErrorMessage = accountInfo.Error.Message };
                }
            }
        }

        public async Task<TradeBotApiResult> Buy(decimal quantity, decimal price)
        {
            using (var binanceClient = new BinanceClient())
            {
                var result = await binanceClient.PlaceOrderAsync(
                        $"{this.TradeCoin.Token}BTC",
                        OrderSide.Buy,
                        OrderType.Limit,
                        TimeInForce.GoodTillCancel,
                        quantity,
                        price
                );

                if (result.Success){
                    _logger.LogInformation($"Buy order {quantity} {this.TradeCoin.Token}, price {price} successfully.");
                    this._currentOrderId = result.Data.OrderId;
                }
                else
                    _logger.LogError(result.Error.Message);

                return new TradeBotApiResult
                {
                    Success = result.Success,
                    ErrorMessage = result.Error == null ? string.Empty : result.Error.Message
                };
            }
        }

        public async Task<TradeBotApiResult> Sell(decimal quantity, decimal price)
        {
            using (var binanceClient = new BinanceClient())
            {
                var result = await binanceClient.PlaceOrderAsync(
                        $"{this.TradeCoin.Token}BTC",
                        OrderSide.Sell,
                        OrderType.Limit,
                        TimeInForce.GoodTillCancel,
                        quantity,
                        price
                );

                if (result.Success)
                {
                    this._currentOrderId = result.Data.OrderId;
                    _logger.LogInformation($"Sell order {quantity} {this.TradeCoin.Token}, price {price} successfully.");
                }
                else
                    _logger.LogError(result.Error.Message);

                return new TradeBotApiResult
                {
                    Success = result.Success,
                    ErrorMessage = result.Error == null ? string.Empty : result.Error.Message
                };
            }
        }

        public async Task<bool> IsOrderMatched()
        {
            using (var binanceClient = new BinanceClient())
            {
                var result = await binanceClient.QueryOrderAsync($"{this.TradeCoin.Token}BTC", this._currentOrderId);
                return result.Data.Status == OrderStatus.Filled;
            }
        }
    }
}