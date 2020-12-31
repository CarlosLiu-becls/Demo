using Common;
using System;

namespace App1
{
  class Program
  {
    static void Main(string[] args)
    {
      for(int i = 0; i < 100; ++i)
      {
        ErrorLogger.Add("App-1", i, "Error from application 1.", DateTime.Now);
        System.Threading.Thread.Sleep(1000);
      }

      Console.ReadKey();
    }
  }
}
