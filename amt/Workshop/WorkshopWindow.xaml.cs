using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using YTY.amt.Model;

namespace YTY.amt
{
  /// <summary>
  /// Interaction logic for WorkshopWindow.xaml
  /// </summary>
  public partial class WorkshopWindow : Window
  {
    public WorkshopWindow()
    {
      InitializeComponent();
    }

    private async void wnd_Loaded(object sender, RoutedEventArgs e)
    {
      try
      {
        await ProgramModel.UpdateResources();
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }
  }
}
