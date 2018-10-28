using System.Diagnostics;

namespace YTY.amt.Model
{
  public class ToolModel:WorkshopResourceModel
  {
    public string Path { get; set; }

    public string IconPath { get; set; }

    public string ToolTip { get; set; }

    public void Open()
    {
      var exe = System.IO.Path.Combine(ProgramModel.MakeExeRelativePath("tools"), Path);
      Process.Start(new ProcessStartInfo(exe)
      {
        WorkingDirectory =System.IO.Path.GetDirectoryName(exe),
        UseShellExecute = true,
      });
    }
  }
}
