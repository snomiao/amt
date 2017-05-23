using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using System.IO;
using AutoMapper;

namespace YTY.amt.Model
{
  public static class ProgramModel
  {
    private static readonly string EXEPATH = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    private static readonly Lazy<ConfigModel> config = new Lazy<ConfigModel>(DatabaseClient.GetConfig);

    private static readonly Lazy<ObservableCollection<WorkshopResourceModel>> resources =
      new Lazy<ObservableCollection<WorkshopResourceModel>>(() =>
        {
          var ret = DatabaseClient.GetResources().ToList();
          foreach (var resource in ret)
          {
            if (resource.Status == WorkshopResourceStatus.Installing)
              resource.Status = WorkshopResourceStatus.Paused;
            resource.LocalLoadFiles();
          }
          return new ObservableCollection<WorkshopResourceModel>(ret);
        });

    private static readonly Lazy<ObservableCollection<ModResourceModel>> mods =
      new Lazy<ObservableCollection<ModResourceModel>>(
        () => new ObservableCollection<ModResourceModel>(Resources
          .OfType<ModResourceModel>()
          .Where(m => m.Status == WorkshopResourceStatus.Installed)));


    private static readonly Lazy<ObservableCollection<ModResourceModel>> games =
      new Lazy<ObservableCollection<ModResourceModel>>(() => new ObservableCollection<ModResourceModel>(
        ModResourceModel.BuiltInGames.Concat(Mods)));


    private static readonly Lazy<ObservableCollection<LanguageResourceModel>> languages =
      new Lazy<ObservableCollection<LanguageResourceModel>>(
        () => new ObservableCollection<LanguageResourceModel>(
          LanguageResourceModel.BuiltInLanguages.Concat(Resources
          .OfType<LanguageResourceModel>()
          .Where(m => m.Status == WorkshopResourceStatus.Installed)
          )));

    private static readonly Lazy<ObservableCollection<DrsResourceModel>> activeDrses =
      new Lazy<ObservableCollection<DrsResourceModel>>(() =>
        new ObservableCollection<DrsResourceModel>(Resources
          .OfType<DrsResourceModel>()
          .Where(d => d.Status == WorkshopResourceStatus.Installed && d.IsActivated)));



    private static Lazy<ObservableCollection<TauntResourceModel>> taunts =
      new Lazy<ObservableCollection<TauntResourceModel>>(
        () => new ObservableCollection<TauntResourceModel>(
          TauntResourceModel.BuiltInTaunts.Concat(Resources.OfType<TauntResourceModel>()
            .Where(t => t.Status == WorkshopResourceStatus.Installed))));

    public static ConfigModel Config => config.Value;

    public static ObservableCollection<WorkshopResourceModel> Resources => resources.Value;

    public static ObservableCollection<ModResourceModel> Mods => mods.Value;

    public static ObservableCollection<ModResourceModel> Games => games.Value;

    public static ObservableCollection<LanguageResourceModel> Languages => languages.Value;

    public static ObservableCollection<DrsResourceModel> ActiveDrses => activeDrses.Value;

    public static ObservableCollection<ToolGroupModel> ToolGroups { get; } =
      new ObservableCollection<ToolGroupModel>(ToolGroupModel.BuiltInToolGroups);

    public static ObservableCollection<TauntResourceModel> Taunts => taunts.Value;

    static ProgramModel()
    {
      ActiveDrses.CollectionChanged += ActiveDrses_CollectionChanged;
      Mods.CollectionChanged += Mods_CollectionChanged;
    }

    private static void Mods_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      var offset = ModResourceModel.BuiltInGames.Length;
      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Add:
          Games.Insert(e.NewStartingIndex + offset, (ModResourceModel)e.NewItems[0]);
          break;
        case NotifyCollectionChangedAction.Move:
          Games.Move(e.OldStartingIndex + offset, e.NewStartingIndex + offset);
          break;
        case NotifyCollectionChangedAction.Remove:
          Games.RemoveAt(e.OldStartingIndex + offset);
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
      return Path.Combine(Config.HawkempirePath, relativePath);
    }

    public static string MakeExeRelativePath(string relativePath)
    {
      return Path.Combine(EXEPATH, relativePath);
    }

    public static async Task UpdateResources()
    {
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
            case WorkshopResourceType.Taunt:
              resource = Mapper.Map<WorkshopResourceDto, TauntResourceModel>(dto);
              break;
            case WorkshopResourceType.Language:
              resource = Mapper.Map<WorkshopResourceDto, LanguageResourceModel>(dto);
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
