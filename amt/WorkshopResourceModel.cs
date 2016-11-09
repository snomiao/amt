using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YTY.amt
{
  public class WorkshopResourceModel
  {
    public int Id { get; }

    public WorkshopResourceType Type { get; }

    public int FileSize { get; }

    public DateTime CreateDate { get; }

    public DateTime UpdateDate { get; }

    public string Name { get; }

    public int AuthorUid { get; }

    public string AuthorName { get; }

    public string Summary { get; }

    public string Intro { get; }

    public GameVersion GameVersion { get; }

    public double Rating { get; }

    public WorkshopResourceStatus Status { get; set; }


    public WorkshopResourceModel()
    {

    }

    public WorkshopResourceModel(string name,double rating,WorkshopResourceType type)
    {
      Name = name;
      Rating = rating;
      Type = type;
      Status = WorkshopResourceStatus.NotInstalled;
    }
  }

  public enum WorkshopResourceType
  {
    Drs,
    Campaign,
    Scenario,
    RandomMap,
    Replay,
    Mod,
    Ai
  }

  [Flags]
  public enum GameVersion
  {
    Aok,
    AocA,
    AocC,
    Aoc15,
    Aofe
  }

  public enum WorkshopResourceStatus
  {
    NotInstalled,
    Installing,
    Installed,
    Activated
  }
}
