using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Threading;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using YTY.amt.Model;

namespace YTY.amt
{
  public static class Commands
  {
    public static ICommand ActivateDrs { get; } = new ActivateDrsCommand();

    public static ICommand DeactivateDrs { get; } = new DeactivateDrsCommand();

    public static ICommand MoveUpDrs { get; } = new MoveUpDrsCommand();

    public static ICommand MoveDownDrs { get; } = new MoveDownDrsCommand();

    public static ICommand MoveUpMod { get; } = new MoveUpModCommand();

    public static ICommand MoveDownMod { get; } = new MoveDownModCommand();

    public static ICommand SwitchAndRunGame { get; } = new SwitchAndRunGameCommand();

    public static ICommand CloseWindow { get; } = new CloseWindowCommand();

    public static ICommand MinimizeWindow { get; } = new MinimizeWindowCommand();

    public static ICommand OpenFolder { get; } = new OpenFolderCommand();

    public static ICommand FilterResourceByType { get; } = new FilterResourceByTypeCommand();

    public static ICommand FilterResourceByContent { get; } = new FilterResourceByContentCommand();

    public static ICommand InstallResource { get; } = new InstallResourceCommand();

    public static ICommand UpdateResource { get; } = new UpdateResourceCommand();

    public static ICommand PauseResource { get; } = new PauseResourceCommand();

    public static ICommand ResumeResource { get; } = new ResumeResourceCommand();

    public static ICommand DeleteResource { get; } = new DeleteResourceCommand();

    public static ICommand ShowSelectedResourceView { get; } = new ShowSelectedResourceViewCommand();

    public static ICommand CreateProcessRelativePath { get; } = new CreateProcessRelativePathCommand();

    public static ICommand Hyperlink { get; } = new HyperlinkCommand();

    public static ICommand OpenTool { get; } = new OpenToolCommand();

    public static ICommand ApplyHotkey { get; } = new ApplyHotkeyCommand();

    public static ICommand CheckUpdate { get; } = new CheckUpdateCommand();

    public static ICommand GenerateDllFromIni { get; } = new GenerateDllFromIniCommand();

    public static ICommand ApplyDrses { get; } = new ApplyDrsesCommand();

    private class ActivateDrsCommand : ICommand
    {
      public event EventHandler CanExecuteChanged;

      public bool CanExecute(object parameter)
      {
        return true;
      }

      public void Execute(object parameter)
      {
        var model = (DrsResourceModel)parameter;
        try
        {
          model.Activate();
        }
        catch (IOException ex)
        {
          MessageBox.Show(ex.Message);
        }
      }
    }

    private class DeactivateDrsCommand : ICommand
    {
      public event EventHandler CanExecuteChanged;

      public bool CanExecute(object parameter)
      {
        return true;
      }

      public void Execute(object parameter)
      {
        var model = (DrsResourceModel)parameter;
        try
        {
          model.Deactivate();
        }
        catch (IOException ex)
        {
          MessageBox.Show(ex.Message);
        }
      }
    }

    private class MoveUpDrsCommand : ICommand
    {
      public bool CanExecute(object parameter)
      {
        var drs = parameter as DrsResourceModel;
        return drs?.CanIncrementPriority ?? false;
      }

      public void Execute(object parameter)
      {
        var drs = (DrsResourceModel)parameter;
        try
        {
          drs.IncrementPriority();
        }
        catch (IOException ex)
        {
          MessageBox.Show(ex.Message);
        }
      }

      public event EventHandler CanExecuteChanged
      {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
      }
    }

    private class MoveDownDrsCommand : ICommand
    {
      public bool CanExecute(object parameter)
      {
        var drs = parameter as DrsResourceModel;
        return drs?.CanDecrementPriority ?? false;
      }

      public void Execute(object parameter)
      {
        var drs = (DrsResourceModel)parameter;
        try
        {
          drs.DecrementPriority();
        }
        catch (IOException ex)
        {
          MessageBox.Show(ex.Message);
        }
      }

      public event EventHandler CanExecuteChanged
      {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
      }
    }

    private class MoveUpModCommand : ICommand
    {
      public bool CanExecute(object parameter)
      {
        var mod = parameter as ModResourceModel;
        return mod?.CanMoveUp ?? false;
      }

      public void Execute(object parameter)
      {
        var mod = parameter as ModResourceModel;
        mod.MoveUp();
      }

      public event EventHandler CanExecuteChanged
      {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
      }
    }

