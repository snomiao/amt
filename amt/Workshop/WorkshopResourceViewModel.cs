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
using System.Collections.Specialized;
using System.Collections.ObjectModel;
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

    public virtual Color ButtonBackground
    {
      get
      {
        switch (Model.Status)
        {
          case WorkshopResourceStatus.NotInstalled:
            return Colors.Green;
          case WorkshopResourceStatus.Installing:
            return Colors.DarkKhaki;
          case WorkshopResourceStatus.Paused:
            return Colors.YellowGreen;
          case WorkshopResourceStatus.Installed:
            return Colors.Gray;
          case WorkshopResourceStatus.NeedUpdate:
            return Colors.HotPink;
        }
        return Colors.DarkRed;
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
          case WorkshopResourceStatus.NeedUpdate:
            return Commands.UpdateResource;
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

    public DateTime UpdateDate => Util.FromUnixTimestamp(Model.LastChangeDate);

    private static BitmapImage GetImageFromFile(string file)
    {
      return new BitmapImage(new Uri($"/resources;component/Resources/{file}.png", UriKind.Relative));
    }

    /// <summary>
    /// Convert <see cref="WorkshopResourceType"/> to resource image file name.
    /// </summary>
    private static readonly BitmapImage[] imageArray = new[] {
      "resdrs", // Drs
      "rescpx", // Campaign
      "resscx", // Scenario
      "resrms", // RandomMap
      "resmgx", // Replay
      "resmod", // Mod
      "resai", // Ai
      "1", // Taunt
      "resdrs", // Undefined
      "2", // Language
    }.Select(GetImageFromFile).ToArray();

    public ImageSource Image => imageArray[(int)Model.Type];

    public string ProgressText => $"{ Converters.ByteCountToText.Convert(Model.FinishedSize, null, null, null)} / {Converters.ByteCountToText.Convert(Model.TotalSize, null, null, null)} ({(double)Model.FinishedSize / Model.TotalSize:P1})";

    public ICollectionView DownloadingFilesView { get; }

    public ObservableCollection<FileViewModel> Files { get; } 

    protected WorkshopResourceViewModel(WorkshopResourceModel model)
    {
      Model = model;
      Files = new ObservableCollection<FileViewModel>(model.Files.Select(FileViewModel.FromModel));
      model.Files.CollectionChanged += Files_CollectionChanged;
      model.PropertyChanged += Model_PropertyChanged;
      DownloadingFilesView = CollectionViewSource.GetDefaultView(Files);
      DownloadingFilesView.Filter = o =>
        {
          var m =((FileViewModel) o).Model;
          return m.Status == ResourceFileStatus.BeforeDownload ||
          m.Status == ResourceFileStatus.Downloading ||
          m.Status == ResourceFileStatus.Paused ||
          m.Status == ResourceFileStatus.ChecksumFailed;
        };
    }

    private void Files_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Add:
          foreach (ResourceFileModel model in e.NewItems)
          {
            Files.Add(FileViewModel.FromModel(model));
          }
          break;
      }
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
