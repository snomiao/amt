using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows;

namespace YTY.amt
{
  public class MainWindowViewModel : INotifyPropertyChanged
  {
    private ConfigModel config;
    private ObservableCollection<GameVersionModel> gameVersionList;
    private List<GameLanguageModel> gameLanguages;
    private Size fullScreen = new Size(SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
    private Size workingArea = new Size(SystemParameters.WorkArea.Width - 2 * SystemParameters.FixedFrameVerticalBorderWidth, SystemParameters.WorkArea.Height - 2 * SystemParameters.FixedFrameHorizontalBorderHeight);

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

    public List<GameLanguageModel> GameLanguages
    {
      get
      {
        if (gameLanguages == null)
        {
          gameLanguages = new List<GameLanguageModel>()
          {
            new GameLanguageModel(){ Code="zh",Name="中文" },
            new GameLanguageModel(){ Code="en",Name="英语" },
            new GameLanguageModel(){ Code="ja",Name="日语"},
            new GameLanguageModel(){ Code="de",Name="德语" },
            new GameLanguageModel(){ Code="fr",Name="法语" },
            new GameLanguageModel(){ Code="es",Name="西班牙语" },
            new GameLanguageModel(){ Code="it",Name="意大利语" },
            new GameLanguageModel(){ Code="pt",Name="葡萄牙语" },
            new GameLanguageModel(){ Code="ru",Name="俄语" },
            new GameLanguageModel(){ Code="ko",Name="朝鲜语" },
            new GameLanguageModel(){ Code="hu",Name="匈牙利语" },
            new GameLanguageModel(){ Code="el",Name="希腊语" },
            new GameLanguageModel(){ Code="tr",Name="土耳其语" },
            new GameLanguageModel(){ Code="pl",Name="波兰语" },
            new GameLanguageModel(){ Code="bg",Name="保加利亚语" },
            new GameLanguageModel(){ Code="cs",Name="捷克语" },
            new GameLanguageModel(){ Code="sk",Name="斯洛伐克语" }
          };
        }
        return gameLanguages;
      }
    }

    public GameLanguageModel CurrentGameLanguage
    {
      get { return GameLanguages.First(g => g.Code == config.CurrentGameLanguage); }
      set
      {
        config.CurrentGameLanguage = value.Code;
        OnPropertyChanged(nameof(CurrentGameLanguage));
      }
    }

    public List<Size> ScreenResolutions => Util.GetScreenResolutions();

    public bool FullScreen
    {
      get { return config.Resolution == fullScreen; }
      set
      {
        config.Resolution = fullScreen;
      }
    }

    public bool WorkingArea
    {
      get { return config.Resolution == workingArea; }
      set { config.Resolution = workingArea; }
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
        case nameof(ConfigModel.Resolution):
          OnPropertyChanged(nameof(FullScreen));
          OnPropertyChanged(nameof(WorkingArea));
          break;
      }
    }
  }
}