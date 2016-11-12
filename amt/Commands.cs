using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Threading;

namespace YTY.amt
{
  public class CloseWindowCommand : ICommand
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

  public class MinimizeWindowCommand : ICommand
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

  public class HideWindowCommand : ICommand
  {
    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
      return true;
    }

    public void Execute(object parameter)
    {
      (parameter as Window).Hide();
    }
  }

  public class CreateProcessCommand : ICommand
  {
    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
      return true;
    }

    public void Execute(object parameter)
    {
      var p = new Process() { StartInfo = new ProcessStartInfo(parameter as string) { UseShellExecute = true } };
      p.Start();
    }
  }

  public class ConfigSwitchGameCommand : ICommand
  {
    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
      return true;
    }

    public void Execute(object parameter)
    {
      return;
    }
  }

  public class OpenFolderCommand : ICommand
  {
    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
      return true;
    }

    public void Execute(object parameter)
    {
      return;
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
      My.WindowViewModel.SelectedItem = viewModel;
      My.WindowViewModel.CurrentView = WindowView.ShowingSelectedResource;
      try
      {
        await viewModel.GetDetailsAsync();
      }
      catch(InvalidOperationException ex)
      {
        MessageBox.Show(ex.ToString());
      }
    }
  }

  public class ShowResourceListViewCommand : ICommand
  {
    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
      return true;
    }

    public void Execute(object parameter)
    {
      My.WindowViewModel.SelectedItem = null;
      My.WindowViewModel.CurrentView = WindowView.ShowingResourceList;
    }
  }
}
