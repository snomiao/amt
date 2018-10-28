using System;
using System.Windows;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace YTY.amt
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    private const string MUTEXNAME = "amtMainMutex";
    private const int WAITTIMEOUT = 5000;

    public static App Program => (App)Application.Current;

    private async void Application_Startup(object sender, StartupEventArgs e)
    {
      if (e.Args.Length > 0)
      {
        if (e.Args[0].Equals("--CheckUpdate", StringComparison.InvariantCultureIgnoreCase))
        {
          await ProgramModel.UpdateServerModel.GetUpdateSourcesAsync();
          Console.Out.Write(ProgramModel.UpdateServerModel.Status);
        }
        Shutdown();
      }
      else
      {
        if (Mutex.TryOpenExisting(MUTEXNAME, out var mutex))
        {
          if (mutex.WaitOne(WAITTIMEOUT))
          {
            await RunWindow();
          }
          else
          {
            Shutdown();
          }
        }
        else
        {
          await RunWindow();
        }
      }
    }

    private async Task UpdateSelf()
    {
      try
      {
        var newVersion = await WebServiceClient.GetSelfVersion();
        var currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        if (newVersion > currentVersion)
        {
          MessageBox.Show($"检查到更新器最新版本 {newVersion}，当前版本 {currentVersion}，点击确定开始更新。");
          await WebServiceClient.DownloadSelf();
          System.IO.File.WriteAllBytes(Util.MakeQualifiedPath("amtLauncher.exe"), amt.Properties.Resources.amtLauncher);
          Process.Start(Util.MakeQualifiedPath("amtLauncher"), "--ReplaceUpdater");
          Shutdown();
        }
      }
      catch (HttpRequestException)
      {
        MessageBox.Show("因网络问题，更新无法完成，请稍候重试！");
        Shutdown();
      }
    }

    private async Task RunWindow()
    {
      try
      {
        await UpdateSelf();
        var processes = Process.GetProcessesByName("amt");
        foreach (var process in processes)
        {
          process.WaitForExit();
        }
        new MainWindow().Show();
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.ToString());
        Shutdown();
      }
    }

    private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
      MessageBox.Show(e.Exception.ToString());
      Shutdown();
    }
  }
}
