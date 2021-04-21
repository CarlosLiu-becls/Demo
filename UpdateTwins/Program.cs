using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace UpdateTwins
{
  class Program
  {
    static string DeviceConnectionString = "HostName=bcls-connect-dev-iothub-westus.azure-devices.net;DeviceId=51648b7a-6516-402d-90e3-2380fabbf26f;SharedAccessKey=Y9Sin0srIeYjZDnzKIgci+UQHI+XI5nL";
    static DeviceClient Client = null;

    static void Main(string[] args)
    {
      try
      {
        DoWorkAsync();
      }
      catch (Exception ex)
      {
        Console.WriteLine();
        Console.WriteLine("Error in sample: {0}", ex.Message);
      }
      Console.WriteLine("Press Enter to exit.");
      Console.ReadLine();
    }

    public static async void DoWorkAsync()
    {
      await InitClient();
      await ReportConnectivity();
    }


    public static async Task InitClient()
    {
      try
      {
        var watcher = new Stopwatch();
        watcher.Start();

        Console.WriteLine("Connecting to hub");
        Client = DeviceClient.CreateFromConnectionString(DeviceConnectionString, TransportType.Mqtt);
        Console.WriteLine("Retrieving twin");
        await Client.GetTwinAsync();

        watcher.Stop();
        Console.WriteLine($"InitClient ellapsed: {watcher.ElapsedMilliseconds} ms");
      }
      catch (Exception ex)
      {
        Console.WriteLine();
        Console.WriteLine("Error in sample: {0}", ex.Message);
      }
    }

    public static async Task ReportConnectivity()
    {
      try
      {
        Console.WriteLine("Sending connectivity data as reported property"); 
        var watcher = new Stopwatch();
        watcher.Start();


        TwinCollection reportedProperties, connectivity;
        reportedProperties = new TwinCollection();
        connectivity = new TwinCollection();
        connectivity["network"] = "{\"IPv4 Address\":\"10.10.10.10\",\"DHCP Enabled\":\"Yes\",\"Physical Address\":\"00 - 56 - FA - C9 - 9A - B0\",\"Proxy Settings\":{\"Proxy Server Enabled\":\"Yes\",\"Proxy Type\":\"socks\",\"Proxy IP Address\":\"192.168.23.7\",\"Proxy Port\":\"3344\"}}";
        reportedProperties["connectivity"] = connectivity;
        await Client.UpdateReportedPropertiesAsync(reportedProperties);

        watcher.Stop();
        Console.WriteLine($"ReportConnectivity Ellapsed: {watcher.ElapsedMilliseconds} ms");
      }
      catch (Exception ex)
      {
        Console.WriteLine();
        Console.WriteLine("Error in sample: {0}", ex.Message);
      }
    }
  }
}
