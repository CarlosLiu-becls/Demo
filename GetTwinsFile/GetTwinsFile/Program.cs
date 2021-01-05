using System;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace GetTwinsFile
{
  class Program
  {
    private const string DeviceConnectionString = "HostName=bcls-connect-dev-iothub-westus.azure-devices.net;DeviceId=6411042d-6d4e-48f3-8338-639e50039d4a;SharedAccessKey=EWKfqaUrR2IikAsg72PVrNl/33S6nH6D";
    private static DeviceClient deviceClient;

    static async Task Main(string[] args)
    {
      var stopwatcher = new Stopwatch();
      stopwatcher.Start();
      deviceClient = DeviceClient.CreateFromConnectionString(DeviceConnectionString);

      stopwatcher.Stop();
      Console.WriteLine($"Connect using: {stopwatcher.ElapsedMilliseconds} ms.");
      stopwatcher.Restart();

      var twin = await deviceClient.GetTwinAsync();//变化了是否能自动更新

      stopwatcher.Stop();
      Console.WriteLine($"Get twin: {stopwatcher.ElapsedMilliseconds} ms.");
      stopwatcher.Restart();

      var cupValue = twin.Properties.Desired["instrument"]["cpuThreshold"];
      var memoryValue = twin.Properties.Desired["instrument"]["memoryThreshold"];

      stopwatcher.Stop();
      Console.WriteLine($"Read threshold values: {stopwatcher.ElapsedMilliseconds} ms.");
      stopwatcher.Restart();

      // update the timestamp
      var reportedProperties = new TwinCollection
      {
        ["DateTimeLastDesiredPropertyChangeReceived"] = DateTime.UtcNow
      };
      await deviceClient.UpdateReportedPropertiesAsync(reportedProperties);

      stopwatcher.Stop();
      Console.WriteLine($"Write timestamp value: {stopwatcher.ElapsedMilliseconds} ms.");


      await deviceClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertyChangedAsync, null);
      Console.ReadKey();

      //using var cts = new CancellationTokenSource();
      //while (!cts.IsCancellationRequested)
      //{
      //  Console.WriteLine("Waiting desired property changed...");
      //  await Task.Delay(1000, cts.Token);
      //}
    }

    private static async Task OnDesiredPropertyChangedAsync(TwinCollection desiredProperties, object userContext)
    {
      // get twins specific tags
      var cpu = desiredProperties["instrument"]["cpuThreshold"];
      var memory = desiredProperties["instrument"]["memoryThreshold"];

      // notify twins you received
      var deviceClient = DeviceClient.CreateFromConnectionString(DeviceConnectionString);
      var reportedProperties = new TwinCollection
      {
        ["DateTimeLastDesiredPropertyChangeReceived"] = DateTime.UtcNow
      };

      await deviceClient.UpdateReportedPropertiesAsync(reportedProperties);
    }
  }
}
