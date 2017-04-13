using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Threading;
using System.ComponentModel;
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

    public static ICommand FilterResource { get; } = new FilterResourceCommand();

    public static ICommand InstallResource { get; } = new InstallResourceCommand();

    public static ICommand PauseResource { get; } = new PauseResourceCommand();

    public static ICommand ResumeResource { get; } = new ResumeResourceCommand();

    public static ICommand DeleteResource { get; } = new DeleteResourceCommand();

    public static ICommand ShowSelectedResourceView { get; } = new ShowSelectedResourceViewCommand();

    private class ActivateDrsCommand : ICommand
    {
      public event EventHandler CanExecuteChanged;

      public bool CanExecute(object parameter)
      {
        return true;
      }

      public void Execute(object parameter)
      {
        var model = parameter as DrsResourceModel;
        model.Activate();
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
        var model = parameter as DrsResourceModel;
        model.Deactivate();
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
        var drs = parameter as DrsResourceModel;
        drs.IncrementPriority();
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
        var drs = parameter as DrsResourceModel;
        drs.DecrementPriority();
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
        ProgramModel.Config.CurrentGame = game;
        game.Run();
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

    private class FilterResourceCommand : ICommand
    {
      public event EventHandler CanExecuteChanged;

      public bool CanExecute(object parameter)
      {
        return true;
      }

      public void Execute(object parameter)
      {
        var param = parameter as string;
        if (param == "All")
        {
          ProgramViewModel.WorkshopViewModel.ByTypeResourcesView.Filter = null;
        }
        else
        {
          WorkshopResourceType filter;
          Enum.TryParse(param, out filter);
          ProgramViewModel.WorkshopViewModel.ByTypeResourcesView.Filter = item => (item as WorkshopResourceViewModel).Model.Type == filter;
        }
        ProgramViewModel.WorkshopViewModel.CurrentTab = 0;
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
          ProgramViewModel.WorkshopViewModel.DownloadingResourcesView.Refresh();
          ProgramViewModel.WorkshopViewModel.CurrentTab = 2;
          await task;
        }
        catch (InvalidOperationException ex)
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
        await model.ResumeAsync();
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
          await viewModel.Model.GetResourceImagesAsync();
        }
        catch (InvalidOperationException ex)
        {
          MessageBox.Show(ex.ToString());
        }
      }
    }
  }




  public class CreateProcessCommand : ICommand
  {
    public event EventHandler CanExecuteChanged
    {
      add { CommandManager.RequerySuggested += value; }
      remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object parameter)
    {
      return !string.IsNullOrWhiteSpace(parameter as string);
    }

    public void Execute(object parameter)
    {
      var p = new Process() { StartInfo = new ProcessStartInfo(parameter as string) { UseShellExecute = true } };
      try
      {
        p.Start();
      }
      catch (Win32Exception ex)
      {
        MessageBox.Show(ex.ToString());
      }
    }
  }


}
