using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Data;
using YTY.amt.Model;

namespace YTY.amt
{
  public class WorkshopWindowViewModel : INotifyPropertyChanged
  {
    private int currentTab;
    private WorkshopResourceViewModel selectedItem;

    public int CurrentTab
    {
      get { return currentTab; }
      set
      {
        currentTab = value;
        OnPropertyChanged(nameof(CurrentTab));
      }
    }

    public ObservableCollection<WorkshopResourceViewModel> WorkshopResources { get; } =
      new ObservableCollection<WorkshopResourceViewModel>(
        ProgramModel.Resources.Select(WorkshopResourceViewModel.FromModel));

    public ICollectionView ByTypeResourcesView { get; } 

    public ICollectionView DownloadingResourcesView { get; }

    public WorkshopResourceViewModel SelectedItem
    {
      get { return selectedItem; }
      set
      {
        selectedItem = value;
        OnPropertyChanged(nameof(SelectedItem));
      }
    }

    public WorkshopWindowViewModel()
    {
      ProgramModel.Resources.CollectionChanged += Resources_CollectionChanged;
      ByTypeResourcesView = new CollectionViewSource {Source = WorkshopResources}.View;
      DownloadingResourcesView = new CollectionViewSource() { Source = WorkshopResources }.View;
      DownloadingResourcesView.Filter = o =>
      {
        var status = (o as WorkshopResourceViewModel).Model.Status;
        return status == WorkshopResourceStatus.Installing || status == WorkshopResourceStatus.Paused;
      };
    }

    private void Resources_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.Action == NotifyCollectionChangedAction.Add)
      {
        foreach (WorkshopResourceModel model in e.NewItems)
          WorkshopResources.Add(WorkshopResourceViewModel.FromModel(model));
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
