using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.IO;
using System.Net;
using System.Windows;

namespace YTY.amt
{
  public class UpdateItemViewModel : ViewModelBase
  {
    private WebDownloader wd;
    private XElement xe;
    private long size;
    private Version version;
    private string md5;
    private UpdateItemStatus status;
    private ObservableCollection<ChunkViewModel> chunks;

    public int Id { get; }

    public string SourceUri { get; }

    public string FileName { get; }

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
      chunks = new ObservableCollection<ChunkViewModel>(xe.Elements("Chunk").Select(ele => new ChunkViewModel(ele)));
    }

    public async void Start()
    {
      wd = new WebDownloader(SourceUri);
      if (status == UpdateItemStatus.Finished)
        throw new InvalidOperationException();

      try
      {
        if (status == UpdateItemStatus.Ready)
          Chunks = new ObservableCollection<ChunkViewModel>(Enumerable.Range(0, await wd.GetNumChunks()).Select(n => new ChunkViewModel(n)));

        var dir = Path.GetDirectoryName(Util.MakeQualifiedPath(FileName));
        if (!Directory.Exists(dir))
          Directory.CreateDirectory(dir);
        var bw = new BinaryWriter(new FileStream(Util.MakeQualifiedPath(FileName), FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read));
        wd.ChunkCompleted += (s, e) =>
        {
          if (!e.Error)
          {
            bw.Seek(wd.ChunkSize * e.Index, SeekOrigin.Begin);
            bw.Write(e.Data);
            bw.Flush();
            chunks.First(ch => ch.Index == e.Index).Status = DownloadChunkStatus.Done;
          }
        };
        wd.DownloadCompleted += (s, e) =>
        {
          bw.Close();
          Chunks = null;
          if (e.Error)
            status = UpdateItemStatus.Error;
          else
            Status = UpdateItemStatus.Finished;
        };
        wd.Start(chunks.Select(ch => ch.Index));
        Status = UpdateItemStatus.Downloading;
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

