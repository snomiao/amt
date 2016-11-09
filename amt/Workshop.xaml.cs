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

    private void wnd_Loaded(object sender, RoutedEventArgs e)
    {
      itc.ItemsSource = new List<WorkshopResourceModel>()
        {
           new WorkshopResourceModel("test1",4.9, WorkshopResourceType.Drs),
           new WorkshopResourceModel("name2",4.2,WorkshopResourceType.Campaign),
           new WorkshopResourceModel("This is an extraordinarily long workshop resource name",1.2, WorkshopResourceType.Scenario ),
           new WorkshopResourceModel() ,
           new WorkshopResourceModel() ,
           new WorkshopResourceModel() ,
           new WorkshopResourceModel() ,
           new WorkshopResourceModel() ,
           new WorkshopResourceModel()
        };
    }
  }
}
