using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml.Linq;
using System.IO;

namespace YTY.amt
{
  internal class DownloadTask : INotifyPropertyChanged
  {
    private WebDownloader wd;
    private XElement xe;
    private DownloadTaskStatus status;
    private IDictionary<int, ChunkEntity> chunks;

    public event PropertyChangedEventHandler PropertyChanged;

    internal string Uri { get; }

    internal string FileName { get; }

    internal DownloadTaskStatus Status
    {
      get { return status; }
      set
      {
        status = value;
        OnPropertyChanged(nameof(Status));
      }
    }

    internal IDictionary<int, ChunkEntity> Chunks { get { return chunks; } }

    internal DownloadTask(string uri, string fileName)
    {
      Uri = uri;
      FileName = fileName;
      status = DownloadTaskStatus.New;
      xe = new XElement("DownloadTask",
        new XElement("Uri", Uri),
        new XElement("FileName", FileName),
        new XElement("Status", Status));
      ConfigRoot.root.Add(xe);
    }

    internal DownloadTask(XElement xe)
    {
      this.xe = xe;
      Uri = xe.Element("Uri").Value;
      FileName = xe.Element("FileName").Value;
      status = (DownloadTaskStatus)Enum.Parse(typeof(DownloadTaskStatus), xe.Element("Status").Value);
    }

    internal void Start()
    {
      wd = new WebDownloader(Uri);
      chunks = Enumerable.Range(0, wd.GetNumChunks()).ToDictionary(n => n, n => new ChunkEntity(ChunkStatus.New));
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
      };
      wd.Start();
    }

    private void OnPropertyChanged(string propertyName)
    {
      PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }

    internal class ChunkEntity : INotifyPropertyChanged
    {
      private ChunkStatus status;

      internal ChunkStatus Status
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
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
      }
    }

    internal enum DownloadTaskStatus
    {
      New
    }

    internal enum ChunkStatus
    {
      New,
      Done
    }
  }
}
