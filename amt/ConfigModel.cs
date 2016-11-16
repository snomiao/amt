using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using Size = System.Windows.Size;

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
    private Size resolution;

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

    public Size Resolution
    {
      get { return resolution; }
      set
      {
        resolution = value;
        DAL.SaveResolution(this);
        OnPropertyChanged(nameof(Resolution));
      }
    }

    public ConfigModel() { }

    public ConfigModel(string hawkempirePath,int currentGameVersion,bool populationLimit,bool multipleQueue,string gameLanguage,bool splash,Size resolution)
    {
      this.hawkempirePath = hawkempirePath;
      this.currentGameVersion = currentGameVersion;
      this.populationLimit = populationLimit;
      currentGameLanguage = gameLanguage;
      this.splash = splash;
      this.resolution = resolution;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

  }
}
