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

    public bool CanMoveUp => Index > 0;

    public bool CanMoveDown => -1 < Index && Index < ProgramModel.Mods.Count - 1;

    protected override void AfterDownload()
    {
      ExePath = Files.First(f =>
        f.Path.StartsWith("age2_x1", StringComparison.InvariantCultureIgnoreCase) &&
        f.Path.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase)).Path;
      ProgramModel.Mods.Add(this);
    }

    public void MoveUp()
    {
      var index = Index;
      ProgramModel.Mods.Move(index, index - 1);
    }

    public void MoveDown()
    {
      var index = Index;
      ProgramModel.Mods.Move(index, index + 1);
    }
  }
}
