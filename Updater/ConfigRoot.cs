using System.Linq;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using System.IO;

namespace YTY.amt
{
  internal static class ConfigRoot
  {
    private const string DEFAULT_ConfigFile = @"c:\1.xml";
    private static XDocument xDoc;

    internal static XElement Root { get { return xDoc.Root; } }

    internal static ObservableCollection<DownloadModel> DownloadTasks { get; }

    static ConfigRoot()
    {
      if (File.Exists(DEFAULT_ConfigFile))
        xDoc = XDocument.Load(DEFAULT_ConfigFile);
      else
        xDoc = new XDocument(new XElement("amt"));
      DownloadTasks = new ObservableCollection<DownloadModel>(xDoc.Elements("DownloadTask").Select(ele => new DownloadModel(ele)));
    }

    internal static void Save()
    {
      xDoc.Save(DEFAULT_ConfigFile);
    }
  }
}
