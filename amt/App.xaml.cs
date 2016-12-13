using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace YTY.amt
{
  public partial class App : Application
  {


    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      if (e.ChangedButton == MouseButton.Left)
        (sender as Window).DragMove();
    }
  }

  public static class My
  {
    public static App App => Application.Current as App;

    public static Workshop WorkshopWindow
    {
      get
      {
        var workshop = App.FindResource(nameof(WorkshopWindow)) as Workshop;
        return workshop;
      }
    }

    public static WorkshopWindowViewModel WorkshopWindowViewModel
    {
      get
      {
        return App.FindResource(nameof(WorkshopWindowViewModel)) as WorkshopWindowViewModel;
      }
    }

    public static MainWindowViewModel MainWindowViewModel
    {
      get
      {
        return App.FindResource(nameof(MainWindowViewModel)) as MainWindowViewModel;
      }
    }

    public static CreateProcessCommand CreateProcessCommand => App.FindResource(nameof(CreateProcessCommand)) as CreateProcessCommand;

    public static ByteCountToTextConverter ByteCountToTextConverter => App.FindResource(nameof(ByteCountToTextConverter)) as ByteCountToTextConverter;

    public static ScenarioTranslatorViewModel ScenarioTranslatorViewModel => App.FindResource(nameof(ScenarioTranslatorViewModel)) as ScenarioTranslatorViewModel;

    public static string MakeHawkempirePath(string relativePath)
    {
      return System.IO.Path.Combine(ConfigModel.CurrentConfig.HawkempirePath, relativePath);
    }
  }
}
