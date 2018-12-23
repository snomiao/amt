using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace YTY.amt.Model
{
  public class TauntResourceModel : WorkshopResourceModel
  {
    private string Path { get; set; }

    public void Activate()
    {
      var tauntPath = ProgramModel.MakeHawkempirePath("taunt");
      if (IsBuiltIn)
      {
        foreach (var file in Directory.GetFiles(ProgramModel.MakeExeRelativePath(Path)))
        {
          File.Copy(file, System.IO.Path.Combine(tauntPath, System.IO.Path.GetFileName(file)), true);
        }
      }
      else
      {
        foreach (var file in Files)
        {
          File.Copy(file.FullPathName, System.IO.Path.Combine(tauntPath, System.IO.Path.GetFileName(file.Path)), true);
        }
      }
    }

    internal static TauntResourceModel[] BuiltInTaunts =
    {
      new TauntResourceModel
      {
        Id=-2,
        Name="中文嘲讽音效",
        Path=@"taunt\zh",
      },
      new TauntResourceModel
      {
        Id=-1,
        Name="英文嘲讽音效",
        Path=@"taunt\en",
      },
    };
  }
}
