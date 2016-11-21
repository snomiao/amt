using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows.Threading;
using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;

namespace YTY.amt
{
  public static class DAL
  {
    public const int CHUNKSIZE = 65536;
    private const string WORKSHOPSERVER = "http://121.42.152.168/ssrc/";
    private static IPEndPoint WORKSHOPDBSERVER = new IPEndPoint(IPAddress.Parse("121.42.152.168"), 3306);
    private const string WORKSHOPDBUSER = "amtclient";
    private const string WORKSHOPDBPASSWORD = "read@hawkempire";
    private const string WORKSHOPDBNAME = "ssrc";
    private const string CONFIGFILE = "config.db";
    private static Dictionary<string, WorkshopResourceType> dic_String_Type;
    private static MySqlOp WorkshopOp = new MySqlOp(WORKSHOPDBSERVER, WORKSHOPDBUSER, WORKSHOPDBPASSWORD, WORKSHOPDBNAME);
    private static SqliteOp ConfigOp = new SqliteOp(CONFIGFILE);
    private static HttpClientHandler handler = new HttpClientHandler();
    private static HttpClient client = new HttpClient(handler, false);

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
      client.BaseAddress = new Uri(WORKSHOPSERVER);
    }

    public static async Task<ObservableCollection<WorkshopResourceModel>> GetWorkshopResourcesAsync(IProgress<WorkshopResourceModel> progress)
    {
      var ret = new ObservableCollection<WorkshopResourceModel>();
      try
      {
        using (var reader = await WorkshopOp.ExecuteReaderAsync("SELECT id,name,votereview,e_type FROM res"))
        {
          while (await reader.ReadAsync())
          {
            var model = new WorkshopResourceModel(
              await reader.GetFieldValueAsync<uint>(0),
             await reader.GetFieldValueAsync<string>(1),
             await reader.GetFieldValueAsync<uint>(2),
             dic_String_Type[await reader.GetFieldValueAsync<string>(3)]);
            ret.Add(model);
            progress.Report(model);
          }
        }
      }
      catch (MySqlException ex)
      {
        throw new InvalidOperationException(ex.ToString(), ex);
      }
      return ret;
    }

    public static async Task GetResourceDetailsAsync(this WorkshopResourceModel model)
    {
      try
      {
        using (var reader = await WorkshopOp.ExecuteReaderAsync($"SELECT t_update,totalsize,count_download,author_name,content,b_gamebase,fromurl FROM res WHERE id={model.Id}"))
        {
          while (await reader.ReadAsync())
          {
            model.UpdateDate = Util.FromUnixTimestamp(await reader.GetFieldValueAsync<ulong>(0));
            model.TotalSize = await reader.GetFieldValueAsync<ulong>(1);
            model.DownloadCount = await reader.GetFieldValueAsync<uint>(2);
            model.AuthorName = await reader.GetFieldValueAsync<string>(3);
            model.Discription = await reader.GetFieldValueAsync<string>(4);
            model.GameVersion = (GameVersion)await reader.GetFieldValueAsync<uint>(5);
            model.SourceUrl = await reader.GetFieldValueAsync<string>(6);
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
        while (await reader.ReadAsync())
        {
          var file = new ResourceFileModel();
          file.Id = await reader.GetFieldValueAsync<uint>(0);
          file.Path = await reader.GetFieldValueAsync<string>(1);
          file.UpdateDate = Util.FromUnixTimestamp(await reader.GetFieldValueAsync<ulong>(2));
          file.Size = await reader.GetFieldValueAsync<uint>(3);
          ret.Add(file);
        }
      }
      return ret;
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

    public static async Task<byte[]> GetChunk(uint fileId,int chunkId)
    {
      var request = new HttpRequestMessage(HttpMethod.Get, $"res.php?action=ls&file={Util.UInt2CSID(fileId)}");
      request.Headers.Range = new RangeHeaderValue(chunkId * CHUNKSIZE, (chunkId + 1) * CHUNKSIZE - 1);
      return await (await client.SendAsync(request)).Content.ReadAsByteArrayAsync();
    }

    public static List<GameVersionModel> GetGameVersions()
    {
      var ret = new List<GameVersionModel>();
      ConfigOp.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS GameVersions(id int PRIMARY KEY,name text,exePath text)");
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

    public static void EnsureTablesExist()
    {
      ConfigOp.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS Config(key text,value text)");
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
