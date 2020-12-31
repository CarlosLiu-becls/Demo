using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace BecLSA.Connect.Sync.Utils
{
  internal static class CpuMemoryMetrics4LinuxUtils
  {
    private const int DigitsInResult = 2;
    private static long totalMemoryInKb;

    /// <summary>
    /// Get the system overall CPU usage percentage.
    /// </summary>
    /// <returns>The percentange value with the '%' sign. e.g. if the CPU usage is 30 %,
    /// then it will return 30.</returns>
    public static double GetOverallCpuUsagePercentage()
    {
      // refer to https://stackoverflow.com/questions/59465212/net-core-cpu-usage-for-machine
      var startTime = DateTime.UtcNow;
      var startCpuUsage = Process.GetProcesses().Sum(a => a.TotalProcessorTime.TotalMilliseconds);

      System.Threading.Thread.Sleep(500);

      var endTime = DateTime.UtcNow;
      var endCpuUsage = Process.GetProcesses().Sum(a => a.TotalProcessorTime.TotalMilliseconds);

      var cpuUsedMs = endCpuUsage - startCpuUsage;
      var totalMsPassed = (endTime - startTime).TotalMilliseconds;
      var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

      return Math.Round(cpuUsageTotal * 100, DigitsInResult);
    }

    /// <summary>
    /// Get the system overall memory usage percentage.
    /// </summary>
    /// <returns>The percentange value with the '%' sign. e.g. if the usage is 30 %,
    /// then it will return 30.</returns>
    public static double GetOccupiedMemoryPercentage()
    {
      var totalMemory = GetTotalMemoryInKb();
      var usedMemory = GetUsedMemoryForAllProcessesInKb();
      Console.WriteLine($"Total Memory :{Math.Round((double)totalMemory/1024, 2)}, \tFree Memory: {Math.Round((totalMemory - usedMemory)/1024, 2)}");

      var percentage = (usedMemory * 100) / totalMemory;
      return Math.Round(percentage, DigitsInResult);
    }

    private static double GetUsedMemoryForAllProcessesInKb()
    {
      var totalAllocatedMemoryInBytes = Process.GetProcesses().Sum(a => a.PrivateMemorySize64);
      return totalAllocatedMemoryInBytes / 1024.0;
    }

    private static long GetTotalMemoryInKb()
    {
      // only parse the file once
      if (totalMemoryInKb > 0)
      {
        Console.WriteLine($"Totaol memory already got:{totalMemoryInKb}.");
        return totalMemoryInKb;
      }

      const int invalidResult = -1;
      string path = "/proc/meminfo";
      if (!File.Exists(path))
      {
        return invalidResult;
      }

      using (var reader = new StreamReader(path))
      {
        try
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
              return result ? totalMemoryInKb : invalidResult;
            }
          }

          return invalidResult;
        }
        catch (Exception)
        {
          return invalidResult;
        }
      }
    }
  }
}
