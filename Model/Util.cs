using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using IniParser;
using Vestris.ResourceLib;

namespace YTY.amt.Model
{
  public static class Util
  {
    private const string CSIDDIGITS = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!-._~";
    private const string SECTION_NAME = "Language.dll";
    private static readonly int NUMCSIDDIGITS;
    private static readonly SHA1 sha1 = SHA1.Create();

    static Util()
    {
      NUMCSIDDIGITS = CSIDDIGITS.Length;
    }

    public static uint Csid2Int(string csid)
    {
      var weight = 1;
      return (uint)csid.Aggregate(0, (sum, digit) =>
      {
        sum += CSIDDIGITS.IndexOf(digit) * weight;
        weight *= NUMCSIDDIGITS;
        return sum;
      });
    }

    public static string Int2Csid(int value)
    {
      var ret = string.Empty;
      do
      {
        ret += CSIDDIGITS[value % NUMCSIDDIGITS];
        value = value / NUMCSIDDIGITS;
      } while (value > 0);
      return ret;
    }

    public static string GetFileSha1(string fileName)
    {
      return BitConverter.ToString(sha1.ComputeHash(File.ReadAllBytes(fileName))).Replace("-", string.Empty).ToLower();
    }

    public static bool ClearResource(string fileName)
    {
      var h = BeginUpdateResource(fileName, true);
      return EndUpdateResource(h, false);
    }

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
      var parser = new FileIniDataParser();
      
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

      ClearResource(dllFileName);
      //var setIni = new HashSet<int>(groups.Select(g => g.Key));
      using (var ri = new ResourceInfo())
      {
        ri.Load(dllFileName);
        var resources = new List<Resource>();
        //ri[Kernel32.ResourceTypes.RT_STRING] = resources;
        //ri.ResourceTypes.Add(rid);
        foreach (var id in groups)
        {
          var sr = new StringResource((ushort)id.Key);
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

    [DllImport ("kernel32")]
    public static extern IntPtr BeginUpdateResource(string fileName, bool deleteExistingResources);

    [DllImport("kernel32")]
    public static extern bool EndUpdateResource(IntPtr hUpdate, bool discard);
  }
}
