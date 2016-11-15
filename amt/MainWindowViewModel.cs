using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Collections.ObjectModel;

namespace YTY.amt
{
  public class MainWindowViewModel : INotifyPropertyChanged
  {
    private ConfigModel config;
    private ObservableCollection<GameVersionModel> gameVersionList;

    public bool WorkshopShown
    {
      set
      {
        if (value) My.WorkshopWindow.Show();
      }
    }

    public ConfigModel Config
    {
      get
      {
        if (config == null)
        {
          config = DAL.GetConfig();
          config.PropertyChanged += Config_PropertyChanged;
        }
        return config;
      }
    }

    public bool IsValidHawkempirePath
    {
      get
      {
        return File.Exists(Path.Combine(Config.HawkempirePath, "empires2.exe"));
      }
    }

    public ObservableCollection<GameVersionModel> GameVersionList
    {
      get
      {
        if (gameVersionList == null)
        {
          gameVersionList = new ObservableCollection<GameVersionModel>(new[]
          {
            new GameVersionModel() { ResourceId=-1, Name="帝国时代Ⅱ 1.5",ExePath=@"exe\age2_x1.5.exe" },
            new GameVersionModel() {ResourceId=-2,Name="帝国时代Ⅱ 1.0C",ExePath=@"exe\age2_x1.0c.exe" },
            new GameVersionModel() {ResourceId=-3,Name="被遗忘的帝国",ExePath=@"exe\age2_x2.exe" },
            new GameVersionModel() {ResourceId=-4,Name="WAIFor 触发扩展版",ExePath=@"exe\age2_wtep.exe" }
          }); // TODO: concat with installed mods
        }
        return gameVersionList;
      }
    }

    public GameVersionModel CurrentGameVersion
    {
      get { return gameVersionList.FirstOrDefault(g => g.ResourceId == config.CurrentGameVersion); }
      set
      {
        config.CurrentGameVersion = value.ResourceId;
        OnPropertyChanged(nameof(CurrentGameVersion));
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

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
      }
    }
  }
}