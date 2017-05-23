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
      if (IsBuiltIn)
      {
        var tauntPath = ProgramModel.MakeHawkempirePath("taunt");
        foreach (var file in Directory.GetFiles(ProgramModel.MakeExeRelativePath(Path)))
        {
          File.Copy(file, System.IO.Path.Combine(tauntPath, System.IO.Path.GetFileName(file)), true);
        }
      }
      else
      {

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
