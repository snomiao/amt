using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows;
using System.Diagnostics;
using YTY.amt.Model;

namespace YTY.amt
{
  public static class ProgramViewModel
  {
    public static App App { get; } = Application.Current as App;

    public static WorkshopWindowViewModel WorkshopViewModel { get; } = new WorkshopWindowViewModel();

    public static MainWindowViewModel MainWindowViewModel { get; } = new MainWindowViewModel();

    public static ConfigViewModel ConfigViewModel { get; } = ConfigViewModel.FromModel(ProgramModel.Config);
  }
}
