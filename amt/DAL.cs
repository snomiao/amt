using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows.Threading;
using System.Data.Common;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Script.Serialization;
using System.Collections;

namespace YTY.amt
{
  public static class DAL
  {
    public const int CHUNKSIZE = 65536;
    private const string CONFIGFILE = "config.db";
    private static Dictionary<string, WorkshopResourceType> dic_String_Type;
    private static SqliteOp ConfigOp = new SqliteOp(CONFIGFILE);
    private static HttpClientHandler handler = new HttpClientHandler();
    private static HttpClient client = new HttpClient(handler, false);
    private static JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();

    static DAL()
    {
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
      handler.CookieContainer = new CookieContainer();
      client.BaseAddress = new Uri("http://121.42.152.168/ssrc/");
    }

    public static List<WorkshopResourceModel> GetLocalResources()
    {
      var ret = new List<WorkshopResourceModel>();
      using (var reader = ConfigOp.ExecuteReader("SELECT Id,CreateDate,LastChangeDate,LastFileChangeDate,Size,Rating,NumDownloads,AuthorId,AuthorName,Name,Discription,GameVersion,Url,Type,Status,Flags FROM Resources"))
      {
        while (reader.Read())
        {
          var type = (WorkshopResourceType)reader.GetInt32(13);
          WorkshopResourceModel resource = null;
          switch (type)
          {
            case WorkshopResourceType.Drs:
              resource = new DrsResourceModel(reader.GetInt32(0));
              resource.Flags = (WorkshopResourceFlag)reader.GetInt32(15);
              break;
            default:
              resource = new WorkshopResourceModel(reader.GetInt32(0), type);
              break;
          }
          resource.CreateDate = reader.GetInt32(1);
          resource.LastChangeDate = reader.GetInt32(2);
          resource.LastFileChangeDate = reader.GetInt32(3);
          resource.TotalSize = reader.GetInt64(4);
          resource.Rating = reader.GetInt32(5);
          resource.DownloadCount = reader.GetInt32(6);
          resource.AuthorId = reader.GetInt32(7);
          resource.AuthorName = reader.GetString(8);
          resource.Name = reader.GetString(9);
          resource.Discription = reader.GetString(10);
          resource.GameVersion = (GameVersion)reader.GetInt32(11);
          resource.SourceUrl = reader.GetString(12);
          resource.Status = (WorkshopResourceStatus)reader.GetInt32(14);
          if (resource.Status == WorkshopResourceStatus.Installing)
            resource.UpdateStatus(WorkshopResourceStatus.Paused);
          if (resource.Status == WorkshopResourceStatus.Paused)
          {
            resource.LocalLoadFiles();
            foreach (var file in resource.Files)
            {
              if (file.Status == ResourceFileStatus.Downloading)
                file.UpdateStatus(ResourceFileStatus.Paused);
              if (file.Status == ResourceFileStatus.Paused)
                file.Chunks = LoadChunks(file.Id);
            }
          }
          ret.Add(resource);
        }
      }
      return ret;
    }

    public static List<ResourceFileModel> GetLocalResourceFiles(int resourceId)
    {
      var ret = new List<ResourceFileModel>();
      using (var reader = ConfigOp.ExecuteReader($"SELECT Id,UpdateDate,Size,Path,Sha1,Status FROM Files WHERE ResId={resourceId}"))
      {
        while (reader.Read())
        {
          var resource = new ResourceFileModel()
          {
            ResId = resourceId,
            Id = reader.GetInt32(0),
            UpdateDate = reader.GetInt32(1),
            Size = reader.GetInt32(2),
            Path = reader.GetString(3),
            Sha1 = reader.GetString(4),
            Status = (ResourceFileStatus)reader.GetInt32(5)
          };
          ret.Add(resource);
        }
      }
      return ret;
    }


    public static async Task<Tuple<int, List<WorkshopResourceModel>>> GetUpdatedServerResourcesAsync()
    {
      var json = await client.GetStringAsync($"api/?q=lsres&t={ConfigModel.CurrentConfig.WorkshopTimestamp}");
      if (string.IsNullOrEmpty(json))
        throw new InvalidOperationException();
      var obj = jsonSerializer.Deserialize<IDictionary<string, object>>(json);
      var latestTimestamp = (int)obj["t"];
      var resources = obj["r"] as IEnumerable<object>;
      return Tuple.Create(latestTimestamp,
        resources.Select(resource =>
          {
            var dic = resource as IDictionary<string, object>;
            var type = dic_String_Type[dic["ty"] as string];
            WorkshopResourceModel ret = null;
            switch (type)
            {
              case WorkshopResourceType.Drs:
                ret = new DrsResourceModel((int)dic["id"]);
                ret.Flags = WorkshopResourceFlag.Deactivated;
                break;
              default:
                ret = new WorkshopResourceModel((int)dic["id"], type);
                break;
            }
            ret.LastFileChangeDate = (int)dic["tf"];
            ret.LastChangeDate = (int)dic["tu"];
            ret.Rating = (int)dic["vr"];
            ret.TotalSize = (int)dic["ts"];
            ret.DownloadCount = (int)dic["cd"];
            ret.CreateDate = (int)dic["tc"];
            ret.AuthorId = (int)dic["ai"];
            ret.AuthorName = dic["an"] as string;
            ret.Name = dic["n"] as string;
            ret.Discription = dic["co"] as string;
            ret.GameVersion = (GameVersion)dic["gb"];
            ret.SourceUrl = dic["ur"] as string;
            ret.Status = (WorkshopResourceStatus)dic["st"];
            return ret;
          }).ToList()
        );
    }

