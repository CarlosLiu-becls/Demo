using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace GetMemoryUsageInLinux
{
  class Program
  {
    private const int DigitsInResult = 2;
    private static long totalMemoryInKb;
    static void Main(string[] args)
    {
      for (int i = 0; i < 100; ++i)
      {
        GetOccupiedMemoryPercentage();
        Console.WriteLine("Press any key to get memory usage.");
        Console.ReadKey();
      }
    }

    /// <summary>
    /// Get the system overall memory usage percentage.
    /// </summary>
    /// <returns>The percentange value with the '%' sign. e.g. if the usage is 30.1234 %,
    /// then it will return 30.12.</returns>
    public static double GetOccupiedMemoryPercentage()
    {
      var totalMemory = GetTotalMemoryInKb();

      var usedMemory = GetUsedMemoryForAllProcessesInKb();

      var percentage = (usedMemory * 100) / totalMemory;

      var retVal = Math.Round(percentage, DigitsInResult);
      Console.WriteLine();
      Console.WriteLine($"Usage={retVal},\t\tTotalMemory: {totalMemory}, \t\tUsedMemory: {usedMemory}");
      return retVal;
    }

    private static double GetUsedMemoryForAllProcessesInKb()
    {
      long totalAllocatedMemoryInBytes = 0;
      foreach(var process in Process.GetProcesses())
      {
        var usedSize = process.PrivateMemorySize64;

        if (usedSize > 0)
        {
          Console.WriteLine($"Process:{process.ProcessName},\t\tPrivateMemorySize64:{process.PrivateMemorySize64}");
        }

        totalAllocatedMemoryInBytes += process.PrivateMemorySize64;
      }

      //var totalAllocatedMemoryInBytes = Process.GetProcesses().Sum(a => a.PrivateMemorySize64);
      return totalAllocatedMemoryInBytes / 1024.0;
    }

    private static long GetTotalMemoryInKb()
    {
      var watcher = new Stopwatch();
      watcher.Start();

      // only parse the file once
      if (totalMemoryInKb > 0)
      {
        return totalMemoryInKb;
      }

      string path = "/proc/meminfo";
      if (!File.Exists(path))
      {
        throw new FileNotFoundException($"File not found: {path}");
      }

      using (var reader = new StreamReader(path))
      {
        string line = string.Empty;
        while (!string.IsNullOrWhiteSpace(line = reader.ReadLine()))
        {
          if (line.Contains("MemTotal", StringComparison.OrdinalIgnoreCase))
          {
            // e.g. MemTotal:       16370152 kB
            var parts = line.Split(':');
            var valuePart = parts[1].Trim();
            parts = valuePart.Split(' ');
            var numberString = parts[0].Trim();

            var result = long.TryParse(numberString, out totalMemoryInKb);

            watcher.Stop();
            Console.WriteLine($"==== How long to read memory info (ms): {watcher.ElapsedMilliseconds}");

            return result ? totalMemoryInKb : throw new Exception($"Cannot parse 'MemTotal' value from the file {path}.");
          }
        }

        throw new Exception($"Cannot find the 'MemTotal' property from the file {path}.");
      }
    }
  }
}
