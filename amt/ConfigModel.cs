using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;

namespace YTY.amt
{
  public class ConfigModel : INotifyPropertyChanged
  {
    private string hawkempirePath;
    private int currentGameVersion;
    private bool populationLimit;
    private bool multipleQueue;
    private string currentGameLanguage;
    private bool splash;
    private int resolutionX;
    private int resolutionY;
    private bool backgroundMusic;
    private bool isEnglishCampaignNarration;

    public string HawkempirePath
    {
      get { return hawkempirePath; }
      set
      {
        hawkempirePath = value;
        DAL.SaveHawkempirePath(this);
        OnPropertyChanged(nameof(HawkempirePath));
      }
    }

    public int CurrentGameVersion
    {
      get { return currentGameVersion; }
      set
      {
        currentGameVersion = value;
        // TODO: copy game exe
        DAL.SaveCurrentGameVersion(this);
      }
    }

    public bool PopulationLimit
    {
      get { return populationLimit; }
      set
      {
        populationLimit = value;
        // TODO
        DAL.SavePopulationLimit(this);
        OnPropertyChanged(nameof(PopulationLimit));
      }
    }

    public bool MultipleQueue
    {
      get { return multipleQueue; }
      set
      {
        multipleQueue = value;
        //TODO
        DAL.SaveMultipleQueue(this);
        OnPropertyChanged(nameof(MultipleQueue));
      }
    }

    public string CurrentGameLanguage
    {
      get { return currentGameLanguage; }
      set
      {
        currentGameLanguage = value;
        //TODO
        DAL.SaveGameLanguage(this);
        OnPropertyChanged(nameof(CurrentGameLanguage));
      }
    }

    public bool Splash
    {
      get { return splash; }
      set
      {
        splash = value;
        DAL.SaveSplash(this);
        OnPropertyChanged(nameof(Splash));
      }
    }

    public int ResolutionX
    {
      get { return resolutionX; }
      set
      {
        resolutionX = value;
        DAL.SaveResolutionX(this);
        OnPropertyChanged(nameof(ResolutionX));
      }
    }

    public int ResolutionY
    {
      get { return resolutionY; }
      set
      {
        resolutionY = value;
        DAL.SaveResolutionY(this);
        OnPropertyChanged(nameof(ResolutionY));
      }
    }

    public bool BackgroundMusic
    {
      get { return backgroundMusic; }
      set
      {
        backgroundMusic = value;
        DAL.SaveBackgroundMusic(this);
        OnPropertyChanged(nameof(BackgroundMusic));
      }
    }

    public bool IsEnglishCampaignNarration
    {
      get { return isEnglishCampaignNarration; }
      set
      {
        isEnglishCampaignNarration = value;
        DAL.SaveIsEnglishCampaignNarration(this);
        OnPropertyChanged(nameof(IsEnglishCampaignNarration));
      }
    }

    public ConfigModel() { }

    public ConfigModel(string hawkempirePath,int currentGameVersion,bool populationLimit,bool multipleQueue,string gameLanguage,bool splash,int resolutionX,int resolutionY,bool backgroundMusic,bool isEnglishCampaignNarration)
    {
      this.hawkempirePath = hawkempirePath;
      this.currentGameVersion = currentGameVersion;
      this.populationLimit = populationLimit;
      currentGameLanguage = gameLanguage;
      this.splash = splash;
      this.resolutionX = resolutionX;
      this.resolutionY = resolutionY;
      this.backgroundMusic = backgroundMusic;
      this.isEnglishCampaignNarration = isEnglishCampaignNarration;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

  }
}
