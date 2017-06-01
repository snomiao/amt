using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using YTY.amt.Model;

namespace YTY.amt.Model.Config
{
  public static class Configs
  {
    public static ConfigEntry<string> HawkempirePath = new HawkempirePath();
  }

  public abstract class ConfigEntry<T> : INotifyPropertyChanged
  {
    protected T value;
    protected string name;

    protected string Name => GetType().Name;

    public virtual T GetDefaultValue() => default(T);

    protected virtual T Parse(string s)
    {
      return (T)Convert.ChangeType(s, typeof(T));
    }

    public T Value
    {
      get
      {
        return value;
      }
      set
      {
        OnSettingValue(value);
        this.value = value;
        DatabaseClient.SaveConfigEntry(Name, value);
        OnPropertyChanged(nameof(Value));
      }
    }

    protected virtual string SerializeValue()
    {
      return value.ToString();
    }

    protected ConfigEntry()
    {
      name = GetType().Name;
      value = Parse(DatabaseClient.GetConfigs()[Name]);
    }

    protected abstract void OnSettingValue(T value);

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }

  public class HawkempirePath : ConfigEntry<string>
  {
    public override string GetDefaultValue()
    {
      return "";
    }

    protected override void OnSettingValue(string value)
    {
      
    }
  }

  public class CurrentGame : ConfigEntry<ModResourceModel>
  {
    public override ModResourceModel GetDefaultValue()
    {
      return ProgramModel.Mods.First(m => m.Id == -1);
    }

    protected override ModResourceModel Parse(string s)
    {
      return ProgramModel.Mods.First(m => m.Id == int.Parse(s));
    }

    protected override string SerializeValue()
    {
      return Value.Id.ToString();
    }

    protected override void OnSettingValue(ModResourceModel value)
    {
      value.CopyExe();
    }
  }
}
