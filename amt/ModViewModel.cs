using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using YTY.amt.Model;

namespace YTY.amt
{
  public class ModViewModel : WorkshopResourceViewModel
  {
    private ModViewModel(ModResourceModel model) : base(model) { }

    public static ModViewModel FromModel(ModResourceModel model)
    {
      return new ModViewModel(model);
    }
  }
}
