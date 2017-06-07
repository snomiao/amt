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
      //LanguageIniToDll.ExtractDllToIni(@"language.dll", "1.ini");
      LanguageIniToDll.ParseIniToDll("1.ini","language.dll");
      Console.WriteLine("Done");
      Console.ReadKey();
    }
  }
}
