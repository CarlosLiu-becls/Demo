using Common;
using System;

namespace App2
{
  class Program
  {
    static void Main(string[] args)
    {
      for (int i = 500; i < 700; ++i)
      {
        ErrorLogger.Add("App-2", i, "Error from application 2.", DateTime.Now);
        System.Threading.Thread.Sleep(500);
      }

      Console.ReadKey();
    }
  }
}
