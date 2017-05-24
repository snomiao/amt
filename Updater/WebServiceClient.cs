using System;
using System.Net;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;

namespace YTY.amt
{
  public static class WebServiceClient
  {
    public const int CHUNKSIZE = 1<<18;

    private static HttpClient client = new HttpClient();

    public static async Task<Tuple<int, byte[]>> GetChunk(FileModel file, int chunkId)
    {
      var request = new HttpRequestMessage(HttpMethod.Get, file.SourceUri);
      request.Headers.Range = new RangeHeaderValue(chunkId * CHUNKSIZE, (chunkId + 1) * CHUNKSIZE - 1);
      return Tuple.Create(chunkId, await (await client.SendAsync(request)).Content.ReadAsByteArrayAsync());
    }
  }
}
