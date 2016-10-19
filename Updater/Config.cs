using System.Linq;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using System.IO;

namespace YTY.amt
{
  public static class Config
  {
    private const string DEFAULT_ConfigFile = @"c:\1.xml";
    private static XDocument xDoc;

    public static XElement Root { get { return xDoc.Root; } }

    public static ObservableCollection<DownloadTaskViewModel> DownloadTasks { get; }

    static Config()
    {
      if (File.Exists(DEFAULT_ConfigFile))
        xDoc = XDocument.Load(DEFAULT_ConfigFile);
      else
        xDoc = new XDocument(new XElement("amt"));

      DownloadTasks = new ObservableCollection<DownloadTaskViewModel>(Root.Elements(nameof(DownloadTaskViewModel)).Select(ele => new DownloadTaskViewModel(ele)));
      xDoc.Changed += XDoc_Changed;
    }

    private static void XDoc_Changed(object sender, XObjectChangeEventArgs e)
    {
      Save();
    }

    public static void Save()
    {
      xDoc.Save(DEFAULT_ConfigFile);
    }
  }
}
