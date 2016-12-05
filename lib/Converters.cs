using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Globalization;
using Visibility = System.Windows.Visibility;

namespace YTY
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

  public class NegateBooleanConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return !(bool)value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return !(bool)value;
    }
  }

  public class BoolToVisibilityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return (bool)value ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
