using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YTY.amt
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

    public DrsResourceModel(int id) : base(id, WorkshopResourceType.Drs)
    {

    }

    public async override Task InstallAsync()
    {
      DAL.SaveDrsResource(Id);
      await base.InstallAsync();
    }

    public void Activate()
    {
      if (isActivated) return;
      IsActivated = true;
      Priority =  My.Drses.Count(d => d.IsActivated);
      DAL.UpdateDrsResource(this);
    }

    public void Deactivate()
    {
      if (!isActivated) return;
      IsActivated = false;
      foreach(var drs in My.Drses.Where(d=>d.Priority>Priority))
      {
        drs.Priority--;
        DAL.UpdateDrsResource(drs);
      }
      Priority = -1;
      DAL.UpdateDrsResource(this);
    }

    public override void Delete()
    {
      base.Delete();
      Deactivate();
      DAL.DeleteDrsResource(Id);
    }
  }
}
