using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace StatusFilePrototype
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
    }

    private void DoWorkForSameCode()
    {
      StatusFileUtil.AddError(1001, "Dummy error.", DateTime.Now);
    }
    
    private int GetRandom(int min, int max)
    {
      var random = new Random((int)DateTime.Now.Ticks);
      return random.Next(min, max);
    }

    private async Task WorkToWriteSameErrorCodeAsync()
    {
      int threadsCount = int.Parse(ThreadsCountTextBox.Text.Trim());
      for (int i = 0; i < threadsCount; ++i)
      {
        var newThread = new Thread(DoWorkForSameCode);
        newThread.Start();
        LogListBox.Items.Add($"#{i}\t{DateTime.Now} New thread is started.");

        await Task.Delay(GetRandom(100, 1000)).ConfigureAwait(true);
      }
    }

    private void SameCodeButton_Click(object sender, RoutedEventArgs e)
    {
      WorkToWriteSameErrorCodeAsync();
    }


    private void DoWorkForDiffCode()
    {
      var randomErrorCode = GetRandom(1001, 1006);
      StatusFileUtil.AddError(randomErrorCode, "Dummy error.", DateTime.Now);
    }

    private async Task WorkToWriteDiffErrorCodeAsync()
    {
      int threadsCount = int.Parse(ThreadsCountTextBox.Text.Trim());
      for (int i = 0; i < threadsCount; ++i)
      {
        var newThread = new Thread(DoWorkForDiffCode);
        newThread.Start();
        LogListBox.Items.Add($"#{i}\t{DateTime.Now} New thread is started.");

        await Task.Delay(GetRandom(100, 1000)).ConfigureAwait(true);
      }
    }

    private void DiffCodeButton_Click(object sender, RoutedEventArgs e)
    {
      WorkToWriteDiffErrorCodeAsync();
    }
  }
}
