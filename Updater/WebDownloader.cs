using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using YTY;

namespace YTY
{
  public class WebDownloader
  {
    private const int DEFAULT_ChunkSize = 65536;
    private const int DEFAULT_NumThreads = 5;
    private const int DEFAULT_Timeout = 8000;

    private long? contentLength;
    private int? numChunks;
    private ManualResetEvent paused;
    private ManualResetEvent stopped;
    private Semaphore semaphore;
    private CountdownEvent cde;
    private SynchronizationContext syncContext;

    public int ChunkSize { get; set; }

    public int NumThreads { get; set; }

    public int Timeout { get; set; }

    public string Uri { get; set; }

    public WebDownloader()
    {
      syncContext = SynchronizationContext.Current;
      ChunkSize = DEFAULT_ChunkSize;
      NumThreads = DEFAULT_NumThreads;
      Timeout = DEFAULT_Timeout;
      paused = new ManualResetEvent(true);
      stopped = new ManualResetEvent(false);
    }

    public WebDownloader(string uri) : this()
    {
      Uri = uri;
    }

    public long GetContentLength()
    {
      if (!contentLength.HasValue)
      {
        try
        {
          var req = WebRequest.Create(Uri);
          req.Method = "HEAD";
          var resp = req.GetResponse() as HttpWebResponse;
          contentLength = resp.ContentLength;
        }
        catch
        {
          throw;
        }
      }
      return contentLength.Value;
    }

    public int GetNumChunks()
    {
      if (!numChunks.HasValue)
      {
        numChunks = (int)((GetContentLength() + ChunkSize - 1) / ChunkSize);
      }
      return numChunks.Value;
    }

    /// <summary>
    /// Starts downloading all chunks.
    /// </summary>
    public void Start()
    {
      Start(Enumerable.Range(0, GetNumChunks()));
    }

    /// <summary>
    /// Starts downloading specified chunks.
    /// </summary>
    /// <param name="indexes">Indexes of chunks to download.</param>
    public void Start(IEnumerable<int> indexes)
    {
      ThreadPool.QueueUserWorkItem(state =>
      {
        semaphore = new Semaphore(NumThreads, NumThreads);
        cde = new CountdownEvent(indexes.Count());
        var waitHandles = new WaitHandle[] { stopped, semaphore };
        foreach (var index in indexes)
        {
          if (WaitHandle.WaitAny(waitHandles) == 0) // Stopped
            return;
          ThreadPool.QueueUserWorkItem(state1 => DownloadChunk(index));
        }
        if (WaitHandle.WaitAny(new WaitHandle[] { stopped, cde.WaitHandle }) == 0) // Stopped
          return;
        OnDownloadCompleted();
      });
    }

    public void Pause()
    {
      paused.Reset();
    }

    public void Stop()
    {
      stopped.Set();
    }

    public void Resume()
    {
      paused.Set();
    }

    public event EventHandler<DownloadChunkEventArgs> ChunkCompleted;

    private void OnChunkCompleted(int index, byte[] data)
    {
      syncContext.Send(state => ChunkCompleted(this, new DownloadChunkEventArgs() { Index = index, Data = data }), null);
    }

    public event EventHandler DownloadCompleted;

    private void OnDownloadCompleted()
    {
      syncContext.Send(state => DownloadCompleted(this, EventArgs.Empty), null);
    }

    private void DownloadChunk(int index)
    {
      for (var iTry = 0; ; iTry++)
      {
        try
        {
          byte[] bytes;
          using (var wd = new WebClientEx())
          {
            wd.Timeout = Timeout;
            if (index == GetNumChunks() - 1)
              wd.AddRange(ChunkSize * index);
            else
              wd.AddRange(ChunkSize * index, ChunkSize * (index + 1) - 1);
            bytes = wd.DownloadData(Uri);
          }
          paused.WaitOne();
          semaphore.Release();
          cde.Signal();
          OnChunkCompleted(index, bytes);
          break;
        }
        catch (WebException ex)
        {

        }
      }
    }
  }
}
