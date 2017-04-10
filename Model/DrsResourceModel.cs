using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YTY.amt.Model
{
  public class DrsResourceModel : WorkshopResourceModel
  {
    private bool isActivated;

    public bool IsActivated
    {
      get { return isActivated; }
      set
      {
        isActivated = value;
        OnPropertyChanged(nameof(IsActivated));
      }
    }

    public int Priority => ProgramModel.ActiveDrses.IndexOf(this);

    public bool CanIncrementPriority => Priority > 0;

    public bool CanDecrementPriority => -1 < Priority && Priority < ProgramModel.ActiveDrses.Count - 1;

    public void Activate()
    {
      IsActivated = true;
      ProgramModel.ActiveDrses.Add(this);
    }

    public void Deactivate()
    {
      IsActivated = false;
      ProgramModel.ActiveDrses.Remove(this);
    }

    public void IncrementPriority()
    {
      var index = Priority;
      ProgramModel.ActiveDrses.Move(index, index - 1);
    }

    public void DecrementPriority()
    {
      var index = Priority;
      ProgramModel.ActiveDrses.Move(index, index + 1);
    }

    public override void Delete()
    {
      Deactivate();
      base.Delete();
    }
  }
}
