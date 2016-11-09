using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace YTY.amt
{
  public class WindowViewModel : INotifyPropertyChanged
  {
    private WindowView view;

    public WindowView CurrentView
    {
      get { return view; }
      set
      {
        view = value;
        OnPropertyChanged(nameof(CurrentView));
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }

  public enum WindowView
  {
    ShowingResourceList,
    ShowingSelectedResource
  }
}
