using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTY.amt
{
  public class FileViewModel
  {
    public FileModel Model { get; private set; }

    public static FileViewModel FromModel(FileModel model)
    {
      return new FileViewModel { Model = model };
    }
  }
}
