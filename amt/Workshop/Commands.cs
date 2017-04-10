using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using YTY.amt.Model;

namespace YTY.amt
{
  public static class WorkshopCommands
  {
    public static ICommand InstallResource { get; }
    public static ICommand PauseResource { get; }
    public static ICommand ResumeResource { get; }
    public static ICommand DeactivateResource { get; }
    public static ICommand ActivateResource { get; }
    public static ICommand DeleteResource { get; }

    static WorkshopCommands()
    {
      InstallResource = new InstallResourceCommand();
      PauseResource = new PauseResourceCommand();
      ResumeResource = new ResumeResourceCommand();
      DeactivateResource = new DeactivateResourceCommand();
      ActivateResource = new ActivateResourceCommand();
      DeleteResource = new DeleteResourceCommand();
    }
  }

  public class ShowSelectedResourceViewCommand : ICommand
  {
    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
      return true;
    }

    public async void Execute(object parameter)
    {
      var viewModel = parameter as WorkshopResourceViewModel;
      ProgramViewModel.WorkshopWindow.SelectedItem = viewModel;
      ProgramViewModel.WorkshopWindow.CurrentTab = 1;
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

  public class FilterResourceCommand : ICommand
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
        ProgramViewModel.WorkshopWindow.ByTypeResourcesView.Filter = null;
      }
      else
      {
        WorkshopResourceType filter;
        Enum.TryParse(param, out filter);
        ProgramViewModel.WorkshopWindow.ByTypeResourcesView.Filter = item => (item as WorkshopResourceViewModel).Model.Type == filter;
      }
      ProgramViewModel.WorkshopWindow.CurrentTab = 0;
    }
  }

  public class InstallResourceCommand : ICommand
  {
    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
      return true;
    }

    public async void Execute(object parameter)
    {
      var model = parameter as WorkshopResourceModel;
      var task = model.InstallAsync();
      ProgramViewModel.WorkshopWindow.DownloadingResourcesView.Refresh();
      ProgramViewModel.WorkshopWindow.CurrentTab = 2;
      await task;
    }
  }

  public class PauseResourceCommand : ICommand
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

  public class ResumeResourceCommand : ICommand
  {
    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
      return true;
    }

    public async void Execute(object parameter)
    {
      var model = parameter as WorkshopResourceModel;
      await model.ResumeAsync();
    }
  }

  public class DeleteResourceCommand : ICommand
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

  public class DeactivateResourceCommand : ICommand
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

  public class ActivateResourceCommand : ICommand
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
}
