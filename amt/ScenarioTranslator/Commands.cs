using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;

namespace YTY.amt
{
  public class OpenScenarioCommand : ICommand
  {
    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
      return true;
    }

    public void Execute(object parameter)
    {
      var ofd = new OpenFileDialog();
      ofd.Filter = "帝国时代Ⅱ场景文件|*.scx";
      ofd.ShowDialog();
      My.ScenarioTranslatorViewModel.Scx = new ScxFile(ofd.FileName);
    }
  }
}
