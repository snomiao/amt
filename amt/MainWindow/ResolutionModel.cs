using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTY.amt
{
  public class ResolutionModel
  {
    public int X { get; set; }

    public int Y { get; set; }

    public override bool Equals(object obj)
    {
      if (obj == null) return false;
      var other = obj as ResolutionModel;
      if (other == null) return false;
      return X == other.X && Y == other.Y;
    }

    public override int GetHashCode()
    {
      return X ^ Y;
    }
  }
}
