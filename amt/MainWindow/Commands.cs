using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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
      var game = parameter as GameVersionModel;
      My.CreateProcessCommand.Execute(game.ExePath);
      My.MainWindowViewModel.CurrentGameVersion = game;
    }
  }

}
