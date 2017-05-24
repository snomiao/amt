using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Threading.Tasks;
using System.Collections.Specialized;

namespace YTY.amt
{
  public class MainViewModel:INotifyPropertyChanged
  {
    public ObservableCollection<FileViewModel> Files { get; } 
      = new ObservableCollection<FileViewModel>(ProgramModel.Files.Select(FileViewModel.FromModel));

    public ICollectionView FilesView { get; }

    public bool IsGetting => ProgramModel.UpdateServerModel.Status == UpdateServerStatus.Getting;

    public bool IsUpToDate => ProgramModel.UpdateServerModel.Status == UpdateServerStatus.UpToDate;

    public bool IsDownloading => ProgramModel.UpdateServerModel.Status == UpdateServerStatus.NeedUpdate;

    public bool IsDisconnected => ProgramModel.UpdateServerModel.Status == UpdateServerStatus.ConnectFailed;

    public bool IsServerError => ProgramModel.UpdateServerModel.Status == UpdateServerStatus.ServerError;
    
    public MainViewModel()
    {
      ProgramModel.Files.CollectionChanged += Files_CollectionChanged;
      FilesView = new CollectionViewSource { Source = Files }.View;
      FilesView.Filter = item => (item as FileViewModel).Model.Status != FileStatus.Finished;
      ProgramModel.UpdateServerModel.PropertyChanged += UpdateServerModel_PropertyChanged;
    }

    private void UpdateServerModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      switch(e.PropertyName)
      {
        case nameof(UpdateServerModel.Status):
          OnPropertyChanged(nameof(IsGetting));
          OnPropertyChanged(nameof(IsUpToDate));
          OnPropertyChanged(nameof(IsDownloading));
          OnPropertyChanged(nameof(IsDisconnected));
          OnPropertyChanged(nameof(IsServerError));
          break;
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void Files_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Add:
          foreach (FileModel model in e.NewItems)
          {
            Files.Add(FileViewModel.FromModel(model));
          }
          break;
      }
    }
  }
}
