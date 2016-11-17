using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTY.amt
{
  public abstract class DrsTable:SortedDictionary<uint,byte[]>
  {
    public abstract string Signature { get; }
  }
}
