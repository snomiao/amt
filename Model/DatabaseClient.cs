﻿using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;
using Dapper;
using AutoMapper;

namespace YTY.amt.Model
{
  public static class DatabaseClient
  {
    private static readonly SQLiteConnectionStringBuilder connectionStringBuilder = new SQLiteConnectionStringBuilder
    {
      Pooling = true,
      DataSource = ProgramModel.MakeExeRelativePath(CONFIGFILE),
      JournalMode = SQLiteJournalModeEnum.Persist,
    };
    private const string CONFIGFILE = "config.db";

    static DatabaseClient()
    {
      Mapper.Initialize(cfg =>
      {
        cfg.CreateMap<WorkshopResourceDto, WorkshopResourceModel>();
        cfg.CreateMap<WorkshopResourceDto, DrsResourceModel>();
        cfg.CreateMap<WorkshopResourceDto, ModResourceModel>();
        cfg.CreateMap<WorkshopResourceDto, TauntResourceModel>();
        cfg.CreateMap<WorkshopResourceDto, LanguageResourceModel>();
        cfg.CreateMap<WorkshopResourceModel, WorkshopResourceDto>();
      });
      InitializeDatabase();
      UpdateSchema();
    }

    private static SQLiteConnection GetConnection()
    {
      return new SQLiteConnection(connectionStringBuilder.ConnectionString).OpenAndReturn();
    }

    private static SQLiteTransaction GetTransaction()
    {
      return GetConnection().BeginTransaction();
    }

    internal static ConfigModel GetConfig()
    {
      var ret = new ConfigModel();
      using (var connection = GetConnection())
      {
        var dic = connection.Query<KeyValuePair<string, string>>("SELECT `Key`,`Value` FROM `Config`").ToDictionary(q => q.Key, q => q.Value);
        ret.hawkempirePath = GetString(nameof(ConfigModel.HawkempirePath), (string)Util.GetAocRegistryValue("EXE Path") ?? string.Empty);
        ret.currentGame =
          ProgramModel.Games.First(m => m.Id == GetInt(nameof(ConfigModel.CurrentGame), -1));
        ret.allShown_Aoc15 = GetBool(nameof(ConfigModel.AllShown_Aoc15), false);
        ret.allShown_AocA = GetBool(nameof(ConfigModel.AllShown_AocA), false);
        ret.allShown_AocC = GetBool(nameof(ConfigModel.AllShown_AocC), false);
        ret.allShown_AoFE = GetBool(nameof(ConfigModel.AllShown_AoFE), false);
        ret.populationLimit = GetBool(nameof(ConfigModel.PopulationLimit), true);
        ret.multipleQueue = GetBool(nameof(ConfigModel.MultipleQueue), true);
        ret.currentLanguage = ProgramModel.Languages.First(l => l.Id == GetInt(nameof(ConfigModel.CurrentLanguage), -1));
        ret.splash = GetBool(nameof(ConfigModel.Splash), true);
        ret.resolution = GetSize(nameof(ConfigModel.Resolution), new Size(1366, 768));
        ret.backgroundMusic = GetBool(nameof(ConfigModel.BackgroundMusic), true);
        ret.isEnglishCampaignNarration = GetBool(nameof(ConfigModel.IsEnglishCampaignNarration), false);
        ret.workshopTimestamp = GetInt(nameof(ConfigModel.WorkshopTimestamp), 0);
        ret.currentTaunt = ProgramModel.Taunts.First(t => t.Id == GetInt(nameof(ConfigModel.CurrentTaunt), -2));
        ret.NumericAge = GetBool(nameof(ConfigModel.NumericAge), true);
        ret.hiddenCivs = GetBool(nameof(ConfigModel.HiddenCivs), false);
        ret.WkVersion = GetInt(nameof(ConfigModel.WkVersion), 0);
        ret.isSteamHDInstalled =(SteamHDInstallStatus) GetInt(nameof(ConfigModel.IsSteamHDInstalled), (int)SteamHDInstallStatus.NotInstalled);
        ret.steamHDInstallCheckDate = GetDateTime(nameof(ConfigModel.SteamHDInstallCheckDate), DateTime.Now);

        string GetString(string key, string defaultValue) => dic.TryGetValue(key, out var s) ? s : defaultValue;
        bool GetBool(string key, bool defaultValue) => dic.TryGetValue(key, out var s) && bool.TryParse(s, out var b) ? b : defaultValue;
        int GetInt(string key, int defaultValue)
          => dic.TryGetValue(key, out var s) ? int.TryParse(s, out var i) ? i : defaultValue : defaultValue;
        Size GetSize(string key, Size defaultValue) => dic.TryGetValue(key, out var s) ? Size.Parse(s) : defaultValue;
        DateTime GetDateTime(string key, DateTime defaultValue) => dic.TryGetValue(key, out var s) && DateTime.TryParse(s, out var d) ? d : defaultValue;
      }
      return ret;
    }

