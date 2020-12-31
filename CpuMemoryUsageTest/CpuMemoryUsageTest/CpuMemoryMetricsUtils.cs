using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace BecLSA.Connect.Sync.Utils
{
  internal static class CpuMemoryMetricsUtils
  {
    /// <summary>
    /// Get the system overall CPU usage percentage.
    /// </summary>
    /// <returns>The percentange value with the '%' sign. e.g. if the CPU usage is 30 %,
    /// then it will return 30.</returns>
    public static double GetOverallCpuUsagePercentage()
    {
      return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
        CpuMemoryMetrics4WindowsUtils.GetOverallCpuUsagePercentage() :
        CpuMemoryMetrics4LinuxUtils.GetOverallCpuUsagePercentage();
    }

    /// <summary>
    /// Get the system overall memory usage percentage.
    /// </summary>
    /// <returns>The percentange value with the '%' sign. e.g. if the usage is 30 %,
    /// then it will return 30.</returns>
    public static double GetOccupiedMemoryPercentage()
    {
      return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
        CpuMemoryMetrics4WindowsUtils.GetOccupiedMemoryPercentage() :
        CpuMemoryMetrics4LinuxUtils.GetOccupiedMemoryPercentage();
    }
  }
}