    public static async Task GetResourceImagesAsync(this WorkshopResourceModel model)
    {

    }

    public async static Task GetCookie()
    {
      //var response = await client.PostAsync("login.php", new FormUrlEncodedContent(new[] {
      //  new KeyValuePair<string,string>("pmd5", "10f0dce340bc2008d8e620ffce2537ca"),
      //  new KeyValuePair<string,string>("u","amt") }));
      // await client.GetStringAsync("res.php?action=ls&res=6");
    }

    public static async Task<Tuple<int, List<ResourceFileModel>>> GetResourceUpdatedFilesAsync(int workshopResourceid, int timestamp = 0)
    {
      var json = await client.GetStringAsync($"api/?q=lsfile&resid={Util.Int2CSID(workshopResourceid)}&t={timestamp}");
      if (string.IsNullOrEmpty(json))
        throw new InvalidOperationException();
      var obj = jsonSerializer.Deserialize<IDictionary<string, object>>(json);
      var lastFileChange = (int)obj["t"];
      var files = obj["r"] as IEnumerable<object>;
      return Tuple.Create(lastFileChange,
        files.Select(f =>
        {
          var dic = f as IDictionary<string, object>;
          return new ResourceFileModel()
          {
            ResId = workshopResourceid,
            Id = (int)dic["id"],
            Size = (int)dic["s"],
            Path = (dic["p"] as string).TrimStart('/'),
            Sha1 = dic["h"] as string,
            UpdateDate = (int)dic["t"],
            Status = (ResourceFileStatus)dic["d"]
          };
        }).ToList()
        );
    }

