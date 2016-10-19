using System.Xml.Linq;
using System.Xml;
using System.Diagnostics;
using WebException = System.Net.WebException;

namespace YTY.amt
{
  public class UpdateServer
  {
    private const string SERVERURI = "http://www.hawkaoc.net/amt/UpdateSources.xml";

    private XDocument xdoc;

    public void Init()
    {
      try
      {
        xdoc = XDocument.Load(new XmlTextReader(SERVERURI));
      }
      catch (WebException)
      {
        throw;
      }
    }
  }
}
