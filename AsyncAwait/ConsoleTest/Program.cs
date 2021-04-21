using System;
using System.Threading.Tasks;

namespace ConsoleTest
{
  class Program
  {
    static void Main(string[] args)
    {
      DoWorkAsync();
      Console.Read();
    }

    private static async Task DoWorkAsync()
    {
      ShowLog("Before calling to DoWorkImplAsync");
      var task = DoWorkImplAsync();
      ShowLog("After calling to  DoWorkImplAsync");

      long result = 0;
      for (int i = 0; i < 1000000000; ++i)
      {
        result += i;
      }
      ShowLog($"After long calculation, result={result}");

      await task;
      ShowLog("After await the task.");
    }

    private static async Task DoWorkImplAsync()
    {
      ShowLog("Before async operation...");
      await Task.Delay(10000);
      ShowLog("After async operation.");
    }

    private static void ShowLog(string message)
    {
      var timestamp = DateTime.Now.ToString("O");
      var threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
      Console.WriteLine($"{timestamp}\tTid:{threadId}\t{message}");
    }
  }
}
