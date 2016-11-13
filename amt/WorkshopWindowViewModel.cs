using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Data;

namespace YTY.amt
{
  public class WorkshopWindowViewModel : INotifyPropertyChanged
  {
    private WindowView view;
    private ObservableCollection<WorkshopResourceViewModel> workshopResources;
    private WorkshopResourceViewModel selectedItem;

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
        OnPropertyChanged(nameof(WorkshopResourcesView));
      }
    }

    public ICollectionView WorkshopResourcesView => CollectionViewSource.GetDefaultView(workshopResources);

    public WorkshopResourceViewModel SelectedItem
    {
      get { return selectedItem; }
      set
      {
        selectedItem = value;
        OnPropertyChanged(nameof(SelectedItem));
      }
    }

    public async Task InitAsync()
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
