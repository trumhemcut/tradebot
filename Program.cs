using System;

namespace tradebot
{
    class Program
    {
        static void Main(string[] args)
        {
            TradeBot.Instance.Execute().Wait();
        }
    }
}
