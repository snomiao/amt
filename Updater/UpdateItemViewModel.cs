using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.IO;
using System.Net;
using System.Windows;
using System.Threading.Tasks;

namespace YTY.amt
{
  public class UpdateItemViewModel : ViewModelBase
  {
    private XElement xe;
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
        xe.Element(nameof(Size)).SetValue(size);
      }
    }

    public Version Version
    {
      get { return version; }
      set
      {
        version = value;
        OnPropertyChanged(nameof(Version));
        xe.Element(nameof(Version)).SetValue(version.ToString());
      }
    }

    public string MD5
    {
      get { return md5; }
      set
      {
        md5 = value;
        OnPropertyChanged(nameof(MD5));
        xe.Element(nameof(MD5)).SetValue(md5);
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
        xe.Element(nameof(Status)).SetValue(status);
      }
    }

    public ObservableCollection<ChunkViewModel> Chunks
    {
      get { return chunks; }
      set
      {
        chunks = value;
        if (chunks == null)
          xe.Elements("Chunk").Remove();
        else
        {
          OnPropertyChanged(nameof(Chunks));
          xe.Add(chunks.Select(chunk => chunk.GetXElement()));
        }
      }
    }

    public UpdateItemViewModel(int id, string sourceUri, string fileName)
    {
      Id = id;
      SourceUri = sourceUri;
      FileName = fileName;
      status = UpdateItemStatus.Ready;
      xe = new XElement("File",
        new XAttribute(nameof(Id), id),
        new XElement(nameof(SourceUri), sourceUri),
        new XElement(nameof(FileName), fileName),
        new XElement(nameof(Status), Status),
        new XElement(nameof(Size)),
        new XElement(nameof(Version)),
        new XElement(nameof(MD5)));
      GlobalVars.Config.Root.Add(xe);
    }

    public UpdateItemViewModel(XElement xe)
    {
      this.xe = xe;
      Id = (int)xe.Attribute(nameof(Id));
      SourceUri = xe.Element(nameof(SourceUri)).Value;
      FileName = xe.Element(nameof(FileName)).Value;
      Size = (long)xe.Element(nameof(Size));
      Version = new Version(xe.Element(nameof(Version)).Value);
      MD5 = xe.Element(nameof(MD5)).Value;
      Enum.TryParse(xe.Element(nameof(Status)).Value, out status);
    }

    public async Task StartAsync()
    {
      if (status == UpdateItemStatus.Finished)
        throw new InvalidOperationException();

      try
      {
        var wd = new WebDownloader(SourceUri, size);
        if (status == UpdateItemStatus.Ready)
          Chunks = new ObservableCollection<ChunkViewModel>(Enumerable.Range(0, wd.GetNumChunks()).Select(n => new ChunkViewModel(n)));
        else // status == Downloading or Error
          Chunks = new ObservableCollection<ChunkViewModel>(xe.Elements("Chunk").Select(ele => new ChunkViewModel(ele)));

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
    private int index;
    private DownloadChunkStatus status;
    private XElement xe;

    public int Index
    {
      get { return index; }
      set
      {
        index = value;
        OnPropertyChanged(nameof(Index));
        xe.Element(nameof(Index)).SetValue(index);
      }
    }

    public DownloadChunkStatus Status
    {
      get { return status; }
      set
      {
        status = value;
        OnPropertyChanged(nameof(Status));
        xe.Element(nameof(Status)).SetValue(status);
      }
    }

    public ChunkViewModel(int index)
    {
      this.index = index;
      status = DownloadChunkStatus.New;
      xe = new XElement("Chunk",
        new XAttribute(nameof(Index), index),
        new XElement(nameof(Status), status));
    }

    public ChunkViewModel(XElement xe)
    {
      this.xe = xe;
      index = (int)xe.Attribute(nameof(Index));
      Enum.TryParse(xe.Element(nameof(Status)).Value, out status);
    }

    public XElement GetXElement()
    {
      return xe;
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

