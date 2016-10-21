using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Net;
using System.Threading.Tasks;

namespace YTY
{
  public class WebDownloader
  {
    private const int DEFAULT_ChunkSize = 65536;
    private const int DEFAULT_NumThreads = 5;
    private const int DEFAULT_Timeout = 8000;
    private const int DEFAULT_Retries = 5;

    private long? contentLength;
    private int? numChunks;
    private ManualResetEvent paused;
    private ManualResetEvent stopped;
    private Semaphore semaphore;
    private CountdownEvent cde;
    private SynchronizationContext syncContext;
    private bool anyChunkError = false;

    public int ChunkSize { get; set; }

    public int NumThreads { get; set; }

    public int Timeout { get; set; }

    public int Retries { get; set; }

    public string Uri { get; set; }

    public WebDownloader()
    {
      syncContext = SynchronizationContext.Current;
      ChunkSize = DEFAULT_ChunkSize;
      NumThreads = DEFAULT_NumThreads;
      Timeout = DEFAULT_Timeout;
      Retries = DEFAULT_Retries;
      paused = new ManualResetEvent(true);
      stopped = new ManualResetEvent(false);
    }

    public WebDownloader(string uri) : this()
    {
      Uri = uri;
    }

    public async Task<long> GetContentLength()
    {
      if (!contentLength.HasValue)
      {
        var req = WebRequest.Create(Uri);
        req.Timeout = Timeout;
        req.Method = "HEAD";
        var resp = await req.GetResponseAsync();
        contentLength = resp.ContentLength;
      }
      return contentLength.Value;
    }

    public async Task<int> GetNumChunks()
    {
      if (!numChunks.HasValue)
      {
        numChunks = (int)((await GetContentLength() + ChunkSize - 1) / ChunkSize);
      }
      return numChunks.Value;
    }

    /// <summary>
    /// Starts downloading all chunks.
    /// </summary>
    public async void Start()
    {
      Start(Enumerable.Range(0, await GetNumChunks()));
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

    private void OnChunkCompleted(int index, byte[] data, bool error)
    {
      syncContext.Send(state => ChunkCompleted(this, new DownloadChunkEventArgs() { Index = index, Data = data, Error = error }), null);
    }

    public event EventHandler<DownloadCompletedEventArgs> DownloadCompleted;

    private void OnDownloadCompleted()
    {
      syncContext.Send(state => DownloadCompleted(this, new DownloadCompletedEventArgs() { Error = anyChunkError }), null);
    }

    private async void DownloadChunk(int index)
    {
      for (var iTry = 1; ; iTry++)
      {
        try
        {
          byte[] bytes;
          using (var wc = new WebClientEx())
          {
            wc.Timeout = Timeout;
            if (index == await GetNumChunks() - 1)
              wc.AddRange(ChunkSize * index);
            else
              wc.AddRange(ChunkSize * index, ChunkSize * (index + 1) - 1);
            bytes = wc.DownloadData(Uri);
          }
          paused.WaitOne();
          semaphore.Release();
          cde.Signal();
          OnChunkCompleted(index, bytes, false);
          break;
        }
        catch (WebException)
        {
          if (iTry == Retries)
          {
            anyChunkError = true;
            OnChunkCompleted(index, null, true);
          }
        }
      }
    }
  }

  public class DownloadCompletedEventArgs : EventArgs
  {
    public bool Error { get; set; }
  }
}
