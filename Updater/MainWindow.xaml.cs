using System.Windows;

namespace YTY.amt
{
  public partial class MainWindow : Window
  {

    public MainWindow()
    {
      InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
      await ProgramModel.StartUpdate();
    }
  }
}
