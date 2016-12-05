using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Data;

namespace YTY.amt
{
  public class WorkshopResourceViewModel : INotifyPropertyChanged
  {
    private ICollectionView downloadingFilesView;

    public WorkshopResourceModel Model { get; }

    public string ButtonText
    {
      get
      {
        switch (Model.Status)
        {
          case WorkshopResourceStatus.NotInstalled:
            return "安装资源";
        }
        return string.Empty;
      }
    }

    public SolidColorBrush ButtonBackground
    {
      get
      {
        switch (Model.Status)
        {
          case WorkshopResourceStatus.NotInstalled:
            return Brushes.Green;
        }
        return Brushes.Transparent;
      }
    }

    public string GameVersion
    {
      get
      {
        var ret = new List<string>(5);
        if (Model.GameVersion.HasFlag(amt.GameVersion.Aok)) ret.Add("AoK");
        if (Model.GameVersion.HasFlag(amt.GameVersion.AocA)) ret.Add("1.0A");
        if (Model.GameVersion.HasFlag(amt.GameVersion.AocC)) ret.Add("1.0C");
        if (Model.GameVersion.HasFlag(amt.GameVersion.Aoc15)) ret.Add("1.5");
        if (Model.GameVersion.HasFlag(amt.GameVersion.Aofe)) ret.Add("AoFE");
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

    public ICollectionView DownloadingFilesView
    {
      get
      {
        if (downloadingFilesView == null)
        {
          downloadingFilesView = CollectionViewSource.GetDefaultView(Model.Files);
          downloadingFilesView.Filter = o =>
            {
              var model = o as ResourceFileModel;
              return model.Status == ResourceFileStatus.NotDownloaded || model.Status == ResourceFileStatus.Downloading;
            };
        }
        return downloadingFilesView;
      }
    }

    public WorkshopResourceViewModel(WorkshopResourceModel model)
    {
      Model = model;
      model.PropertyChanged += Model_PropertyChanged;
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
        case "Status":
          OnPropertyChanged(nameof(ButtonText));
          OnPropertyChanged(nameof(ButtonBackground));
          break;
        case nameof(GameVersion):
          OnPropertyChanged(nameof(GameVersion));
          break;
        case "Type":
          OnPropertyChanged(nameof(Image));
          break;
        case "FinishedSize":
          OnPropertyChanged(nameof(ProgressText));
          break;
      }
    }
  }
}
