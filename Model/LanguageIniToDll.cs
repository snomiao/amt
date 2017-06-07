using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vestris.ResourceLib;
using IniParser;

namespace YTY.amt.Model
{
  public static class LanguageIniToDll
  {
    private const string SECTION_NAME = "Language.dll";

    public static void ExtractDllToIni(string dllFileName, string iniFileName)
    {
      var ri = new ResourceInfo();
      ri.Load(dllFileName);
      var ini = new IniParser.Model.IniData();
      ini.Configuration.AssigmentSpacer = string.Empty;
      var section = ini[SECTION_NAME];
      var resources = ri[Kernel32.ResourceTypes.RT_STRING];
      foreach (StringResource resource in resources)
      {
        foreach (var str in resource.Strings)
        {
          section[str.Key.ToString()] = str.Value.Replace("\n", @"\n");
        }
      }
      new FileIniDataParser().WriteFile(iniFileName, ini, Encoding.UTF8);
    }

    public static void ParseIniToDll(string iniFileName, string dllFileName)
    {
      var ini = new FileIniDataParser().ReadFile(iniFileName, Encoding.UTF8)[SECTION_NAME];
      var toWrite = new Dictionary<ushort, string>();
      foreach (var kv in ini)
      {
        if (ushort.TryParse(kv.KeyName, out var key))
        {
          toWrite.Add(key, kv.Value.Replace(@"\n", "\n"));
        }
      }
      var groups = toWrite.OrderBy(kv => kv.Key).GroupBy(kv => kv.Key / 16 + 1);
     

      //var setIni = new HashSet<int>(groups.Select(g => g.Key));
      using (var ri = new ResourceInfo())
      {
        ri.Load(dllFileName);
        
        var resources = ri[Kernel32.ResourceTypes.RT_STRING];
        foreach (var res in resources)
        {
          res.DeleteFrom(dllFileName);
        }
        resources.Clear();
        foreach (var id in groups)
        {
          var sr =new StringResource((ushort) id.Key);
          foreach (var kv in id)
          {
            sr[kv.Key] = kv.Value;
          }
          resources.Add(sr);
        }
        //var resByLanguage = resources.GroupBy(r => r.Language);
        //foreach (var language in resByLanguage)
        //{
        //  var dicIdResource = language.ToDictionary(r => r.Name.Id.ToInt32(), r => r);
        //  var setDll = new HashSet<int>(dicIdResource.Keys);
        //  var toAdd = new HashSet<int>(setIni);
        //  toAdd.ExceptWith(setDll);
        //  var toDel = new HashSet<int>(setDll);
        //  toDel.ExceptWith(setIni);
        //  foreach (var del in toDel)
        //  {
        //    dicIdResource[del].DeleteFrom(dllFileName);
        //  }
        //  foreach (var add in toAdd)
        //  {
        //    resources.Add(new StringResource((ushort)add));
        //  }
        //  foreach (var id in groups)
        //  {
        //    var sr = (StringResource)dicIdResource[id.Key];
        //    foreach (var kv in id)
        //    {
        //      sr[kv.Key] = kv.Value;
        //    }
        //  }
        //}
        Resource.Save(dllFileName, resources);
      }
    }
  }
}
