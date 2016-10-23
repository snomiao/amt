using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Threading.Tasks;
using System.Timers;
using YTY;

namespace YTY.amt
{
  public class Config
  {
    private string fullPath;
    private XDocument xDoc;
    private Timer timer = new Timer(5000);

    public XElement Root => xDoc.Root;

    public int Build
    {
      get { return (int)Root.Element(nameof(Build)); }
      set { Root.Element(nameof(Build)).SetValue(value); }
    }

    public ObservableCollection<UpdateItemViewModel> LocalFiles { get; }

    public ICollectionView LocalFilesView { get; }

    public Config()
    {
      fullPath = Util.MakeQualifiedPath("amt.xml");
      if (File.Exists(fullPath))
      {
        xDoc = XDocument.Load(Util.MakeQualifiedPath(fullPath));
      }
      else
        xDoc = new XDocument(
          new XElement("amt",
          new XElement("Build", 0)));

      LocalFiles = new ObservableCollection<UpdateItemViewModel>(Root.Elements("File").Select(ele => new UpdateItemViewModel(ele)));
      LocalFilesView = CollectionViewSource.GetDefaultView(LocalFiles);
      LocalFilesView.Filter = item => (item as UpdateItemViewModel).Status != UpdateItemStatus.Finished;
      timer.Elapsed += (s, e) => Save();
      EnableAutoSave();
    }

    public async Task StartUpdate()
    {
      await GlobalVars.UpdateServerViewModel.GetUpdateSourcesAsync();
      if (GlobalVars.UpdateServerViewModel.Status == UpdateServerStatus.NeedUpdate)
      {
        DisableAutoSave();
        foreach (var serverFile in GlobalVars.UpdateServerViewModel.ServerFiles)
        {
          var localFile = LocalFiles.FirstOrDefault(l => l.Id == serverFile.Id);
          if (localFile == null)
          {
            LocalFiles.Add(new UpdateItemViewModel(serverFile.Id, serverFile.SourceUri, serverFile.TargetPath)
            {
              Size = serverFile.Size,
              Version = serverFile.Version.Clone() as Version,
              MD5 = serverFile.MD5
            });
          }
          else
          {
            if (serverFile.Version > localFile.Version)
            {
              localFile.Size = serverFile.Size;
              localFile.MD5 = serverFile.MD5;
              localFile.Version = serverFile.Version.Clone() as Version;
              localFile.Status = UpdateItemStatus.Ready;
              LocalFiles.Add(localFile);
            }
          }
        }
        EnableAutoSave();
      }
      foreach (var pendingItem in LocalFiles.Where(f => f.Status == UpdateItemStatus.Ready || f.Status == UpdateItemStatus.Downloading || f.Status == UpdateItemStatus.Error))
      {
        await pendingItem.StartAsync();
        LocalFilesView.Refresh();
      }
    }

    public void EnableAutoSave()
    {
      timer.Start();
      Save();
    }

    public void DisableAutoSave()
    {
      timer.Stop();
    }

    public void Save()
    {
      xDoc.Save(fullPath);
    }
  }
}
