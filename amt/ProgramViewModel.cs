using System.Windows;
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
