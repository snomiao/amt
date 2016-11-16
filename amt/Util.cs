using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows;

namespace YTY.amt
{
  public static class Util
  {
    private static DateTime UNIXTIMESTAMPBASE = new DateTime(1970, 1, 1, 0, 0, 0);
    private static List<Size> screenResolutions;

    public static DateTime FromUnixTimestamp(ulong unixTimestamp)
    {
      return UNIXTIMESTAMPBASE + TimeSpan.FromSeconds(unixTimestamp);
    }

    public static List<Size> GetScreenResolutions()
    {
      if (screenResolutions == null)
      {
        var temp = new List<Size>();
        var dm = new DEVMODE();
        var i = 0;
        while (EnumDisplaySettings(null, i++, ref dm))
        {
          temp.Add(new Size(dm.dmPelsWidth, dm.dmPelsHeight));
        }
        screenResolutions = temp.Distinct().OrderBy(s => s.Width).ThenBy(s => s.Height).ToList();
      }
      return screenResolutions;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DEVMODE
    {
      private const int CCHDEVICENAME = 0x20;
      private const int CCHFORMNAME = 0x20;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
      public string dmDeviceName;
      public short dmSpecVersion;
      public short dmDriverVersion;
      public short dmSize;
      public short dmDriverExtra;
      public int dmFields;
      public int dmPositionX;
      public int dmPositionY;
      public ScreenOrientation dmDisplayOrientation;
      public int dmDisplayFixedOutput;
      public short dmColor;
      public short dmDuplex;
      public short dmYResolution;
      public short dmTTOption;
      public short dmCollate;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
      public string dmFormName;
      public short dmLogPixels;
      public int dmBitsPerPel;
      public int dmPelsWidth;
      public int dmPelsHeight;
      public int dmDisplayFlags;
      public int dmDisplayFrequency;
      public int dmICMMethod;
      public int dmICMIntent;
      public int dmMediaType;
      public int dmDitherType;
      public int dmReserved1;
      public int dmReserved2;
      public int dmPanningWidth;
      public int dmPanningHeight;
    }

    [DllImport("user32")]
    private static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

  }
}
