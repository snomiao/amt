using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace YTY.amt
{
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
      My.WorkshopWindowViewModel.SelectedItem = viewModel;
      My.WorkshopWindowViewModel.CurrentTab = 1;
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
        My.WorkshopWindowViewModel.WorkshopResourcesView.Filter = null;
      }
      else
      {
        WorkshopResourceType filter;
        Enum.TryParse(param, out filter);
        My.WorkshopWindowViewModel.WorkshopResourcesView.Filter = item => (item as WorkshopResourceViewModel).Model.Type == filter;
      }
      My.WorkshopWindowViewModel.CurrentTab = 0;
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
      My.WorkshopWindowViewModel.DownloadingResourcesView.Refresh();
      My.WorkshopWindowViewModel.CurrentTab = 2;
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
}
