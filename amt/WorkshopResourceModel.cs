using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace YTY.amt
{
  public class WorkshopResourceModel : INotifyPropertyChanged
  {
    private int authorId;
    private string authorName;
    private WorkshopResourceType type;
    private string name;
    private int rating;
    private long totalSize;
    private int createDate;
    private int lastChangeDate;
    private int lastFileChangeDate;
    private string discription;
    private GameVersion gameVersion;
    private int downloadCount;
    private string sourceUrl;
    private WorkshopResourceStatus status;
    private ObservableCollection<ResourceFileModel> files;

    public int Id { get; }

    public WorkshopResourceType Type
    {
      get { return type; }
      set
      {
        type = value;
        OnPropertyChanged(nameof(Type));
      }
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

    public string Discription
    {
      get { return discription; }
      set
      {
        discription = value;
        OnPropertyChanged(nameof(Discription));
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

    public ObservableCollection<ResourceFileModel> Files
    {
      get
      {
        if (files == null)
        {
          files = new ObservableCollection<ResourceFileModel>(DAL.GetLocalResourceFiles(Id));
          OnPropertyChanged(nameof(Files));
        }
        return files;
      }
    }

    public WorkshopResourceStatus Status
    {
      get { return status; }
      set
      {
        status = value;
        OnPropertyChanged(nameof(Status));
      }
    }

    public void UpdateStatus(WorkshopResourceStatus value)
    {
      Status = value;
      DAL.UpdateResourceStatus(Id, value);
    }

    public WorkshopResourceModel(int id)
    {
      Id = id;
    }

    public async Task InstallAsync()
    {
      IEnumerable<ResourceFileModel> updatedFiles = null;
      Tuple<int, List<ResourceFileModel>> serviceResult = null;
      switch (Status)
      {
        case WorkshopResourceStatus.NotInstalled:
          serviceResult = await DAL.GetResourceUpdatedFilesAsync(Id);
          updatedFiles = serviceResult.Item2
            .Where(f => f.Status == ResourceFileStatus.NotDownloaded);
          updatedFiles.ToList().ForEach(f => Files.Add(f));
          UpdateStatus(WorkshopResourceStatus.Installing);
          break;
        case WorkshopResourceStatus.Installing:

          break;
        case WorkshopResourceStatus.NeedUpdate:
          serviceResult = await DAL.GetResourceUpdatedFilesAsync(Id, LastFileChangeDate);
          updatedFiles = serviceResult.Item2;
          foreach (var updatedFile in updatedFiles)
          {
            var localFile = Files.FirstOrDefault(l => l.Id == updatedFile.Id);
            if (localFile == null)
            // new resource file
            {
              Files.Add(updatedFile);
            }
            else
            // resource file exists locally
            {
              localFile.Sha1 = updatedFile.Sha1;
              localFile.Size = updatedFile.Size;
              localFile.UpdateDate = updatedFile.UpdateDate;
              localFile.Status = updatedFile.Status;
              if (updatedFile.Status == ResourceFileStatus.Deleted)
              {

              }
            }
          }
          DAL.UpdateResourceLastFileChange(Id, serviceResult.Item1);
          break;
      }
      DAL.SaveResourceFileModels(updatedFiles);

      foreach (var f in Files)
      {
        await f.DownloadAsync();
      }
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
    Undefined
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
    Editing = 1,
    Published,
    Deleted,
    NotInstalled = 101,
    Installing,
    Installed,
    NeedUpdate,
    Activated
  }
}
