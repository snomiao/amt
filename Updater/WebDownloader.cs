using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace YTY
{
  public class WebDownloader
  {
    private const int DEFAULT_ChunkSize = 65536;
    private const int DEFAULT_Timeout = 8000;
    private const int DEFAULT_Retries = 5;

    private long? contentLength;
    private int? numChunks;

    public int ChunkSize { get; set; }

    public int Timeout { get; set; }

    public int Retries { get; set; }

    public string Uri { get; set; }

    public WebDownloader()
    {
      ChunkSize = DEFAULT_ChunkSize;
      Timeout = DEFAULT_Timeout;
      Retries = DEFAULT_Retries;
    }

    public WebDownloader(string uri) : this()
    {
      Uri = uri;
    }

    public WebDownloader(string uri, long contentLength) : this(uri)
    {
      this.contentLength = contentLength;
    }

    public async Task<long> GetContentLength()
    {
      if (!contentLength.HasValue)
      {
        var request = WebRequest.Create(Uri);
        request.Method = "HEAD";
        var taskTimeout = TaskEx.Delay(Timeout);
        var taskResponse = request.GetResponseAsync();
        var firstCompleted = await TaskEx.WhenAny(new[] { taskTimeout, taskResponse });
        if (firstCompleted == taskTimeout)
        {
          request.Abort();
          throw new WebException("", WebExceptionStatus.Timeout);
        }
        else
        {
          using (var response = await (firstCompleted as Task<WebResponse>))
          {
            contentLength = response.ContentLength;
          }
        }
      }
      return contentLength.Value;
    }

    public int GetNumChunks()
    {
      if (!numChunks.HasValue)
        numChunks = (int)((contentLength + ChunkSize - 1) / ChunkSize);

      return numChunks.Value;
    }

    /// <summary>
    /// Starts downloading specified chunks.
    /// </summary>
    /// <param name="indexes">Indexes of chunks to download.</param>
    public async void Start(IEnumerable<int> indexes)
    {
      var tasks = indexes.Select(i => DownloadChunkAsync(i));
      await TaskEx.WhenAll(tasks);
      //try
      //{
      //  await TaskEx.WhenAll(tasks.Keys);
      //}
      //catch (WebException) { throw; }
    }

    public async Task<Tuple<int, byte[]>> DownloadChunkAsync(int index)
    {
      for (var iTry = 0; iTry < Retries; iTry++)
      {
        var request = WebRequest.Create(Uri) as HttpWebRequest;
        request.AddRange(ChunkSize * index, ChunkSize * (index + 1) - 1);
        var taskTimeout = TaskEx.Delay(Timeout);
        var taskResponse = request.GetResponseAsync();
        var firstCompleted = await TaskEx.WhenAny(new[] { taskTimeout, taskResponse });
        if (firstCompleted == taskTimeout)
        {
          request.Abort();
        }
        else
        {
          using (var response = await (firstCompleted as Task<WebResponse>))
          {
            using (var ms = new MemoryStream(ChunkSize))
            {
              response.GetResponseStream().CopyTo(ms);
              return Tuple.Create(index, ms.ToArray());
            }
          }
        }
      }
      throw new WebException($"Retry exceeded {Retries} times.", WebExceptionStatus.MessageLengthLimitExceeded);
    }
  }
}
