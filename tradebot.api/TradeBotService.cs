using System;
using System.Threading;
using System.Threading.Tasks;

namespace tradebot.api
{
    public class TradeBotService : HostedService
    {
        public TradeBotService()
        {

        }

        protected override Task ExecuteAsync(System.Threading.CancellationToken cancellationToken)
        {
            while (true)
            {
                Console.WriteLine($"Hello, I'm a tradebot {DateTime.Now}");
                Thread.Sleep(5000);
            }
        }
    }
}