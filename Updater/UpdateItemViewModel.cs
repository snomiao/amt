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
      set
      {
        size = value;
        OnPropertyChanged(nameof(Size));
        GlobalVars.Dal.SetUpdateItemSize(this);
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
        if (chunks == null)
          GlobalVars.Dal.DeleteChunks(this);
        else
        {
          OnPropertyChanged(nameof(Chunks));
        }
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
      this.chunks = new ObservableCollection<ChunkViewModel>(chunks);
    }

    public static UpdateItemViewModel MakeNew(int id, string sourceUri, string fileName)
    {
      var newMe = new UpdateItemViewModel()
      {
        Id = id,
        SourceUri = sourceUri,
        FileName = fileName,
        status = UpdateItemStatus.Ready
      };
      GlobalVars.Dal.CreateUpdateItem(newMe);
      return newMe;
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
          Chunks = new ObservableCollection<ChunkViewModel>(Enumerable.Range(0, wd.GetNumChunks()).Select(n => new ChunkViewModel(Id, n, DownloadChunkStatus.New)));
          GlobalVars.Dal.SaveChunks(chunks);
        }
        else // status == Downloading or Error
          Chunks = new ObservableCollection<ChunkViewModel>(GlobalVars.Dal.GetChunks(Id));

        var dir = Path.GetDirectoryName(Util.MakeQualifiedPath(FileName));
        if (!Directory.Exists(dir))
          Directory.CreateDirectory(dir);

        using (var bw = new BinaryWriter(new FileStream(Util.MakeQualifiedPath(FileName), FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read)))
        {
          var tasks = chunks.Select(ch => wd.DownloadChunkAsync(ch.Index)).ToList();
          var numTasks = tasks.Count;
          for (var i = 0; i < numTasks; i++)
          {
            var recentlyCompletedTask = await TaskEx.WhenAny(tasks);
            tasks.Remove(recentlyCompletedTask);
            var indexAndData = await recentlyCompletedTask;
            bw.Seek(wd.ChunkSize * indexAndData.Item1, SeekOrigin.Begin);
            bw.Write(indexAndData.Item2);
            bw.Flush();
            chunks.First(ch => ch.Index == indexAndData.Item1).Status = DownloadChunkStatus.Done;
          }
        }
        Chunks = null;
        Status = UpdateItemStatus.Finished;
      }
      catch (WebException)
      {
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

    public static ChunkViewModel MakeNew(int updateItemId, int index)
    {
      var newMe = new ChunkViewModel(updateItemId, index, DownloadChunkStatus.New);
      GlobalVars.Dal.CreateChunk(newMe);
      return newMe;
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

