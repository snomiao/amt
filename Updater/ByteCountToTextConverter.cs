using System;
using System.Globalization;
using System.Windows.Data;

namespace YTY.amt
{
  public class ByteCountToTextConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      var numBytes = (long)value;
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
