using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace YTY.amt
{
  public class WorkshopResourceViewModel : INotifyPropertyChanged
  {
    public WorkshopResourceModel Model { get; }

    public WorkshopResourceViewModel(WorkshopResourceModel model)
    {
      Model = model;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
