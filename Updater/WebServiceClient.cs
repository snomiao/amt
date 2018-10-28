using System;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

namespace YTY.amt
{
  public static class WebServiceClient
  {
    public const int CHUNKSIZE = 1<<18;

    private static readonly HttpClient client = new HttpClient();

    public static async Task<Tuple<int, byte[]>> GetChunk(FileModel file, int chunkId)
    {
      var request = new HttpRequestMessage(HttpMethod.Get, file.SourceUri);
      request.Headers.Range = new RangeHeaderValue(chunkId * CHUNKSIZE, (chunkId + 1) * CHUNKSIZE - 1);
      return Tuple.Create(chunkId, await (await client.SendAsync(request)).Content.ReadAsByteArrayAsync());
    }

    public static async Task<Version> GetSelfVersion()
    {
      var result = await client.GetStringAsync("http://www.hawkaoe.net/amt/UpdaterVersion.txt");
      return Version.Parse(result);
    }

    public static async Task DownloadSelf()
    {
      using (var fs = new FileStream(Util.MakeQualifiedPath("updater.exe.rename"), FileMode.Create,
        FileAccess.ReadWrite, FileShare.None, 4096, true))
      {
        await (await client.GetStreamAsync("http://www.hawkaoe.net/amt/updater.exe")).CopyToAsync(fs);
      }
    }
  }
}
