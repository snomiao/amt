using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace YTY.amt
{
  public class MainWindowViewModel : INotifyPropertyChanged
  {
    private ConfigModel config;

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