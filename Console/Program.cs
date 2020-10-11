using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using YTY.amt.Model;
using System.IO;
using PETools;

namespace YTY.amt.Test
{
  class Program
  {
    static void Main(string[] args)
    {
      var petool = new PETool();
      petool.Read(@"d:\hawkaoc\aoc\language.dll");
      petool.Layout();
      petool.WriteFile(@"d:\hawkaoc\aoc\`language.dll");
      Console.ReadKey();
    }
  }
}
