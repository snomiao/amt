﻿using System;
using System.Xml.Linq;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;

namespace YTY.amt
{
  public class UpdateServerModel:INotifyPropertyChanged
  {
    private const string SERVERURI = "http://www.hawkaoe.net/amt/UpdateSources.xml";

    private XElement xe;
    private UpdateServerStatus status;
    private readonly List<FileDto> files = new List<FileDto>();

    public int Build => (int) xe.Attribute(nameof(Build));

    public UpdateServerStatus Status
    {
      get { return status; }
      set
      {
        status = value;
        OnPropertyChanged(nameof(Status));
      }
    }

    public IEnumerable<FileDto> ServerFiles => files;

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
            if (Build > ProgramModel.Build)
            {
              foreach (var ele in xe.Elements("UpdateSource"))
              {
                using (var wc1 = new WebClient())
                {
                  using (var ms1 = new MemoryStream(await wc1.DownloadDataTaskAsync(ele.Value)))
                  {
                    var dir = ele.Value.Remove(ele.Value.LastIndexOf("/") + 1);
                    foreach (var file in XElement.Load(ms1).Elements("File"))
                      files.Add(new FileDto
                      {
                        Id = (int) file.Attribute("Id"),
                        SourceUri = new Uri(new Uri(dir), file.Element("Name").Value).ToString(),
                        FileName = file.Element("Name").Value,
                        Size = (long) file.Element("Size"),
                        Version = (int) file.Element("Version"),
                        Md5 = file.Element("MD5").Value,
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

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
