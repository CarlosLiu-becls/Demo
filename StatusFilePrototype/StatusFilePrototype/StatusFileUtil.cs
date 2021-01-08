using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace StatusFilePrototype
{
  public static class StatusFileUtil
  {
    private const string LogFilename = "beclsa.connect.events.log";
    private static readonly string LogFileFullPath = GetLogFileWithFullPath();
    private static Timer timer;
    private static ILogger appLogger;
    private static readonly ConcurrentDictionary<int, EventItem> EventList = new ConcurrentDictionary<int, EventItem>();
    private static string LastContent;

    public static void Initialize(ILogger logger)
    {
      appLogger = logger;
    }

    public static void AddEvent(string moduleName, int errorCode, string description, DateTime timestamp)
    {
      var eventItem = new EventItem
      {
        Module = moduleName,
        ErrorCode = errorCode,
        Message = description,
        LocalTimestamp = timestamp,
      };

      EventList.AddOrUpdate(
        errorCode,
        eventItem,
        (code, existingItem) =>
          {
            existingItem.LocalTimestamp = eventItem.LocalTimestamp;
            return existingItem;
          }
        );


      if (appLogger != null)
      {
        appLogger.LogDebug($"Added event to the queue: {eventItem}.");
      }

      if (timer == null)
      {
        // the 1st run will be in 10 seconds later, then
        // run every 60 seconds
        timer = new Timer(WriteLogFile, null, 0, 5000);
      }
    }

    private static string GetLogFileWithFullPath()
    {
      var logDirectory = @"C:\temp";
      return Path.Combine(logDirectory, LogFilename);
    }

    private static void WriteLogFile(object state)
    {
      try
      {
        var content = JsonConvert.SerializeObject(EventList);
        if(content.Equals(LastContent, StringComparison.OrdinalIgnoreCase))
        {
          // file content does not change, so do not need to write
          return;
        }

        File.WriteAllText(LogFileFullPath, content);
        LastContent = content;
      }
      catch (Exception exception)
      {
        if (appLogger != null)
        {
          appLogger.LogError(exception, $"Failed to write events to file {LogFileFullPath}.");
        }
      }
    }

    private class EventItem
    {
      public string Module { get; set; }
      public int ErrorCode { get; set; }
      public string Message { get; set; }
      public DateTime LocalTimestamp { get; set; }
    }
  }
}
