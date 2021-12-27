using System;
using System.IO;

namespace GetFileCreationTime
{
  class Program
  {
    static void Main(string[] args)
    {
      Console.WriteLine("Please tell the file ...");

      string filename = Console.ReadLine();

      if(!string.IsNullOrWhiteSpace(filename))
      {
        var fileInfo = new FileInfo(filename);
        fileInfo.Refresh();

        var ctime = fileInfo.CreationTime;
        var latime = fileInfo.LastAccessTime;
        Console.WriteLine($"CreationTime: {ctime.ToString("O")}, LastAccessTime={latime.ToString("O")}");
        Console.Read();
      }
    }
  }
}
