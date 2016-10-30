using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace YTY.amt
{
  public class Commands
  {
  }

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

}