    private class MoveDownModCommand : ICommand
    {
      public bool CanExecute(object parameter)
      {
        var mod = parameter as ModResourceModel;
        return mod?.CanMoveDown ?? false;
      }

      public void Execute(object parameter)
      {
        var mod = parameter as ModResourceModel;
        mod.MoveDown();
      }

      public event EventHandler CanExecuteChanged
      {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
      }
    }

    private class SwitchAndRunGameCommand : ICommand
    {
      public event EventHandler CanExecuteChanged;

      public bool CanExecute(object parameter)
      {
        return true;
      }

      public void Execute(object parameter)
      {
        var game = parameter as ModResourceModel;
        if(game.Id==ModResourceModel.AGE2_WK)
        {
          if(ProgramModel.Config.IsSteamHDInstalled== SteamHDInstallStatus.NotInstalled
            ||(ProgramModel.Config.IsSteamHDInstalled== SteamHDInstallStatus.Trial
            &&(DateTime.Now- ProgramModel.Config.SteamHDInstallCheckDate).TotalDays>=30))
          {
            if(MessageBox.Show("若你没有帝国时代 2 HD版，你将无法通过翔鹰帝国管家更新与启动 WololoKingdoms。\n\n你是否已经购买帝国时代2 HD版？", "正版验证", MessageBoxButton.YesNo,MessageBoxImage.Question)== MessageBoxResult.Yes)
            {
              if (Util.IsAGE2HDInstalled())
              {
                ProgramModel.Config.IsSteamHDInstalled =  SteamHDInstallStatus.Permanent;
              }
              else
              {
                MessageBox.Show("管家无法找到你购买帝国时代2 HD版的记录，请确定你已经正确在电脑上安装Steam平台与帝国时代2 HD版。","验证失败",MessageBoxButton.OK, MessageBoxImage.Warning);
                Promise();
              }
            }
            else
            {
              if (Util.IsAGE2HDInstalled())
              {
                MessageBox.Show("管家检查到你已经购买帝国时代2 HD版，管家仍然会提供 WololoKingdoms 版本的更新与启动服务。", "验证成功", MessageBoxButton.OK, MessageBoxImage.Information);
                ProgramModel.Config.IsSteamHDInstalled =  SteamHDInstallStatus.Permanent;
              }
              else
              {
                Promise();
              }
            }
          }
        }

        try
        {
          ProgramModel.Config.CurrentGame = game;
          game.Run();
        }
        catch (IOException ex)
        {
          MessageBox.Show(ex.Message);
        }
      }

      void Promise()
      {
        if (MessageBox.Show("如果你打算在未来30天购买帝国时代2 HD版，管家仍然能暂时提供相关服务充当其试玩版体验。\n期限到达之后，管家将会再次自动进行这次检测。\n\n你是否承诺会在未来30天内购买帝国时代2 HD版？", "询问", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
          ProgramModel.Config.IsSteamHDInstalled =  SteamHDInstallStatus.Trial;
          ProgramModel.Config.SteamHDInstallCheckDate = DateTime.Now;
        }
        else
        {
          ProgramModel.Config.IsSteamHDInstalled = SteamHDInstallStatus.NotInstalled;
        }
      }
    }

    private class CloseWindowCommand : ICommand
    {
      public event EventHandler CanExecuteChanged;

      public bool CanExecute(object parameter)
      {
        return true;
      }

      public void Execute(object parameter)
      {
        (parameter as Window).Close();
      }
    }

    private class MinimizeWindowCommand : ICommand
    {
      public event EventHandler CanExecuteChanged;

      public bool CanExecute(object parameter)
      {
        return true;
      }

      public void Execute(object parameter)
      {
        (parameter as Window).WindowState = WindowState.Minimized;
      }
    }

    private class OpenFolderCommand : ICommand
    {
      public event EventHandler CanExecuteChanged;

      public bool CanExecute(object parameter)
      {
        return true;
      }

      public void Execute(object parameter)
      {
        Enum.TryParse<ModOpenFolder>((string)parameter, out var folder);
        ProgramModel.Config.CurrentGame.OpenFolder(folder);
      }
    }

    private class FilterResourceByTypeCommand : ICommand
    {
      public event EventHandler CanExecuteChanged;

      public bool CanExecute(object parameter)
      {
        return true;
      }

