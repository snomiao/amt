using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.IO;
using System.Net;
using System.Windows;
using System.Threading.Tasks;
using System.Data;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace YTY.amt
{
  public class UpdateItemViewModel : ViewModelBase
  {
    private long size;
    private Version version;
    private string md5;
    private UpdateItemStatus status;
    private ObservableCollection<ChunkViewModel> chunks;
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
        GlobalVars.Dal.SetUpdateItemSize(this);
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
        GlobalVars.Dal.SetUpdateItemVersion(this);
      }
    }

    public string MD5
    {
      get { return md5; }
      set
      {
        md5 = value;
        OnPropertyChanged(nameof(MD5));
        GlobalVars.Dal.SetUpdateItemMD5(this);
      }
    }

    public UpdateItemStatus Status
    {
      get { return status; }
      set
      {
        status = value;
        if (status == UpdateItemStatus.Ready)
          Chunks = null;
        OnPropertyChanged(nameof(Status));
        GlobalVars.Dal.SetUpdateItemStatus(this);
      }
    }

    public ObservableCollection<ChunkViewModel> Chunks
    {
      get { return chunks; }
      set
      {
        chunks = value;
        OnPropertyChanged(nameof(Chunks));
      }
    }

    private UpdateItemViewModel()
    {

    }

    public UpdateItemViewModel(int id, string sourceUri, string fileName, long size, Version version, string md5, UpdateItemStatus status, IEnumerable<ChunkViewModel> chunks)
    {
      Id = id;
      SourceUri = sourceUri;
      FileName = fileName;
      this.size = size;
      this.version = version;
      this.md5 = md5;
      this.status = status;
      this.chunks = new ObservableCollection<ChunkViewModel>(chunks);
      finishedSize = 0;
    }

    public async Task StartAsync()
    {
      if (status == UpdateItemStatus.Finished)
        throw new InvalidOperationException();

      try
      {
        var wd = new WebDownloader(SourceUri, size);
        if (status == UpdateItemStatus.Ready)
        {
          Status = UpdateItemStatus.Downloading;
          Chunks = new ObservableCollection<ChunkViewModel>(Enumerable.Range(0, wd.NumChunks).Select(n => new ChunkViewModel(Id, n, DownloadChunkStatus.New)));
          GlobalVars.Dal.SaveChunks(chunks);
        }
        if (status == UpdateItemStatus.Error)
          Status = UpdateItemStatus.Downloading;

        var dir = Path.GetDirectoryName(Util.MakeQualifiedPath(FileName));
        if (!Directory.Exists(dir))
          Directory.CreateDirectory(dir);

        using (var fs = new FileStream(Util.MakeQualifiedPath(FileName), FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.RandomAccess))
        {
          //var tasks = chunks.Where(ch => ch.Status == DownloadChunkStatus.New);
          FinishedSize = chunks.Count(ch => ch.Status == DownloadChunkStatus.Done) * 65536;
          var tasks = chunks.Where(ch => ch.Status == DownloadChunkStatus.New).Select(ch => wd.DownloadChunkAsync(ch.Index)).ToList();
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
            chunks.First(ch => ch.Index == indexAndData.Item1).Status = DownloadChunkStatus.Done;
            FinishedSize += indexAndData.Item2.Length;
          }
        }
        GlobalVars.Dal.DeleteChunks(this);
        Status = UpdateItemStatus.Finished;
      }
      catch (WebException ex)
      {
        Debug.WriteLine(ex.Status);
        Status = UpdateItemStatus.Error;
      }
    }
  }

  public class ChunkViewModel : ViewModelBase
  {
    private int updateItemId;
    private int index;
    private DownloadChunkStatus status;

    public int UpdateItemId => updateItemId;

    public int Index => index;

    public DownloadChunkStatus Status
    {
      get { return status; }
      set
      {
        status = value;
        OnPropertyChanged(nameof(Status));
        GlobalVars.Dal.SetChunkStatus(this, status);
      }
    }

    public ChunkViewModel(int updateItemId, int index, DownloadChunkStatus status)
    {
      this.updateItemId = updateItemId;
      this.index = index;
      this.status = status;
    }
  }

  public enum UpdateItemStatus
  {
    Ready,
    Downloading,
    Finished,
    Error
  }

  public enum DownloadChunkStatus
  {
    New,
    Done
  }

}

