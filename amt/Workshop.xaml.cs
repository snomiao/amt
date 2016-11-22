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

namespace YTY.amt
{
  /// <summary>
  /// Interaction logic for Workshop.xaml
  /// </summary>
  public partial class Workshop : Window
  {
    public Workshop()
    {
      InitializeComponent();
    }

    private void wnd_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (!(bool)e.NewValue) return;
      try
      {
        var _ = My.WorkshopWindowViewModel.Init();
      }
      catch (InvalidOperationException ex)
      {
        MessageBox.Show(ex.ToString());
      }
    }
  }
}
