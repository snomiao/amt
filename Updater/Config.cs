using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using YTY;

namespace YTY.amt
{
  public class Config
  {
    private string fullPath;
    private XDocument xDoc;

    public XElement Root => xDoc.Root;

    public int Build
    {
      get { return (int)Root.Element(nameof(Build)); }
      set { Root.Element(nameof(Build)).SetValue(value); }
    }

    public IEnumerable<XElement> Files => Root.Elements("File");

    public ObservableCollection<DownloadTaskViewModel> DownloadTasks { get; }

    public Config()
    {
      fullPath = Util.MakeQualifiedPath("amt.xml");
      if (File.Exists(fullPath))
        xDoc = XDocument.Load(Util.MakeQualifiedPath(fullPath));
      else
        xDoc = new XDocument(
          new XElement("amt",
          new XElement("Build", 0)));

      (Application.Current.FindResource("UpdateServerViewModel") as UpdateServerViewModel).PropertyChanged += UpdateServerViewModel_PropertyChanged;
      xDoc.Changed += XDoc_Changed;
      DownloadTasks = new ObservableCollection<DownloadTaskViewModel>(Root.Elements(nameof(DownloadTaskViewModel)).Select(ele => new DownloadTaskViewModel(ele)));
    }

    private void UpdateServerViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      var usvm = sender as UpdateServerViewModel;
      if (e.PropertyName == "Status" && usvm.Status == UpdateServerStatus.NeedUpdate)
      {
        var updateList = usvm.Files.Where(f =>
 !GlobalVars.Config.Files.Any(l =>
 (int)(l.Attribute("Id")) == (int)(f.Item1.Attribute("Id"))) ||
 new Version(f.Item1.Element("Version").Value) >
 new Version(GlobalVars.Config.Files.First(l =>
 (int)(l.Attribute("Id")) == (int)(f.Item1.Attribute("Id"))).Element("Version").Value));
        foreach (var updateItem in updateList)
          DownloadTasks.Add(new DownloadTaskViewModel(updateItem.Item2, updateItem.Item1.Element("Name").Value));

      }
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
