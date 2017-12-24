using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Bittrex.Net;
using Bittrex.Net.Objects;
using Newtonsoft.Json;

namespace tradebot.core
{
    public class BittrexAccount : ITradeAccount
    {
        public Coin Bitcoin { get; set; }
        public Coin TradeCoin { get; set; }
        public decimal TradingFee { get; set; }
        public decimal CurrentAskPrice { get { return this.TradeCoin.CoinPrice.AskPrice; } }
        public decimal CurrentBidPrice { get { return this.TradeCoin.CoinPrice.BidPrice; } }
        public decimal CurrentBidQty { get { return this.TradeCoin.CoinPrice.BidQuantity; } }
        public decimal CurrentAskQty { get { return this.TradeCoin.CoinPrice.AskQuantity; } }

        public BittrexAccount(string coin,
                              decimal tradingFee,
                              decimal bitcoinTransferFee,
                              string apiKey,
                              string apiSecret)
        {
            BittrexDefaults.SetDefaultApiCredentials(apiKey, apiSecret);

            this.TradeCoin = new Coin { Token = coin };
            this.Bitcoin = new Coin { Token = "BTC", TransferFee = bitcoinTransferFee };
            this.TradingFee = tradingFee;
        }

        public async Task UpdatePrices()
        {
            using (var client = new HttpClient())
            {
                string result = await client.GetStringAsync($"https://bittrex.com/api/v1.1/public/getticker?market=BTC-{this.TradeCoin.Token}");
                dynamic d = JsonConvert.DeserializeObject(result);

                this.TradeCoin.CoinPrice = new CoinPrice
                {
                    LastPrice = d.result.Last,
                    AskPrice = d.result.Ask,
                    BidPrice = d.result.Bid,
                    RetrivalTime = DateTime.Now
                };
            }
        }

        public async Task<object> Buy(decimal quantity, decimal price)
        {
            using (var bittrexClient = new BittrexClient())
            {
                // REMOVE THIS LINE WHEN PRODUCTION
                quantity = 1M; // FOR TESTING

                var result = await bittrexClient.PlaceOrderAsync(
                    OrderType.Buy,
                    $"BTC-{this.TradeCoin.Token}",
                    quantity,
                    price);

                return result;

            }
        }

        public async Task<object> Sell(decimal quantity, decimal price)
        {
            using (var bittrexClient = new BittrexClient())
            {
                // REMOVE THIS LINE WHEN PRODUCTION
                quantity = 1M; // FOR TESTING

                var result = await bittrexClient.PlaceOrderAsync(
                    OrderType.Sell,
                    $"BTC-{this.TradeCoin.Token}",
                    quantity,
                    price);
                
                return result;
            }
        }
    }
}