using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml.Linq;
using System.IO;

namespace YTY.amt
{
  internal class DownloadModel : INotifyPropertyChanged
  {
    private WebDownloader wd;
    private XElement xe;
    private DownloadTaskStatus status;
    private IDictionary<int, ChunkEntity> chunks;

    public event PropertyChangedEventHandler PropertyChanged;

    public string Uri { get; }

    public string FileName { get; }

    public DownloadTaskStatus Status
    {
      get { return status; }
      set
      {
        status = value;
        OnPropertyChanged(nameof(Status));
      }
    }

    public IDictionary<int, ChunkEntity> Chunks
    {
      get { return chunks; }
      set
      {
        chunks = value;
        OnPropertyChanged(nameof(Chunks));
      }
    }

    internal DownloadModel(string uri, string fileName)
    {
      Uri = uri;
      FileName = fileName;
      status = DownloadTaskStatus.Ready;
      xe = new XElement("DownloadTask",
        new XElement("Uri", Uri),
        new XElement("FileName", FileName),
        new XElement("Status", Status));
      ConfigRoot.Root.Add(xe);
    }

    internal DownloadModel(XElement xe)
    {
      this.xe = xe;
      Uri = xe.Element("Uri").Value;
      FileName = xe.Element("FileName").Value;
      Status = (DownloadTaskStatus)Enum.Parse(typeof(DownloadTaskStatus), xe.Element("Status").Value);
    }

    internal void Start()
    {
      wd = new WebDownloader(Uri);
      Chunks = Enumerable.Range(0, wd.GetNumChunks()).ToDictionary(n => n, n => new ChunkEntity(ChunkStatus.New));
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

    private void OnPropertyChanged(string propertyName)
    {
      var handler = PropertyChanged;
      if (handler != null)
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }
  }

  internal class ChunkEntity : INotifyPropertyChanged
  {
    private ChunkStatus status;

    public ChunkStatus Status
    {
      get { return status; }
      set
      {
        status = value;
        OnPropertyChanged(nameof(Status));
      }
    }

    public ChunkEntity(ChunkStatus status)
    {
      this.status = status;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
      var handler = PropertyChanged;
      if (handler != null)
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }
  }

  internal enum DownloadTaskStatus
  {
    Ready,
    Downloading,
    Finished
  }

  internal enum ChunkStatus
  {
    New,
    Done
  }

}

