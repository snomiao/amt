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

    private static readonly LanguageResourceModel[] builtInLanguages =
    {
      new LanguageResourceModel {Id = -1, Name = "中文"},
      new LanguageResourceModel {Id = -2, Name = "英语"},
    };

    private static readonly Lazy<ObservableCollection<LanguageResourceModel>> languages =
      new Lazy<ObservableCollection<LanguageResourceModel>>(
        () => new ObservableCollection<LanguageResourceModel>(
          builtInLanguages.Concat(Resources
          .OfType<LanguageResourceModel>()
          .Where(m => m.Status == WorkshopResourceStatus.Installed)
          )));

    private static readonly Lazy<ObservableCollection<DrsResourceModel>> activeDrses =
      new Lazy<ObservableCollection<DrsResourceModel>>(() =>
        new ObservableCollection<DrsResourceModel>(Resources
          .OfType<DrsResourceModel>()
          .Where(d => d.Status == WorkshopResourceStatus.Installed && d.IsActivated)));

    private static readonly ToolGroupModel[] builtInToolGroups =
    {
      new ToolGroupModel(new []
      {
        new ToolModel
        {
          Id=-1,
          Name ="触发工作室 1.0",
          Path ="",
          IconPath ="/resources;component/Resources/触发工作室.png",
          ToolTip ="打开【触发工作室】(Trigger Studio) 程序。\n该程序可以编辑帝国时代的 scx、scn 格式场景文件，并拥有地图编辑器以外的多种强大功能。",
        },
        new ToolModel
        {
          Id=-1,
          Name="触发工作室 1.2",
          Path="",
          IconPath="/resources;component/Resources/触发工作室2.png",
          ToolTip="打开【触发工作室】(Trigger Studio) 程序 1.2 版。\n该程序适用于编辑 UserPatch 1.4和 HD 版本的scx格式场景文件。",
        },
        new ToolModel
        {
          Id=-1,
          Name="战役管理器",
          Path="",
          IconPath="/resources;component/Resources/战役管理器.png",
          ToolTip="打开【战役管理器】程序。\n该程序可以将帝国时代的 CPX、CPN 格式战役文件分解为可在地图编辑器中编辑的 SCX、SCN 格式文件。",
        },
        new ToolModel
        {
          Id=-1,
          Name="单位ID修改器",
          Path="",
          IconPath="/resources;component/Resources/ID修改器.png",
          ToolTip="打开【单位ID修改器】内置工具。\n该程序可以修改帝国时代场景中单位的ID。",
        },
        new ToolModel
        {
          Id=-1,
          Name="场景翻译器",
          Path="",
          IconPath="/resources;component/Resources/修改器.png",
          ToolTip="打开【场景翻译器】内置工具。\n该程序可以用各种编码载入并翻译场景的信息、玩家名称和触发文本。",
        },
      })
      {
        Id=-4,
        Name ="战役与场景",
      },
      new ToolGroupModel(new[]
      {
        new ToolModel
        {
          Id=-1,
          Name="AI Editor",
          Path="",
          IconPath="/resources;component/Resources/AI-Editor.png",
          ToolTip="打开【AI Editor】程序。\n该程序可以编辑人工智能脚本，支持代码自动完成等多种拓展功能，内置建造城墙插件。内置AI Builder插件，无需输入，自动生成人工智能代码，且支持模板功能。",
        },
        new ToolModel
        {
          Id=-1,
          Name="AI Script Builder",
          Path="",
          IconPath="/resources;component/Resources/AI-Script-Builder.png",
          ToolTip="打开【AI Script Builder】程序。\n该程序可以通过设置向导生成人工智能脚本。内置建造城墙插件。",
        },
        new ToolModel
        {
          Id=-1,
          Name="SetupAI",
          Path="",
          IconPath="/resources;component/Resources/SetupAI.png",
          ToolTip="打开【SetupAI】程序（英文软件）。\n该程序可以通过填写一系列参数生成人工智能脚本。",
        },
        new ToolModel
        {
          Id=-1,
          Name="RMS 编辑器",
          Path="",
          IconPath="/resources;component/Resources/RMS编辑器.png",
          ToolTip="打开【RMS 编辑器】程序。\n该程序可以编辑随机地图脚本，内置自动创建向导，并可以根据已有语句模板进行修改。",
        },
        new ToolModel
        {
          Id=-1,
          Name="随机地图脚本生成器",
          Path="",
          IconPath="/resources;component/Resources/RMS编辑器3.png",
          ToolTip="打开【RMS 生成器】程序。\n该程序可以通过填写一系列参数生成随机地图脚本，并可以编辑脚本细节。",
        },
        new ToolModel
        {
          Id=-1,
          Name="UserPatch 1.3脚本编写参考",
          Path="",
          IconPath="/resources;component/Resources/RMS编辑器2.png",
          ToolTip="打开【UserPatch 1.3 脚本编写参考】Word 文档。\n该文档囊括了 UserPatch 1.3 相对于 1.0C 版在 AI 与随机地图脚本方面的改进。",
        },
      })
      {
        Id=-3,
        Name="AI与随机地图",
      },
      new ToolGroupModel(new []
      {
        new ToolModel
        {
          Id=-1,
          Name="乌龟包",
          Path="",
          IconPath="/resources;component/Resources/乌龟包.png",
          ToolTip="打开【乌龟包】程序。\n该程序是一套修改帝国时代游戏资料的软件组合，包含三个子程序SLP Editor 2.9.3，DRS Editor 1.6.3，Animation Preview 1.3。\nDRS 编辑器：可以修改游戏的DRS资料档。\nSLP 编辑器：可以修改游戏SLP图像。\n动画预览：可以替 SLP 图片的视觉效果进行评测。",
        },
        new ToolModel
        {
          Id=-1,
          Name="高级数据编辑器3.8",
          Path="",
          IconPath="/resources;component/Resources/AGE.png",
          ToolTip="打开【高级数据编辑器】(Advanced Genie Editor，AGE) 3.8 版。\n该程序可以修改帝国时代的数据库DAT文件。",
        },
        new ToolModel
        {
          Id=-1,
          Name="高级数据编辑器2015",
          Path="",
          IconPath="/resources;component/Resources/AGE2.png",
          ToolTip="打开【高级数据编辑器】(Advanced Genie Editor，AGE) 2015 版。\n该程序可以修改帝国时代的数据库DAT文件。",
        },
        new ToolModel
        {
          Id=-1,
          Name="语言DLL编辑器",
          Path="",
          IconPath="/resources;component/Resources/语言DLL.png",
          ToolTip="打开【语言DLL编辑器】内置工具。\n该程序可以修改帝国时代的语言DLL文件。",
        },
        new ToolModel
        {
          Id=-1,
          Name="单位ID对应表",
          Path="",
          IconPath="/resources;component/Resources/ID修改器.png",
          ToolTip="打开【单位 ID 对应表】Excel 文档。\n该表包括帝国时代单位名称、内部名称、单位ID。",
        },
      })
      {
        Id=-2,
        Name="MOD",
      },
      new ToolGroupModel(new []
      {
        new ToolModel
        {
          Id=-1,
          Name="录像管理器",
          Path="",
          IconPath="/resources;component/Resources/录像1.png",
          ToolTip="打开【录像浏览器】程序。\n本程序可以读取录像文件的玩家名称、民族、组队、聊天内容、地图等信息。",
        },
        new ToolModel
        {
          Id=-1,
          Name="录像分析器",
          Path="",
          IconPath="/resources;component/Resources/录像1.png",
          ToolTip="打开【录像分析器】程序。\n本程序可以解析录像文件的格式内容，分析录像文件中的部分信息，支持文件重命名功能，并可以自定命名格式。",
        },
        new ToolModel
        {
          Id=-1,
          Name="1.0C 数据修改器",
          Path="",
          IconPath="/resources;component/Resources/修改器.png",
          ToolTip="打开【1.0C 数据修改器】程序。\n本程序必须在帝国时代2主程序运行时才可以打开！\n本程序仅能应用于 1.0C 版！\n本程序通过修改内存，可以修改帝国时代2的实时运行数据，用于场景调试及单机游戏作弊。\n禁止在多人游戏中使用，会产生同步错误。",
        },
      })
      {
        Id=-1,
        Name="录像及其他",
      },
    };

    public static ConfigModel Config => config.Value;

    public static ObservableCollection<WorkshopResourceModel> Resources => resources.Value;

    public static ObservableCollection<ModResourceModel> Mods => mods.Value;

    public static ObservableCollection<ModResourceModel> Games => games.Value;

    public static ObservableCollection<LanguageResourceModel> Languages => languages.Value;

    public static ObservableCollection<DrsResourceModel> ActiveDrses => activeDrses.Value;

    public static ObservableCollection<ToolGroupModel> ToolGroups { get; } =
      new ObservableCollection<ToolGroupModel>(builtInToolGroups);

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
