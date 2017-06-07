using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using YTY.amt.Model;

namespace YTY.amt.Test
{
  class Program
  {
    static void Main(string[] args)
    {
      //Console.WriteLine(Util.ClearResource("language.dll"));
      //Util.ExtractDllToIni(@"language.dll", "1.ini");
      Util.ParseIniToDll("1.ini","language.dll");
      Console.WriteLine("Done");
      Console.ReadKey();
    }
  }
}
