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
      var builtIn = new LanguageResourceModel
      {
        Id = -1,
        Name = "中文",
        Files = {new ResourceFileModel
      {
        Id = -1,
        Path = @"manager\dll\zh\language.dll",
      },new ResourceFileModel
      {
        Id = -2,
        Path = @"manager\dll\zh\language_x1.dll",
      } ,new ResourceFileModel
      {
        Id = -3,
        Path = @"manager\dll\zh\language_x1_p1.dll",
      },new ResourceFileModel
      {
        Id=-4,
        Path=@"manager\dll\zh\language_x1_p1_wk.dll",
      },},
      };
      BuiltInLanguages.Add(builtIn);
      builtIn = new LanguageResourceModel
      {
        Id = -2,
        Name = "英语",
        Files =        {          new ResourceFileModel
      {
        Id = -1,
        Path = @"manager\dll\en\language.dll",
      },new ResourceFileModel
      {
        Id = -2,
        Path = @"manager\dll\en\language_x1.dll",
      },new ResourceFileModel
      {
        Id = -3,
        Path = @"manager\dll\en\language_x1_p1.dll",
      },new ResourceFileModel
      {
        Id=-4,
        Path=@"manager\dll\en\language_x1_p1_wk.dll",
      },        },
      };
      BuiltInLanguages.Add(builtIn);
      builtIn = new LanguageResourceModel
      {
        Id = -3,
        Name = "自定义",
        Files =        {          new ResourceFileModel
      {
        Id = -1,
        Path = @"manager\dll\ini\language.dll",
      },new ResourceFileModel
      {
        Id = -2,
        Path = @"manager\dll\ini\language_x1.dll",
      },new ResourceFileModel
      {
        Id = -3,
        Path = @"manager\dll\ini\language_x1_p1.dll",
      },new ResourceFileModel
      {
        Id=-4,
        Path=@"manager\dll\ini\language.dll",
      },        },
      };
      BuiltInLanguages.Add(builtIn);
    }

    public void Activate()
    {
      if (IsBuiltIn)
      {
        CopyLanguageDll(-1, "", "language.dll");
        CopyLanguageDll(-2, "","language_x1.dll");
        CopyLanguageDll(-3, "","language_x1_p1.dll");
        CopyLanguageDll(-4, @"games\wololokingdoms\data","language_x1_p1.dll");
      }
      else
      {
        //TODO
      }
    }

    private void CopyLanguageDll(int id, string dstPath,string dstName)
    {
      var fm = Files.FirstOrDefault(f => f.Id == id);
      if (fm != null)
      {
        try
        {
          File.Copy(fm.FullPathName,
            Path.Combine(ProgramModel.MakeHawkempirePath(dstPath), dstName),
            true);
        }
        catch (IOException)
        {

        }
      }
    }
  }
}
