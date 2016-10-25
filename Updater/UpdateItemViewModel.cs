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

namespace YTY.amt
{
  public class UpdateItemViewModel : ViewModelBase
  {

    private long size;
    private Version version;
    private string md5;
    private UpdateItemStatus status;
    private ObservableCollection<ChunkViewModel> chunks;

    public int Id { get; private set; }

    public string SourceUri { get; private set; }

    public string FileName { get; private set; }

    public long Size
    {
      get { return size; }
    }

    public async Task SetSizeAsync(long value)
    {
      size = value;
      OnPropertyChanged(nameof(Size));
      await GlobalVars.Dal.SetUpdateItemSize(this).ConfigureAwait(false);
    }

    public Version Version
    {
      get { return version; }
    }

    public async Task SetVersionAsync(Version value)
    {
      version = value;
      OnPropertyChanged(nameof(Version));
      await GlobalVars.Dal.SetUpdateItemVersion(this).ConfigureAwait(false);
    }

    public string MD5
    {
      get { return md5; }
    }

    public async Task SetMD5Async(string value)
    {
      md5 = value;
      OnPropertyChanged(nameof(MD5));
      await GlobalVars.Dal.SetUpdateItemMD5(this).ConfigureAwait(false);
    }

    public UpdateItemStatus Status
    {
      get { return status; }
    }

    public async Task SetStatusAsync(UpdateItemStatus value)
    {
      status = value;
      if (status == UpdateItemStatus.Ready)
        Chunks = null;
      OnPropertyChanged(nameof(Status));
      await GlobalVars.Dal.SetUpdateItemStatus(this).ConfigureAwait(false);
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
          await SetStatusAsync(UpdateItemStatus.Downloading).ConfigureAwait(false);
          Chunks = new ObservableCollection<ChunkViewModel>(Enumerable.Range(0, wd.GetNumChunks()).Select(n => new ChunkViewModel(Id, n, DownloadChunkStatus.New)));
          await GlobalVars.Dal.SaveChunks(chunks).ConfigureAwait(false);
        }
        if( status == UpdateItemStatus.Error)
          await SetStatusAsync(UpdateItemStatus.Downloading).ConfigureAwait(false);

        var dir = Path.GetDirectoryName(Util.MakeQualifiedPath(FileName));
        if (!Directory.Exists(dir))
          Directory.CreateDirectory(dir);

        using (var bw = new BinaryWriter(new FileStream(Util.MakeQualifiedPath(FileName), FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read)))
        {
          var tasks = chunks.Where(ch => ch.Status == DownloadChunkStatus.New).Select(ch => wd.DownloadChunkAsync(ch.Index)).ToList();
          var numTasks = tasks.Count;
          for (var i = 0; i < numTasks; i++)
          {
            var recentlyCompletedTask = await TaskEx.WhenAny(tasks).ConfigureAwait(false);
            tasks.Remove(recentlyCompletedTask);
            var indexAndData = await recentlyCompletedTask;
            bw.Seek(wd.ChunkSize * indexAndData.Item1, SeekOrigin.Begin);
            bw.Write(indexAndData.Item2);
            bw.Flush();
            await chunks.First(ch => ch.Index == indexAndData.Item1).SetStatusAsync(DownloadChunkStatus.Done).ConfigureAwait(false);
          }
        }
        await GlobalVars.Dal.DeleteChunks(this).ConfigureAwait(false);
        await SetStatusAsync(UpdateItemStatus.Finished).ConfigureAwait(false);
      }
      catch (WebException)
      {
        await SetStatusAsync(UpdateItemStatus.Error).ConfigureAwait(false);
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
    }

    public async Task SetStatusAsync(DownloadChunkStatus value)
    {
      status = value;
      OnPropertyChanged(nameof(Status));
      await GlobalVars.Dal.SetChunkStatus(this, status).ConfigureAwait(false);
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

