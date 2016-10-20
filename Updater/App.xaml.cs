using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace YTY.amt
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {

    
    private void Application_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
    {

    }
  }

  public static class GlobalVars
  {
    private static App currentApp;
    private static Config config;
    private static UpdateServerViewModel updateServerViewModel;

    static GlobalVars()
    {
      currentApp = Application.Current as App;
      updateServerViewModel = currentApp.FindResource("UpdateServerViewModel") as UpdateServerViewModel;
      config = currentApp.FindResource("Config") as Config;
    }

    public static Config Config => config;
    public static UpdateServerViewModel UpdateServerViewModel { get { return updateServerViewModel; } }
    public static Dispatcher Dispatcher => currentApp.Dispatcher;
  }
}