      public void Execute(object parameter)
      {
        ProgramViewModel.WorkshopViewModel.SetByTypeFilter(parameter as string);
        ProgramViewModel.WorkshopViewModel.CurrentTab = 0;
      }
    }

    private class FilterResourceByContentCommand : ICommand
    {
      public event EventHandler CanExecuteChanged;

      public bool CanExecute(object parameter)
      {
        return true;
      }

      public void Execute(object parameter)
      {
        ProgramViewModel.WorkshopViewModel.SetByNameFilter(parameter as string);
      }
    }

    private class InstallResourceCommand : ICommand
    {
      public event EventHandler CanExecuteChanged;

      public bool CanExecute(object parameter)
      {
        return true;
      }

      public async void Execute(object parameter)
      {
        var model = parameter as WorkshopResourceModel;
        try
        {
          var task = model.InstallAsync();
          //ProgramViewModel.WorkshopViewModel.DownloadingResourcesView.Refresh();
          ProgramViewModel.WorkshopViewModel.CurrentTab = 2;
          await task;
        }
        catch (HttpRequestException ex)
        {
          MessageBox.Show(ex.Message);
        }
      }
    }

    private class UpdateResourceCommand : ICommand
    {
      public event EventHandler CanExecuteChanged;

      public bool CanExecute(object parameter)
      {
        return true;
      }

      public async void Execute(object parameter)
      {
        var model = parameter as WorkshopResourceModel;
        try
        {
          var task = model.UpdateAsync();
          //ProgramViewModel.WorkshopViewModel.DownloadingResourcesView.Refresh();
          ProgramViewModel.WorkshopViewModel.CurrentTab = 2;
          await task;
        }
        catch (HttpRequestException ex)
        {
          MessageBox.Show(ex.Message);
        }
      }
    }

    private class PauseResourceCommand : ICommand
    {
      public event EventHandler CanExecuteChanged;

      public bool CanExecute(object parameter)
      {
        return true;
      }

      public void Execute(object parameter)
      {
        var model = parameter as WorkshopResourceModel;
        model.Pause();
      }
    }

    private class ResumeResourceCommand : ICommand
    {
      public event EventHandler CanExecuteChanged;

      public bool CanExecute(object parameter)
      {
        return true;
      }

      public async void Execute(object parameter)
      {
        var model = parameter as WorkshopResourceModel;
        ProgramViewModel.WorkshopViewModel.CurrentTab = 2;
        try
        {
          await model.ResumeAsync();
        }
        catch (HttpRequestException ex)
        {
          MessageBox.Show(ex.Message);
        }
      }
    }

    private class DeleteResourceCommand : ICommand
    {
      public event EventHandler CanExecuteChanged;

      public bool CanExecute(object parameter)
      {
        return true;
      }

