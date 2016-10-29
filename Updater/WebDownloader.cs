using System;
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

    private long contentLength;
    private int numChunks;

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

    public WebDownloader(string uri, long contentLength) : this()
    {
      Uri = uri;
      this.contentLength = contentLength;
      numChunks = (int)((contentLength + ChunkSize - 1) / ChunkSize);
    }

    public int NumChunks => numChunks;

    public async Task<Tuple<int, byte[]>> DownloadChunkAsync(int index)
    {
      for (var iTry = 0; iTry < Retries; iTry++)
      {
        try
        {
          var request = WebRequest.Create(Uri) as HttpWebRequest;
          request.AddRange(ChunkSize * index, ChunkSize * (index + 1) - 1);
          var taskTimeout = TaskEx.Delay(Timeout);
          var taskResponse = TaskEx.Run(() => request.GetResponse());
          var firstCompleted = await TaskEx.WhenAny(taskResponse, taskTimeout).ConfigureAwait(false);
          if (firstCompleted == taskTimeout)
          {
            request.Abort();
          }
          else
          {
            using (var response = await (firstCompleted as Task<WebResponse>).ConfigureAwait(false))
            {
              using (var ms = new MemoryStream(ChunkSize))
              {
                response.GetResponseStream().CopyTo(ms);
                return Tuple.Create(index, ms.ToArray());
              }
            }
          }
        }
        catch (WebException ex)
        {
          Debug.WriteLine(ex.Message);
        }
      }
      throw new WebException($"Retry exceeded {Retries} times.", WebExceptionStatus.MessageLengthLimitExceeded);
    }
  }
}
