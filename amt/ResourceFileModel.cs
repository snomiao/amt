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

    public uint Id { get; set; }

    public uint Size { get; set; }

    public string Path { get; set; }

    public DateTime UpdateDate { get; set; }

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
        case ResourceFileStatus.Ready:
          Chunks = Enumerable.Range(0, (int)(Size + DAL.CHUNKSIZE - 1) / DAL.CHUNKSIZE).Select(i => new FileChunkModel() { FileId = Id, Id = i}).ToList();
          this.SaveChunks();
          break;
        case ResourceFileStatus.Downloading:
          Chunks = DAL.LoadChunks(Id);
          break;
        default:
          throw new InvalidOperationException("file already finished");
      }
      var tasks = chunks.Where(c => !c.Finished).Select(c => c.DownloadAsync()).ToList();
      var numTasks = tasks.Count();
      for (var i = 0; i < numTasks; i++)
      {
        var finished = await Task.WhenAny();
        tasks.Remove(finished);
        await finished;
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

  }

  public enum ResourceFileStatus
  {
    Ready,
    Downloading,
    Finished
  }
}
