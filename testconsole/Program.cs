using System;
using System.Threading;

namespace testconsole
{
    class Program
    {
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

        public static void PrintDashboard()
        {
            Console.WriteLine($"Left: {Console.CursorLeft}, Top: {Console.CursorTop}");
            var left = Console.CursorLeft;
            var top = Console.CursorTop;
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
                Console.SetCursorPosition(left, top);
                Thread.Sleep(1000);
            }
        }
    }
}
