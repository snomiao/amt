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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Windows.Forms;
using System.Net;
using MessageBox = System.Windows.MessageBox;

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

    private void btnSetHawkempirePath_Click(object sender, RoutedEventArgs e)
    {
      var fbd = new FolderBrowserDialog();
      if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
      txbHawkempirePath.Text = fbd.SelectedPath;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
      ProgramViewModel.MainWindowViewModel.GetFrontPage();

      Enum.TryParse(await ProgramViewModel.IpcCheckUpdate(), out UpdateServerStatus checkUpdate);
      if (checkUpdate == UpdateServerStatus.NeedUpdate)
      {
        if (MessageBox.Show("程序有更新，是否开始下载？", "有更新", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
        {
          Process.Start("updater.exe");
          ProgramViewModel.App.Shutdown();
        }
      }
    }

    private enum UpdateServerStatus
    {
      Getting,
      NeedUpdate,
      UpToDate,
      ConnectFailed,
      ServerError
    }
  }
}