      public void Execute(object parameter)
      {
        var model = parameter as WorkshopResourceModel;
        if (MessageBox.Show($"确定要删除资源 {model.Name} 吗？", string.Empty, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
          return;
        model.Delete();
      }
    }

    private class ShowSelectedResourceViewCommand : ICommand
    {
      public event EventHandler CanExecuteChanged;

      public bool CanExecute(object parameter)
      {
        return true;
      }

      public async void Execute(object parameter)
      {
        var viewModel = parameter as WorkshopResourceViewModel;
        ProgramViewModel.WorkshopViewModel.SelectedItem = viewModel;
        ProgramViewModel.WorkshopViewModel.CurrentTab = 1;
        try
        {
          await viewModel.Model.GetImages();
        }
        catch (InvalidOperationException ex)
        {
          MessageBox.Show(ex.ToString());
        }
      }
    }

    private class CreateProcessRelativePathCommand : ICommand
    {
      public event EventHandler CanExecuteChanged;

      public bool CanExecute(object parameter)
      {
        return true;
      }

      public void Execute(object parameter)
      {
        var paras = ((string)parameter).Split(':');
        var hawk = paras[0].Equals("hawk", StringComparison.InvariantCultureIgnoreCase);
        var path = paras[1];
        try
        {
          var exePath = hawk ? ProgramModel.MakeHawkempirePath(path) : ProgramModel.MakeExeRelativePath(path);
          Process.Start(new ProcessStartInfo(exePath)
          {
            WorkingDirectory = Path.GetDirectoryName(exePath),
            UseShellExecute = true,
          });
        }
        catch (Win32Exception ex)
        {
          MessageBox.Show(ex.ToString());
        }
      }
    }

    private class HyperlinkCommand : ICommand
    {
      public event EventHandler CanExecuteChanged
      {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
      }

      public bool CanExecute(object parameter)
      {
        return !string.IsNullOrEmpty((string)parameter);
      }

      public void Execute(object parameter)
      {
        Process.Start(new ProcessStartInfo((string)parameter)
        {
          UseShellExecute = true,
        });
      }
    }

    private class OpenToolCommand : ICommand
    {
      public bool CanExecute(object parameter)
      {
        return true;
      }

      public void Execute(object parameter)
      {
        var tool = (ToolModel)parameter;
        try
        {
          tool.Open();
        }
        catch (Win32Exception ex)
        {
          MessageBox.Show(ex.Message);
        }
      }

      public event EventHandler CanExecuteChanged;
    }

    private class ApplyHotkeyCommand : ICommand
    {
      public event EventHandler CanExecuteChanged;

      public bool CanExecute(object parameter)
      {
        return true;
      }

      public void Execute(object parameter)
      {
        var isC = "c".Equals((string)parameter, StringComparison.InvariantCultureIgnoreCase);
        if (MessageBox.Show("本按钮会将所有玩家的快捷键设置为 " + (isC ? "C" : "AoFE") + " 版默认键位，确认继续？", string.Empty, MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
        {
          var hkiSrc = ProgramModel.MakeExeRelativePath(@"hki\" + (isC ? "c" : "fe") + ".hki");
          foreach (var hkiDest in Util.EnumerateHkiFiles())
          {
            try
            {
              File.Copy(hkiSrc, hkiDest, true);
            }
            catch (IOException ex)
            {
              MessageBox.Show(ex.Message);
            }
          }
        }
      }
    }

    private class CheckUpdateCommand : ICommand
    {
      public event EventHandler CanExecuteChanged;

      public bool CanExecute(object parameter)
      {
        return true;
      }

      public async void Execute(object parameter)
      {
        var process = new Process();
        var exe = Path.GetFullPath("updater.exe");
        process.StartInfo = new ProcessStartInfo(exe, "--CheckUpdate")
        {
          RedirectStandardOutput = true,
          UseShellExecute = false,
        };
        process.Start();
        var ret = await process.StandardOutput.ReadToEndAsync();
        Enum.TryParse(ret, out UpdateServerStatus checkUpdate);
        switch (checkUpdate)
        {
          case UpdateServerStatus.NeedUpdate:
            if (MessageBox.Show("程序有更新，是否开始下载？", "有更新", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
              Process.Start("updater.exe");
              ProgramViewModel.App.Shutdown();
            }
            break;
          case UpdateServerStatus.UpToDate:
            if(bool.Parse((string)parameter))
            {
              MessageBox.Show("程序已是最新版本");
            }
            break;
        }


      }

      private enum UpdateServerStatus
      {
        Getting,
        NeedUpdate,
        UpToDate,
        ConnectFailed,
        ServerError
      }
    }

    private class GenerateDllFromIniCommand:ICommand
    {
      public bool CanExecute(object parameter)
      {
        return true;
      }

      public void Execute(object parameter)
      {
        try
        {
          File.Copy(ProgramModel.MakeExeRelativePath(@"dll\ini\language_empty.dll"),ProgramModel.MakeExeRelativePath(@"dll\ini\language.dll"), true);
          File.Copy(ProgramModel.MakeExeRelativePath(@"dll\ini\language_empty.dll"), ProgramModel.MakeExeRelativePath(@"dll\ini\language_x1.dll"), true);
          File.Copy(ProgramModel.MakeExeRelativePath(@"dll\ini\language_empty.dll"), ProgramModel.MakeExeRelativePath(@"dll\ini\language_x1_p1.dll"), true);
          var count = Model.Util.ParseIniToDll(ProgramModel.MakeExeRelativePath(@"dll\ini\language.dll.ini"),
            ProgramModel.MakeExeRelativePath(@"dll\ini\language.dll"));
          MessageBox.Show($"生成完成！共写入 {count} 条字符串。");
        }
        catch (Exception ex)
        {
          MessageBox.Show(ex.Message);
        }
      }

      public event EventHandler CanExecuteChanged;
    }

    private class ApplyDrsesCommand:ICommand
    {
      public bool CanExecute(object parameter)
      {
        return true;
      }

      public void Execute(object parameter)
      {
        DrsResourceModel.ApplyDrses();
      }

      public event EventHandler CanExecuteChanged;
    }
  }
}
