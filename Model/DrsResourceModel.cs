using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YTY.DrsLib;

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
      ApplyDrses();
    }

    public void Deactivate()
    {
      IsActivated = false;
      ProgramModel.ActiveDrses.Remove(this);
      ApplyDrses();
    }

    public void IncrementPriority()
    {
      var index = Priority;
      ProgramModel.ActiveDrses.Move(index, index - 1);
      ApplyDrses();
    }

    public void DecrementPriority()
    {
      var index = Priority;
      ProgramModel.ActiveDrses.Move(index, index + 1);
      ApplyDrses();
    }

    public override void Delete()
    {
      if (IsActivated)
      {
        try
        {
          Deactivate();
        }
        catch (IOException) { }
      }
      base.Delete();
    }

    private void ApplyDrses()
    {
      var dic = builtInDrsFiles.ToDictionary(f => f,
        f => DrsFile.Load(Path.Combine(ProgramModel.MakeExeRelativePath("drs"), f)));
      foreach (var drs in ProgramModel.ActiveDrses.Reverse())
      {
        foreach (var file in drs.Files)
        {
          var extension = Path.GetExtension(file.Path).TrimStart('.').ToLowerInvariant();
          var id = int.Parse(Path.GetFileNameWithoutExtension(file.Path));
          var drsName = Path.GetFileName(Path.GetDirectoryName(file.Path)).ToLowerInvariant();
          dic[drsName][(DrsTableClass)Array.IndexOf(drsTables, extension)][id] = File.ReadAllBytes(ProgramModel.MakeHawkempirePath(file.Path));
        }
      }
      foreach (var pair in dic)
      {
        pair.Value.Save(Path.Combine(ProgramModel.MakeHawkempirePath("Data"), pair.Key));
      }
    }

    private static readonly string[] builtInDrsFiles =
    {
      "gamedata.drs",
      "graphics.drs",
      "interfac.drs",
      "sounds.drs",
      "terrain.drs",
    };


    private static readonly string[] drsTables =
    {
      "bina","shp","slp","wav",
    };
  }
}
