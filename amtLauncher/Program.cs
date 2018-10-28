using System;
using System.IO;
using System.Diagnostics;

namespace YTY.amt
{
  class Program
  {
    static void Main(string[] args)
    {
      FixCurrentDirectory();
      var processes = Process.GetProcessesByName("updater");
      foreach (var process in processes)
      {
        process.WaitForExit();
      }
      UpdaterRenamedFilesRemoveExtension();
      if (args.Length > 0 && args[0].Equals("--ReplaceUpdater", StringComparison.InvariantCultureIgnoreCase))
      {
        Process.Start("updater");
      }
      else
      {
        Process.Start("amt");
      }
    }

    private static void UpdaterRenamedFilesRemoveExtension()
    {
      var path = System.Reflection.Assembly.GetExecutingAssembly().Location;
      path = Path.GetDirectoryName(path);
      foreach (var needRename in Directory.GetFiles(path, "*.rename", SearchOption.AllDirectories))
      {
        var removeExt = Path.GetFileNameWithoutExtension(needRename);
        File.Delete(removeExt);
        File.Move(needRename, removeExt);
      }
    }

    /// <summary>
    /// 修复当前工作目录为exe所在目录，使得其他位置快捷方式也可正确使用
    /// </summary>
    private static void FixCurrentDirectory()
    {
      var assem = System.Reflection.Assembly.GetExecutingAssembly();
      Environment.CurrentDirectory = Path.GetDirectoryName(assem.Location);
    }
  }
}
