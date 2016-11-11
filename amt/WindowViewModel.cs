using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Diagnostics;

namespace YTY.amt
{
  public class WindowViewModel : INotifyPropertyChanged
  {
    private WindowView view;
    private ObservableCollection<WorkshopResourceViewModel> workshopResources;

    public WindowView CurrentView
    {
      get { return view; }
      set
      {
        view = value;
        OnPropertyChanged(nameof(CurrentView));
      }
    }

    public bool WorkshopShown
    {
      set
      {
        if (value) My.WorkshopWindow.Show();
      }
    }

    public ObservableCollection<WorkshopResourceViewModel> WorkshopResources
    {
      get { return workshopResources; }
      set
      {
        workshopResources = value;
        OnPropertyChanged(nameof(WorkshopResources));
      }
    }

    public async Task Get()
    {
      WorkshopResources = new ObservableCollection<WorkshopResourceViewModel>();
      await DAL.GetWorkshopResourcesAsync(new Progress<WorkshopResourceModel>(model => workshopResources.Add(new WorkshopResourceViewModel(model))));
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
