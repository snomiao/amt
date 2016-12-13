﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Data;
using HttpRequestException = System.Net.Http.HttpRequestException;

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
        workshopResources.CollectionChanged += (s, e) =>
        {
          if (e.NewItems != null)
            foreach (WorkshopResourceViewModel added in e.NewItems)
              added.PropertyChanged += WorkshopResource_PropertyChanged;
          if (e.OldItems != null)
            foreach (WorkshopResourceViewModel removed in e.OldItems)
              removed.PropertyChanged -= WorkshopResource_PropertyChanged;
        };
        foreach (var r in workshopResources)
          r.Model.PropertyChanged += WorkshopResource_PropertyChanged;
        WorkshopResourcesView = CollectionViewSource.GetDefaultView(workshopResources);
        DownloadingResourcesView = new CollectionViewSource() { Source = workshopResources }.View;
        DownloadingResourcesView.Filter = o =>
        {
          var status = (o as WorkshopResourceViewModel).Model.Status;
          return status == WorkshopResourceStatus.Installing || status == WorkshopResourceStatus.Paused;
        };
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
      LoadLocal();
      try
      {
        await Refresh();
      }
      catch (HttpRequestException)
      {

      }
    }

    private void LoadLocal()
    {
      var localResources = DAL.GetLocalResources();
      foreach (var localResource in localResources)
      {
        if (localResource.Status == WorkshopResourceStatus.Installing)
          localResource.UpdateStatus(WorkshopResourceStatus.Paused);
      }
      WorkshopResources = new ObservableCollection<WorkshopResourceViewModel>(localResources.Select(r => new WorkshopResourceViewModel(r)));
    }

    private async Task Refresh()
    {
      try
      {
        var serviceResult = await DAL.GetUpdatedServerResourcesAsync();
        var updatedResources = serviceResult.Item2;
        foreach (var updatedResource in updatedResources)
        {
          var localResource = workshopResources.Select(r => r.Model).FirstOrDefault(l => l.Id == updatedResource.Id);
          if (localResource == null)
          // resource does not exist locally
          {
            updatedResource.Status = WorkshopResourceStatus.NotInstalled;
            var New = new WorkshopResourceViewModel(updatedResource);
            WorkshopResources.Add(New);
          }
          else
          // resource exists locally
          {
            if (updatedResource.LastChangeDate > localResource.LastChangeDate)
            // resource metadata updated
            {
              localResource.Rating = updatedResource.Rating;
              localResource.DownloadCount = updatedResource.DownloadCount;
              localResource.Name = updatedResource.Name;
              localResource.Discription = updatedResource.Discription;
              localResource.GameVersion = updatedResource.GameVersion;
              localResource.SourceUrl = updatedResource.SourceUrl;
              localResource.LastChangeDate = updatedResource.LastChangeDate;
              if (updatedResource.Status == WorkshopResourceStatus.Deleted)
              // resource has been deleted from server
              {

              }
            }
            if (updatedResource.LastFileChangeDate > localResource.LastFileChangeDate)
            // resource file list updated
            {
              localResource.UpdateStatus(WorkshopResourceStatus.NeedUpdate);
            }
            updatedResource.LastFileChangeDate = localResource.LastFileChangeDate;
            updatedResource.Status = localResource.Status;
          }
        }
        DAL.SaveResourceModels(updatedResources);
        ConfigModel.CurrentConfig.WorkshopTimestamp = serviceResult.Item1;
      }
      catch (InvalidOperationException)
      {
        throw;
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void WorkshopResource_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "Status")
        DownloadingResourcesView.Refresh();
    }
  }
}
