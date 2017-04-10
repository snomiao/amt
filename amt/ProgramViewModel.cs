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

    public static WorkshopWindowViewModel WorkshopWindow { get; } =
      App.FindResource(nameof(WorkshopWindowViewModel)) as WorkshopWindowViewModel;
  }
}
