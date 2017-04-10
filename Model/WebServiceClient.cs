using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace YTY.amt.Model
{
  internal static class WebServiceClient
  {
    private static HttpClient client = new HttpClient();
    private static Dictionary<string, WorkshopResourceType> dic_String_Type;

    static WebServiceClient()
    {
      client.BaseAddress = new Uri("http://121.42.152.168/ssrc/");
      dic_String_Type = new Dictionary<string, WorkshopResourceType>()
      {
        {"cpx", WorkshopResourceType.Campaign },
        {"drs",WorkshopResourceType.Drs },
        {"rms",WorkshopResourceType.RandomMap },
        {"scx", WorkshopResourceType.Scenario },
        {"mgx",WorkshopResourceType.Replay },
        {"mod", WorkshopResourceType.Mod },
        {"ai",WorkshopResourceType.Ai },
        {"tau",WorkshopResourceType.Taunt },
        {"undefined", WorkshopResourceType.Undefined }
      };
    }

    public static async Task<(int TimeStamp, IEnumerable<WorkshopResourceDto> Resources)> GetUpdatedServerResourcesAsync()
    {
      var json = await client.GetStringAsync($"api/?q=lsres&t={ProgramModel.Config.WorkshopTimestamp}");
      dynamic dyna = JsonConvert.DeserializeObject(json);
      var latestTimestamp = (int)dyna.t;
      return (latestTimestamp, Map(dyna.r));

      IEnumerable<WorkshopResourceDto> Map(dynamic r)
      {
        foreach (var o in r)
        {
          yield return new WorkshopResourceDto
          {
            Id = (int)o.id,
            Type = dic_String_Type[(string)o.ty],
            LastFileChangeDate = (int)o.tf,
            LastChangeDate = (int)o.tu,
            Rating = (int)o.vr,
            TotalSize = (int)o.ts,
            DownloadCount = (int)o.cd,
            CreateDate = (int)o.tc,
            AuthorId = (int)o.ai,
            AuthorName = (string)o.an,
            Name = (string)o.n,
            Description = (string)o.co,
            GameVersion = (GameVersion)o.gb,
            SourceUrl = (string)o.ur,
            Status = (int)o.st,
          };
        }
      }
    }

    public static async Task<(int TimeStamp, List<ResourceFileDto> Dtos)> GetResourceUpdatedFilesAsync(int resourceId, int timestamp = 0)
    {
      var json = await client.GetStringAsync($"api/?q=lsfile&resid={Util.Int2Csid(resourceId)}&t={timestamp}");
      dynamic dyna = JsonConvert.DeserializeObject(json);
      var lastFileChange = (int)dyna.t;
      var dtos = new List<ResourceFileDto>();
      foreach (var o in dyna.r)
      {
        var model = new ResourceFileDto
        {
          Id = (int)o.id,
          Size = (int)o.s,
          Path = ( (string)o.p).Replace('/','\\').TrimStart('\\'),
          Sha1 = (string)o.h,
          UpdateDate = (int)o.t,
          Status = (int)o.d,
        };
        dtos.Add(model);
      }
      return (lastFileChange, dtos);
    }

    public static async Task<byte[]> GetChunk(int fileId, int chunkId)
    {
      var request = new HttpRequestMessage(HttpMethod.Get, $"res.php?action=ls&file={Util.Int2Csid(fileId)}");
      request.Headers.Range = new RangeHeaderValue(chunkId * ConfigModel.CHUNKSIZE, (chunkId + 1) * ConfigModel.CHUNKSIZE - 1);
      return await (await client.SendAsync(request)).Content.ReadAsByteArrayAsync();
    }
  }
}
