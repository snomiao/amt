using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace YTY
{
  public static class Util
  {
    private static readonly string thisExeFullPath;
    private static readonly MD5 md5 = MD5.Create();
    private static readonly string[] SelfReferencedFiles = {
      "Updater.exe",
      "Dapper.dll",
      "System.Data.SQLite.dll",
      "x64\\SQLite.Interop.dll",
      "x86\\SQLite.Interop.dll",
    };

    static Util()
    {
      thisExeFullPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

    }

    public static string MakeQualifiedPath(string relativePath)
    {
      return Path.Combine(thisExeFullPath, relativePath);
    }

    public static string GetFileMd5(string fileName)
    {
      return BitConverter.ToString(md5.ComputeHash(File.ReadAllBytes(fileName))).Replace("-", string.Empty).ToLower();
    }

    public static string RenameSelfReferencedFile(string relativePath)
    {
      if (SelfReferencedFiles.Contains(relativePath))
      {
        return relativePath + ".rename";
      }
      else
      {
        return relativePath;
      }
    }

    public static void CreateShortcut(string shortcutTarget,string lnkPath, string description)
    {
      var link = (IShellLink)new ShellLink();
      link.SetDescription(description);
      link.SetPath(shortcutTarget);
      var file = (IPersistFile)link;
      file.Save(lnkPath, false);
    }

    [ComImport]
    [Guid("00021401-0000-0000-C000-000000000046")]
    private class ShellLink
    {
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    private interface IShellLink
    {
      void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
      void GetIDList(out IntPtr ppidl);
      void SetIDList(IntPtr pidl);
      void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
      void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
      void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
      void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
      void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
      void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
      void GetHotkey(out short pwHotkey);
      void SetHotkey(short wHotkey);
      void GetShowCmd(out int piShowCmd);
      void SetShowCmd(int iShowCmd);
      void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
      void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
      void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
      void Resolve(IntPtr hwnd, int fFlags);
      void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }
  }
}
