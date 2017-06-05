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
      LanguageIniToDll.ParseIniToDll(@"C:\1.ini",@"c:\test\1.dll");
      Console.WriteLine("Done");
      Console.ReadKey();
    }
  }
}
