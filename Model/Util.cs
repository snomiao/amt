using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows;

namespace YTY.amt.Model
{
  public static class Util
  {
    private const string CSIDDIGITS = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!-._~";
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
  }
}
