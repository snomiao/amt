using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Threading;
using System.IO;
using System.Collections.ObjectModel;
using YTY.amt;

namespace YTY.amt.Model
{
  public class ResourceFileModel : INotifyPropertyChanged
  {
    private const string PATH_MANAGER = @"manager\";

    private ResourceFileStatus status;

    public int ResourceId { get; set; }

    public int Id { get; set; }

    public int Size { get; set; }

    public string Path { get; set; }

    public string FullPathName
    {
      get
      {
        if (Path.StartsWith(PATH_MANAGER, StringComparison.InvariantCultureIgnoreCase))
        {
          return ProgramModel.MakeExeRelativePath(Path.Remove(0, PATH_MANAGER.Length));
        }
        else
        {
          return ProgramModel.MakeHawkempirePath(Path);
        }
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
      DatabaseClient.UpdateResourceFileStatus(Id, value);
    }

    public ObservableCollection<FileChunkModel> Chunks { get; } = new ObservableCollection<FileChunkModel>();

    public int FinishedSize
    {
      get
      {
        switch (Status)
        {
          case ResourceFileStatus.Downloading:
          case ResourceFileStatus.Paused:
            return Chunks.Count(c => c.Finished) * ConfigModel.CHUNKSIZE;
          case ResourceFileStatus.Finished:
            return Size;
          default:
            return 0;
        }
      }
    }

    public void DeleteFromDisk()
    {
      try
      {
        File.Delete(FullPathName);
      }
      catch
      {
        // ignored
      }
    }

    internal void LocalLoadChunks()
    {
      foreach (var chunk in DatabaseClient.GetChunks(Id))
        Chunks.Add(chunk);
    }

    private void EnsureDirectoryExists()
    {
      var dir = System.IO.Path.GetDirectoryName(FullPathName);
      if (!Directory.Exists(dir))
        Directory.CreateDirectory(dir);
    }

    public async Task DownloadAsync(CancellationToken cancellationToken)
    {
      EnsureDirectoryExists();
      switch (status)
      {
        case ResourceFileStatus.BeforeDownload:
        case ResourceFileStatus.ChecksumFailed:
          File.WriteAllText(FullPathName, string.Empty);
          var numChunks = (Size + ConfigModel.CHUNKSIZE - 1) / ConfigModel.CHUNKSIZE;
          for (var i = 0; i < numChunks; i++)
            Chunks.Add(new FileChunkModel { FileId = Id, Id = i });
          DatabaseClient.SaveChunks(Chunks);
          UpdateStatus(ResourceFileStatus.Downloading);
          break;
        case ResourceFileStatus.Paused:
          Status = ResourceFileStatus.Downloading;
          break;
        case ResourceFileStatus.Downloading:
          break;
        case ResourceFileStatus.Deleted:
        case ResourceFileStatus.Finished:
          return;
      }

      var tasks = Chunks.Where(c => !c.Finished).Select(c => c.DownloadAsync(cancellationToken));
      var working = new List<Task<(int, byte[])>>();

      using (var fs = new FileStream(FullPathName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read, 4096, true))
      {
        foreach (var task in tasks)
        {
          if (working.Count < 5)
          {
            working.Add(task);
          }
          else
          {
            var finished = await Task.WhenAny(working);
            working.Remove(finished);
            working.Add(task);
            await ContinueWith(await finished);
          }
        }
        while (working.Count > 0)
        {
          var finished = await Task.WhenAny(working);
          working.Remove(finished);
          await ContinueWith(await finished);
        }

        async Task ContinueWith((int Id, byte[] Data) finished)
        {
          if (cancellationToken.IsCancellationRequested)
          {
            UpdateStatus(ResourceFileStatus.Paused);
            cancellationToken.ThrowIfCancellationRequested();
          }
          fs.Seek(finished.Id * ConfigModel.CHUNKSIZE, SeekOrigin.Begin);
          await fs.WriteAsync(finished.Data, 0, finished.Data.Length);
          OnPropertyChanged(nameof(FinishedSize));
          DatabaseClient.UpdateFileChunkFinished(Id, finished.Id, true);
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
  }
}
