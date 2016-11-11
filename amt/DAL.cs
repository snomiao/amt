using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows.Threading;

namespace YTY.amt
{
  public class DAL
  {
    private static Dictionary<string, WorkshopResourceType> dic_String_Type;

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
      var mysqlOp = new MySqlOp(new IPEndPoint(IPAddress.Parse("121.42.152.168"), 3306), "amtclient", "read@hawkempire", "ssrc");
      using (var reader = await mysqlOp.ExecuteReaderAsync("SELECT name,id,e_type FROM res"))
      {
        while (await reader.ReadAsync())
        {
          var model = new WorkshopResourceModel(
           await reader.GetFieldValueAsync<string>(0),
           await reader.GetFieldValueAsync<uint>(1),
           dic_String_Type[await reader.GetFieldValueAsync<string>(2)]);
          ret.Add(model);
          progress.Report(model);
          await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
        }
      }
      return ret;
    }
  }
}
