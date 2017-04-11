using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTY.amt.Model
{
  public class ModResourceModel : WorkshopResourceModel
  {
    public int Index => ProgramModel.Mods.IndexOf(this);

    public string ExePath { get; set; }

    protected override void AfterDownload()
    {
      ExePath = Files.First(f =>
        f.Path.StartsWith("age2_x1", StringComparison.InvariantCultureIgnoreCase) &&
        f.Path.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase)).Path;
      ProgramModel.Mods.Add(this);
    }
  }
}
