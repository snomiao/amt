using System;
using System.Xml.Linq;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Collections.ObjectModel;

namespace YTY.amt
{
  public class UpdateServerViewModel : ViewModelBase
  {
    private const string SERVERURI = "http://www.hawkaoc.net/amt/UpdateSources.xml";

    private XElement xe;
    private UpdateServerStatus status;
    private List<Tuple<XElement, string>> files;

    public int Build => (int)xe.Attribute(nameof(Build));

    public UpdateServerStatus Status
    {
      get { return status; }
      set
      {
        status = value;
        OnPropertyChanged(nameof(Status));
      }
    }

    public IEnumerable<Tuple<XElement, string>> Files => files;

    public UpdateServerViewModel()
    {
      files = new List<Tuple<XElement, string>>();
      BeginGet();
    }

    public void BeginGet()
    {
      Status = UpdateServerStatus.Getting;
      using (var wc = new WebClient())
      {
        wc.DownloadDataCompleted += GetUpdateServerComplete;
        wc.DownloadDataAsync(new Uri(SERVERURI), wc);
      }
    }

    private void GetUpdateServerComplete(object sender, DownloadDataCompletedEventArgs e)
    {
      (e.UserState as WebClient).DownloadDataCompleted -= GetUpdateServerComplete;
      if (e.Error == null)
      {
        using (var ms = new MemoryStream(e.Result))
          xe = XElement.Load(ms);
        if (Build > GlobalVars.Config.Build)
        {
          ThreadPool.QueueUserWorkItem(state =>
          {
          try
          {
            foreach (var ele in xe.Elements("UpdateSource"))
            {
              using (var wc = new WebClient())
              {
                using (var ms = new MemoryStream(wc.DownloadData(ele.Value)))
                {
                    var dir = ele.Value.Remove(ele.Value.LastIndexOf("/"));
                  foreach (var file in XElement.Load(ms).Elements("File"))
                    files.Add(Tuple.Create(file,new Uri(new Uri( dir),file.Element("Name").Value).ToString()));
                }
              }
            }
              GlobalVars.Dispatcher.Invoke(new Action(() => Status = UpdateServerStatus.NeedUpdate));
            }
            catch (WebException)
            {
              Status = UpdateServerStatus.ServerError;
            }
          });
        }
        else
          Status = UpdateServerStatus.UpToDate;
      }
      else
      {
        if ((e.Error as WebException).Status == WebExceptionStatus.ProtocolError)
          Status = UpdateServerStatus.ServerError;
        else
          Status = UpdateServerStatus.ConnectFailed;
      }
    }
  }

  public enum UpdateServerStatus
  {
    Getting,
    NeedUpdate,
    UpToDate,
    ConnectFailed,
    ServerError
  }
}
