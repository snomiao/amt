using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using YTY.amt.Model;
using System.IO;

namespace YTY.amt.Test
{
  class Program
  {
    static void Main(string[] args)
    {
      if (args.Length >= 1)
      {
        if (File.Exists(args[0]))
        {
          //Console.WriteLine(Util.ClearResource("language.dll"));
          Util.ExtractDllToIni(args[0], Path.ChangeExtension(args[0],"ini"));
          //Util.ParseIniToDll("1.ini","language.dll");
          Console.WriteLine("Done");
        }
        else
        {
          Console.WriteLine("Dll file does not exist.");
        }
      }
      else
      {
        Console.WriteLine("Please parse dll file name as argument.");
      }
      Console.ReadKey();
    }
  }
}
