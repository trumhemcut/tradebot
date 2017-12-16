using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace tradebot.TradePlatform
{
    public class BinanceAccount
    {
        public Coin Bitcoin { get; set; }
        public Coin TradeCoin { get; set; }

        // Reference to https://www.binance.com/restapipub.html#user-content-market-data-endpoints
        public async Task UpdatePrices()
        {
            
            using (var client = new HttpClient())
            {
                string result = await client.GetStringAsync($"https://api.binance.com/api/v1/depth?symbol={this.TradeCoin.Token}BTC&limit=5");
                dynamic d = JsonConvert.DeserializeObject(result);

                this.TradeCoin.CoinPrice.LastPrice = d.bids[0][0];
                this.TradeCoin.CoinPrice.BidPrice = d.bids[0][0];
                this.TradeCoin.CoinPrice.AskPrice = d.asks[0][0];
                this.TradeCoin.CoinPrice.RetrivalTime = DateTime.Now;
            }
        }
    }
}