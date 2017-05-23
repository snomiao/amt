using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using YTY.amt.Model;

namespace YTY.amt
{
  public class ConfigViewModel : INotifyPropertyChanged
  {
    public ConfigModel Model{ get; }

    public ModResourceModel CurrentGame
    {
      get => Model.CurrentGame;
      set => Model.CurrentGame = value;
    }

    public LanguageResourceModel CurrentLanguage
    {
      get => Model.CurrentLanguage;
      set
      {
        try
        {
          Model.CurrentLanguage = value;
        }
        catch (IOException ex)
        {
          MessageBox.Show(ex.Message);
        }
      }
    }

    public bool BackgroundMusic
    {
      get => Model.BackgroundMusic;
      set
      {
        try
        {
          Model.BackgroundMusic = value;
        }
        catch (IOException ex)
        {
          MessageBox.Show(ex.Message);
        }
      }
    }

    public bool IsEnglishCampaignNarration
    {
      get => Model.IsEnglishCampaignNarration;
      set
      {
        try
        {
          Model.IsEnglishCampaignNarration = value;
        }
        catch (IOException ex)
        {
          MessageBox.Show(ex.Message);
        }
      }
    }

    public bool AllShown_AocA
    {
      get => Model.AllShown_AocA;
      set
      {
        try
        {
          Model.AllShown_AocA = value;
        }
        catch (IOException ex)
        {
          MessageBox.Show(ex.Message);
        }
      }
    }

    public bool AllShown_AocC
    {
      get => Model.AllShown_AocC;
      set
      {
        try
        {
          Model.AllShown_AocC = value;
        }
        catch (IOException ex)
        {
          MessageBox.Show(ex.Message);
        }
      }
    }

    public bool AllShown_AoFE
    {
      get => Model.AllShown_AoFE;
      set
      {
        try
        {
          Model.AllShown_AoFE = value;
        }
        catch (IOException ex)
        {
          MessageBox.Show(ex.Message);
        }
      }
    }

    public bool AllShown_Aoc15
    {
      get => Model.AllShown_Aoc15;
      set
      {
        try
        {
          Model.AllShown_Aoc15 = value;
        }
        catch (IOException ex)
        {
          MessageBox.Show(ex.Message);
        }
      }
    }

    public TauntResourceModel CurrentTaunt
    {
      get => Model.CurrentTaunt;
      set
      {
        try
        {
          Model.CurrentTaunt = value;
        }
        catch (IOException ex)
        {
          MessageBox.Show(ex.Message);
        }
      }
    }

    private ConfigViewModel(ConfigModel model)
    {
      Model = model;
      model.PropertyChanged += Model_PropertyChanged;
    }

    public static ConfigViewModel FromModel(ConfigModel model)
    {
      return new ConfigViewModel(model);
    }

    private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      OnPropertyChanged(e.PropertyName);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
