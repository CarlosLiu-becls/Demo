using System;
using BecLSA.Connect.Sync.Utils;

namespace MockSyncLogic
{
  class Program
  {
    static void Main(string[] args)
    {
      for (; ; )
      {
        System.Threading.Thread.Sleep(1000);

        const double cpuThreshold = 30;
        const double memoryThreshold = 30;
        var cpuUsage = CpuMemoryMetricsUtils.GetOverallCpuUsagePercentage();
        Console.WriteLine($"CPU usage: {cpuUsage}");
        if (cpuUsage >= cpuThreshold)
        {
          Console.WriteLine($"Upload terminated due to CPU usage is {cpuUsage}% which is higher than the threshold value {cpuThreshold}%.");
        }

        var memoryUsage = CpuMemoryMetricsUtils.GetOccupiedMemoryPercentage();
        if (memoryUsage < 0)
        {
          // log error then ignore it to ensure file uploading
          Console.WriteLine($"Failed to get the memory usage {memoryUsage}.");
        }
        else if (memoryUsage >= memoryThreshold)
        {
          Console.WriteLine($"Upload terminated due to memory usage is {memoryUsage}% which is higher than the threshold value {memoryThreshold}%.");
        }
      }
    }
  }
}
