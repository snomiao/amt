using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using YTY.amt.Model;

namespace YTY.amt
{
  public class DrsViewModel : WorkshopResourceViewModel
  {
    private DrsResourceModel DrsModel => Model as DrsResourceModel;

    public override string ButtonText =>
      Model.Status == WorkshopResourceStatus.Installed ?
      DrsModel.IsActivated ? "停用该模组" : "启用该模组"
      : base.ButtonText;

    public override SolidColorBrush ButtonBackground =>
      Model.Status == WorkshopResourceStatus.Installed ?
      DrsModel.IsActivated ? Brushes.Gray : Brushes.Blue
      : base.ButtonBackground;

    public override ICommand Command =>
      Model.Status == WorkshopResourceStatus.Installed ?
      DrsModel.IsActivated ? WorkshopCommands.DeactivateResource : WorkshopCommands.ActivateResource
      : base.Command;

    private DrsViewModel(DrsResourceModel model) : base(model) { }

    public static DrsViewModel FromModel(DrsResourceModel model)
    {
      return new DrsViewModel(model);
    }
  }
}
