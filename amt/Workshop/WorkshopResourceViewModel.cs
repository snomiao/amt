using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using System.Windows.Input;
using System.Diagnostics;
using YTY.amt.Model;

namespace YTY.amt
{
  public class WorkshopResourceViewModel : INotifyPropertyChanged
  {
    public WorkshopResourceModel Model { get; protected set; }

    public virtual string ButtonText
    {
      get
      {
        switch (Model.Status)
        {
          case WorkshopResourceStatus.NotInstalled:
            return "安装资源";
          case WorkshopResourceStatus.Installing:
            return "暂停安装";
          case WorkshopResourceStatus.Paused:
            return "继续安装";
          case WorkshopResourceStatus.Installed:
            return "删除资源";
          case WorkshopResourceStatus.NeedUpdate:
            return "更新资源";
        }
        return "未知状态";
      }
    }

    public virtual SolidColorBrush ButtonBackground
    {
      get
      {
        switch (Model.Status)
        {
          case WorkshopResourceStatus.NotInstalled:
            return Brushes.Green;
          case WorkshopResourceStatus.Installing:
            return Brushes.Yellow;
          case WorkshopResourceStatus.Paused:
            return Brushes.Green;
          case WorkshopResourceStatus.Installed:
            return Brushes.Gray;
          case WorkshopResourceStatus.NeedUpdate:
            return Brushes.HotPink;
        }
        return Brushes.Transparent;
      }
    }

    public virtual ICommand Command
    {
      get
      {
        switch (Model.Status)
        {
          case WorkshopResourceStatus.NotInstalled:
            return Commands.InstallResource;
          case WorkshopResourceStatus.Installing:
            return Commands.PauseResource;
          case WorkshopResourceStatus.Paused:
            return Commands.ResumeResource;
          case WorkshopResourceStatus.Installed:
            return Commands.DeleteResource;
        }
        return null;
      }
    }

    public string GameVersion
    {
      get
      {
        var ret = new List<string>(5);
        if (Model.GameVersion.HasFlag(amt.Model.GameVersion.Aok)) ret.Add("AoK");
        if (Model.GameVersion.HasFlag(amt.Model.GameVersion.AocA)) ret.Add("1.0A");
        if (Model.GameVersion.HasFlag(amt.Model.GameVersion.AocC)) ret.Add("1.0C");
        if (Model.GameVersion.HasFlag(amt.Model.GameVersion.Aoc15)) ret.Add("1.5");
        if (Model.GameVersion.HasFlag(amt.Model.GameVersion.Aofe)) ret.Add("AoFE");
        return string.Join("/", ret);
      }
    }

    private static BitmapImage getImageFromFile(string file)
    {
      return new BitmapImage(new Uri($"/resources;component/Resources/{file}.png", UriKind.Relative));
    }

    /// <summary>
    /// Convert <see cref="WorkshopResourceType"/> to resource image file name.
    /// </summary>
    private static BitmapImage[] imageArray = new[] {
      "resdrs", // Drs
      "rescpx", // Campaign
      "resscx", // Scenario
      "resrms", // RandomMap
      "resmgx", // Replay
      "resmod", // Mod
      "resai", // Ai
      "resdrs", // Taunt
      "resdrs", // Undefined
    }.Select(f => getImageFromFile(f)).ToArray();

    public ImageSource Image
    {
      get
      {
        return imageArray[(int)Model.Type];
      }
    }

    public string ProgressText
    {
      get
      {
        return $"{ My.ByteCountToTextConverter.Convert(Model.FinishedSize, null, null, null)} / {My.ByteCountToTextConverter.Convert(Model.TotalSize, null, null, null)} ({(double)Model.FinishedSize / Model.TotalSize:P1})";
      }
    }

    public ICollectionView DownloadingFilesView { get; }

    protected WorkshopResourceViewModel(WorkshopResourceModel model)
    {
      Model = model;
      model.PropertyChanged += Model_PropertyChanged;
      DownloadingFilesView = CollectionViewSource.GetDefaultView(model.Files);
      DownloadingFilesView.Filter = o =>
        {
          var m = o as ResourceFileModel;
          return m.Status == ResourceFileStatus.BeforeDownload ||
          m.Status == ResourceFileStatus.Downloading ||
          m.Status == ResourceFileStatus.Paused;
        };
    }

    public static WorkshopResourceViewModel FromModel(WorkshopResourceModel model)
    {
      switch (model)
      {
        case ModResourceModel m:
          return ModViewModel.FromModel(m);
        case DrsResourceModel d:
          return DrsViewModel.FromModel(d);
        default:
          return new WorkshopResourceViewModel(model);
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      switch (e.PropertyName)
      {
        case nameof(WorkshopResourceModel.Status):
          OnPropertyChanged(nameof(ButtonText));
          OnPropertyChanged(nameof(ButtonBackground));
          OnPropertyChanged(nameof(Command));
          //DownloadingFilesView.Refresh();
          break;
        case nameof(DrsResourceModel.IsActivated):
          OnPropertyChanged(nameof(ButtonText));
          OnPropertyChanged(nameof(ButtonBackground));
          OnPropertyChanged(nameof(Command));
          break;
        case nameof(GameVersion):
          OnPropertyChanged(nameof(GameVersion));
          break;
        case nameof(WorkshopResourceModel.FinishedSize):
          OnPropertyChanged(nameof(ProgressText));
          break;
      }
    }
  }
}
