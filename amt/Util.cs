using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTY.amt
{
  public static class Util
  {
    private static DateTime UNIXTIMESTAMPBASE = new DateTime(1970, 1, 1, 0, 0, 0);

    public static DateTime FromUnixTimestamp(ulong unixTimestamp)
    {
      return UNIXTIMESTAMPBASE + TimeSpan.FromSeconds(unixTimestamp);
    }
  }
}
