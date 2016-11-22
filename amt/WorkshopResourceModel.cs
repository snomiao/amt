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
    private string authorName;
    private ulong totalSize;
    private DateTime updateDate;
    private string discription;
    private GameVersion gameVersion;
    private uint downloadCount;
    private string sourceUrl;
    private List<ResourceFileModel> files;

    public uint Id { get; }

    public WorkshopResourceType Type { get; }

    public string Name { get; }

    public uint Rating { get; }

    public ulong TotalSize
    {
      get { return totalSize; }
      set
      {
        totalSize = value;
        OnPropertyChanged(nameof(TotalSize));
      }
    }

    public DateTime UpdateDate
    {
      get { return updateDate; }
      set
      {
        updateDate = value;
        OnPropertyChanged(nameof(UpdateDate));
      }
    }

    public string AuthorName
    {
      get { return authorName; }
      set
      {
        authorName = value; OnPropertyChanged(nameof(AuthorName));
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

    public uint DownloadCount
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

    public WorkshopResourceStatus Status { get; set; }

    public void UpdateStatus(WorkshopResourceStatus value)
    {
      Status = value;
      OnPropertyChanged(nameof(Status));
    }

    public WorkshopResourceModel(uint id, string name, uint rating, WorkshopResourceType type)
    {
      Id = id;
      Name = name;
      Rating = rating;
      Type = type;
      Status = WorkshopResourceStatus.NotInstalled;
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
