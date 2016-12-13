using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTY.amt
{
  public class DrsResourceModel : WorkshopResourceModel
  {
    public bool IsActivated
    {
      get { return Flags.HasFlag(WorkshopResourceFlag.Activated); }
      set
      {
        Flags = value ? WorkshopResourceFlag.Activated : WorkshopResourceFlag.Deactivated;
        OnPropertyChanged(nameof(IsActivated));
      }
    }

    public DrsResourceModel(int id) : base(id, WorkshopResourceType.Drs)
    {

    }

    public void Activate()
    {
      IsActivated = true;
      DAL.UpdateResourceFlags(Id, Flags);
    }

    public void Deactivate()
    {
      IsActivated = false;
      DAL.UpdateResourceFlags(Id, Flags);
    }
  }
}