    public static void SaveResourceModels(IEnumerable<WorkshopResourceModel> models)
    {
      ConfigOp.ExecuteNonQueryTransaction(models.Select(model => $@"INSERT OR REPLACE INTO Resources(
Id,CreateDate,LastChangeDate,LastFileChangeDate,Size,Rating,NumDownloads,AuthorId,AuthorName,Name,Discription,GameVersion,Url,Type,Status)
VALUES({model.Id},
{model.CreateDate},
{model.LastFileChangeDate},
{model.LastChangeDate},
{model.TotalSize},
{model.Rating},
{model.DownloadCount},
{model.AuthorId},
'{Util.EscapeSqliteString(model.AuthorName)}',
'{Util.EscapeSqliteString(model.Name)}',
'{Util.EscapeSqliteString(model.Discription)}',
{(int)model.GameVersion},
'{model.SourceUrl}',
{(int)model.Type},
{(int)model.Status}
)"));
    }

    public static void SaveResourceFileModels(IEnumerable<ResourceFileModel> models)
    {
      ConfigOp.ExecuteNonQueryTransaction(models.Select(model => $@"INSERT OR REPLACE INTO Files(
ResId,Id,Size,Path,UpdateDate,Sha1,Status) 
VALUES({model.ResId},
{model.Id},
{model.Size},
'{Util.EscapeSqliteString(model.Path)}',
{model.UpdateDate},
'{model.Sha1}',
{(int)model.Status}
)"));
    }

    public static void UpdateResourceLastFileChange(int id, int lastFileChange)
    {
      ConfigOp.ExecuteNonQuery($"UPDATE Resources SET LastFileChangeDate={lastFileChange} WHERE Id={id}");
    }

    public static void UpdateResourceStatus(int id, WorkshopResourceStatus status)
    {
      ConfigOp.ExecuteNonQuery($"UPDATE Resources SET Status={(int)status} WHERE Id={id}");
    }

    public static void UpdateResourceFileStatus(int id, ResourceFileStatus status)
    {
      ConfigOp.ExecuteNonQuery($"UPDATE Files SET Status={(int)status} WHERE Id={id}");
    }

    public static void UpdateResourceFlags(int id, WorkshopResourceFlag flags)
    {
      ConfigOp.ExecuteNonQuery($"UPDATE Resources SET Flags={(int)flags} WHERE Id={id}");
    }

    public static void UpdateFileChunkFinished(int fileId, int id, bool finished)
    {
      ConfigOp.ExecuteNonQuery($"UPDATE Chunks SET Finished={Convert.ToInt32(finished)} WHERE FileId={fileId}");
    }

    public static void DeleteFileChunks(int fileId)
    {
      ConfigOp.ExecuteNonQuery($"DELETE FROM Chunks WHERE FileId={fileId}");
    }

    public static List<FileChunkModel> LoadChunks(int fileId)
    {
      var ret = new List<FileChunkModel>();
      using (var reader = ConfigOp.ExecuteReader($"SELECT Id,Finished FROM Chunks WHERE FileId={fileId}"))
      {
        while (reader.Read())
        {
          ret.Add(new FileChunkModel() { FileId = fileId, Id = reader.GetInt32(0), Finished = reader.GetBoolean(1) });
        }
      }
      return ret;
    }

    public static void SaveChunks(this ResourceFileModel fileModel)
    {
      ConfigOp.ExecuteNonQueryTransaction(fileModel.Chunks.Select(c => $"INSERT INTO Chunks (FileId,Id,Finished) VALUES({c.FileId},{c.Id},0)"));
    }

    public static async Task<byte[]> GetChunk(int fileId, int chunkId)
    {
      var request = new HttpRequestMessage(HttpMethod.Get, $"res.php?action=ls&file={Util.Int2CSID(fileId)}");
      request.Headers.Range = new RangeHeaderValue(chunkId * CHUNKSIZE, (chunkId + 1) * CHUNKSIZE - 1);
      return await (await client.SendAsync(request)).Content.ReadAsByteArrayAsync();
    }

    public static void DeleteResourceFiles(int resourceId)
    {
      ConfigOp.ExecuteNonQuery($"DELETE FROM Files WHERE ResId={resourceId}");
    }

    public static List<GameVersionModel> GetGameVersions()
    {
      var ret = new List<GameVersionModel>();
      using (var reader = ConfigOp.ExecuteReader("SELECT id,name,exePath FROM GameVersions"))
      {
        while (reader.Read())
        {
          ret.Add(new GameVersionModel()
          {
            ResourceId = reader.GetInt32(0),
            Name = reader.GetString(1),
            ExePath = reader.GetString(2)
          });
        }
      }
      AddGameVersionIfNotExists(ret, new GameVersionModel()
      {
        ResourceId = -1,
        Name = "帝国时代Ⅱ 1.5",
        ExePath = @"exe\age2_x1.5.exe",
      });
      AddGameVersionIfNotExists(ret, new GameVersionModel()
      {
        ResourceId = -2,
        Name = "帝国时代Ⅱ 1.0C",
        ExePath = @"exe\age2_x1.0c.exe",
      });
      AddGameVersionIfNotExists(ret, new GameVersionModel()
      {
        ResourceId = -3,
        Name = "被遗忘的帝国",
        ExePath = @"exe\age2_x2.exe",
      });
      AddGameVersionIfNotExists(ret, new GameVersionModel()
      {
        ResourceId = -4,
        Name = "WAIFor 触发扩展版",
        ExePath = @"exe\age2_wtep.exe",
      });
      return ret;
    }

    private static void AddGameVersionIfNotExists(List<GameVersionModel> list, GameVersionModel model)
    {
      if (!list.Any(m => m.ResourceId == model.ResourceId))
      {
        list.Add(model);
        AddGameVersion(model);
      }
    }

    public static void AddGameVersion(GameVersionModel model)
    {
      ConfigOp.ExecuteNonQuery($"INSERT INTO GameVersions(id,name,exePath) VALUES({model.ResourceId},'{model.Name}','{model.ExePath}')");
    }

    public static void CreateTablesIfNotExist()
    {
      ConfigOp.ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS Config(key text,value text);
        CREATE TABLE IF NOT EXISTS Resources(Id int PRIMARY KEY,CreateDate int,LastChangeDate int,LastFileChangeDate int,Size int,Rating int,NumDownloads int,AuthorId int,AuthorName text,Name text,Discription text,GameVersion int,Url text,Type int,Status int,Flags int DEFAULT 0);
        CREATE TABLE IF NOT EXISTS Files(ResId int,Id int PRIMARY KEY,Size int,Path text,UpdateDate int,Sha1 text,Status int);
        CREATE TABLE IF NOT EXISTS Chunks(FileId int,Id int,Finished int,PRIMARY KEY(FileId,Id));    
        CREATE TABLE IF NOT EXISTS GameVersions(id int PRIMARY KEY, name text, exePath text)");
    }

    public static void SaveConfigString(string key, string value)
    {
      ConfigOp.ExecuteNonQuery($"UPDATE Config SET value='{value}' WHERE key='{key}'");
    }

    public static void SaveConfigInt(string key, int value)
    {
      SaveConfigString(key, value.ToString());
    }

    public static void SaveConfigBool(string key, bool value)
    {
      SaveConfigInt(key, Convert.ToInt32(value));
    }

    public static string GetConfigString(string key, string defaultValue)
    {
      var ret = ConfigOp.ExecuteScalar<string>($"SELECT value FROM Config WHERE key='{key}'");
      if (ret == null)
        ConfigOp.ExecuteNonQuery($"INSERT INTO Config (key,value) VALUES('{key}','{defaultValue}')");
      return ret ?? defaultValue;
    }

    public static int GetConfigInt(string key, int defaultValue)
    {
      int ret;
      if (!int.TryParse(GetConfigString(key, defaultValue.ToString()), out ret))
        ret = defaultValue;
      return ret;
    }

    public static bool GetConfigBool(string key, bool defaultValue)
    {
      return Convert.ToBoolean(GetConfigInt(key, Convert.ToInt32(defaultValue)));
    }
  }
}
