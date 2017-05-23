using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Windows.Media.Imaging;

namespace YTY.amt.Model
{
  public class WorkshopResourceModel : INotifyPropertyChanged
  {
    #region PRIVATE FIELDS
    private CancellationTokenSource cts;
    private int authorId;
    private string authorName;
    private WorkshopResourceType type;
    private string name;
    private int rating;
    private long totalSize;
    private int createDate;
    private int lastChangeDate;
    private int lastFileChangeDate;
    private string _description;
    private GameVersion gameVersion;
    private int downloadCount;
    private string sourceUrl;
    private WorkshopResourceStatus status;
    private long finishedSize;
    private bool begunGettingImages;
    #endregion

    #region PROTECTED PROPERTIES
    protected bool IsBuiltIn => Id < 0;
    #endregion

    #region PUBLIC PROPERTIES 
    public int Id { get; internal set; }

    public WorkshopResourceType Type
    {
      get { return type; }
      internal set { type = value; }
    }

    public int CreateDate
    {
      get { return createDate; }
      set
      {
        createDate = value;
        OnPropertyChanged(nameof(CreateDate));
      }
    }

    public string Name
    {
      get { return name; }
      set
      {
        name = value;
        OnPropertyChanged(nameof(Name));
      }
    }

    public int Rating
    {
      get { return rating; }
      set
      {
        rating = value;
        OnPropertyChanged(nameof(Rating));
      }
    }

    public long TotalSize
    {
      get { return totalSize; }
      set
      {
        totalSize = value;
        OnPropertyChanged(nameof(TotalSize));
      }
    }

    public long FinishedSize
    {
      get { return finishedSize; }
      set
      {
        finishedSize = value;
        OnPropertyChanged(nameof(FinishedSize));
      }
    }


    public int LastChangeDate
    {
      get { return lastChangeDate; }
      set
      {
        lastChangeDate = value;
        OnPropertyChanged(nameof(LastChangeDate));
      }
    }

    public int LastFileChangeDate
    {
      get { return lastFileChangeDate; }
      set
      {
        lastFileChangeDate = value;
        OnPropertyChanged(nameof(LastFileChangeDate));
      }
    }

    public int AuthorId
    {
      get { return authorId; }
      set
      {
        authorId = value;
        OnPropertyChanged(nameof(AuthorId));
      }
    }

    public string AuthorName
    {
      get { return authorName; }
      set
      {
        authorName = value;
        OnPropertyChanged(nameof(AuthorName));
      }
    }

    public string Description
    {
      get { return _description; }
      set
      {
        _description = value;
        OnPropertyChanged(nameof(Description));
      }
    }

    public GameVersion GameVersion
    {
      get { return gameVersion; }
      set
      {
        gameVersion = value;
        OnPropertyChanged(nameof(GameVersion));
      }
    }

    public int DownloadCount
    {
      get { return downloadCount; }
      set
      {
        downloadCount = value;
        OnPropertyChanged(nameof(DownloadCount));
      }
    }

    public string SourceUrl
    {
      get { return sourceUrl; }
      set
      {
        sourceUrl = value;
        OnPropertyChanged(nameof(SourceUrl));
      }
    }

    public ObservableCollection<ResourceFileModel> Files { get; } = new ObservableCollection<ResourceFileModel>();

    public ObservableCollection<BitmapImage> Images { get; } = new ObservableCollection<BitmapImage>();

    public WorkshopResourceStatus Status
    {
      get { return status; }
      set
      {
        status = value;
        OnPropertyChanged(nameof(Status));
      }
    }
    #endregion

    #region PUBLIC METHODS

    public WorkshopResourceModel()
    {
      Files.CollectionChanged += Files_CollectionChanged;
    }

    public void UpdateStatus(WorkshopResourceStatus value)
    {
      Status = value;
      DatabaseClient.UpdateResourceStatus(Id, value);
    }

    public async Task GetImages()
    {
      if (begunGettingImages)
        return;
      begunGettingImages = true;
      await WebServiceClient.GetResourceImageBytes(Id,
        new Progress<byte[]>(imageBytes =>
        {
          var bitmapImage = new BitmapImage();
          bitmapImage.BeginInit();
          bitmapImage.StreamSource = new MemoryStream(imageBytes);
          bitmapImage.EndInit();
          Images.Add(bitmapImage);
        }));
    }

    internal void LocalLoadFiles()
    {
      FinishedSize = 0;
      foreach (var f in DatabaseClient.GetResourceFiles(Id))
      {
        if (f.Status == ResourceFileStatus.Finished)
          FinishedSize += f.Size;
        if (f.Status == ResourceFileStatus.Downloading)
          f.UpdateStatus(ResourceFileStatus.Paused);
        if (f.Status == ResourceFileStatus.Paused)
        {
          f.LocalLoadChunks();
          FinishedSize += f.FinishedSize;
        }
        Files.Add(f);
      }
    }


