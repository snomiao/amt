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
using YTY;

namespace YTY.amt
{
  public class Config
  {
    public int Build
    {
      get { return GlobalVars.Dal.Build; }
      set { GlobalVars.Dal.Build = value; }
    }

    public ObservableCollection<UpdateItemViewModel> LocalFiles { get; private set; }

    public ICollectionView LocalFilesView { get; private set; }

    public Config()
    {
    }

    public void Init()
    {
      LocalFiles = new ObservableCollection<UpdateItemViewModel>(GlobalVars.Dal.GetUpdateItems());
      LocalFilesView = CollectionViewSource.GetDefaultView(LocalFiles);
      LocalFilesView.Filter = item => (item as UpdateItemViewModel).Status != UpdateItemStatus.Finished;
    }

    public async Task StartUpdate()
    {
      await GlobalVars.UpdateServerViewModel.GetUpdateSourcesAsync();
      if (GlobalVars.UpdateServerViewModel.Status == UpdateServerStatus.NeedUpdate)
      {
        var newUpdateItems = new List<UpdateItemViewModel>();
        foreach (var serverFile in GlobalVars.UpdateServerViewModel.ServerFiles)
        {
          var localFile = LocalFiles.FirstOrDefault(l => l.Id == serverFile.Id);
          if (localFile == null)
          {
            var newUpdateItem = new UpdateItemViewModel(serverFile.Id, serverFile.SourceUri, serverFile.TargetPath, serverFile.Size, serverFile.Version.Clone() as Version, serverFile.MD5, UpdateItemStatus.Ready, new ChunkViewModel[0]);
            newUpdateItems.Add(newUpdateItem);
            LocalFiles.Add(newUpdateItem);
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
        GlobalVars.Dal.SaveUpdateItems(newUpdateItems);
      }
      foreach (var pendingItem in LocalFiles.Where(f => f.Status == UpdateItemStatus.Ready || f.Status == UpdateItemStatus.Downloading || f.Status == UpdateItemStatus.Error))
      {
        await pendingItem.StartAsync();
        LocalFilesView.Refresh();
      }
    }
  }
}
