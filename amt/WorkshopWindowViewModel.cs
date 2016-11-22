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
    private int currentTab;
    private ObservableCollection<WorkshopResourceViewModel> workshopResources;
    private WorkshopResourceViewModel selectedItem;
    private ICollectionView workshopResourcesView;
    private ICollectionView downloadingResourcesView;

    public int CurrentTab
    {
      get { return currentTab; }
      set
      {
        currentTab = value;
        OnPropertyChanged(nameof(CurrentTab));
      }
    }

    public ObservableCollection<WorkshopResourceViewModel> WorkshopResources
    {
      get { return workshopResources; }
      set
      {
        workshopResources = value;
        WorkshopResourcesView = CollectionViewSource.GetDefaultView(workshopResources);
        DownloadingResourcesView = new CollectionViewSource() { Source = workshopResources }.View;
        DownloadingResourcesView.Filter = o => (o as WorkshopResourceViewModel).Model.Status == WorkshopResourceStatus.Installing;
      }
    }

    public ICollectionView WorkshopResourcesView
    {
      get { return workshopResourcesView; }
      set
      {
        workshopResourcesView = value;
        OnPropertyChanged(nameof(WorkshopResourcesView));
      }
    }

    public ICollectionView DownloadingResourcesView
    {
      get { return downloadingResourcesView; }
      set
      {
        downloadingResourcesView = value;
        OnPropertyChanged(nameof(DownloadingResourcesView));
      }
    }

    public WorkshopResourceViewModel SelectedItem
    {
      get { return selectedItem; }
      set
      {
        selectedItem = value;
        OnPropertyChanged(nameof(SelectedItem));
      }
    }

    public async Task Init()
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
}
