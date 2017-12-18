using System;
using System.Threading;
using System.Threading.Tasks;

namespace testconsole
{
    class Program
    {
        enum TradeAction
        {
            ReadyToBuy,
            Buy,
            SellPriceUp,
            SellPriceDown,
            BuyPriceUp,
            BuyPriceDown,
            LookAndSee
        }
        static volatile TradeAction tradeAction = TradeAction.LookAndSee;
        static void Main(string[] args)
        {
            PrintDashboard();
        }

        public static decimal Random(int from, int to)
        {
            Random r = new Random();
            var number = r.Next(from, to) / 100000000M;
            return number;
        }

        private static void WaitingForKeyPress()
        {
            while (true)
            {
                var key = Console.ReadKey(false);
                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        tradeAction = TradeAction.ReadyToBuy;
                        break;
                    case ConsoleKey.UpArrow:
                        tradeAction = TradeAction.BuyPriceUp;
                        break;
                    case ConsoleKey.DownArrow:
                        tradeAction = TradeAction.BuyPriceDown;
                        break;
                    case ConsoleKey.RightArrow:
                        tradeAction = TradeAction.SellPriceUp;
                        break;
                    case ConsoleKey.LeftArrow:
                        tradeAction = TradeAction.SellPriceDown;
                        break;
                    case ConsoleKey.B:
                        tradeAction = TradeAction.Buy;
                        break;
                    default:
                        tradeAction = TradeAction.LookAndSee;
                        break;
                }
            }
        }

        public static void PrintDashboard()
        {
            Console.WriteLine($"Left: {Console.CursorLeft}, Top: {Console.CursorTop}");
            var left = Console.CursorLeft;
            var top = Console.CursorTop;

            Task.Factory.StartNew(() => WaitingForKeyPress());

            while (true)
            {
                Console.WriteLine(string.Format(@"
--------------------------------------
 SELL              | BUY              
-------------------|------------------
 {0} BTC    | {1} BTC       

 {2} ADA     | {3} ADA      
 {0} BTC    | {1} BTC        
--------------------------------------
 Profit: {4} ADA                    
 Delta: {5}
--------------------------------------
                ",
                Random(2508, 3008).ToString("0.00000000"),
                Random(2508, 3008).ToString("0.00000000"),
                10800M.ToString("#,###.#0"),
                19000M.ToString("#,###.#0"),
                1000M.ToString("#,###.#0"),
                100
                ));

                Console.WriteLine(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                switch (tradeAction)
                {
                    case TradeAction.Buy:
                        Console.WriteLine("BUY FOR NOW");
                        break;
                    case TradeAction.BuyPriceDown:
                        Console.WriteLine("BUY PRICE DOWN 1-");
                        break;

                    case TradeAction.BuyPriceUp:
                        Console.WriteLine("BUY PRICE UP 1+");
                        break;

                    case TradeAction.SellPriceDown:
                        Console.WriteLine("SELL PRICE DOWN 1-");
                        break;

                    case TradeAction.SellPriceUp:
                        Console.WriteLine("SELL PRICE UP 1+");
                        break;

                    case TradeAction.ReadyToBuy:
                        Console.WriteLine("READY TO BUY");
                        break;

                    default:
                        break;
                }
                
                Console.SetCursorPosition(left, top);
                
                Thread.Sleep(1000);
            }
        }
    }
}