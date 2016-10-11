using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using YTY;

namespace YTY.amt
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      ConfigRoot.DownloadTasks.Add(new DownloadModel("http://www.hawkaoc.net/hawkclient/hawkadv.avi", @"c:\1.avi"));
      ConfigRoot.DownloadTasks.Add(new DownloadModel("http://www.hawkaoc.net/hawkclient/age2_x1.5.exe", @"c:\1.exe"));
      lbxMain.DataContext = lbxMain.ItemsSource;
      lbxMain.ItemsSource = ConfigRoot.DownloadTasks;
    }
  }
}
