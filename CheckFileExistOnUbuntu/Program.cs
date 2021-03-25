using System;
using System.IO;

namespace CheckFileExistOnUbuntu
{
  class Program
  {
    static void Main(string[] args)
    {
      Console.WriteLine("Please input the directory:");
      var directory = Console.ReadLine();

      var existed = File.Exists(directory);
      Console.WriteLine($"Is directory '{directory}' eixsted: {existed}");
    }
  }
}
