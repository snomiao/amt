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
    private int priority;

    public bool IsActivated
    {
      get { return isActivated; }
      set
      {
        isActivated = value;
        OnPropertyChanged(nameof(IsActivated));
      }
    }

    public int Priority
    {
      get { return priority; }
      set
      {
        priority = value;
        OnPropertyChanged(nameof(Priority));
      }
    }

    public bool CanIncrementPriority => Priority > 0;

    public bool CanDecrementPriority => -1 < Priority && Priority < ProgramModel.ActiveDrses.Count - 1;

    public void Activate()
    {
      IsActivated = true;
      ProgramModel.ActiveDrses.Add(this);
      Priority = ProgramModel.ActiveDrses.Count - 1;
      DatabaseClient.UpdateDrsResource(this);
    }

    public void Deactivate()
    {
      IsActivated = false;
      ProgramModel.ActiveDrses.Remove(this);
      //foreach (var drs in ProgramModel.Resources.OfType<DrsResourceModel>().Where(d => d.Priority > Priority))
      //{
      //  drs.Priority--;
      //  DatabaseClient.UpdateDrsResource(drs);
      //}
      Priority = -1;
      DatabaseClient.UpdateDrsResource(this);
    }

    public void IncrementPriority()
    {
      
    }

    public void DecrementPriority()
    {
      
    }

    public override void Delete()
    {
      Deactivate();
      base.Delete();
    }
  }
}
