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
      var iotHubService = new IoTHubService();
      await iotHubService.SyncWithTwinAsync("HostName=bcls-connect-dev-iothub-westus.azure-devices.net;DeviceId=a1b754b6-efd4-4193-a1b3-cbc6128e6acf;SharedAccessKey=LoboBMfTHn68DxWQh9b9fl1ZDdg0bEsO");
      
      Console.ReadKey();
    }

    public static async Task GetCpuMemoryAsync()
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

  public class IoTHubService
  {
    private const string InstrumentNode = "instrument";
    private const string AvailableDriverSizeThresholdNode = "availableDriverSizeInMbThreshold";
    private const double DefaultAvaialbeDriverSizeThreshold = 500;
    private DeviceClient deviceClient;

    public double AvailableDriverSizeInMbThreshold { get; private set; }

    public IoTHubService()
    {
      AvailableDriverSizeInMbThreshold = DefaultAvaialbeDriverSizeThreshold;
    }

    ~IoTHubService() => Dispose(false);

    public async Task SyncWithTwinAsync(string devicePrimaryConnString)
    {
      Console.WriteLine("Before reading IoT Hub twin file.");

      try
      {
        // dispose the previous object before creating the new one
        deviceClient?.Dispose();
        deviceClient = DeviceClient.CreateFromConnectionString(devicePrimaryConnString, TransportType.Mqtt_WebSocket_Only);
        var twin = await deviceClient.GetTwinAsync().ConfigureAwait(false);
        AvailableDriverSizeInMbThreshold = twin.Properties.Desired[InstrumentNode][AvailableDriverSizeThresholdNode];

        Console.WriteLine($"Read value from IoT Hub twin file, AvailableDriverSizeInMbThreshold={AvailableDriverSizeInMbThreshold}.");
      }
      catch (Exception exception)
      {
        Console.WriteLine($"Failed to sync with IoT Hub twin, please check the connection string or if there is desired property '{InstrumentNode}/{AvailableDriverSizeThresholdNode}' for the device.");
        AvailableDriverSizeInMbThreshold = DefaultAvaialbeDriverSizeThreshold;
      }

      // register a callback to get the updated value.
      if (deviceClient != null)
      {
        await deviceClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertyChangedAsync, null).ConfigureAwait(false);
      }
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        deviceClient?.Dispose();
      }
    }

    private async Task OnDesiredPropertyChangedAsync(TwinCollection desiredProperties, object userContext)
    {
      // get twins specific tags
      try
      {
        Console.WriteLine("Desired property is changed, and before reading IoT Hub twin file.");

        AvailableDriverSizeInMbThreshold = desiredProperties[InstrumentNode][AvailableDriverSizeThresholdNode];
        Console.WriteLine($"Read value when desired property is changed, AvailableDriverSizeInMbThreshold={AvailableDriverSizeInMbThreshold}.");
      }
      catch (Exception exception)
      {
        Console.WriteLine("Failed to read threshold when desired property is changed.");
        AvailableDriverSizeInMbThreshold = DefaultAvaialbeDriverSizeThreshold;
      }

      var succeeded = await IoTHubServiceUtils.UpdateLastDesiredPropertyChangeReceivedAsync(deviceClient).ConfigureAwait(false);
      if (succeeded)
      {
        Console.WriteLine("UpdateReportedPropertiesAsync is called successfully.");
      }
      else
      {
        Console.WriteLine($"Failed to call the UpdateReportedPropertiesAsync.");
      }
    }
  }

  public static class IoTHubServiceUtils
  {
    private const string PropReceivedTimestampNode = "DateTimeLastDesiredPropertyChangeReceived";

    /// <summary>
    /// Update the timestamp for the last desired property changed event.
    /// </summary>
    /// <param name="deviceClient">The device client, and the caller is responsible for disposing it.</param>
    /// <returns>True if there is no error.</returns>
    public static async Task<bool> UpdateLastDesiredPropertyChangeReceivedAsync(DeviceClient deviceClient)
    {
      if (deviceClient == null)
      {
        return false;
      }

      try
      {
        // notify twins you received
        var reportedProperties = new TwinCollection
        {
          [PropReceivedTimestampNode] = DateTime.UtcNow,
        };

        await deviceClient.UpdateReportedPropertiesAsync(reportedProperties).ConfigureAwait(false);
        return true;
      }
      catch
      {
        return false;
      }
    }
  }
}
