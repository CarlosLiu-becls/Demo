using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GetDirectoryFileSize
{
  class Program
  {
    static string thread1Output;
    static string thread2Output;
    const int retryCounts = 10;

    static void Main(string[] args)
    {
      Console.WriteLine("Please input the directoy:");
      var directory = Console.ReadLine();


      var thread1 = new Thread(Thread1Impl);
      thread1.Start(directory);

      var thread2 = new Thread(Thread2Impl);
      thread2.Start(directory);

      System.Threading.Thread.Sleep(15000);

      Console.WriteLine(thread1Output);
      Console.WriteLine(thread2Output);     

      Console.Read();
    }

    private static void Thread1Impl(object obj)
    {
      long totalTime = 0;
      for (int i = 0; i < retryCounts; ++i)
      {
        string directory = (string)obj;
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        var totalSize = DirSize(directory, true);
        stopWatch.Stop();

        thread1Output += $"Total Size: {totalSize} bytes, consume: {stopWatch.ElapsedMilliseconds} ms\r\n.";
        totalTime += stopWatch.ElapsedMilliseconds;
      }

      thread1Output += $"Avg ({retryCounts}): {totalTime / retryCounts} ms\r\n";
    }

    private static void Thread2Impl(object obj)
    {
      long totalTime = 0;
      for (int i = 0; i < retryCounts; ++i)
      {
        string directory = (string)obj;
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        var totalSize = CalcDirSize(directory, true);
        stopWatch.Stop();

        thread2Output += $"Use DirectoryInfo. Total Size: {totalSize} bytes, consume: {stopWatch.ElapsedMilliseconds} ms.\r\n";
        totalTime += stopWatch.ElapsedMilliseconds;
      }

      thread2Output += $"Use DirectoryInfo. Avg ({retryCounts}): {totalTime / retryCounts} ms\r\n";
    }

    private static long DirSize(string sourceDir, bool recurse)
    {
      long size = 0;
      string[] fileEntries = Directory.GetFiles(sourceDir);

      foreach (string fileName in fileEntries)
      {
        Interlocked.Add(ref size, (new FileInfo(fileName)).Length);
      }

      if (recurse)
      {
        string[] subdirEntries = Directory.GetDirectories(sourceDir);

        Parallel.For<long>(0, subdirEntries.Length, () => 0, (i, loop, subtotal) =>
        {
          if ((File.GetAttributes(subdirEntries[i]) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
          {
            subtotal += DirSize(subdirEntries[i], true);
            return subtotal;
          }
          return 0;
        },
            (x) => Interlocked.Add(ref size, x)
        );
      }
      return size;
    }

    public static long CalcDirSize(string sourceDir, bool recurse = true)
    {
      return _CalcDirSize(new DirectoryInfo(sourceDir), recurse);
    }

    private static long _CalcDirSize(DirectoryInfo di, bool recurse = true)
    {
      long size = 0;
      FileInfo[] fiEntries = di.GetFiles();
      foreach (var fiEntry in fiEntries)
      {
        Interlocked.Add(ref size, fiEntry.Length);
      }

      if (recurse)
      {
        DirectoryInfo[] diEntries = di.GetDirectories("*.*", SearchOption.TopDirectoryOnly);
        System.Threading.Tasks.Parallel.For<long>(0, diEntries.Length, () => 0, (i, loop, subtotal) =>
        {
          if ((diEntries[i].Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint) return 0;
          subtotal += _CalcDirSize(diEntries[i], true);
          return subtotal;
        },
            (x) => Interlocked.Add(ref size, x)
        );

      }
      return size;
    }
  }
}
