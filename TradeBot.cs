using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace tradebot
{
    public class TradeBot
    {
        public int ResumeAfterExpectedDelta { get; protected set; }
        public decimal ExpectedDelta { get; protected set; }
        public string Coin { get; protected set; }
        public string EmailTo { get; set; }
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
        public TradeBot()
        {
            this.Coin = "ADA";
        }
        public TradeBot(string coin, 
                        decimal expectedDelta, 
                        int resumeAfterExpectedData,
                        string emailTo)
        {
            this.Coin = coin;
            this.ExpectedDelta = expectedDelta;
            this.ResumeAfterExpectedDelta = resumeAfterExpectedData;
            this.EmailTo = emailTo;
        }
        public async Task Execute()
        {
            while (true)
            {
                var getBittrexPriceTask = GetCoinPriceFromBittrex(this.Coin);
                var getBinancePriceTask = GetCoinPriceFromBinance(this.Coin);
                await Task.WhenAll(getBittrexPriceTask, getBinancePriceTask);

                var deltaBidBid = getBinancePriceTask.Result.BidPrice - getBittrexPriceTask.Result.BidPrice;
                var deltaBidAsk = getBinancePriceTask.Result.BidPrice - getBittrexPriceTask.Result.AskPrice;
                Console.WriteLine($"Bittrex Price: {getBittrexPriceTask.Result.BidPrice} - " +
                                  $"Binance Price: {getBinancePriceTask.Result.BidPrice} - " +
                                  $"Bid-Bid: {deltaBidBid} - " +
                                  $"Bid-Ask: {deltaBidAsk}");

                // Check to send notification
                if (deltaBidBid >= this.ExpectedDelta)
                {
                    Console.WriteLine("Time to buy ...");
                    await EmailHelper.SendEmail($"Time to buy {deltaBidBid}", this.EmailTo, "Buy di pa");

                    Thread.Sleep(TimeSpan.FromMinutes(this.ResumeAfterExpectedDelta));
                }

                Thread.Sleep(2000);
            }
        }

        public async Task<CoinPrice> GetCoinPriceFromBittrex(string coin)
        {
            using (var client = new HttpClient())
            {
                string result = await client.GetStringAsync($"https://bittrex.com/api/v1.1/public/getticker?market=BTC-{coin}");
                dynamic d = JsonConvert.DeserializeObject(result);
                return new CoinPrice
                {
                    Coin = coin,
                    LastPrice = d.result.Last,
                    BidPrice = d.result.Bid,
                    AskPrice = d.result.Ask,
                    RetrivalTime = DateTime.Now
                };
            }
        }

        // Reference to https://www.binance.com/restapipub.html#user-content-market-data-endpoints
        public async Task<CoinPrice> GetCoinPriceFromBinance(string coin)
        {
            using (var client = new HttpClient())
            {
                string result = await client.GetStringAsync($"https://api.binance.com/api/v1/depth?symbol={coin}BTC&limit=5");
                dynamic d = JsonConvert.DeserializeObject(result);
                return new CoinPrice
                {
                    Coin = coin,
                    LastPrice = d.bids[0][0],
                    BidPrice = d.bids[0][0],
                    AskPrice = d.asks[0][0],
                    RetrivalTime = DateTime.Now
                };
            }
        }
    }
}
