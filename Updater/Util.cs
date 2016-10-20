using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace YTY
{
  public static class Util
  {
    private static string thisExeFullPath;

    static Util()
    {
      thisExeFullPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
      
    }
      
    public static string MakeQualifiedPath(string relativePath)
    {
      return Path.Combine(thisExeFullPath, relativePath);
    }

  }
}
