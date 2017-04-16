﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace YTY.amt
{
  public partial class App : Application
  {


    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      if (e.ChangedButton == MouseButton.Left)
        (sender as Window).DragMove();
    }
  }

  public static class My
  {
    public static App App => Application.Current as App;

    public static ByteCountToTextConverter ByteCountToTextConverter => App.FindResource(nameof(ByteCountToTextConverter)) as ByteCountToTextConverter;

  }
}
