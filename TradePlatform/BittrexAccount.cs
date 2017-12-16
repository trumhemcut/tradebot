using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using tradebot.TradePlatform;

namespace tradebot.TradePlatform
{
    public class BittrexAccount : ITradeAccount
    {
        public Coin Bitcoin { get; set; }
        public Coin TradeCoin { get; set; }
        public decimal TradingFee { get; set; }

        public BittrexAccount(string coin,
                              decimal tradingFee,
                              decimal bitcoinTransferFee)
        {
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
                    RetrivalTime = DateTime.Now
                };
            }
        }
    }
}