using System.Windows;

namespace YTY.amt
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    private void Application_Startup(object sender, StartupEventArgs e)
    {
      GlobalVars.MainViewModel.Init();
    }
  }

  public static class GlobalVars
  {
    private static App currentApp;
    private static MainViewModel mainViewModel;
    private static UpdateServerViewModel updateServerViewModel;
    private static DAL dal;

    static GlobalVars()
    {
      currentApp = Application.Current as App;
      updateServerViewModel = currentApp.FindResource(nameof(UpdateServerViewModel)) as UpdateServerViewModel;
      dal = currentApp.FindResource(nameof(DAL)) as DAL;
      mainViewModel = currentApp.FindResource(nameof(amt.MainViewModel)) as MainViewModel;
    }

    public static MainViewModel MainViewModel => mainViewModel;
    public static UpdateServerViewModel UpdateServerViewModel { get { return updateServerViewModel; } }
    public static DAL Dal => dal;
  }
}
