using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace YTY.amt.Model
{
  public class LanguageResourceModel : WorkshopResourceModel
  {
    internal static readonly List<LanguageResourceModel> BuiltInLanguages = new List<LanguageResourceModel>();

    static LanguageResourceModel()
    {
      var builtIn = new LanguageResourceModel { Id = -1, Name = "中文" };
      builtIn.Files.Add(new ResourceFileModel { Path = @"manager\dll\zh\language.dll" });
      builtIn.Files.Add(new ResourceFileModel { Path = @"manager\dll\zh\language_x1.dll" });
      builtIn.Files.Add(new ResourceFileModel { Path = @"manager\dll\zh\language_x1_p1.dll" });
      BuiltInLanguages.Add(builtIn);
      builtIn = new LanguageResourceModel { Id = -2, Name = "英语" };
      builtIn.Files.Add(new ResourceFileModel { Path = @"manager\dll\en\language.dll" });
      builtIn.Files.Add(new ResourceFileModel { Path = @"manager\dll\en\language_x1.dll" });
      builtIn.Files.Add(new ResourceFileModel { Path = @"manager\dll\en\language_x1_p1.dll" });
      BuiltInLanguages.Add(builtIn);
      builtIn = new LanguageResourceModel { Id = -3, Name = "自定义" };
      builtIn.Files.Add(new ResourceFileModel { Path = @"manager\dll\ini\language.dll" });
      builtIn.Files.Add(new ResourceFileModel { Path = @"manager\dll\ini\language_x1.dll" });
      builtIn.Files.Add(new ResourceFileModel { Path = @"manager\dll\ini\language_x1_p1.dll" });
      BuiltInLanguages.Add(builtIn);
    }

    public void Activate()
    {
      foreach (var file in Files)
      {
        File.Copy(file.FullPathName,
          Path.Combine(ProgramModel.MakeHawkempirePath(string.Empty), Path.GetFileName(file.FullPathName)),
          true);
      }
    }
  }
}
