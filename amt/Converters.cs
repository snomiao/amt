using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace YTY.amt
{
  public class WorkshopResourceTypeToImageConverter : IValueConverter
  {
    public static Dictionary<WorkshopResourceType, BitmapImage> dic_WorkshopResourceType_Image =
      new Dictionary<WorkshopResourceType, BitmapImage>()
      {
        { WorkshopResourceType.Drs, new BitmapImage(new Uri("Resources\\resdrs.png", UriKind.Relative))},
        { WorkshopResourceType.Scenario, new BitmapImage(new Uri("Resources\\resscx.png", UriKind.Relative))},
        { WorkshopResourceType.Ai, new BitmapImage(new Uri("Resources\\resai.png", UriKind.Relative))},
        { WorkshopResourceType.Campaign, new BitmapImage(new Uri("Resources\\rescpx.png", UriKind.Relative))},
        { WorkshopResourceType.Mod, new BitmapImage(new Uri("Resources\\resmod.png", UriKind.Relative))},
        { WorkshopResourceType.RandomMap, new BitmapImage(new Uri("Resources\\resrms.png", UriKind.Relative))},
        { WorkshopResourceType.Replay, new BitmapImage(new Uri("Resources\\resmgx.png", UriKind.Relative))},
        { WorkshopResourceType.Taunt, new BitmapImage(new Uri("Resources\\resdrs.png", UriKind.Relative))},
        { WorkshopResourceType.Undefined, new BitmapImage(new Uri("Resources\\resdrs.png", UriKind.Relative))}
      };

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return dic_WorkshopResourceType_Image[(WorkshopResourceType)value];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }

  public class GameVersionToStringConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      var gv = (GameVersion)value;
      var ret = new List<string>(5);
      if (gv.HasFlag(GameVersion.Aok)) ret.Add("AoK");
      if (gv.HasFlag(GameVersion.AocA)) ret.Add("1.0A");
      if (gv.HasFlag(GameVersion.AocC)) ret.Add("1.0C");
      if (gv.HasFlag(GameVersion.Aoc15)) ret.Add("1.5");
      if (gv.HasFlag(GameVersion.Aofe)) ret.Add("AoFE");
      return string.Join("/", ret);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }

  public class ByteCountToTextConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      var numBytes = (ulong)value;
      if (numBytes < 0)
        throw new ArgumentOutOfRangeException(">=0");
      if (numBytes < 1024)
        return $"{numBytes} B";
      var KB = numBytes / 1024.0;
      if (KB < 1024.0)
        return $"{KB:F2} KB";
      var MB = KB / 1024.0;
      if (MB < 1024.0)
        return $"{MB:F2} MB";
      var GB = MB / 1024.0;
      if (GB < 1024.0)
        return $"{GB:F2} GB";
      return $"{GB / 1024.0:F2} TB";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
