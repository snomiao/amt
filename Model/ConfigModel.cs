using System.ComponentModel;
using System.Windows;
using System.Collections.Generic;
using Microsoft.Win32;
using System.IO;

namespace YTY.amt.Model
{
  public class ConfigModel : INotifyPropertyChanged
  {
    #region CONSTANTS 
    internal const int CHUNKSIZE = 1<<18;
    private const string REGISTRYKEY = @"SOFTWARE\Microsoft\Microsoft Games\Age of Empires II: The Conquerors Expansion\1.0";
    #endregion

    internal string hawkempirePath;
    internal ModResourceModel currentGame;
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
    internal TauntResourceModel currentTaunt;

    public string HawkempirePath
    {
      get { return hawkempirePath; }
      set
      {
        DatabaseClient.SaveConfigEntry(nameof(HawkempirePath), value);
        hawkempirePath = value;
        OnPropertyChanged(nameof(HawkempirePath));
      }
    }

    public ModResourceModel CurrentGame
    {
      get { return currentGame; }
      set
      {
        value.CopyExe();
        DatabaseClient.SaveConfigEntry(nameof(CurrentGame), value.Id);
        currentGame = value;
        OnPropertyChanged(nameof(CurrentGame));
      }
    }

    public bool PopulationLimit
    {
      get { return populationLimit; }
      set
      {
        Registry.CurrentUser
          .CreateSubKey(REGISTRYKEY)
          .SetValue("Extend Population", value ? 1 : 0, RegistryValueKind.DWord);
        DatabaseClient.SaveConfigEntry(nameof(PopulationLimit), value);
        populationLimit = value;
        OnPropertyChanged(nameof(PopulationLimit));
      }
    }

    public bool MultipleQueue
    {
      get { return multipleQueue; }
      set
      {
        Registry.CurrentUser
          .CreateSubKey(REGISTRYKEY)
          .SetValue("Multiple Queue", value ? 1 : 0, RegistryValueKind.DWord);
        DatabaseClient.SaveConfigEntry(nameof(MultipleQueue), value);
        multipleQueue = value;
        OnPropertyChanged(nameof(MultipleQueue));
      }
    }

    public LanguageResourceModel CurrentLanguage
    {
      get { return currentLanguage; }
      set
      {
        value.Activate();
        DatabaseClient.SaveConfigEntry(nameof(CurrentLanguage), value.Id);
        currentLanguage = value;
        OnPropertyChanged(nameof(CurrentLanguage));
      }
    }

    public bool Splash
    {
      get { return splash; }
      set
      {
        DatabaseClient.SaveConfigEntry(nameof(Splash), value);
        splash = value;
        OnPropertyChanged(nameof(Splash));
      }
    }

    public Size Resolution
    {
      get { return resolution; }
      set
      {
        Registry.CurrentUser
          .CreateSubKey(REGISTRYKEY)
          .SetValue("Screen Width", value.Width, RegistryValueKind.DWord);
        Registry.CurrentUser
          .CreateSubKey(REGISTRYKEY)
          .SetValue("Screen Height", value.Height, RegistryValueKind.DWord);
        DatabaseClient.SaveConfigEntry(nameof(Resolution), value);
        resolution = value;
        OnPropertyChanged(nameof(Resolution));
      }
    }

    public bool BackgroundMusic
    {
      get { return backgroundMusic; }
      set
      {
        var soundM3UManager = ProgramModel.MakeExeRelativePath(@"sound\music.m3u");
        var soundM3uC = ProgramModel.MakeHawkempirePath(@"sound\music.m3u");
        var soundM3U4 = ProgramModel.MakeHawkempirePath(@"games\the conquerors 1.4\sound\music.m3u");
        var soundM3UFe = ProgramModel.MakeHawkempirePath(@"games\forgotten empires\sound\music.m3u");
        if (value)
        {
          File.Copy(soundM3UManager, soundM3uC, true);
          File.Copy(soundM3UManager, soundM3U4, true);
          File.Copy(soundM3UManager, soundM3UFe, true);
        }
        else
        {
          File.Delete(soundM3uC);
          File.Delete(soundM3U4);
          File.Delete(soundM3UFe);
        }
        DatabaseClient.SaveConfigEntry(nameof(BackgroundMusic), value);
        backgroundMusic = value;
        OnPropertyChanged(nameof(BackgroundMusic));
      }
    }

