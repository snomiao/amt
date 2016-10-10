using System.Linq;
using System.Xml.Linq;
using System.Collections.ObjectModel;

namespace YTY.amt
{
  internal static class ConfigRoot
  {
    internal static XDocument root;

    internal static ObservableCollection<DownloadTask> DownloadTasks { get; }

    static ConfigRoot()
    {
      root = XDocument.Load(@"c:\1.xml");
      DownloadTasks = new ObservableCollection<DownloadTask>(root.Elements("DownloadTask").Select(ele => new DownloadTask(ele)));
    }
  }
}