    private static Dictionary<string, string> configs;
    internal static Dictionary<string, string> GetConfigs()
    {
      if (configs == null)
      {
        using (var connection = GetConnection())
        {
          configs = connection.Query<KeyValuePair<string, string>>("SELECT `Key`,`Value` FROM `Config`").ToDictionary(q => q.Key, q => q.Value);
        }
      }
      return configs;
    }

    internal static void SaveConfigEntry(string key, object value)
    {
      using (var connection = GetConnection())
      {
        connection.Execute("INSERT OR REPLACE INTO `Config`(`Key`,`Value`) VALUES(@Key,@Value)", new KeyValuePair<string, string>(key, value.ToString()));
      }
    }

    public static IEnumerable<WorkshopResourceModel> GetResources()
    {
      using (var connection = GetConnection())
      {

        var dtos = connection.Query<WorkshopResourceDto>(@"
SELECT r.`Id`,r.`CreateDate`,r.`LastChangeDate`,r.`LastFileChangeDate`,r.`TotalSize`,r.`Rating`,r.`DownloadCount`,r.`AuthorId`,r.`AuthorName`,r.`Name`,r.`Description`,r.`GameVersion`,r.`SourceUrl`,r.`Type`,r.`Status`,r.`DeletePending`,
d.`IsActivated`,
m.`ExePath`,m.`XmlPath`,m.`FolderPath`
FROM `Resource` r
LEFT JOIN `Drs` d ON r.`Id`=d.`Id`
LEFT JOIN `Mod` m ON r.`Id`=m.`Id`
ORDER BY d.`Priority`,m.`Index`");

        foreach (var dto in dtos)
        {
          switch (dto.Type)
          {
            case WorkshopResourceType.Drs:
              yield return Mapper.Map<WorkshopResourceDto, DrsResourceModel>(dto);
              break;
            case WorkshopResourceType.Mod:
              yield return Mapper.Map<WorkshopResourceDto, ModResourceModel>(dto);
              break;
            case WorkshopResourceType.Language:
              yield return Mapper.Map<WorkshopResourceDto, LanguageResourceModel>(dto);
              break;
            case WorkshopResourceType.Taunt:
              yield return Mapper.Map<WorkshopResourceDto, TauntResourceModel>(dto);
              break;
            default:
              yield return Mapper.Map<WorkshopResourceDto, WorkshopResourceModel>(dto);
              break;
          }
        }
      }
    }


    public static void SaveResources(IEnumerable<WorkshopResourceModel> resources)
    {
      using (var transaction = GetTransaction())
      {
        transaction.Connection.Execute(@"
INSERT OR REPLACE INTO `Resource`(`Id`,`CreateDate`,`LastChangeDate`,`LastFileChangeDate`,`TotalSize`,`Rating`,`DownloadCount`,`AuthorId`,`AuthorName`,`Name`,`Description`,`GameVersion`,`SourceUrl`,`Type`,`Status`,`DeletePending`)
VALUES(@Id,@CreateDate,@LastFileChangeDate,@LastChangeDate,@TotalSize,@Rating,@DownloadCount,@AuthorId,@AuthorName,@Name,@Description,@GameVersion,@SourceUrl,@Type,@Status,@DeletePending)",
          Mapper.Map<IEnumerable<WorkshopResourceModel>, IEnumerable<WorkshopResourceDto>>(resources), transaction);
        transaction.Connection.Execute("INSERT OR REPLACE INTO `Drs`(`Id`) VALUES(@Id)",
           resources.OfType<DrsResourceModel>(), transaction);
        transaction.Connection.Execute("INSERT OR REPLACE INTO `Mod`(`Id`) VALUES(@Id)",
           resources.OfType<ModResourceModel>(), transaction);
        transaction.Commit();
      }
    }

    public static List<ResourceFileModel> GetResourceFiles(int resourceId)
    {
      using (var connection = GetConnection())
      {
        return
          connection.Query<ResourceFileModel>("SELECT `Id`,`UpdateDate`,`Size`,`Path`,`Sha1`,`Status` FROM `File` WHERE `ResourceId`=@resourceId",
            new { resourceId }).ToList();
      }
    }

    public static void SaveResourceFiles(IEnumerable<ResourceFileModel> models)
    {
      using (var transaction = GetTransaction())
      {
        transaction.Connection.Execute("INSERT OR REPLACE INTO `File`(`ResourceId`,`Id`,`Size`,`Path`,`UpdateDate`,`Sha1`,`Status`) VALUES(@ResourceId,@Id,@Size,@Path,@UpdateDate,@Sha1,@Status)", models, transaction);
        transaction.Commit();
      }
    }

    public static void UpdateResourceLastFileChange(int id, int lastFileChange)
    {
      using (var transaction = GetTransaction())
      {
        transaction.Connection.Execute("UPDATE `Resource` SET `LastFileChangeDate`=@lastFileChange WHERE `Id`=@id", new { lastFileChange, id }, transaction);
        transaction.Commit();
      }
    }

    public static void UpdateResourceStatus(int id, WorkshopResourceStatus status)
    {
      using (var transaction = GetTransaction())
      {
        transaction.Connection.Execute("UPDATE `Resource` SET `Status`=@status WHERE `Id`=@id", new { id, status }, transaction);
        transaction.Commit();
      }
    }

    public static void UpdateResourceFileStatus(int id, ResourceFileStatus status)
    {
      using (var transaction = GetTransaction())
      {
        transaction.Connection.Execute("UPDATE `File` SET `Status`=@status WHERE `Id`=@id", new { id, status }, transaction);
        transaction.Commit();
      }
    }

    public static void UpdateFileChunkFinished(int fileId, int id, bool finished)
    {
      using (var transaction = GetTransaction())
      {
        transaction.Connection.Execute("UPDATE `Chunk` SET `Finished`=@finished WHERE `FileId`=@fileId AND `Id`=@id",
           new { fileId, id, finished }, transaction);
        transaction.Commit();
      }
    }

    public static void DeleteFileChunks(int fileId)
    {
      using (var transaction = GetTransaction())
      {
        transaction.Connection.Execute("DELETE FROM `Chunk` WHERE `FileId`=@fileId", new { fileId }, transaction);
        transaction.Commit();
      }
    }

    public static IEnumerable<FileChunkModel> GetChunks(int fileId)
    {
      using (var connection = GetConnection())
      {
        return connection.Query<FileChunkModel>("SELECT `Id`,`Finished` FROM `Chunk` WHERE `FileId`=@fileId", new { fileId });
      }
    }

    public static void SaveChunks(IEnumerable<FileChunkModel> chunks)
    {
      using (var transaction = GetTransaction())
      {
        transaction.Connection.Execute("INSERT OR REPLACE INTO `Chunk`(`FileId`,`Id`,`Finished`) VALUES(@FileId,@Id,@Finished)", chunks, transaction);
        transaction.Commit();
      }
    }

    public static void DeleteResourceFiles(int resourceId)
    {
      using (var transaction = GetTransaction())
      {
        transaction.Connection.Execute("DELETE FROM `File` WHERE `ResourceId`=@resourceId", new { resourceId }, transaction);
        transaction.Commit();
      }
    }

    private static void InitializeDatabase()
    {
      using (var transaction = GetTransaction())
      {
        transaction.Connection.Execute(@"CREATE TABLE IF NOT EXISTS `Config`(
`Key` TEXT PRIMARY KEY,
`Value` TEXT NOT NULL)", transaction: transaction);
        transaction.Connection.Execute(@"CREATE TABLE IF NOT EXISTS `Resource`(
`Id` INTEGER PRIMARY KEY,
`CreateDate` INTEGER NOT NULL,
`LastChangeDate` INTEGER NOT NULL,
`LastFileChangeDate` INTEGER NOT NULL,
`TotalSize` INTEGER NOT NULL,
`Rating` INTEGER NOT NULL,
`DownloadCount` INTEGER NOT NULL,
`AuthorId` INTEGER NOT NULL,
`AuthorName` TEXT NOT NULL,
`Name` TEXT NOT NULL,
`Description` TEXT NOT NULL,
`GameVersion` INTEGER NOT NULL,
`SourceUrl` TEXT,
`Type` INTEGER NOT NULL,
`Status` INTEGER NOT NULL)", transaction: transaction);
        transaction.Connection.Execute(@"CREATE TABLE IF NOT EXISTS `File`(
`Id` INTEGER PRIMARY KEY,
`ResourceId` INTEGER NOT NULL,
`Size` INTEGER NOT NULL,
`Path` TEXT NOT NULL,
`UpdateDate` INTEGER NOT NULL,
`Sha1` TEXT NOT NULL,
`Status` INTEGER NOT NULL)", transaction: transaction);
        transaction.Connection.Execute(@"CREATE TABLE IF NOT EXISTS `Chunk`(
`Id` INTEGER NOT NULL,
`FileId` INTEGER NOT NULL,
`Finished` INTEGER NOT NULL,
PRIMARY KEY(`FileId`,`Id`))", transaction: transaction);
        transaction.Connection.Execute(@"CREATE TABLE IF NOT EXISTS `Mod`(
`Id` INTEGER PRIMARY KEY,
`Index` INTEGER NOT NULL DEFAULT -1,
`ExePath` TEXT NOT NULL DEFAULT '',
`XmlPath` TEXT NOT NULL DEFAULT '',
`FolderPath` TEXT NOT NULL DEFAULT '')", transaction: transaction);
        transaction.Connection.Execute(@"CREATE TABLE IF NOT EXISTS `Drs`(
`Id` INTEGER PRIMARY KEY,
`IsActivated` INTEGER NOT NULL DEFAULT 0,
`Priority` INTEGER NOT NULL DEFAULT -1)", transaction: transaction);
        transaction.Connection.Execute(@"CREATE TABLE IF NOT EXISTS `ToolGroup`(
`Id` INTEGER PRIMARY KEY,
`Name` TEXT NOT NULL)", transaction: transaction);
        transaction.Connection.Execute(@"CREATE TABLE IF NOT EXISTS `Tool`(
`Id` INTEGER PRIMARY KEY,
`Name` TEXT NOT NULL,
`Path` TEXT NOT NULL,
`IconPath` TEXT NOT NULL,
`ToolTip` TEXT NOT NULL)", transaction: transaction);
        transaction.Commit();
      }
    }

    private static void UpdateSchema()
    {
      using (var c = GetConnection())
      {
        var ver = c.ExecuteScalar<int>("PRAGMA user_version;");
        if (ver == 0)
        {
          ver = 1;
          c.Execute("ALTER TABLE `Resource` ADD COLUMN `DeletePending` INTEGER NOT NULL DEFAULT 0");
        }
       c.Execute($"PRAGMA user_version={ver}");
      }
    }

    public static void SaveDrses(IEnumerable<DrsResourceModel> drses)
    {
      using (var transaction = GetTransaction())
      {
        transaction.Connection.Execute("INSERT OR REPLACE INTO `Drs`(`Id`,`IsActivated`,`Priority`) VALUES(@Id,@IsActivated,@Priority)", drses, transaction);
        transaction.Commit();
      }
    }

    public static void SaveMods(IEnumerable<ModResourceModel> mods)
    {
      using (var transaction = GetTransaction())
      {
        transaction.Connection.Execute("INSERT OR REPLACE INTO `Mod`(`Id`,`Index`,`ExePath`,`XmlPath`,`FolderPath`) VALUES(@Id,@Index,@ExePath,@XmlPath,@FolderPath)", mods, transaction);
        transaction.Commit();
      }
    }

    public static void SaveDrs(DrsResourceModel drs)
    {
      using (var transaction = GetTransaction())
      {
        transaction.Connection.Execute("INSERT OR REPLACE INTO `Drs`(`Id`,`IsActivated`,`Priority`) VALUES(@Id,@IsActivated,@Priority)", drs, transaction);
        transaction.Commit();
      }
    }

    public static void DeleteFile(int id)
    {
      using (var transaction = GetTransaction())
      {
        transaction.Connection.Execute("DELETE FROM `File` WHERE `Id`=@id", new { id }, transaction);
        transaction.Commit();
      }
    }

    public static void DeleteResource(int id)
    {
      using (var transaction = GetTransaction())
      {
        transaction.Connection.Execute("DELETE FROM `Resource` WHERE `Id`=@id", new {id}, transaction);
        transaction.Commit();
      }
    }
  }
}
