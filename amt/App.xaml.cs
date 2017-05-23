using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Threading;

namespace YTY.amt
{
  public partial class App : Application
  {
    private const string MUTEXNAME = "amtMainMutex";

    private Mutex lifetimeMutex;

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      if (e.ChangedButton == MouseButton.Left)
        (sender as Window).DragMove();
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
      lifetimeMutex.ReleaseMutex();
      lifetimeMutex.Close();
    }

    private void Application_Startup(object sender, StartupEventArgs e)
    {
      lifetimeMutex = new Mutex(true, MUTEXNAME);
    }
  }
}
