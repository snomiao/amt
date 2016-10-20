using System;
using System.Xml.Linq;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace YTY.amt
{
  public class UpdateServerViewModel : ViewModelBase
  {
    private const string SERVERURI = "http://www.hawkaoc.net/amt/UpdateSources.xml";

    private XDocument xdoc;
    private UpdateServerStatus status;
    private ObservableCollection<UpdateSourceViewModel> updateSources;

    public UpdateServerStatus Status
    {
      get { return status; }
      set
      {
        status = value;
        OnPropertyChanged(nameof(Status));
      }
    }

    public ObservableCollection<UpdateSourceViewModel> UpdateSources { get { return updateSources; } }

    public UpdateServerViewModel()
    {
      updateSources = new ObservableCollection<UpdateSourceViewModel>();
      BeginGet();
    }

    public void BeginGet()
    {
      Status = UpdateServerStatus.Getting;
      using (var wc = new WebClient())
      {
        wc.DownloadDataCompleted += (s, e) =>
          {
            if (e.Error == null)
            {
              xdoc = XDocument.Load(new MemoryStream(e.Result));
              foreach (var ele in xdoc.Root.Elements("UpdateSource"))
              {
                updateSources.Add(new UpdateSourceViewModel(ele));
              }
              Status = UpdateServerStatus.Ready;
            }
            else
            {
              if ((e.Error as WebException).Status == WebExceptionStatus.ProtocolError)
                Status = UpdateServerStatus.ServerError;
              else
                Status = UpdateServerStatus.ConnectFailed;
            }
          };
        wc.DownloadDataAsync(new Uri(SERVERURI));
      }
    }
  }

  public enum UpdateServerStatus
  {
    Getting,
    Ready,
    ConnectFailed,
    ServerError
  }
}
