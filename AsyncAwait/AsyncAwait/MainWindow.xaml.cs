using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace AsyncAwait
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private List<string> logs = new List<string>();
    public MainWindow()
    {
      InitializeComponent();
    }

    private async void DownloadButton_Click(object sender, RoutedEventArgs e)
    {
      var webs = new List<string>
      {
        "https://stackoverflow.com/",
        "https://cn.bing.com/",
        //"https://www.baidu.com/",
        //"https://www.163.com/",
        //"https://www.microsoft.com/",
        //"https://about.me/",
        //"https://adblockplus.org/",
        "https://alternativeto.net/",
        //"https://boredbutton.com/"
      };

      var tasks = new List<Task>();
      foreach (var webLink in webs)
      {
        var task = DownloadIconAsync(webLink);
        tasks.Add(task);
      }

      //System.Threading.Thread.Sleep(2000);
      long result = 0;
      for(int i = 0; i < 1000000000; ++i)
      {
        result += i;
      }
      ShowStatusAndLog($"[ThreadId]:{CurrentThreadId} Result={result}.");

      //foreach (var task in tasks)
      //{
      //  ShowStatusAndLog($"[ThreadId]:{CurrentThreadId} Before await.");
      //  await task;
      //  ShowStatusAndLog($"[ThreadId]:{CurrentThreadId} After await.");
      //}

      ShowStatusAndLog($"[ThreadId]:{CurrentThreadId} All downloaded.");
    }

    private async Task DoUseAwait(List<string> webs)
    {
      foreach (var webLink in webs)
      {
        await DownloadIconAsync(webLink);
      }
    }

    private async Task DownloadIconAsync(string uri)
    {
      ShowStatusAndLog($"[ThreadId]:{CurrentThreadId} Downloading icon from {uri}...");

      var webClient = new WebClient();
      //var bytes = await webClient.DownloadDataTaskAsync(uri + "favicon.ico");

      ShowStatusAndLog($"[ThreadId]:{CurrentThreadId} before delay.");
      await Task.Delay(1000);
      ShowStatusAndLog($"[ThreadId]:{CurrentThreadId} after delay.");

      ShowStatusAndLog($"[ThreadId]:{CurrentThreadId} download for {uri}.");
      //var imageCtrl = new Image();
      //imageCtrl.Source = LoadImage(bytes);
      //imageCtrl.Stretch = System.Windows.Media.Stretch.None;
      //IconPanel.Children.Add(imageCtrl);
    }

    private static BitmapImage LoadImage(byte[] imageData)
    {
      if (imageData == null || imageData.Length == 0) return null;
      var image = new BitmapImage();
      using (var mem = new MemoryStream(imageData))
      {
        mem.Position = 0;
        image.BeginInit();
        image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.UriSource = null;
        image.StreamSource = mem;
        image.EndInit();
      }
      image.Freeze();
      return image;
    }

    private int CurrentThreadId
    {
      get { return System.Threading.Thread.CurrentThread.ManagedThreadId; }
    }

    private void ShowStatusAndLog(string message)
    {
      string log = DateTime.Now.ToString("O") + " " + message;
      //StatusLabel.Content = log;
      //LogsListBox.Items.Add(log);
      logs.Add(log);
    }

    private void ShowLogButton_Click(object sender, RoutedEventArgs e)
    {
      foreach (var log in logs)
      {
        LogsListBox.Items.Add(log);
      }
    }
  }
}
