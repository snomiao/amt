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
      if (e.ChangedButton == MouseButton.Left)
        (sender as Window).DragMove();
    }
  }

  public static class My
  {
    public static App App => Application.Current as App;

    public static Workshop WorkshopWindow
    {
      get
      {
        var workshop = App.FindResource(nameof(WorkshopWindow)) as Workshop;
        return workshop;
      }
    }

    public static WindowViewModel WindowViewModel
    {
      get
      {
        return App.FindResource(nameof(WindowViewModel)) as WindowViewModel;
      }
    }
  }
}
