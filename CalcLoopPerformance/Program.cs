using System;
using System.Diagnostics;

namespace CalcLoopPerformance
{
    class Program
    {
        private const long MaxCount = 100000;
        static void Main(string[] args)
        {
            DoJobInTwoLoops();

            Console.Read();
        }

        private static void DoJobInALoop()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            double totalResult = 0;
            for (long index = 0; index < MaxCount; ++index)
            {
                Console.WriteLine($"Current index: ${index}.");
                totalResult += index;
            }

            stopWatch.Stop();
            Console.WriteLine($"In one loop: ${stopWatch.ElapsedMilliseconds}ms");

        }

        private static void DoJobInTwoLoops()
        {
            var duration1 = CalculateOnly();
            var duration2 = PrintOnly();

            Console.WriteLine($"In two loops: ${duration1 + duration2}ms");
        }

        private static long CalculateOnly()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            double totalResult = 0;
            for (long index = 0; index < MaxCount; ++index)
            {
                totalResult += index;
            }

            stopWatch.Stop();
            return stopWatch.ElapsedMilliseconds;
        }

        private static long PrintOnly()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            for (long index = 0; index < MaxCount; ++index)
            {
                Console.WriteLine($"Current index: ${index}.");
            }

            stopWatch.Stop();
            return stopWatch.ElapsedMilliseconds;
        }
    }
}
