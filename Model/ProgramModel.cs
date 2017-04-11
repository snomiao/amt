using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using AutoMapper;

namespace YTY.amt.Model
{
  public static class ProgramModel
  {
    private static readonly Lazy<ConfigModel> config = new Lazy<ConfigModel>(DatabaseClient.GetConfig);

    private static readonly Lazy<ObservableCollection<WorkshopResourceModel>> resources =
      new Lazy<ObservableCollection<WorkshopResourceModel>>(() =>
        {
          var ret = DatabaseClient.GetResources().ToList();
          foreach (var resource in ret)
          {
            if (resource.Status == WorkshopResourceStatus.Installing)
              resource.Status = WorkshopResourceStatus.Paused;
            if (resource.Status == WorkshopResourceStatus.Paused)
              resource.LocalLoadFiles();
          }
          return new ObservableCollection<WorkshopResourceModel>(ret);
        });

    private static readonly Lazy<ObservableCollection<ModResourceModel>> mods =
      new Lazy<ObservableCollection<ModResourceModel>>(
        () => new ObservableCollection<ModResourceModel>(Resources
          .OfType<ModResourceModel>()
          .Where(m => m.Status == WorkshopResourceStatus.Installed)));

    private static readonly ModResourceModel[] builtInGames =
    {
      new ModResourceModel
      {
        Id = -1,
        Name = "帝国时代Ⅱ 1.5",
        ExePath = @"exe\age2_x1.5.exe",
        FolderPath=@"Games\The Conquerors 1.4",
      },
      new ModResourceModel
      {
        Id = ModResourceModel.AGE2_1C,
        Name = "帝国时代Ⅱ 1.0C",
        ExePath = @"exe\age2_x1.0c.exe",
        FolderPath="",
      },
      new ModResourceModel
      {
        Id = -3,
        Name = "被遗忘的帝国",
        ExePath = @"exe\age2_x2.exe",
        FolderPath=@"Games\Forgotten Empires",
      },
      new ModResourceModel
      {
        Id = -4,
        Name = "WAIFor 触发扩展版",
        ExePath = @"exe\age2_wtep.exe",
        FolderPath=@"Games\The Conquerors 1.4",
      },
    };

    private static readonly Lazy<ObservableCollection<ModResourceModel>> games =
      new Lazy<ObservableCollection<ModResourceModel>>(() => new ObservableCollection<ModResourceModel>(
        builtInGames.Concat(Mods)));

    private static readonly Lazy<ObservableCollection<LanguageResourceModel>> languages =
      new Lazy<ObservableCollection<LanguageResourceModel>>(
        () => new ObservableCollection<LanguageResourceModel>(Resources.
          OfType<LanguageResourceModel>()
          .Where(m => m.Status == WorkshopResourceStatus.Installed)
          .Concat(new[]
          {
            new LanguageResourceModel {Id=-1, Name = "中文"},
            new LanguageResourceModel {Id=-2,Name = "英语"},
          })));

    private static readonly Lazy<ObservableCollection<DrsResourceModel>> activeDrses =
      new Lazy<ObservableCollection<DrsResourceModel>>(() =>
        new ObservableCollection<DrsResourceModel>(Resources
          .OfType<DrsResourceModel>()
          .Where(d => d.Status == WorkshopResourceStatus.Installed && d.IsActivated)));

    public static ConfigModel Config => config.Value;

    public static ObservableCollection<WorkshopResourceModel> Resources => resources.Value;

    public static ObservableCollection<ModResourceModel> Mods => mods.Value;

    public static ObservableCollection<ModResourceModel> Games => games.Value;

    public static ObservableCollection<LanguageResourceModel> Languages => languages.Value;

    public static ObservableCollection<DrsResourceModel> ActiveDrses => activeDrses.Value;

    static ProgramModel()
    {
      ActiveDrses.CollectionChanged += ActiveDrses_CollectionChanged;
      Mods.CollectionChanged += Mods_CollectionChanged;
    }

    private static void Mods_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Add:
          Games.Insert(e.NewStartingIndex + builtInGames.Length, (ModResourceModel)e.NewItems[0]);
          break;
        case NotifyCollectionChangedAction.Move:
          Games.Move(e.OldStartingIndex + builtInGames.Length, e.NewStartingIndex + builtInGames.Length);
          break;
        case NotifyCollectionChangedAction.Remove:
          Games.RemoveAt(e.OldStartingIndex + builtInGames.Length);
          break;
      }
      DatabaseClient.SaveMods(Mods);
    }

    private static void ActiveDrses_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      DatabaseClient.SaveDrses(ActiveDrses);
    }

    public static string MakeHawkempirePath(string relativePath)
    {
      return System.IO.Path.Combine(Config.HawkempirePath, relativePath);
    }

    public static async Task UpdateResources()
    {
      Mapper.Initialize(cfg =>
      {
        cfg.CreateMap<WorkshopResourceDto, WorkshopResourceModel>();
        cfg.CreateMap<WorkshopResourceDto, DrsResourceModel>();
        cfg.CreateMap<WorkshopResourceDto, ModResourceModel>();
      });
      var (timestamp, dtos) = await WebServiceClient.GetUpdatedServerResourcesAsync();
      var toSave = new List<WorkshopResourceModel>();
      foreach (var dto in dtos)
      {
        var resource = Resources.FirstOrDefault(l => l.Id == dto.Id);
        if (resource == null)
        // resource does not exist locally
        {
          switch (dto.Type)
          {
            case WorkshopResourceType.Drs:
              resource = Mapper.Map<WorkshopResourceDto, DrsResourceModel>(dto);
              break;
            case WorkshopResourceType.Mod:
              resource = Mapper.Map<WorkshopResourceDto, ModResourceModel>(dto);
              break;
            default:
              resource = Mapper.Map<WorkshopResourceDto, WorkshopResourceModel>(dto);
              break;
          }
          resource.Status = WorkshopResourceStatus.NotInstalled;
          Resources.Add(resource);
        }
        else
        // resource exists locally
        {
          if (dto.LastFileChangeDate > resource.LastFileChangeDate)
          // resource file list updated
          {
            resource.LastFileChangeDate = dto.LastFileChangeDate;
            resource.Status = WorkshopResourceStatus.NeedUpdate;
          }
          if (dto.LastChangeDate > resource.LastChangeDate)
          // resource metadata updated
          {
            resource.Rating = dto.Rating;
            resource.DownloadCount = dto.DownloadCount;
            resource.Name = dto.Name;
            resource.Description = dto.Description;
            resource.GameVersion = dto.GameVersion;
            resource.SourceUrl = dto.SourceUrl;
            resource.LastChangeDate = dto.LastChangeDate;
            if ((ResourceServerStatus)dto.Status == ResourceServerStatus.Deleted)
            // resource has been deleted from server
            {
              resource.Status = WorkshopResourceStatus.DeletePending;
            }
          }
        }
        toSave.Add(resource);
      }
      DatabaseClient.SaveResources(toSave);
      Config.WorkshopTimestamp = timestamp;
    }
  }
}
