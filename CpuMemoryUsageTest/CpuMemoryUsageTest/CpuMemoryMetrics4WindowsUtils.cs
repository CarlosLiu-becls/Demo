using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BecLSA.Connect.Sync.Utils
{
  internal static class CpuMemoryMetrics4WindowsUtils
  {
    private const int DigitsInResult = 2;
    private const int InvalidMemorySize = -1;
    private static readonly PerformanceCounter CpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

    /// <summary>
    /// Get the system overall CPU usage percentage.
    /// </summary>
    /// <returns>The percentange value with the '%' sign. e.g. if the usage is 30.1234 %,
    /// then it will return 30.12.</returns>
    public static double GetOverallCpuUsagePercentage()
    {
      var cpuUsage = CpuCounter.NextValue();

      // the 1st call may return 0, so need to call it twice
      // refer to :https://stackoverflow.com/questions/278071/how-to-get-the-cpu-usage-in-c
      if (cpuUsage == 0.0f)
      {
        System.Threading.Thread.Sleep(500);
        cpuUsage = CpuCounter.NextValue();
      }

      return Math.Round(cpuUsage, DigitsInResult);
    }

    // get the memory usage, refer to https://stackoverflow.com/questions/10027341/c-sharp-get-used-memory-in
    [DllImport("psapi.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetPerformanceInfo([Out] out PerformanceInformation performanceInformation, [In] int size);

    [StructLayout(LayoutKind.Sequential)]
    private struct PerformanceInformation
    {
      public int Size;
      public IntPtr CommitTotal;
      public IntPtr CommitLimit;
      public IntPtr CommitPeak;
      public IntPtr PhysicalTotal;
      public IntPtr PhysicalAvailable;
      public IntPtr SystemCache;
      public IntPtr KernelTotal;
      public IntPtr KernelPaged;
      public IntPtr KernelNonPaged;
      public IntPtr PageSize;
      public int HandlesCount;
      public int ProcessCount;
      public int ThreadCount;
    }

    /// <summary>
    /// Get the system overall memory usage percentage.
    /// </summary>
    /// <returns>The percentange value with the '%' sign. e.g. if the usage is 30.1234 %,
    /// then it will return 30.12.</returns>
    public static double GetOccupiedMemoryPercentage()
    {
      var available = GetPhysicalAvailableMemoryInMiB();
      var total = GetTotalMemoryInMiB();
      var percentFree = ((double)available / total) * 100;
      var percentOccupied = Math.Round(100 - percentFree, DigitsInResult);

      return (double)Math.Round(percentOccupied, DigitsInResult);
    }

    private static long GetPhysicalAvailableMemoryInMiB()
    {
      var performanceInfo = default(PerformanceInformation);
      if (GetPerformanceInfo(out performanceInfo, Marshal.SizeOf(performanceInfo)))
      {
        return Convert.ToInt64(performanceInfo.PhysicalAvailable.ToInt64() * performanceInfo.PageSize.ToInt64() / 1048576);
      }
      else
      {
        return InvalidMemorySize;
      }
    }

    private static long GetTotalMemoryInMiB()
    {
      var performanceInfo = default(PerformanceInformation);
      if (GetPerformanceInfo(out performanceInfo, Marshal.SizeOf(performanceInfo)))
      {
        return Convert.ToInt64(performanceInfo.PhysicalTotal.ToInt64() * performanceInfo.PageSize.ToInt64() / 1048576);
      }
      else
      {
        return InvalidMemorySize;
      }
    }
  }
}
