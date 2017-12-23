using System;
using System.Net.Http;
using System.Threading.Tasks;
using Binance.Net;
using Binance.Net.Objects;
using Newtonsoft.Json;

namespace tradebot.core
{
    public class BinanceAccount : ITradeAccount
    {
        private readonly BinanceClient _binanceClient;
        public decimal TradingFee { get; set; }
        public Coin Bitcoin { get; set; }
        public Coin TradeCoin { get; set; }

        public BinanceAccount(string coin,
                              decimal tradingFee,
                              decimal bitcoinTransferFee,
                              string apiKey,
                              string apiSecret)
        {
            BinanceDefaults.SetDefaultApiCredentials(apiKey, apiSecret);

            // Setup coin
            this.TradeCoin = new Coin { Token = coin };
            this.Bitcoin = new Coin { Token = "BTC", TransferFee = bitcoinTransferFee };
            this.TradingFee = tradingFee;
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
                    AskPrice = d.asks[0][0],
                    RetrivalTime = DateTime.Now
                };
            }
        }

        public async Task<object> Buy(decimal amount, decimal price)
        {
            using (var binanceClient = new BinanceClient())
            {
                // REMOVE THIS LINE WHEN PRODUCTION
                // MIN OF ORDER IS 0.01 BTC
                // https://www.reddit.com/r/binance/comments/74ocol/api_errorfilter_failure_min_notional/
                amount = Math.Round(0.01M / price); // FOR TESTING

                var result = await binanceClient.PlaceOrderAsync(
                        $"{this.TradeCoin.Token}BTC",
                        OrderSide.Buy,
                        OrderType.Limit,
                        TimeInForce.GoodTillCancel,
                        amount,
                        price
                );

                return result;
            }

        }

        public async Task<object> Sell(decimal amount, decimal price)
        {
            using (var binanceClient = new BinanceClient())
            {
                // REMOVE THIS LINE WHEN PRODUCTION
                // MIN OF ORDER IS 0.01 BTC
                // https://www.reddit.com/r/binance/comments/74ocol/api_errorfilter_failure_min_notional/
                amount = Math.Round(0.01M / price); // FOR TESTING

                var result = await binanceClient.PlaceOrderAsync(
                        $"{this.TradeCoin.Token}BTC",
                        OrderSide.Sell,
                        OrderType.Limit,
                        TimeInForce.GoodTillCancel,
                        amount,
                        price
                );

                return result;
            }
        }
    }
}