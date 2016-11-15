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

    public ConfigModel() { }

    public ConfigModel(string hawkempirePath,int currentGameVersion,bool populationLimit,bool multipleQueue)
    {
      this.hawkempirePath = hawkempirePath;
      this.currentGameVersion = currentGameVersion;
      this.populationLimit = populationLimit;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

  }
}
