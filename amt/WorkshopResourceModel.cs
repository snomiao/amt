using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;

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
    private DateTime createDate;
    private DateTime lastChangeDate;
    private DateTime lastFileChangeDate;
    private string discription;
    private GameVersion gameVersion;
    private int downloadCount;
    private string sourceUrl;
    private WorkshopResourceStatus status;
    private List<ResourceFileModel> files;

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

    public DateTime CreateDate
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

    public DateTime LastChangeDate
    {
      get { return lastChangeDate; }
      set
      {
        lastChangeDate = value;
        OnPropertyChanged(nameof(LastChangeDate));
      }
    }

    public DateTime LastFileChangeDate
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

    public List<ResourceFileModel> Files
    {
      get { return files; }
      set
      {
        files = value;
        OnPropertyChanged(nameof(Files));
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
    }

    public WorkshopResourceModel(int id)
    {
      Id = id;
    }

    public async Task DownloadAsync()
    {
      switch (Status)
      {
        case WorkshopResourceStatus.NotInstalled:
          Status = WorkshopResourceStatus.Installing;
          Files = await DAL.GetResourceFilesAsync(Id);
          this.SaveFiles();
          break;
        case WorkshopResourceStatus.Installing:

          break;
      }

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
    NotInstalled,
    Installing,
    Installed,
    Activated
  }
}
