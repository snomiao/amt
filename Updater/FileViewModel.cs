using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace YTY.amt
{
  public class FileViewModel:INotifyPropertyChanged
  {
    public FileModel Model { get; }

    public string Status
    {
      get
      {
        switch (Model.Status)
        {
          case FileStatus.Ready:
            return "未下载";
            break;
          case FileStatus.Downloading:
            return "正在下载";
            break;
          case FileStatus.Finished:
            return "下载完成";
            break;
          case FileStatus.Error:
            return "下载出错";
            break;
        }
        return string.Empty;
      }
    }

    private FileViewModel(FileModel model)
    {
      Model = model;
      model.PropertyChanged += Model_PropertyChanged;
    }

    public static FileViewModel FromModel(FileModel model)
    {
      return new FileViewModel ( model );
    }

    private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      switch (e.PropertyName)
      {
        case nameof(FileModel.Status):
          OnPropertyChanged(nameof(Status));
          break;
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
