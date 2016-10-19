using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml.Linq;
using System.IO;
using System.Windows;

namespace YTY.amt
{
  public class DownloadTaskViewModel : ViewModelBase
  {
    private WebDownloader wd;
    private XElement xe;
    private DownloadTaskStatus status;
    private IDictionary<int, ChunkEntity> chunks;

    public string Uri { get; }

    public string FileName { get; }

    public DownloadTaskStatus Status
    {
      get { return status; }
      set
      {
        status = value;
        OnPropertyChanged(nameof(Status));
        xe.Element(nameof(Status)).SetValue(status);
      }
    }

    public IDictionary<int, ChunkEntity> Chunks
    {
      get { return chunks; }
      set
      {
        chunks = value;
        if (chunks == null)
          xe.Elements(nameof(ChunkEntity)).Remove();
        else
        {
          OnPropertyChanged(nameof(Chunks));
          xe.Add(chunks.Select(chunk => chunk.Value.GetXElement()));
        }
      }
    }

    internal DownloadTaskViewModel(string uri, string fileName)
    {
      Uri = uri;
      FileName = fileName;
      status = DownloadTaskStatus.Ready;
      xe = new XElement(nameof(DownloadTaskViewModel),
        new XElement(nameof(Uri), Uri),
        new XElement(nameof(FileName), FileName),
        new XElement(nameof(Status), Status));
      Config.Root.Add(xe);
    }

    internal DownloadTaskViewModel(XElement xe)
    {
      this.xe = xe;
      Uri = xe.Element(nameof(Uri)).Value;
      FileName = xe.Element(nameof(FileName)).Value;
      Enum.TryParse(xe.Element(nameof(Status)).Value, out status);
    }

    internal void Start()
    {
      wd = new WebDownloader(Uri);
      Chunks = Enumerable.Range(0, wd.GetNumChunks()).ToDictionary(n => n, n => new ChunkEntity(n));
      var bw = new BinaryWriter(new FileStream(FileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read));
      wd.ChunkCompleted += (s, e) =>
      {
        bw.Seek(wd.ChunkSize * e.Index, SeekOrigin.Begin);
        bw.Write(e.Data);
        bw.Flush();
        chunks[e.Index].Status = ChunkStatus.Done;
      };
      wd.DownloadCompleted += (s, e1) =>
      {
        bw.Close();
        Status = DownloadTaskStatus.Finished;
      };
      wd.Start();
      Status = DownloadTaskStatus.Downloading;
    }
  }

  public class ChunkEntity : INotifyPropertyChanged
  {
    private int index;
    private ChunkStatus status;
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

    public ChunkStatus Status
    {
      get { return status; }
      set
      {
        status = value;
        OnPropertyChanged(nameof(Status));
        xe.Element(nameof(Status)).SetValue(status);
      }
    }

    public ChunkEntity(int index)
    {
      status = ChunkStatus.New;
      xe = new XElement(nameof(ChunkEntity),
        new XElement(nameof(Index), index),
        new XElement(nameof(Status), status));
    }

    public ChunkEntity(XElement xe)
    {
      this.xe = xe;
      index = (int)xe.Element(nameof(Index));
      status = (ChunkStatus)Enum.Parse(typeof(ChunkStatus), xe.Element(nameof(Status)).Value);
    }

    public XElement GetXElement()
    {
      return xe;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
      var handler = PropertyChanged;
      if (handler != null)
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }
  }

  public enum DownloadTaskStatus
  {
    Ready,
    Downloading,
    Finished
  }

  public enum ChunkStatus
  {
    New,
    Done
  }

}

