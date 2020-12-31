using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace Common
{
  public static class ErrorLogger
  {
    private static readonly ConcurrentQueue<ErrorItem> queue = new ConcurrentQueue<ErrorItem>();
    private static  Timer timer;
    public const string DefaultLinuxLogFolder = "/var/log/beclsa.connect/";
    private const string DefaultWindowsLogFolder = @"C:\Temp\";

    public static void Add(string moduleName, int errorCode, string description, DateTime timestamp)
    {
      var error = new ErrorItem
      {
        Module = moduleName,
        ErrorCode = errorCode,
        Message = description,
        LocalTimestamp = timestamp,
      };

      queue.Enqueue(error);

      if (timer == null)
      {
        // the 1st run will be in 10 seconds later, then
        // run every 60 seconds
        timer = new Timer(WriteLogFile, null, 5000, 10000);
      }
    }

    private static void WriteLogFile(object state)
    {
      ErrorItem error;
      var errorList = new List<string>();
      while (queue.TryDequeue(out error))
      {
        errorList.Add(error.ToString());
      }

      var logRootPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? DefaultWindowsLogFolder
        : DefaultLinuxLogFolder;

      File.AppendAllLines(Path.Combine(logRootPath, "sync.error.log"), errorList);
    }

    private class ErrorItem
    {
      public string Module { get; set; }
      public int ErrorCode { get; set; }
      public string Message { get; set; }
      public DateTime LocalTimestamp { get; set; }

      public override string ToString()
      {
        return $"Timestamp:{LocalTimestamp.ToString("yyyy-MM-dd HH:mm:ss.fff zzz")}, Module: {Module}, ErrorCode: {ErrorCode}, Message: {Message}";
      }
    }
  }
}
