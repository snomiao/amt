using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Threading;
using System.ComponentModel;

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
}
