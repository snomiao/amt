using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace YTY.amt
{
  public class FileModel
  {
    private long size;
    private Version version;
    private string md5;
    private FileStatus status;
    private long finishedSize;

    public int Id { get; private set; }

    public string SourceUri { get; private set; }

    public string FileName { get; private set; }

    public long Size
    {
      get { return size; }
      set
      {
        size = value;
        OnPropertyChanged(nameof(Size));
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

    public Version Version
    {
      get { return version; }
      set
      {
        version = value;
        OnPropertyChanged(nameof(Version));
      }
    }

    public string Md5
    {
      get { return md5; }
      set
      {
        md5 = value;
        OnPropertyChanged(nameof(Md5));
      }
    }

    public FileStatus Status
    {
      get { return status; }
      set
      {
        status = value;
        OnPropertyChanged(nameof(Status));
      }
    }

    public ObservableCollection<ChunkModel> Chunks { get; } = new ObservableCollection<ChunkModel>();

    public async Task DownloadAsync()
    {
      if (status == FileStatus.Finished)
        throw new InvalidOperationException();

      try
      {
        var wd = new WebDownloader(SourceUri, size);
        if (status == FileStatus.Ready)
        {
          Status = FileStatus.Downloading;
          for (var i = 0; i < wd.NumChunks; i++)
          {
            Chunks.Add(new ChunkModel
            {
              FileId = Id,
              Id = i,
              Status = ChunkStatus.New,
            });
          }
          DatabaseClient.SaveChunks(Chunks);
        }
        if (status == FileStatus.Error)
          Status =FileStatus.Downloading;

        var dir = Path.GetDirectoryName(Util.MakeQualifiedPath(FileName));
          Directory.CreateDirectory(dir);

        using (var fs = new FileStream(Util.MakeQualifiedPath(FileName), FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.RandomAccess))
        {
          //var tasks = chunks.Where(ch => ch.Status == DownloadChunkStatus.New);
          FinishedSize = Chunks.Count(ch => ch.Status == ChunkStatus.Done) * 65536;
          var tasks = Chunks.Where(ch => ch.Status == ChunkStatus.New).Select(ch => wd.DownloadChunkAsync(ch.Id)).ToList();
          var numTasks = tasks.Count;
          //var processing = tasks.Take(10).Select(ch=>wd.DownloadChunkAsync(ch.Index)). ToList();
          //foreach (var remaining in tasks.Skip(10))
          //{
          //  var recentlyCompletedTask = await TaskEx.WhenAny(processing).ConfigureAwait(false);
          //  processing.Remove(recentlyCompletedTask);
          //  processing.Add(wd.DownloadChunkAsync(remaining.Index));
          //  var indexAndData = await recentlyCompletedTask.ConfigureAwait(false);
          //  Debug.WriteLine($"{indexAndData.Item1} returned");
          //  fs.Seek(wd.ChunkSize * indexAndData.Item1, SeekOrigin.Begin);
          //  await fs.WriteAsync(indexAndData.Item2, 0, indexAndData.Item2.Length).ConfigureAwait(false);
          //  await chunks.First(ch => ch.Index == indexAndData.Item1).SetStatusAsync(DownloadChunkStatus.Done).ConfigureAwait(false);
          //}
          //var processingCount = processing.Count;
          //for (var i = 0; i < processingCount; i++)
          //{
          //  var recentlyCompletedTask = await TaskEx.WhenAny(processing).ConfigureAwait(false);
          //  processing.Remove(recentlyCompletedTask);
          //  var indexAndData = await recentlyCompletedTask.ConfigureAwait(false);
          //  Debug.WriteLine($"{indexAndData.Item1} returned");
          //  fs.Seek(wd.ChunkSize * indexAndData.Item1, SeekOrigin.Begin);
          //  await fs.WriteAsync(indexAndData.Item2, 0, indexAndData.Item2.Length).ConfigureAwait(false);
          //  await chunks.First(ch => ch.Index == indexAndData.Item1).SetStatusAsync(DownloadChunkStatus.Done).ConfigureAwait(false);
          //}
          for (var i = 0; i < numTasks; i++)
          {
            var recentlyCompletedTask = await Task.WhenAny(tasks);
            tasks.Remove(recentlyCompletedTask);
            var indexAndData = await recentlyCompletedTask;
            fs.Seek(wd.ChunkSize * indexAndData.Item1, SeekOrigin.Begin);
            await fs.WriteAsync(indexAndData.Item2, 0, indexAndData.Item2.Length);
            Chunks.First(ch => ch.Id == indexAndData.Item1).Status = ChunkStatus.Done;
            FinishedSize += indexAndData.Item2.Length;
          }
        }
        DatabaseClient.DeleteChunks(Chunks);
        Status = FileStatus.Finished;
      }
      catch (WebException)
      {
        Status = FileStatus.Error;
      }
    }

    internal static FileModel FromDto(FileDto dto)
    {
      return new FileModel
      {
        Id = dto.Id,
        SourceUri = dto.SourceUri,
        FileName = dto.FileName,
        Size = dto.Size,
        Version = Version.Parse(dto.Version),
        Md5 = dto.Md5,
        Status = (FileStatus)dto.Status,
      };
    }

    internal FileDto ToDto()
    {
      return new FileDto
      {
        Id=Id,
        SourceUri=SourceUri,
        FileName=FileName,
        Size=size,
        Version=Version.ToString(),
        Md5=md5,
        Status=(int)Status,
      };
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }

  public enum FileStatus
  {
    Ready,
    Downloading,
    Finished,
    Error
  }
}
