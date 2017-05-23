using System;
using System.Windows;
using System.Diagnostics;
using System.Threading;

namespace YTY.amt
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    private const string MUTEXNAME = "amtMainMutex";
    private const int WAITTIMEOUT = 5000;

    private async void Application_Startup(object sender, StartupEventArgs e)
    {
      if (e.Args.Length > 0)
      {
        if (e.Args[0].Equals("--CheckUpdate", StringComparison.InvariantCultureIgnoreCase))
        {
          await UpdateServerModel.GetUpdateSourcesAsync();
          Console.Out.Write(UpdateServerModel.Status);
        }
        Shutdown();
      }
      else
      {
        if (Mutex.TryOpenExisting(MUTEXNAME, out var mutex))
        {
          if (mutex.WaitOne(WAITTIMEOUT))
          {
            new MainWindow().Show();
          }
          else
          {
            Shutdown();
          }
        }
        else
        {
          new MainWindow().Show();
        }
      }
    }
  }
}
