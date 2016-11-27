using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace YTY.amt
{
  public class ResourceFileModel : INotifyPropertyChanged
  {
    private ResourceFileStatus status;
    private List<FileChunkModel> chunks;

    public int ResId { get; set; }

    public int Id { get; set; }

    public int Size { get; set; }

    public string Path { get; set; }

    public int UpdateDate { get; set; }

    public string Sha1 { get; set; }

    public ResourceFileStatus Status
    {
      get { return status; }
      set
      {
        status = value;
        OnPropertyChanged(nameof(Status));
      }
    }

    public void UpdateStatus(ResourceFileStatus value)
    {
      Status = value;
      DAL.UpdateResourceFileStatus(Id,value);
    }

    public List<FileChunkModel> Chunks
    {
      get { return chunks; }
      set
      {
        chunks = value;
        OnPropertyChanged(nameof(Chunks));
      }
    }

    public async Task DownloadAsync()
    {
      switch (status)
      {
        case ResourceFileStatus.NotDownloaded:
          Chunks = Enumerable.Range(0, (Size + DAL.CHUNKSIZE - 1) / DAL.CHUNKSIZE).Select(i => new FileChunkModel() { FileId = Id, Id = i }).ToList();
          this.SaveChunks();
          break;
        case ResourceFileStatus.Downloading:
          Chunks = DAL.LoadChunks(Id);
          break;
        default:
          throw new InvalidOperationException("file already finished");
      }
      var tasks = chunks.Where(c => !c.Finished).Select(c => c.DownloadAsync()).ToList();
      UpdateStatus(ResourceFileStatus.Downloading);
      var numTasks = tasks.Count();
      for (var i = 0; i < numTasks; i++)
      {
        var finished = await Task.WhenAny(tasks);
        tasks.Remove(finished);
        await finished;
      }
      UpdateStatus(ResourceFileStatus.Finished);
      DAL.DeleteFileChunks(Id);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

  }

  public enum ResourceFileStatus
  {
    NotDownloaded,
    Deleted,
    Downloading = 101,
    Finished
  }
}
