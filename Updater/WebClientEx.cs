using System;
using System.Net;

namespace YTY
{
  public class WebClientEx : WebClient
  {
    private Tuple<int, int> fromToRange;
    private int? range;

    internal int Timeout { get; set; }

    internal void AddRange(int range)
    {
      this.range = range;
    }

    internal void AddRange(int from, int to)
    {
      fromToRange = Tuple.Create(from, to);
    }

    protected override WebRequest GetWebRequest(Uri address)
    {
      var req = base.GetWebRequest(address) as HttpWebRequest;
      req.Timeout = Timeout;
      if (fromToRange != null)
        req.AddRange(fromToRange.Item1, fromToRange.Item2);
      if (range.HasValue)
        req.AddRange(range.Value);
      return req;
    }
  }
}
