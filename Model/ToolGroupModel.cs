using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace YTY.amt.Model
{
  public class ToolGroupModel
  {
    public int Id { get; set; }

    public string Name { get; set; }

    public ObservableCollection<ToolModel> Tools { get; } 

    internal ToolGroupModel()
    {
      Tools= new ObservableCollection<ToolModel>();
    }

    internal ToolGroupModel(IEnumerable<ToolModel> initial)
    {
      Tools = new ObservableCollection<ToolModel>(initial);
    }
  }
}
