using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using YTY;

namespace YTY.amt
{
  public class Config
  {
    private string fullPath;
    private XDocument xDoc;
    private Queue<UpdateItemViewModel> updateList = new Queue<UpdateItemViewModel>();

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

      (Application.Current.FindResource("UpdateServerViewModel") as UpdateServerViewModel).PropertyChanged += UpdateServerViewModel_PropertyChanged;
      LocalFiles = new ObservableCollection<UpdateItemViewModel>(Root.Elements("File").Select(ele => new UpdateItemViewModel(ele)));
      LocalFilesView = CollectionViewSource.GetDefaultView(LocalFiles);
      LocalFilesView.Filter = item => (item as UpdateItemViewModel).Status != UpdateItemStatus.Finished;
      EnableAutoSave();
    }

    private void UpdateServerViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      var usvm = sender as UpdateServerViewModel;
      if (e.PropertyName == "Status" && usvm.Status == UpdateServerStatus.NeedUpdate)
      {
        DisableAutoSave();
        foreach (var serverFile in usvm.ServerFiles)
        {
          var localFile = LocalFiles.FirstOrDefault(l => l.Id == serverFile.Id);
          if (localFile == null)
          {
            updateList.Enqueue(new UpdateItemViewModel(serverFile.Id, serverFile.SourceUri, serverFile.TargetPath)
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
              updateList.Enqueue(localFile);
            }
          }
        }
        EnableAutoSave();
      }
      foreach (var pendingItem in LocalFiles.Where(f => f.Status == UpdateItemStatus.Ready || f.Status == UpdateItemStatus.Downloading || f.Status == UpdateItemStatus.Error))
        updateList.Enqueue(pendingItem);

      try
      {
        var currentItem = updateList.Dequeue();
        currentItem.PropertyChanged += UpdateItemViewModel_PropertyChanged;
        currentItem.Start();
      }
      catch (InvalidOperationException) { }
    }

    private void UpdateItemViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      var uivm = sender as UpdateItemViewModel;
      if (e.PropertyName == "Status" && (uivm.Status == UpdateItemStatus.Finished))
      {
        try
        {
          var currentItem = updateList.Dequeue();
          currentItem.PropertyChanged += UpdateItemViewModel_PropertyChanged;
          currentItem.Start();
        }
        catch (InvalidOperationException) { }
      }
    }

    public void EnableAutoSave()
    {
      xDoc.Changed += XDoc_Changed;
      Save();
    }

    public void DisableAutoSave()
    {
      xDoc.Changed -= XDoc_Changed;
    }

    private void XDoc_Changed(object sender, XObjectChangeEventArgs e)
    {
      Save();
    }

    public void Save()
    {
      xDoc.Save(fullPath);
    }
  }
}
