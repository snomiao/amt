using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
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
          .Where(m => m.Status == WorkshopResourceStatus.Installed)
          .Concat(new[]{
            new ModResourceModel{
              Id = -1,
          Name = "帝国时代Ⅱ 1.5",
          ExePath = @"exe\age2_x1.5.exe",},
         new ModResourceModel{
          Id = -2,
          Name = "帝国时代Ⅱ 1.0C",
          ExePath = @"exe\age2_x1.0c.exe",},
         new ModResourceModel{
          Id = -3,
          Name = "被遗忘的帝国",
          ExePath = @"exe\age2_x2.exe",},
         new ModResourceModel{
          Id = -4,
          Name = "WAIFor 触发扩展版",
          ExePath = @"exe\age2_wtep.exe"},})));

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
          .Where(d => d.Status == WorkshopResourceStatus.Installed && d.IsActivated)
          .OrderBy(d => d.Priority)));

    public static ConfigModel Config => config.Value;

    public static ObservableCollection<WorkshopResourceModel> Resources => resources.Value;

    public static ObservableCollection<ModResourceModel> Mods => mods.Value;

    public static ObservableCollection<LanguageResourceModel> Languages => languages.Value;

    public static ObservableCollection<DrsResourceModel> ActiveDrses => activeDrses.Value;

    public static string MakeHawkempirePath(string relativePath)
    {
      return System.IO.Path.Combine(Config.HawkempirePath, relativePath);
    }

    public static async Task UpdateResources()
    {
      Mapper.Initialize(cfg => cfg.CreateMap<WorkshopResourceDto, WorkshopResourceModel>());
      var (timestamp, dtos) = await WebServiceClient.GetUpdatedServerResourcesAsync();
      var toSave = new List<WorkshopResourceModel>();
      foreach (var dto in dtos)
      {
        var resource = Resources.FirstOrDefault(l => l.Id == dto.Id);
        if (resource == null)
        // resource does not exist locally
        {
          resource = Mapper.Map<WorkshopResourceDto, WorkshopResourceModel>(dto);
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
