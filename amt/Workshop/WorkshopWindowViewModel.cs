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
    private Predicate<object> byTypeFilter = o => true;
    private Predicate<object> byNameFilter = o => true;

    private Predicate<object> ByTypeFilter
    {
      get => byTypeFilter;
      set
      {
        byTypeFilter = value;
        RefreshResourcesView();
      }
    }

    private Predicate<object> ByNameFilter
    {
      get => byNameFilter;
      set
      {
        byNameFilter = value;
        RefreshResourcesView();
      }
    }

    private void RefreshResourcesView()
    {
      ResourcesView.Filter = o => ByTypeFilter(o as WorkshopResourceViewModel) && ByNameFilter(o as WorkshopResourceViewModel);
    }

    public int CurrentTab
    {
      get => currentTab;
      set
      {
        currentTab = value;
        OnPropertyChanged(nameof(CurrentTab));
      }
    }

    public ObservableCollection<WorkshopResourceViewModel> WorkshopResources { get; } =
      new ObservableCollection<WorkshopResourceViewModel>();

    public ICollectionView ResourcesView { get; }

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
      foreach (var model in ProgramModel.Resources)
      {
        WorkshopResources.Add(WorkshopResourceViewModel.FromModel(model));
        model.PropertyChanged += Model_PropertyChanged;
      }

      ResourcesView = new CollectionViewSource { Source = WorkshopResources }.View;
      ResourcesView.SortDescriptions.Add(
        new SortDescription("Model.LastChangeDate", ListSortDirection.Descending));
      ResourcesView.SortDescriptions.Add(
        new SortDescription("Model.LastFileChangeDate", ListSortDirection.Descending));
      DownloadingResourcesView = new CollectionViewSource { Source = WorkshopResources }.View;
      DownloadingResourcesView.Filter = o =>
      {
        var status = (o as WorkshopResourceViewModel).Model.Status;
        return status == WorkshopResourceStatus.Installing ||
               status == WorkshopResourceStatus.Paused ||
               status == WorkshopResourceStatus.Failed;
      };
    }

    internal void SetByTypeFilter(string resourceType)
    {
      if (resourceType == "All")
      {
        ByTypeFilter = o => true;
      }
      else
      {
        Enum.TryParse(resourceType, out WorkshopResourceType filter);
        ByTypeFilter = o => (o as WorkshopResourceViewModel).Model.Type == filter;
      }
    }

    internal void SetByNameFilter(string contains)
    {
      if (string.IsNullOrWhiteSpace(contains))
      {
        ByNameFilter = o => true;
      }
      else
      {
        ByNameFilter = o => (o as WorkshopResourceViewModel).Model.Name.Contains(contains);
      }
    }

    private void Resources_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.Action == NotifyCollectionChangedAction.Add)
      {
        foreach (WorkshopResourceModel model in e.NewItems)
        {
          model.PropertyChanged += Model_PropertyChanged;
          WorkshopResources.Add(WorkshopResourceViewModel.FromModel(model));
        }
      }
    }

    private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(WorkshopResourceModel.Status))
      {
        DownloadingResourcesView.Refresh();
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
