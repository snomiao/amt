using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace YTY.amt
{
  public class UpdateSourceViewModel:ViewModelBase
  {
    private XElement xe;

    public string Uri
    {
      get
      {
        return xe.Value;
      }
    }

    public UpdateSourceViewModel(XElement xe)
    {
      this.xe = xe;
    }
  }
}