    public async Task InstallAsync()
    {
      ThrowIfInvalidStatus(WorkshopResourceStatus.NotInstalled);
      UpdateStatus(WorkshopResourceStatus.Installing);
      var (_, dtos) = await WebServiceClient.GetResourceUpdatedFilesAsync(Id);
      foreach (var dto in dtos)
      {
        var file = ResourceFileModel.FromDto(dto);
        file.ResourceId = Id;
        file.Status = ResourceFileStatus.BeforeDownload;
        Files.Add(file);
      }
      DatabaseClient.SaveResourceFiles(Files);
      cts = new CancellationTokenSource();
      await DownloadAsync(cts.Token);
    }

    public void Pause()
    {
      ThrowIfInvalidStatus(WorkshopResourceStatus.Installing);
      cts?.Cancel();
    }

    public async Task ResumeAsync()
    {
      ThrowIfInvalidStatus(WorkshopResourceStatus.Paused);
      UpdateStatus(WorkshopResourceStatus.Installing);
      cts = new CancellationTokenSource();
      await DownloadAsync(cts.Token);
    }

    public async Task UpdateAsync()
    {
      ThrowIfInvalidStatus(WorkshopResourceStatus.NeedUpdate);
      var (lastFileChange, dtos) = await WebServiceClient.GetResourceUpdatedFilesAsync(Id, LastFileChangeDate);
      var toSave = new List<ResourceFileModel>(dtos.Count);
      foreach (var dto in dtos)
      {
        var file = Files.FirstOrDefault(l => l.Id == dto.Id);
        if (file == null)
        // new resource file
        {
          file = ResourceFileModel.FromDto(dto);
          file.ResourceId = Id;
          file.Status = ResourceFileStatus.BeforeDownload;
          Files.Add(file);
        }
        else
        // resource file exists locally
        {
          file.Sha1 = dto.Sha1;
          file.Size = dto.Size;
          file.UpdateDate = dto.UpdateDate;
          if ((FileServerStatus)dto.Status == FileServerStatus.Alive)
          {
            file.Status = ResourceFileStatus.BeforeDownload;
          }
          else // Deleted
          {
            file.Status = ResourceFileStatus.Deleted;
          }
        }
        toSave.Add(file);
      }
      DatabaseClient.UpdateResourceLastFileChange(Id, lastFileChange);
      DatabaseClient.SaveResourceFiles(toSave);
      cts = new CancellationTokenSource();
      await DownloadAsync(cts.Token);
    }

    public virtual void Delete()
    {
      var directories = Files.Select(f => Path.GetDirectoryName(f.FullPathName)).Distinct().OrderBy(d => d.Count(s => s.Equals('\\')));
      foreach (var file in Files)
      {
        File.Delete(file.FullPathName);
      }
      foreach (var directory in directories)
      {
        if (!Directory.EnumerateFiles(directory).Any())
          Directory.Delete(directory);
      }
      DatabaseClient.DeleteResourceFiles(Id);
      UpdateStatus(WorkshopResourceStatus.NotInstalled);
    }
    #endregion

    #region PRIVATE EVENT HANDLERS
    private void Files_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.NewItems != null)
        foreach (ResourceFileModel f in e.NewItems)
          f.PropertyChanged += File_PropertyChanged;
      if (e.OldItems != null)
        foreach (ResourceFileModel f in e.OldItems)
          f.PropertyChanged -= File_PropertyChanged;
    }

    private void File_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(FinishedSize))
      {
        OnPropertyChanged(nameof(FinishedSize));
      }
    }
    #endregion


    private void ThrowIfInvalidStatus(WorkshopResourceStatus expectedStatuses)
    {
      if (!expectedStatuses.HasFlag(Status))
        throw new InvalidOperationException($"Invalid status '{Status}', expected '{expectedStatuses}'");
    }

    protected async Task DownloadAsync(CancellationToken cancellationToken)
    {
      try
      {
        foreach (var f in Files)
        {
          await f.DownloadAsync(cancellationToken, new Progress<int>(e => FinishedSize += e));
        }
        UpdateStatus(WorkshopResourceStatus.Installed);
        AfterDownload();
      }
      catch (OperationCanceledException)
      {
        UpdateStatus(WorkshopResourceStatus.Paused);
      }
      catch (InvalidOperationException)
      {
        UpdateStatus(WorkshopResourceStatus.Failed);
        throw;
      }
      finally
      {
        cts = null;
      }
    }

    protected virtual void AfterDownload()
    {

    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }

  public enum WorkshopResourceType
  {
    Drs,
    Campaign,
    Scenario,
    RandomMap,
    Replay,
    Mod,
    Ai,
    Taunt,
    Undefined,
    Language,
  }

  [Flags]
  public enum GameVersion
  {
    None,
    Aok = 0x2,
    AocA = 0x4,
    AocC = 0x8,
    Aoc15 = 0x10,
    Aofe = 0x20,
  }

  public enum WorkshopResourceStatus
  {
    NotInstalled,
    Installing,
    Paused,
    Installed,
    NeedUpdate,
    DeletePending,
    Deleted,
    Failed,
  }

  internal enum ResourceServerStatus
  {
    Editing = 1,
    Published,
    Deleted,
  }
}
