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
    private int currentGameLanguage;
    private bool splash;
    private int resolutionX;
    private int resolutionY;
    private bool backgroundMusic;
    private bool isEnglishCampaignNarration;
    private bool allShown_AocA;
    private bool allShown_AocC;
    private bool allShown_Aoc15;
    private bool allShown_AoFE;

    public string HawkempirePath
    {
      get { return hawkempirePath; }
      set
      {
        hawkempirePath = value;
        DAL.SaveConfigString(nameof(HawkempirePath), value);
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
        DAL.SaveConfigInt(nameof(CurrentGameVersion), value);
        OnPropertyChanged(nameof(CurrentGameVersion));
      }
    }

    public bool PopulationLimit
    {
      get { return populationLimit; }
      set
      {
        populationLimit = value;
        // TODO
        DAL.SaveConfigBool(nameof(PopulationLimit), value);
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
        DAL.SaveConfigBool(nameof(MultipleQueue), value);
        OnPropertyChanged(nameof(MultipleQueue));
      }
    }

    public int CurrentGameLanguage
    {
      get { return currentGameLanguage; }
      set
      {
        currentGameLanguage = value;
        //TODO
        DAL.SaveConfigInt(nameof(CurrentGameLanguage), value);
        OnPropertyChanged(nameof(CurrentGameLanguage));
      }
    }

    public bool Splash
    {
      get { return splash; }
      set
      {
        splash = value;
        DAL.SaveConfigBool(nameof(Splash), value);
        OnPropertyChanged(nameof(Splash));
      }
    }

    public int ResolutionX
    {
      get { return resolutionX; }
      set
      {
        resolutionX = value;
        DAL.SaveConfigInt(nameof(ResolutionX), value);
        OnPropertyChanged(nameof(ResolutionX));
      }
    }

    public int ResolutionY
    {
      get { return resolutionY; }
      set
      {
        resolutionY = value;
        DAL.SaveConfigInt(nameof(ResolutionY), value);
        OnPropertyChanged(nameof(ResolutionY));
      }
    }

    public bool BackgroundMusic
    {
      get { return backgroundMusic; }
      set
      {
        backgroundMusic = value;
        DAL.SaveConfigBool(nameof(BackgroundMusic), value);
        OnPropertyChanged(nameof(BackgroundMusic));
      }
    }

    public bool IsEnglishCampaignNarration
    {
      get { return isEnglishCampaignNarration; }
      set
      {
        isEnglishCampaignNarration = value;
        DAL.SaveConfigBool(nameof(IsEnglishCampaignNarration), value);
        OnPropertyChanged(nameof(IsEnglishCampaignNarration));
      }
    }

    public bool AllShown_AocA
    {
      get { return allShown_AocA; }
      set
      {
        allShown_AocA = value;
        DAL.SaveConfigBool(nameof(AllShown_AocA), value);
        OnPropertyChanged(nameof(AllShown_AocA));
      }
    }

    public bool AllShown_AocC
    {
      get { return allShown_AocC; }
      set
      {
        allShown_AocC = value;
        DAL.SaveConfigBool(nameof(AllShown_AocC), value);
        OnPropertyChanged(nameof(AllShown_AocC));
      }
    }

    public bool AllShown_Aoc15
    {
      get { return allShown_Aoc15; }
      set
      {
        allShown_Aoc15 = value;
        DAL.SaveConfigBool(nameof(AllShown_Aoc15), value);
        OnPropertyChanged(nameof(AllShown_Aoc15));
      }
    }

    public bool AllShown_AoFE
    {
      get { return allShown_AoFE; }
      set
      {
        allShown_AoFE = value;
        DAL.SaveConfigBool(nameof(AllShown_AoFE), value);
        OnPropertyChanged(nameof(AllShown_AoFE));
      }
    }

    public ConfigModel() { }

    public static ConfigModel GetConfig()
    {
      var ret = new ConfigModel();
      DAL.EnsureTablesExist();
      ret.hawkempirePath = DAL.GetConfigString(nameof(HawkempirePath), string.Empty);
      ret.currentGameVersion = DAL.GetConfigInt(nameof(CurrentGameVersion), -1);
      ret.populationLimit = DAL.GetConfigBool(nameof(PopulationLimit), true);
      ret.multipleQueue = DAL.GetConfigBool(nameof(MultipleQueue), false);
      ret.currentGameLanguage = DAL.GetConfigInt(nameof(CurrentGameLanguage), -1);
      ret.splash = DAL.GetConfigBool(nameof(Splash), false);
      ret.resolutionX = DAL.GetConfigInt(nameof(ResolutionX), 1366);
      ret.resolutionY = DAL.GetConfigInt(nameof(ResolutionY), 768);
      ret.backgroundMusic = DAL.GetConfigBool(nameof(BackgroundMusic), true);
      ret.isEnglishCampaignNarration = DAL.GetConfigBool(nameof(IsEnglishCampaignNarration), false);
      ret.allShown_AocA = DAL.GetConfigBool(nameof(AllShown_AocA), false);
      ret.allShown_AocC = DAL.GetConfigBool(nameof(AllShown_AocC), false);
      ret.allShown_Aoc15 = DAL.GetConfigBool(nameof(AllShown_Aoc15), false);
      ret.allShown_AoFE = DAL.GetConfigBool(nameof(AllShown_AoFE), false);
      return ret;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

  }
}
