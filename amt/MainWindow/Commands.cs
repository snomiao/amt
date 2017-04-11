using System;
using System.Windows.Input;
using YTY.amt.Model;

namespace YTY.amt
{
  public class SwitchAndStartGameCommand : ICommand
  {
    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
      return true;
    }

    public void Execute(object parameter)
    {
      var game = parameter as ModResourceModel;
      My.CreateProcessCommand.Execute(game.ExePath);
      ProgramModel.Config.CurrentGame = game;
    }
  }

  public class IncrementDrsPriorityCommand : ICommand
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

  public class DecrementDrsPriorityCommand : ICommand
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
}
