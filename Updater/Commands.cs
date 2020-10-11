using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Diagnostics;

namespace YTY.amt
{
  public static class Commands
  {
    public static ICommand RetryConnect { get; } = new RetryConnectCommand();

    public static ICommand ShutdownAndRunAmt { get; } = new ShutdownAndRunAmtCommand();

    private class RetryConnectCommand : ICommand
    {
      public event EventHandler CanExecuteChanged;

      public bool CanExecute(object parameter)
      {
        return true;
      }

      public async void Execute(object parameter)
      {
        await ProgramModel.StartUpdate();
      }
    }

    private class ShutdownAndRunAmtCommand : ICommand
    {
      public event EventHandler CanExecuteChanged;

      public bool CanExecute(object parameter)
      {
        return true;
      }

      public void Execute(object parameter)
      {
        Process.Start("amt.exe");
        App.Program.Shutdown();
      }
    }
  }
}
