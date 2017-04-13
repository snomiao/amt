using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTY.amt.Model
{
  public class TauntResourceModel : WorkshopResourceModel
  {
    private string Path { get; set; }

    public void Activate()
    {
      if (IsBuiltIn)
      {

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
        Path=@"Manager\taunt\zh",
      },
      new TauntResourceModel
      {
        Id=-1,
        Name="英文嘲讽音效",
        Path=@"Manager\taunt\en",
      },
    };
  }
}
