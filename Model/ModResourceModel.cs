using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Diagnostics;
using YTY.DrsLib;

namespace YTY.amt.Model
{
  public class ModResourceModel : WorkshopResourceModel
  {
    /// <summary>
    /// Dummy ID for age2_x1.0c.exe
    /// </summary>
    private const int AGE2_1C = -2;

    /// <summary>
    /// Dummy ID for age2_wk.exe
    /// </summary>
    private const int AGE2_WK = -3;

    private const int WK_VERSION = 10;

    private static readonly Regex regexXmlPath = new Regex(@"Games\\\w+\.xml", RegexOptions.IgnoreCase);
    private static readonly Regex regexExePath = new Regex(@"age2_x1\\\w+\.exe", RegexOptions.IgnoreCase);
    private static readonly string[] modOpenFolders_AGE2_1C ={
      "Campaign",
      "Scenario",
      "AI",
      "SaveGame",
      @"Sound\Scenario",
      "Data",
      "Random",
      "Screenshots",};
    private static readonly string[] modOpenFolders ={
      "Scenario",
      "Scenario",
      "Script.AI",
      "SaveGame",
      @"Sound\Scenario",
      "Data",
      "Script.RM",
      "Screenshots" };

    public int Index => ProgramModel.Mods.IndexOf(this);

    public string ExePath { get; set; }

    public string XmlPath { get; set; }

    public string FolderPath { get; set; }

    public bool CanMoveUp => Index > 0;

    public bool CanMoveDown => -1 < Index && Index < ProgramModel.Mods.Count - 1;

    protected override void AfterDownload()
    {
      try
      {
        XmlPath = Files.First(f => regexXmlPath.IsMatch(f.Path)).Path;
      }
      catch (InvalidOperationException)
      {
        throw new InvalidOperationException("该 MOD 缺少 XML 配置文件，本资源属于无效资源，请联系上传者。");
      }

      try
      {
        ExePath = Files.First(f => regexExePath.IsMatch(f.Path)).Path;
      }
      catch (InvalidOperationException)
      {
        throw new InvalidOperationException("该 MOD 缺少游戏主程序 EXE 文件，本资源属于无效资源，请联系上传者。");
      }

      var root = XElement.Load(ProgramModel.MakeHawkempirePath(XmlPath));
      FolderPath = @"Games\" + (root.Element("path")?.Value ?? throw new InvalidOperationException("该 MOD 的 XML 配置文件格式错误，本资源属于无效资源，请联系上传者。"));
      ProgramModel.Mods.Add(this);
    }

    public void MoveUp()
    {
      var index = Index;
      ProgramModel.Mods.Move(index, index - 1);
    }

    public void MoveDown()
    {
      var index = Index;
      ProgramModel.Mods.Move(index, index + 1);
    }

    public void OpenFolder(ModOpenFolder which)
    {
      var folder = ProgramModel.MakeHawkempirePath(Path.Combine(FolderPath,
        Id == AGE2_1C ? modOpenFolders_AGE2_1C[(int)which] : modOpenFolders[(int)which]));
      Directory.CreateDirectory(folder);
      Process.Start(folder);
    }

