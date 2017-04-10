using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Threading;
using System.IO;
using System.Collections.ObjectModel;

namespace YTY.amt.Model
{
  public class ResourceFileModel : INotifyPropertyChanged
  {
    private ResourceFileStatus status;
    private int finishedSize;


    public int ResourceId { get; set; }

    public int Id { get; set; }

    public int Size { get; set; }

    public string Path { get; set; }

    public string FullPathName => ProgramModel.MakeHawkempirePath(Path);

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
      DatabaseClient.UpdateResourceFileStatus(Id, value);
    }

    public ObservableCollection<FileChunkModel> Chunks { get; } = new ObservableCollection<FileChunkModel>();

    public int FinishedSize
    {
      get { return finishedSize; }
      set
      {
        finishedSize = value;
        OnPropertyChanged(nameof(FinishedSize));
      }
    }

    public void LocalLoadChunks()
    {
      foreach (var chunk in DatabaseClient.GetChunks(Id))
        Chunks.Add(chunk);
      FinishedSize = Chunks.Count(c => c.Finished) * ConfigModel.CHUNKSIZE;
    }

    private void EnsureDirectoryExists()
    {
      var dir = System.IO.Path.GetDirectoryName(FullPathName);
      if (!Directory.Exists(dir))
        Directory.CreateDirectory(dir);
    }

    public async Task DownloadAsync(CancellationToken cancellationToken, IProgress<int> progress)
    {
      EnsureDirectoryExists();
      using (var fs = new FileStream(FullPathName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read, 4096, true))
      {
        switch (status)
        {
          case ResourceFileStatus.BeforeDownload:
          case ResourceFileStatus.NeedUpdate:
          case ResourceFileStatus.ChecksumFailed:
            var numChunks = (Size + ConfigModel.CHUNKSIZE - 1) / ConfigModel.CHUNKSIZE;
            for (var i = 0; i < numChunks; i++)
              Chunks.Add(new FileChunkModel { FileId = Id, Id = i });
            DatabaseClient.SaveChunks(Chunks);
            UpdateStatus(ResourceFileStatus.Downloading);
            break;
          case ResourceFileStatus.Paused:
            Status = ResourceFileStatus.Downloading;
            break;
          case ResourceFileStatus.Deleted:
          case ResourceFileStatus.Downloading:
          case ResourceFileStatus.Finished:
            return;
        }

        var tasks = Chunks.Where(c => !c.Finished).Select(c => c.DownloadAsync(cancellationToken));
        var working = tasks.Take(5).ToList();
        foreach (var skipped in tasks.Skip(5))
        {
          var finished = await Task.WhenAny(working);
          working.Remove(finished);
          working.Add(skipped);
          ContinueWith(finished);
        }
        while (working.Count > 0)
        {
          var finished = await Task.WhenAny(working);
          working.Remove(finished);
          await ContinueWith(finished);
        }

        async Task ContinueWith(Task<(int Id, byte[] Data)> finished)
        {
          if (cancellationToken.IsCancellationRequested)
          {
            Status = ResourceFileStatus.Paused;
            cancellationToken.ThrowIfCancellationRequested();
          }
          var finishedChunk = await finished;
          fs.Seek(finishedChunk.Id * ConfigModel.CHUNKSIZE, SeekOrigin.Begin);
          await fs.WriteAsync(finishedChunk.Data, 0, finishedChunk.Data.Length);
          FinishedSize += finishedChunk.Data.Length;
          progress.Report(finishedChunk.Data.Length);
          DatabaseClient.UpdateFileChunkFinished(Id, finishedChunk.Id, true);
        }
      }

      DatabaseClient.DeleteFileChunks(Id);
      if (Util.GetFileSha1(FullPathName).Equals(Sha1, StringComparison.InvariantCultureIgnoreCase))
      {
        UpdateStatus(ResourceFileStatus.Finished);
      }
      else
      {
        UpdateStatus(ResourceFileStatus.ChecksumFailed);
        throw new InvalidOperationException(nameof(ResourceFileStatus.ChecksumFailed));
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    internal static ResourceFileModel FromDto(ResourceFileDto dto)
    {
      return new ResourceFileModel
      {
        Id = dto.Id,
        Path = dto.Path,
        Sha1 = dto.Sha1,
        Size = dto.Size,
        UpdateDate = dto.UpdateDate,
      };
    }
  }

  public enum ResourceFileStatus
  {
    Deleted = -1,
    BeforeDownload,
    Downloading,
    Paused,
    Finished,
    ChecksumFailed,
    NeedUpdate,
  }
}
