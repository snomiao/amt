using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace YTY.amt
{
  public class ChunkStatusToBrushConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      switch( (DownloadChunkStatus) value)
      {
        case DownloadChunkStatus.New:
          return Brushes.Yellow;
        case DownloadChunkStatus.Done:
          return Brushes.Green;
      }
      return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