    public void CopyExe()
    {
      if (IsBuiltIn)
      {
        File.Copy(ProgramModel.MakeExeRelativePath(ExePath),
          ProgramModel.MakeHawkempirePath($@"age2_x1\{Path.GetFileName(ExePath)}"), true);
      }
      if (new[] { -1, -3, -4, -5, -6 }.Contains(Id))
      {
        File.Copy(ProgramModel.MakeExeRelativePath(XmlPath),
          Path.Combine(ProgramModel.MakeHawkempirePath("games"), Path.GetFileName(XmlPath)), true);
      }
      if (Id == AGE2_WK && WK_VERSION > ProgramModel.Config.WkVersion)
      {
        foreach (var dir in Directory.GetDirectories(ProgramModel.MakeExeRelativePath(@"builtin\WololoKingdoms"), "*",
          SearchOption.AllDirectories))
        {
          try
          {
            Directory.CreateDirectory(dir.Replace(ProgramModel.MakeExeRelativePath("builtin"),
              ProgramModel.MakeHawkempirePath("games")));
          }
          catch (IOException)
          {

          }
        }
        foreach (var file in Directory.GetFiles(ProgramModel.MakeExeRelativePath(@"builtin\WololoKingdoms"), "*",
          SearchOption.AllDirectories))
        {
          try
          {
            File.Copy(file,
              file.Replace(ProgramModel.MakeExeRelativePath("builtin"), ProgramModel.MakeHawkempirePath("games")),
              true);
          }
          catch (IOException)
          {

          }
        }
        try
        {
          Directory.CreateDirectory(ProgramModel.MakeHawkempirePath(@"games\WololoKingdoms\data"));
        }
        catch (IOException)
        {

        }
        try
        {
          File.Copy(ProgramModel.MakeExeRelativePath(@"dat\original\empires2_x1_p1_wk.dat"),
            ProgramModel.MakeHawkempirePath(@"games\WololoKingdoms\data\empires2_x1_p1.dat"), true);
        }
        catch (IOException)
        {

        }
        try
        {
          File.Copy(ProgramModel.MakeExeRelativePath(@"drs\gamedata_x1_wk.drs"),
            ProgramModel.MakeHawkempirePath(@"games\WololoKingdoms\data\gamedata_x1.drs"), true);
        }
        catch (IOException)
        {

        }
        try
        {
          File.Copy(ProgramModel.MakeExeRelativePath(@"drs\gamedata_x1_p1_wk.drs"),
            ProgramModel.MakeHawkempirePath(@"games\WololoKingdoms\data\gamedata_x1_p1.drs"), false);
        }
        catch (IOException)
        {
          // ignored
        }
        File.Copy(ProgramModel.MakeExeRelativePath(@"dll\zh\language_x1_p1_wk.dll"),
            ProgramModel.MakeHawkempirePath(@"games\WololoKingdoms\data\language_x1_p1.dll"), true);
        ProgramModel.Config.WkVersion = WK_VERSION;
      }
    }

    public void Run()
    {
      CopyExe();
      var exePath = ProgramModel.MakeHawkempirePath($@"age2_x1\{Path.GetFileName(ExePath)}");
      Process.Start(new ProcessStartInfo(exePath, ProgramModel.Config.Splash ? string.Empty : "nostartup")
      {
        WorkingDirectory = Path.GetDirectoryName(exePath),
        UseShellExecute = true,
      });
    }


    internal static readonly ModResourceModel[] BuiltInGames =
    {
      new ModResourceModel
      {
        Id = -1,
        Name = "帝国时代Ⅱ 1.5",
        ExePath = @"exe\age2_up.exe",
        FolderPath=@"Games\UserPatch",
        XmlPath=@"xml\age2_up.xml",
      },
      new ModResourceModel
      {
        Id = AGE2_1C,
        Name = "帝国时代Ⅱ 1.0C",
        ExePath = @"exe\age2_x1.0c.exe",
        FolderPath="",
        XmlPath="",
      },
      new ModResourceModel
      {
        Id = AGE2_WK,
        Name = "WololoKingdoms",
        ExePath = @"exe\age2_wk.exe",
        FolderPath=@"Games\WololoKingdoms",
        XmlPath=@"xml\age2_wk.xml",
      },
      new ModResourceModel
      {
        Id = -4,
        Name = "被遗忘的帝国",
        ExePath = @"exe\age2_x2.exe",
        FolderPath=@"Games\Forgotten Empires",
        XmlPath=@"xml\age2_x2.xml",
      },
      new ModResourceModel
      {
        Id = -5,
        Name = "WAIFor 触发扩展版",
        ExePath = @"exe\age2_w1.exe",
        FolderPath=@"Games\AGE2_W1",
        XmlPath=@"xml\age2_w1.xml",
      },
      new ModResourceModel
      {
        Id=-6,
        Name="触发补丁版",
        ExePath=@"exe\age2_etp.exe",
        FolderPath=@"Games\ETP",
        XmlPath=@"xml\age2_etp.xml",
      }
    };
  }

  public enum ModOpenFolder
  {
    Campaign,
    Scenario,
    Ai,
    SaveGame,
    ScenarioSound,
    Data,
    Rms,
    Screenshot,
  }
}
