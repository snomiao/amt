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
    }

    public static ObservableCollection<WorkshopResourceModel> GetLocalResources()
    {
      var ret = new ObservableCollection<WorkshopResourceModel>();
      using (var reader = ConfigOp.ExecuteReader("SELECT Id,CreateDate,LastChangeDate,LastFileChangeDate,Size,Rating,NumDownloads,AuthorId,AuthorName,Discription,GameVersion,Url,Type,Status FROM Resources"))
      {
        while (reader.Read())
        {
          var resource = new WorkshopResourceModel(reader.GetInt32(0))
          {
            CreateDate = reader.GetDateTime(1),
            LastChangeDate = reader.GetDateTime(2),
            LastFileChangeDate = reader.GetDateTime(3),
            TotalSize = reader.GetInt64(4),
            Rating = reader.GetInt32(5),
            DownloadCount = reader.GetInt32(6),
            AuthorId = reader.GetInt32(7),
            AuthorName = reader.GetString(8),
            Discription = reader.GetString(9),
            GameVersion = (GameVersion)reader.GetInt32(10),
            SourceUrl = reader.GetString(11),
            Type = (WorkshopResourceType)reader.GetInt32(12),
            Status = (WorkshopResourceStatus)reader.GetInt32(13)
          };
        }
      }
      return ret;
    }

    public static async Task<IEnumerable<WorkshopResourceModel>> RefreshResourcesAsync()
    {
      var json = await client.GetStringAsync("api/?q=lsres");
      var resources = jsonSerializer.Deserialize<List<Dictionary<string,int>>>(json);
      return resources.Select(dic => new WorkshopResourceModel(dic["id"])
      {
       
      });
    }

    public static async Task GetResourceDetailsAsync(this WorkshopResourceModel model)
    {
      try
      {
        using (var reader = await WorkshopOp.ExecuteReaderAsync($"SELECT t_update,totalsize,count_download,author_name,content,b_gamebase,fromurl FROM res WHERE id={model.Id}"))
        {
          while (reader.Read())
          {
            model.LastChangeDate = Util.FromUnixTimestamp(reader.GetUInt64(0));
            model.TotalSize = reader.GetUInt64(1);
            model.DownloadCount = reader.GetUInt32(2);
            model.AuthorName = reader.GetString(3);
            model.Discription = reader.GetString(4);
            model.GameVersion = (GameVersion)reader.GetUInt32(5);
            model.SourceUrl = reader.GetString(6);
          }
        }
      }
      catch (MySqlException ex)
      {
        throw new InvalidOperationException(ex.ToString(), ex);
      }
    }

    public async static Task GetCookie()
    {
      //var response = await client.PostAsync("login.php", new FormUrlEncodedContent(new[] {
      //  new KeyValuePair<string,string>("pmd5", "10f0dce340bc2008d8e620ffce2537ca"),
      //  new KeyValuePair<string,string>("u","amt") }));
      // await client.GetStringAsync("res.php?action=ls&res=6");
    }

    public static async Task<List<ResourceFileModel>> GetResourceFilesAsync(uint workshopResourceid)
    {
      var ret = new List<ResourceFileModel>();
      using (var reader = await WorkshopOp.ExecuteReaderAsync($"SELECT id,PathFile(id),t_update,size FROM resfile WHERE resid={workshopResourceid}"))
      {
        while (reader.Read())
        {
          var file = new ResourceFileModel();
          file.Id = reader.GetUInt32(0);
          file.Path = reader.GetString(1);
          file.UpdateDate = Util.FromUnixTimestamp(reader.GetUInt64(2));
          file.Size = reader.GetUInt32(3);
          ret.Add(file);
        }
      }
      return ret;
    }

    public static void SaveFiles(this WorkshopResourceModel resource)
    {
      ConfigOp.ExecuteNonQueryTransaction(resource.Files.Select(f => $"INSERT INTO Files (Id,Size,Path,UpdateDate,Sha1,Status) vALUES({f.Id},{f.Size},'{f.Path}','{f.UpdateDate}','{f.Sha1}',{(int)f.Status});"));
    }

    public static List<FileChunkModel> LoadChunks(uint fileId)
    {
      var ret = new List<FileChunkModel>();
      using (var reader = ConfigOp.ExecuteReader($"SELECT Id FROM Chunks WHERE FileId={fileId}"))
      {
        while (reader.Read())
        {
          ret.Add(new FileChunkModel() { FileId = fileId, Id = reader.GetInt32(0) });
        }
      }
      return ret;
    }

    public static void SaveChunks(this ResourceFileModel fileModel)
    {
      ConfigOp.ExecuteNonQueryTransaction(fileModel.Chunks.Select(c => $"INSERT INTO Chunks (FileId,Id,Finished) VALUES({c.FileId},{c.Id},0)"));
    }

    public static async Task<byte[]> GetChunk(uint fileId, int chunkId)
    {
      var request = new HttpRequestMessage(HttpMethod.Get, $"res.php?action=ls&file={Util.UInt2CSID(fileId)}");
      request.Headers.Range = new RangeHeaderValue(chunkId * CHUNKSIZE, (chunkId + 1) * CHUNKSIZE - 1);
      return await (await client.SendAsync(request)).Content.ReadAsByteArrayAsync();
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
        CREATE TABLE IF NOT EXISTS Resources(Id int PRIMARY KEY,CreateDate text,LastChangeDate text,LastFileChangeDate text,Size int,Rating int,NumDownloads int,AuthorId int,AuthorName text,Discription text,GameVersion int,Url text,Type int,Status int);
        CREATE TABLE IF NOT EXISTS Files(Id int PRIMARY KEY,Size int,Path text,UpdateDate text,Sha1 text,Status int);
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

    private class JSON_Resource
    {
      public int id { get; set; }
      public int t_fileup { get; set; }
      public int t_update { get; set; }
      public int votereview { get; set; }
      public int votecomment { get; set; }
      public int count_download { get; set; }
    }
  }
}
