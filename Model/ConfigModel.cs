using System.ComponentModel;
using System.Windows;
using System.Collections.Generic;

namespace YTY.amt.Model
{
  public class ConfigModel : INotifyPropertyChanged
  {
#region CONSTANTS 
    internal const int CHUNKSIZE = 65536;
#endregion

    internal string hawkempirePath;
    internal ModResourceModel currentMod;
    internal bool populationLimit;
    internal bool multipleQueue;
    internal LanguageResourceModel currentLanguage;
    internal bool splash;
    internal Size resolution;
    internal bool backgroundMusic;
    internal bool isEnglishCampaignNarration;
    internal bool allShown_AocA;
    internal bool allShown_AocC;
    internal bool allShown_Aoc15;
    internal bool allShown_AoFE;
    internal int workshopTimestamp;

    public string HawkempirePath
    {
      get { return hawkempirePath; }
      set
      {
        hawkempirePath = value;
        DatabaseClient.SaveConfigEntry(nameof(HawkempirePath), value);
        OnPropertyChanged(nameof(HawkempirePath));
      }
    }

    public ModResourceModel CurrentMod
    {
      get { return currentMod; }
      set
      {
        currentMod = value;
        // TODO: copy game exe
        DatabaseClient.SaveConfigEntry(nameof(CurrentMod), value.Id);
        OnPropertyChanged(nameof(CurrentMod));
      }
    }

    public bool PopulationLimit
    {
      get { return populationLimit; }
      set
      {
        populationLimit = value;
        // TODO
        DatabaseClient.SaveConfigEntry(nameof(PopulationLimit), value);
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
        DatabaseClient.SaveConfigEntry(nameof(MultipleQueue), value);
        OnPropertyChanged(nameof(MultipleQueue));
      }
    }

    public LanguageResourceModel CurrentLanguage
    {
      get { return currentLanguage; }
      set
      {
        currentLanguage = value;
        //TODO
        DatabaseClient.SaveConfigEntry(nameof(CurrentLanguage), value.Id);
        OnPropertyChanged(nameof(CurrentLanguage));
      }
    }

    public bool Splash
    {
      get { return splash; }
      set
      {
        splash = value;
        DatabaseClient.SaveConfigEntry(nameof(Splash), value);
        OnPropertyChanged(nameof(Splash));
      }
    }

    public Size Resolution
    {
      get { return resolution; }
      set
      {
        resolution = value;
        DatabaseClient.SaveConfigEntry(nameof(Resolution), value);
        OnPropertyChanged(nameof(Resolution));
      }
    }

    public bool BackgroundMusic
    {
      get { return backgroundMusic; }
      set
      {
        backgroundMusic = value;
        DatabaseClient.SaveConfigEntry(nameof(BackgroundMusic), value);
        OnPropertyChanged(nameof(BackgroundMusic));
      }
    }

    public bool IsEnglishCampaignNarration
    {
      get { return isEnglishCampaignNarration; }
      set
      {
        isEnglishCampaignNarration = value;
        DatabaseClient.SaveConfigEntry(nameof(IsEnglishCampaignNarration), value);
        OnPropertyChanged(nameof(IsEnglishCampaignNarration));
      }
    }

    public bool AllShown_AocA
    {
      get { return allShown_AocA; }
      set
      {
        allShown_AocA = value;
        DatabaseClient.SaveConfigEntry(nameof(AllShown_AocA), value);
        OnPropertyChanged(nameof(AllShown_AocA));
      }
    }

    public bool AllShown_AocC
    {
      get { return allShown_AocC; }
      set
      {
        allShown_AocC = value;
        DatabaseClient.SaveConfigEntry(nameof(AllShown_AocC), value);
        OnPropertyChanged(nameof(AllShown_AocC));
      }
    }

    public bool AllShown_Aoc15
    {
      get { return allShown_Aoc15; }
      set
      {
        allShown_Aoc15 = value;
        DatabaseClient.SaveConfigEntry(nameof(AllShown_Aoc15), value);
        OnPropertyChanged(nameof(AllShown_Aoc15));
      }
    }

    public bool AllShown_AoFE
    {
      get { return allShown_AoFE; }
      set
      {
        allShown_AoFE = value;
        DatabaseClient.SaveConfigEntry(nameof(AllShown_AoFE), value);
        OnPropertyChanged(nameof(AllShown_AoFE));
      }
    }

    public int WorkshopTimestamp
    {
      get { return workshopTimestamp; }
      set
      {
        workshopTimestamp = value;
        DatabaseClient.SaveConfigEntry(nameof(WorkshopTimestamp), value);
        OnPropertyChanged(nameof(WorkshopTimestamp));
      }
    }

    internal ConfigModel() { }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

  }
}
