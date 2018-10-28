using YTY.amt.Model;

namespace YTY.amt
{
  public class ModViewModel : WorkshopResourceViewModel
  {
    private ModViewModel(ModResourceModel model) : base(model) { }

    public static ModViewModel FromModel(ModResourceModel model)
    {
      return new ModViewModel(model);
    }
  }
}
