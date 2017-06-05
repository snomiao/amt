using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vestris.ResourceLib;

namespace YTY.amt.Model
{
  public class LanguageIniToDll
  {
    public static LanguageIniToDll LoadIni(string fileName)
    {
      var ri = new ResourceInfo();
      ri.Load(fileName);
      //((StringResource) ri[  Kernel32.ResourceTypes.RT_STRING]).
      return new LanguageIniToDll();
    }


  }
}
