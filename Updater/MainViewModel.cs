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
  public class MainViewModel
  {
    public ObservableCollection<FileViewModel> Files { get; } 
      = new ObservableCollection<FileViewModel>(ProgramModel.Files.Select(FileViewModel.FromModel));

    public ICollectionView FilesView { get; }

    public MainViewModel()
    {
      ProgramModel.Files.CollectionChanged += Files_CollectionChanged;
      FilesView = new CollectionViewSource { Source = Files }.View;
      FilesView.Filter = item => (item as FileViewModel).Model.Status != FileStatus.Finished;
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
