using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YTY.amt.Model;

namespace YTY.amt
{
  public class FileViewModel : INotifyPropertyChanged
  {
    public ResourceFileModel Model { get; }

    private FileViewModel(ResourceFileModel model)
    {
      Model = model;
      model.PropertyChanged += Model_PropertyChanged;
    }

    private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      
    }

    public static FileViewModel FromModel(ResourceFileModel model)
    {
      return new FileViewModel(model);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