    public bool IsEnglishCampaignNarration
    {
      get { return isEnglishCampaignNarration; }
      set
      {
        foreach (var file in Directory.GetFiles(ProgramModel.MakeExeRelativePath(@"sound\campaign\" + (value ? "en" : "zh"))))
        {
          File.Copy(file, Path.Combine(ProgramModel.MakeHawkempirePath(@"sound\campaign"), Path.GetFileName(file)), true);
        }
        foreach (var file in Directory.GetFiles(ProgramModel.MakeExeRelativePath(@"sound\scenario\" + (value ? "en" : "zh"))))
        {
          File.Copy(file, Path.Combine(ProgramModel.MakeHawkempirePath(@"sound\scenario"), Path.GetFileName(file)), true);
        }
        DatabaseClient.SaveConfigEntry(nameof(IsEnglishCampaignNarration), value);
        isEnglishCampaignNarration = value;
        OnPropertyChanged(nameof(IsEnglishCampaignNarration));
      }
    }

    public bool AllShown_AocA
    {
      get { return allShown_AocA; }
      set
      {
        File.Copy(ProgramModel.MakeExeRelativePath(
          value ? @"dat\allshown\empires2_x1_age2x1a.dat" : @"dat\original\empires2_x1.dat"),
          ProgramModel.MakeHawkempirePath(@"data\empires2_x1.dat"), true);
        DatabaseClient.SaveConfigEntry(nameof(AllShown_AocA), value);
        allShown_AocA = value;
        OnPropertyChanged(nameof(AllShown_AocA));
      }
    }

    public bool AllShown_AocC
    {
      get { return allShown_AocC; }
      set
      {
        File.Copy(ProgramModel.MakeExeRelativePath(
          value ? @"dat\allshown\empires2_x1_p1_age2x1c.dat" : @"dat\original\empires2_x1_p1.dat"),
          ProgramModel.MakeHawkempirePath(@"data\empires2_x1_p1.dat"), true);
        DatabaseClient.SaveConfigEntry(nameof(AllShown_AocC), value);
        allShown_AocC = value;
        OnPropertyChanged(nameof(AllShown_AocC));
      }
    }

    public bool AllShown_Aoc15
    {
      get { return allShown_Aoc15; }
      set
      {
        File.Copy(ProgramModel.MakeExeRelativePath(
          value ? @"dat\allshown\empires2_x1_p1_age2x1c.dat" : @"dat\original\empires2_x1_p1.dat"),
          ProgramModel.MakeHawkempirePath(@"games\the conquerors 1.4\data\empires2_x1_p1.dat"), true);
        DatabaseClient.SaveConfigEntry(nameof(AllShown_Aoc15), value);
        allShown_Aoc15 = value;
        OnPropertyChanged(nameof(AllShown_Aoc15));
      }
    }

    public bool AllShown_AoFE
    {
      get { return allShown_AoFE; }
      set
      {
        File.Copy(ProgramModel.MakeExeRelativePath(
          value ? @"dat\allshown\empires2_x1_p1_age2x2.dat" : @"dat\original\empires2_x1_p1_fe.dat"),
          ProgramModel.MakeHawkempirePath(@"Games\forgotten empires\data\empires2_x1_p1.dat"), true);
        DatabaseClient.SaveConfigEntry(nameof(AllShown_AoFE), value);
        allShown_AoFE = value;
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

    public TauntResourceModel CurrentTaunt
    {
      get { return currentTaunt; }
      set
      {
        value.Activate();
        DatabaseClient.SaveConfigEntry(nameof(CurrentTaunt), value.Id);
        currentTaunt = value;
        OnPropertyChanged(nameof(CurrentTaunt));
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
