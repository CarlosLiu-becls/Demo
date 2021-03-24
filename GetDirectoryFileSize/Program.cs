using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GetDirectoryFileSize
{
  class Program
  {
    static void Main(string[] args)
    {
      Console.WriteLine("Please input the directoy:");
      var directory = Console.ReadLine();

      var stopWatch = new Stopwatch();
      stopWatch.Start();

      var totalSize = DirSize(directory, true);
      stopWatch.Stop();
      Console.WriteLine($"Total Size: {totalSize} bytes, consume: {stopWatch.ElapsedMilliseconds} ms.");
      Console.Read();
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
  }
}
