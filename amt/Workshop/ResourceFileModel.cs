using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Threading;
using System.IO;

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

    public string FullPathName
    {
      get
      {
        return My.MakeHawkempirePath(Path);
      }
    }

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
      DAL.UpdateResourceFileStatus(Id, value);
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

    public int FinishedSize
    {
      get
      {
        return Chunks?.Count(c => c.Finished) * DAL.CHUNKSIZE ?? 0;
      }
    }

    public void LocalLoadChunks()
    {
      Chunks = DAL.LoadChunks(Id);
    }

    private void EnsureDirectoryExists()
    {
      var dir = System.IO.Path.GetDirectoryName(FullPathName);
      if (!Directory.Exists(dir))
        Directory.CreateDirectory(dir);
    }

    public async Task DownloadAsync(CancellationToken cancellationToken,IProgress <int> progress)
    {
      EnsureDirectoryExists();
      using (var fs = new FileStream(FullPathName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read, 4096, true))
      {
        switch (status)
        {
          case ResourceFileStatus.NotDownloaded:
            Chunks = Enumerable.Range(0, (Size + DAL.CHUNKSIZE - 1) / DAL.CHUNKSIZE).Select(i => new FileChunkModel() { FileId = Id, Id = i }).ToList();
            this.SaveChunks();
            UpdateStatus(ResourceFileStatus.Downloading);
            break;
          case ResourceFileStatus.Paused:
            break;
          default:
            throw new InvalidOperationException("file already finished");
        }
        var tasks = chunks.Where(c => !c.Finished).Select(c => c.DownloadAsync()).ToList();
        var numTasks = tasks.Count();
        for (var i = 0; i < numTasks; i++)
        {
          var finished = await Task.WhenAny(tasks);
          if (cancellationToken.IsCancellationRequested)
          {
            UpdateStatus(ResourceFileStatus.Paused);
          }
          cancellationToken.ThrowIfCancellationRequested();
          tasks.Remove(finished);
          var finishedChunk = await finished;
          fs.Seek(finishedChunk.Id * DAL.CHUNKSIZE, SeekOrigin.Begin);
          await fs.WriteAsync(finishedChunk.Data, 0, finishedChunk.Data.Length);
          progress.Report(finishedChunk.Data.Length);
          DAL.UpdateFileChunkFinished(Id, finishedChunk.Id, true);
        }
      }
      if (Util.GetFileSha1(FullPathName) == Sha1)
      {
        UpdateStatus(ResourceFileStatus.Finished);
        DAL.DeleteFileChunks(Id);
      }
      else
      {
        UpdateStatus(ResourceFileStatus.ChecksumFailed);
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
    NotDownloaded,
    Deleted,
    Downloading = 101,
    Paused,
    Finished,
    ChecksumFailed
  }
}
