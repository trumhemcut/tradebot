using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using tradebot.TradePlatform;

namespace tradebot.TradePlatform
{
    public abstract class BittrexAccount : ITradeAccount
    {
        public Coin Bitcoin { get; set; }
        public Coin TradeCoin { get; set; }
        public async Task UpdatePrices()
        {
            using (var client = new HttpClient())
            {
                string result = await client.GetStringAsync($"https://bittrex.com/api/v1.1/public/getticker?market=BTC-{this.TradeCoin.Token}");
                dynamic d = JsonConvert.DeserializeObject(result);

                this.TradeCoin.CoinPrice.LastPrice = d.result.Last;
                this.TradeCoin.CoinPrice.BidPrice = d.result.Bid;
                this.TradeCoin.CoinPrice.AskPrice = d.result.Ask;
                this.TradeCoin.CoinPrice.RetrivalTime = DateTime.Now;
            }
        }
    }
}