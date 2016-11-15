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

namespace YTY.amt
{
  public static class DAL
  {
    private static IPEndPoint WORKSHOPDBSERVER = new IPEndPoint(IPAddress.Parse("121.42.152.168"), 3306);
    private const string WORKSHOPDBUSER = "amtclient";
    private const string WORKSHOPDBPASSWORD = "read@hawkempire";
    private const string WORKSHOPDBNAME = "ssrc";
    private const string CONFIGFILE = "config.db";
    private static Dictionary<string, WorkshopResourceType> dic_String_Type;
    private static MySqlOp WorkshopOp = new MySqlOp(WORKSHOPDBSERVER, WORKSHOPDBUSER, WORKSHOPDBPASSWORD, WORKSHOPDBNAME);
    private static SqliteOp ConfigOp = new SqliteOp(CONFIGFILE);

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

    public static async Task GetDetailsAsync(this WorkshopResourceViewModel viewModel)
    {
      try
      {
        using (var reader = await WorkshopOp.ExecuteReaderAsync($"SELECT t_update,totalsize,count_download,author_name,content,b_gamebase,fromurl FROM res WHERE id={viewModel.Model.Id}"))
        {
          while (await reader.ReadAsync())
          {
            viewModel.Model.UpdateDate = Util.FromUnixTimestamp(await reader.GetFieldValueAsync<ulong>(0));
            viewModel.Model.TotalSize = await reader.GetFieldValueAsync<ulong>(1);
            viewModel.Model.DownloadCount = await reader.GetFieldValueAsync<uint>(2);
            viewModel.Model.AuthorName = await reader.GetFieldValueAsync<string>(3);
            viewModel.Model.Discription = await reader.GetFieldValueAsync<string>(4);
            viewModel.Model.GameVersion = (GameVersion)await reader.GetFieldValueAsync<uint>(5);
            viewModel.Model.SourceUrl = await reader.GetFieldValueAsync<string>(6);
          }
        }
      }
      catch (MySqlException ex)
      {
        throw new InvalidOperationException(ex.ToString(), ex);
      }
    }

    public static ConfigModel GetConfig()
    {
      try
      {
        ConfigOp.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS Config(hawkempirePath text,currentGameVersion int)");
        using (var reader = ConfigOp.ExecuteReader("SELECT hawkempirePath,currentGameVersion FROM Config"))
        {
          if(reader.Read())
          {
            return new ConfigModel(reader.GetString(0),
              reader.GetInt32(1));
          }
          else
            ConfigOp.ExecuteNonQuery("INSERT INTO Config VALUES('',-1)");
        }
      }
      catch (DbException ex)
      {
        throw new InvalidOperationException(ex.ToString(), ex);
      }
      return new ConfigModel("",-1);
    }

    public static void SaveHawkempirePath(ConfigModel config)
    {
      ConfigOp.ExecuteNonQuery($"UPDATE Config SET hawkempirePath='{config.HawkempirePath}'");
    }

    public static void SaveCurrentGameVersion(ConfigModel config )
    {
      ConfigOp.ExecuteNonQuery($"UPDATE Config SET currentGameVersion={config.CurrentGameVersion}");
    }
  }
}
