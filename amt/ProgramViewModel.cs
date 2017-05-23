using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows;
using YTY.amt.Model;

namespace YTY.amt
{
  public static class ProgramViewModel
  {
    public static App App { get; } = Application.Current as App;

    public static WorkshopWindowViewModel WorkshopViewModel { get; } =
      App.FindResource(nameof(WorkshopWindowViewModel)) as WorkshopWindowViewModel;

    public static MainWindowViewModel MainWindowViewModel { get; } =
      (MainWindowViewModel) App.FindResource(nameof(MainWindowViewModel));
  }
}
