using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Collections.ObjectModel;
using System.Linq;
using YTY.amt.Model;

namespace YTY.amt
{
  public class MainWindowViewModel : INotifyPropertyChanged
  {
    private static readonly Size fullScreen = new Size(SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);

    private static readonly Size workingArea =
      new Size(SystemParameters.WorkArea.Width - 2 * SystemParameters.FixedFrameVerticalBorderWidth,
        SystemParameters.WorkArea.Height - 2 * SystemParameters.FixedFrameHorizontalBorderHeight);

    private static  ObservableCollection<ModViewModel> mods;

    public bool WorkshopShown
    {
      set
      {
        if (value)
          new WorkshopWindow().Show();
      }
    }

    public bool IsValidHawkempirePath => File.Exists(ProgramModel.MakeHawkempirePath("empires2.exe"));

    public List<Size> ScreenResolutions { get; } = new List<Size>(Util.GetScreenResolutions());

    public Size CurrentResolution
    {
      get { return ProgramModel.Config.Resolution; }
      set
      {
        ProgramModel.Config.Resolution = value;
        OnPropertyChanged(nameof(CurrentResolution));
      }
    }

    public bool FullScreen
    {
      get { return CurrentResolution == fullScreen; }
      set
      {
        CurrentResolution = fullScreen;
      }
    }

    public bool WorkingArea
    {
      get { return CurrentResolution == workingArea; }
      set
      {
        CurrentResolution = workingArea;
      }
    }


    public event PropertyChangedEventHandler PropertyChanged;

    public MainWindowViewModel()
    {
      mods = new ObservableCollection<ModViewModel>(ProgramModel.Mods.Select(ModViewModel.FromModel));
      ProgramModel.Config.PropertyChanged += Config_PropertyChanged;
    }

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void Config_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      switch (e.PropertyName)
      {
        case nameof(ConfigModel.HawkempirePath):
          OnPropertyChanged(nameof(IsValidHawkempirePath));
          break;
        case nameof(ConfigModel.Resolution):
          OnPropertyChanged(nameof(CurrentResolution));
          OnPropertyChanged(nameof(FullScreen));
          OnPropertyChanged(nameof(WorkingArea));
          break;
      }
    }
  }
}