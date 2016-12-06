using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;
using System.Windows;
using Path = System.IO.Path;

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
      if(!ofd.ShowDialog().Value) return;
      My.ScenarioTranslatorViewModel.Scx = new ScxFile(ofd.FileName);
    }
  }

  public class SaveScenarioCommand : ICommand
  {
    public event EventHandler CanExecuteChanged
    {
      add { CommandManager.RequerySuggested += value; }
      remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object parameter)
    {
      return My.ScenarioTranslatorViewModel.FileOpened;
    }

    public void Execute(object parameter)
    {
      My.ScenarioTranslatorViewModel.Scx.Save();
    }
  }

  public class SaveScenarioAsCommand : ICommand
  {
    public event EventHandler CanExecuteChanged
    {
      add { CommandManager.RequerySuggested += value; }
      remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object parameter)
    {
      return My.ScenarioTranslatorViewModel.FileOpened;
    }

    public void Execute(object parameter)
    {
      var sfd = new SaveFileDialog();
      sfd.FileName = My.ScenarioTranslatorViewModel.Scx.FileName;
      if (!sfd.ShowDialog().Value) return;
      My.ScenarioTranslatorViewModel.Scx.SaveAs(sfd.FileName);
    }
  }

  public class CloseScenarioCommand : ICommand
  {
    public event EventHandler CanExecuteChanged
    {
      add { CommandManager.RequerySuggested += value; }
      remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object parameter)
    {
      return My.ScenarioTranslatorViewModel.FileOpened;
    }

    public void Execute(object parameter)
    {
      My.ScenarioTranslatorViewModel.Scx = null;
    }
  }

  public class CopyAllCommand : ICommand
  {
    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
      return true;
    }

    public void Execute(object parameter)
    {
      if (MessageBox.Show("该操作将重写所有条目内容，确认继续？", string.Empty, MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) return;
      foreach (var node in My.ScenarioTranslatorViewModel.GetAllNodes().Where(n => n.HasContent))
      {
        node.To = node.Source;
      }
    }
  }

  public class EmptyAllNamesCommand : ICommand
  {
    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
      return true;
    }

    public void Execute(object parameter)
    {
      if (MessageBox.Show("该操作将清空所有【触发-名称】条目内容，确认继续？", string.Empty, MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) return;
      foreach (var node in My.ScenarioTranslatorViewModel.GetAllNodes().Where(n => n.Type == NodeType.TriggerName))
      {
        node.To = string.Empty;
      }
    }
  }

  public class NumberNamesCommand : ICommand
  {
    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
      return true;
    }

    public void Execute(object parameter)
    {
      if (MessageBox.Show("该操作将重写所有【触发-名称】条目内容，确认继续？", string.Empty, MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) return;
      foreach (var node in My.ScenarioTranslatorViewModel.GetAllNodes().Where(n => n.Type == NodeType.TriggerName).Select((n, i) => new { i = i, n = n }))
      {
        node.n.To = $"{My.ScenarioTranslatorViewModel.Prefix}{node.i + 1}";
      }
    }
  }

  public class ExportScenarioCommand : ICommand
  {
    public event EventHandler CanExecuteChanged
    {
      add { CommandManager.RequerySuggested += value; }
      remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object parameter)
    {
      return My.ScenarioTranslatorViewModel.FileOpened;
    }

    public void Execute(object parameter)
    {
      var sfd = new SaveFileDialog();
      sfd.FileName = Path.ChangeExtension(My.ScenarioTranslatorViewModel.Scx.FileName, "txt");
      if (!sfd.ShowDialog().Value) return;
      My.ScenarioTranslatorViewModel.Export(sfd.FileName);
    }
  }

  public class ImportScenarioCommand : ICommand
  {
    public event EventHandler CanExecuteChanged
    {
      add { CommandManager.RequerySuggested += value; }
      remove { CommandManager.RequerySuggested -= value; }
    }


    public bool CanExecute(object parameter)
    {
      return My.ScenarioTranslatorViewModel.FileOpened;
    }

    public void Execute(object parameter)
    {
      var ofd = new OpenFileDialog();
      ofd.Filter = "文本文件 (UTF-8)|*.txt";
      ofd.ShowDialog();
      My.ScenarioTranslatorViewModel.Import(ofd.FileName);
    }
  }
}
