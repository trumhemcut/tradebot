using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace tradebot
{
    public class TradeBot
    {
        private static TradeBot _tradebot;
        public static TradeBot Instance
        {
            get
            {
                if (_tradebot == null)
                    _tradebot = new TradeBot();

                return _tradebot;
            }
        }

        public async Task Execute()
        {
            var result = await GetCoinPriceFromBittrex("ADA");
        }

        public async Task<CoinPrice> GetCoinPriceFromBittrex(string coin)
        {
            using (var client = new HttpClient())
            {
                string result = await client.GetStringAsync($"https://bittrex.com/api/v1.1/public/getticker?market=BTC-{coin}");
                dynamic d = JsonConvert.DeserializeObject(result);
                return new CoinPrice{
                    Coin = coin,
                    LastPrice = d.result.Last,
                    BidPrice = d.result.Bid,
                    AskPrice = d.result.Ask,
                    RetrivalTime = DateTime.Now
                };
            }
        }

        // Reference to https://www.binance.com/restapipub.html#user-content-market-data-endpoints
        public async Task<CoinPrice> GetCoinPriceFromBinance(string coin){
            using (var client = new HttpClient())
            {
                string result = await client.GetStringAsync($"https://api.binance.com/api/v1/depth?symbol=ADABTC");
                dynamic d = JsonConvert.DeserializeObject(result);
                return new CoinPrice{
                    Coin = coin,
                    LastPrice = d.result.Last,
                    BidPrice = d.result.Bid,
                    AskPrice = d.result.Ask,
                    RetrivalTime = DateTime.Now
                };
            }
        }
    }
}
