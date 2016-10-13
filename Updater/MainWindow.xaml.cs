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
      //Config.DownloadTasks.Add(new DownloadModel("http://www.hawkaoc.net/hawkclient/hawkadv.avi", @"c:\1.avi"));
      //Config.DownloadTasks.Add(new DownloadModel("http://www.hawkaoc.net/hawkclient/age2_x1.5.exe", @"c:\1.exe"));
      //Config.DownloadTasks.Add(new DownloadModel("http://download.microsoft.com/download/9/5/A/95A9616B-7A37-4AF6-BC36-D6EA96C8DAAE/dotNetFx40_Full_x86_x64.exe", @"c:\2.exe"));
    }

    private void btnStart_Click(object sender, RoutedEventArgs e)
    {
      //Config.DownloadTasks[0].Start();
      //ConfigRoot.DownloadTasks[1].Start();
    }
  }
}
