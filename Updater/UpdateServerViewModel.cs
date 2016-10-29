using System;
using System.Xml.Linq;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.ObjectModel;

namespace YTY.amt
{
  public class UpdateServerViewModel : ViewModelBase
  {
    private const string SERVERURI = "http://www.hawkaoc.net/amt/UpdateSources.xml";

    private XElement xe;
    private UpdateServerStatus status;
    private List<ServerFile> files;

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

    public IEnumerable<ServerFile> ServerFiles => files;

    public UpdateServerViewModel()
    {
      files = new List<ServerFile>();
    }

    public async Task GetUpdateSourcesAsync()
    {
      Status = UpdateServerStatus.Getting;
      try
      {
        using (var wc = new WebClient())
        {
          using (var ms = new MemoryStream(await wc.DownloadDataTaskAsync(SERVERURI)))
          {
            xe = XElement.Load(ms);
            if (Build > GlobalVars.MainViewModel.Build)
            {
              foreach (var ele in xe.Elements("UpdateSource"))
              {
                using (var wc1 = new WebClient())
                {
                  using (var ms1 = new MemoryStream(await wc1.DownloadDataTaskAsync(ele.Value)))
                  {
                    var dir = ele.Value.Remove(ele.Value.LastIndexOf("/") + 1);
                    foreach (var file in XElement.Load(ms1).Elements("File"))
                      files.Add(new ServerFile()
                      {
                        Id = (int)file.Attribute("Id"),
                        SourceUri = new Uri(new Uri(dir), file.Element("Name").Value).ToString(),
                        TargetPath = file.Element("Name").Value,
                        Size = (long)file.Element("Size"),
                        Version = new Version(file.Element("Version").Value),
                        MD5 = file.Element("MD5").Value
                      });
                  }
                }
              }
              Status = UpdateServerStatus.NeedUpdate;
            }
            else
              Status = UpdateServerStatus.UpToDate;
          }
        }
      }
      catch (WebException ex)
      {
        if (ex.Status == WebExceptionStatus.ProtocolError)
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

  public class ServerFile
  {
    public int Id { get; set; }

    public string SourceUri { get; set; }

    public string TargetPath { get; set; }

    public long Size { get; set; }

    public Version Version { get; set; }

    public string MD5 { get; set; }
  }
}
