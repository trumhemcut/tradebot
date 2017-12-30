using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Bittrex.Net;
using Bittrex.Net.Objects;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace tradebot.core
{
    public class BittrexAccount : ITradeAccount
    {
        private Guid _currentOrderId;
        public Coin Bitcoin { get; set; }
        public Coin TradeCoin { get; set; }
        public decimal TradingFee { get; set; }
        public decimal CurrentAskPrice { get { return this.TradeCoin.CoinPrice.AskPrice; } }
        public decimal CurrentBidPrice { get { return this.TradeCoin.CoinPrice.BidPrice; } }
        public decimal CurrentBidQty { get { return this.TradeCoin.CoinPrice.BidQuantity; } }
        public decimal CurrentAskQty { get { return this.TradeCoin.CoinPrice.AskQuantity; } }
        private ILogger _logger;
        public BittrexAccount(string coin,
                              decimal tradingFee,
                              decimal bitcoinTransferFee,
                              string apiKey,
                              string apiSecret,
                              ILogger<BittrexAccount> logger)
        {
            BittrexDefaults.SetDefaultApiCredentials(apiKey, apiSecret);

            this.TradeCoin = new Coin { Token = coin };
            this.Bitcoin = new Coin { Token = "BTC", TransferFee = bitcoinTransferFee };
            this.TradingFee = tradingFee;
            this._logger = logger;
        }

        public async Task UpdatePrices()
        {
            using (var client = new HttpClient())
            {
                // string result = await client.GetStringAsync($"https://bittrex.com/api/v1.1/public/getticker?market=BTC-{this.TradeCoin.Token}");
                string result = await client.GetStringAsync($"https://bittrex.com/api/v1.1/public/getorderbook?market=BTC-{this.TradeCoin.Token}&type=both&depth=3");
                dynamic d = JsonConvert.DeserializeObject(result);

                this.TradeCoin.CoinPrice = new CoinPrice
                {
                    LastPrice = d.result.sell[0].Rate,
                    AskPrice = d.result.sell[0].Rate,
                    AskQuantity = d.result.sell[0].Quantity,
                    BidPrice = d.result.buy[0].Rate,
                    BidQuantity = d.result.buy[0].Quantity,
                    RetrivalTime = DateTime.Now
                };
            }
        }

        public async Task<TradeBotApiResult> UpdateBalances()
        {
            using (var bittrextClient = new BittrexClient())
            {
                var coinBalanceResult = await bittrextClient.GetBalanceAsync(this.TradeCoin.Token);
                if (coinBalanceResult.Success)
                {
                    this.TradeCoin.Balance = coinBalanceResult.Result == null ?
                                             0 : coinBalanceResult.Result.Balance;
                    var bitcoinBalanceResult = await bittrextClient.GetBalanceAsync(this.Bitcoin.Token);
                    this.Bitcoin.Balance = bitcoinBalanceResult.Result == null ?
                                           0 : bitcoinBalanceResult.Result.Balance;
                    return new TradeBotApiResult { Success = true };
                }
                else
                {
                    _logger.LogError(coinBalanceResult.Error.ErrorMessage);
                }
                return new TradeBotApiResult { Success = false, ErrorMessage = coinBalanceResult.Error.ErrorMessage };
            }
        }
        public async Task<TradeBotApiResult> Buy(decimal quantity, decimal price)
        {
            using (var bittrexClient = new BittrexClient())
            {
                var result = await bittrexClient.PlaceOrderAsync(
                    OrderType.Buy,
                    $"BTC-{this.TradeCoin.Token}",
                    quantity,
                    price);

                if (result.Success)
                {
                    _logger.LogInformation($"Buy order {quantity} {this.TradeCoin.Token}, price {price} successfully.");
                    this._currentOrderId = result.Result.Uuid;
                }
                else
                    _logger.LogError(result.Error.ErrorMessage);

                return new TradeBotApiResult
                {
                    Success = result.Success,
                    ErrorMessage = result.Error == null ? string.Empty : result.Error.ErrorMessage
                };
            }
        }

        public async Task<TradeBotApiResult> Sell(decimal quantity, decimal price)
        {
            using (var bittrexClient = new BittrexClient())
            {
                var result = await bittrexClient.PlaceOrderAsync(
                    OrderType.Sell,
                    $"BTC-{this.TradeCoin.Token}",
                    quantity,
                    price);

                if (result.Success){
                    _logger.LogInformation($"Sell order {quantity} {this.TradeCoin.Token}, price {price} successfully.");
                    this._currentOrderId = result.Result.Uuid;
                }
                else
                    _logger.LogError(result.Error.ErrorMessage);
                
                return new TradeBotApiResult
                {
                    Success = result.Success,
                    ErrorMessage = result.Error == null ? string.Empty : result.Error.ErrorMessage
                };
            }
        }

        public async Task<bool> IsOrderMatched()
        {
            using (var bittrexClient = new BittrexClient())
            {
                var result = await bittrexClient.GetOrderAsync(this._currentOrderId);
                return result.Result.Closed != null;
            }
        }
    }
}