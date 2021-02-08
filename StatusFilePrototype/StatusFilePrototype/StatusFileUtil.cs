using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    private static string LastContent;
    private static readonly EventItem eventItem = new EventItem();

    static StatusFileUtil()
    {
      eventItem.AppStartupTime = DateTime.Now;
    }

    public static void Initialize(ILogger logger)
    {
      appLogger = logger;
    }

    public static void AddError(int errorCode, string description, DateTime timestamp)
    {
      eventItem.LastError = new ErrorItem
      {
        ErrorCode = errorCode,
        Description = description,
        LocalTimestamp = timestamp,
        Details = new List<FileError>
        {
          new FileError { File = @"c:\temp\file-1.log", Description = "The process cannot access the file because it is being used by another process even after retry" },
          new FileError { File = @"c:\temp\file-2.log", Description = "File not found." }
        }
      };

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

    public static void ClearLastError()
    {
      eventItem.LastError = null;
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
        var content = JsonConvert.SerializeObject(eventItem);
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

    internal class EventItem
    {
      public DateTime? AppStartupTime { get; set; }
      public DateTime? LastUploadTime { get; set; }
      public ErrorItem LastError { get; set; }
      
    }

    internal class ErrorItem
    {
      public int ErrorCode { get; set; }
      public string Description { get; set; }
      public DateTime LocalTimestamp { get; set; }

      public List<FileError> Details { get; set; }
    }

    internal class FileError
    {
      public string File { get; set; }
      public string Description { get; set; }
    }
  }
}
