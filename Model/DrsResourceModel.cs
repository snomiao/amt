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
      UpdateIsActivated(true);
      ProgramModel.ActiveDrses.Insert(0,this);
      ApplyDrses();
    }

    public void Deactivate()
    {
      UpdateIsActivated(false);
      ProgramModel.ActiveDrses.Remove(this);
      ApplyDrses();
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

    public static void ApplyDrses()
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
          dic[drs.MapDrsName(drsName)].GetOrAdd((DrsTableClass)Array.IndexOf(drsTables, extension))[id] = File.ReadAllBytes(file.FullPathName);
        }
      }
      foreach (var pair in dic)
      {
        pair.Value.Save(ProgramModel.MakeHawkempirePath(GetDrsPath(pair.Key)));
      }
    }

    private void UpdateIsActivated(bool isActivated)
    {
      IsActivated = isActivated;
      DatabaseClient.SaveDrs(this);
    }

    private string MapDrsName(string drsName)
    {
      if (GameVersion == GameVersion.Aofe)
      {
        if (drsName.Equals("gamedata_x1.drs", StringComparison.InvariantCultureIgnoreCase))
        {
          return "gamedata_x1_fe.drs";
        }
        else if (drsName.Equals("gamedata_x1_p1.drs", StringComparison.InvariantCultureIgnoreCase))
        {
          return "gamedata_x1_p1_fe.drs";
        }
        else
        {
          return drsName;
        }
      }
      else if (GameVersion == GameVersion.WololoKingdoms)
      {
        if (drsName.Equals("gamedata_x1_p1.drs", StringComparison.InvariantCultureIgnoreCase))
        {
          return "gamedata_x1_p1_wk.drs";
        }
        else
        {
          return drsName;
        }
      }
      else
      {
        return drsName;
      }
    }

    private static string GetDrsPath(string drsName)
    {
      if (drsName.Equals("gamedata_x1_fe.drs", StringComparison.InvariantCultureIgnoreCase))
      {
        return @"games\forgotten empires\data\gamedata_x1.drs";
      }
      else if (drsName.Equals("gamedata_x1_p1_fe.drs", StringComparison.InvariantCultureIgnoreCase))
      {
        return @"games\forgotten empires\data\gamedata_x1_p1.drs";
      }
      else if (drsName.Equals("gamedata_x1_p1_wk.drs", StringComparison.InvariantCultureIgnoreCase))
      {
        return @"games\WololoKingdoms\data\gamedata_x1_p1.drs";
      }
      else
      {
        return $@"data\{drsName}";
      }
    }

    private static readonly string[] builtInDrsFiles =
    {
      "gamedata.drs",
      "gamedata_x1.drs",
      "gamedata_x1_fe.drs",
      "gamedata_x1_p1.drs",
      "gamedata_x1_p1_fe.drs",
      "gamedata_x1_p1_wk.drs",
      "gamedata_x1_wk.drs",
      "graphics.drs",
      "interfac.drs",
      "sounds.drs",
      "sounds_x1.drs",
      "terrain.drs",
    };


    private static readonly string[] drsTables =
    {
      "bina","shp","slp","wav",
    };
  }
}
