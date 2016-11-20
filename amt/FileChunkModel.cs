using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTY.amt
{
  public class FileChunkModel
  {
    public uint FileId { get; set; }

    public int Id { get; set; }

    public bool Finished { get; set; }

  }
}
