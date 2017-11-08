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

    private ToolGroupModel(IEnumerable<ToolModel> initial)
    {
      Tools = new ObservableCollection<ToolModel>(initial);
    }

    internal static readonly ToolGroupModel[] BuiltInToolGroups =
    {
      new ToolGroupModel(new []
      {
        new ToolModel
        {
          Id=-1,
          Name ="触发工作室 1.0",
          Path =@"AoKTS\aoktschs.exe",
          IconPath ="/resources;component/Resources/触发工作室.png",
          ToolTip ="打开【触发工作室】(Trigger Studio) 程序。\n该程序可以编辑帝国时代的 scx、scn 格式场景文件，并拥有地图编辑器以外的多种强大功能。",
        },
        new ToolModel
        {
          Id=-1,
          Name="触发工作室 1.2",
          Path=@"tsup\tschs.exe",
          IconPath="/resources;component/Resources/触发工作室2.png",
          ToolTip="打开【触发工作室】(Trigger Studio) 程序 1.2 版。\n该程序适用于编辑 UserPatch 1.4和 HD 版本的scx格式场景文件。",
        },
        new ToolModel
        {
          Id=-1,
          Name="战役管理器",
          Path=@"cpnman\cpnman.exe",
          IconPath="/resources;component/Resources/战役管理器.png",
          ToolTip="打开【战役管理器】程序。\n该程序可以将帝国时代的 CPX、CPN 格式战役文件分解为可在地图编辑器中编辑的 SCX、SCN 格式文件。",
        },
        new ToolModel
        {
          Id=-1,
          Name="单位ID修改器",
          Path=@"AocScenarioUnitIdEditor\AocScenarioUnitIdEditor.exe",
          IconPath="/resources;component/Resources/ID修改器.png",
          ToolTip="打开【单位ID修改器】程序。\n该程序可以修改帝国时代场景中单位的ID。",
        },
        new ToolModel
        {
          Id=-1,
          Name="场景翻译器",
          Path=@"AocScenarioTranslator\AocScenarioTranslator.exe",
          IconPath="/resources;component/Resources/修改器.png",
          ToolTip="打开【场景翻译器】程序。\n该程序可以用各种编码载入并翻译场景的信息、玩家名称和触发文本。",
        },
      })
      {
        Id=-4,
        Name ="战役与场景",
      },
      new ToolGroupModel(new[]
      {
        new ToolModel
        {
          Id=-1,
          Name="AI Editor",
          Path=@"AI Editor\AIEditor.exe",
          IconPath="/resources;component/Resources/AI-Editor.png",
          ToolTip="打开【AI Editor】程序。\n该程序可以编辑人工智能脚本，支持代码自动完成等多种拓展功能，内置建造城墙插件。内置AI Builder插件，无需输入，自动生成人工智能代码，且支持模板功能。",
        },
        new ToolModel
        {
          Id=-1,
          Name="AI Script Builder",
          Path=@"AI Script Builder\AOKAISB.exe",
          IconPath="/resources;component/Resources/AI-Script-Builder.png",
          ToolTip="打开【AI Script Builder】程序。\n该程序可以通过设置向导生成人工智能脚本。内置建造城墙插件。",
        },
        new ToolModel
        {
          Id=-1,
          Name="SetupAI",
          Path=@"SetupAI\SetupAI.exe",
          IconPath="/resources;component/Resources/SetupAI.png",
          ToolTip="打开【SetupAI】程序（英文软件）。\n该程序可以通过填写一系列参数生成人工智能脚本。",
        },
        new ToolModel
        {
          Id=-1,
          Name="RMS 编辑器",
          Path=@"RMSEdit\RMSEdit.exe",
          IconPath="/resources;component/Resources/RMS编辑器.png",
          ToolTip="打开【RMS 编辑器】程序。\n该程序可以编辑随机地图脚本，内置自动创建向导，并可以根据已有语句模板进行修改。",
        },
        new ToolModel
        {
          Id=-1,
          Name="随机地图脚本生成器",
          Path=@"RMSG.exe",
          IconPath="/resources;component/Resources/RMS编辑器3.png",
          ToolTip="打开【RMS 生成器】程序。\n该程序可以通过填写一系列参数生成随机地图脚本，并可以编辑脚本细节。",
        },
        new ToolModel
        {
          Id=-1,
          Name="UserPatch 1.3脚本编写参考",
          Path=@"UserPatch1.3Reference.docx",
          IconPath="/resources;component/Resources/RMS编辑器2.png",
          ToolTip="打开【UserPatch 1.3 脚本编写参考】Word 文档。\n该文档囊括了 UserPatch 1.3 相对于 1.0C 版在 AI 与随机地图脚本方面的改进。",
        },
      })
      {
        Id=-3,
        Name="AI与随机地图",
      },
      new ToolGroupModel(new []
      {
        new ToolModel
        {
          Id=-1,
          Name="乌龟包",
          Path=@"Turtle Pack\Turtle Pack.exe",
          IconPath="/resources;component/Resources/乌龟包.png",
          ToolTip="打开【乌龟包】程序。\n该程序是一套修改帝国时代游戏资料的软件组合，包含三个子程序SLP Editor 2.9.3，DRS Editor 1.6.3，Animation Preview 1.3。\nDRS 编辑器：可以修改游戏的DRS资料档。\nSLP 编辑器：可以修改游戏SLP图像。\n动画预览：可以替 SLP 图片的视觉效果进行评测。",
        },
        new ToolModel
        {
          Id=-1,
          Name="高级数据编辑器2017.11.7(英文)",
          Path=@"AGE\AdvancedGenieEditor3.exe",
          IconPath="/resources;component/Resources/AGE.png",
          ToolTip="打开【高级数据编辑器】(Advanced Genie Editor，AGE) 2017.11.7 版。\n该程序可以修改帝国时代的数据库DAT文件。",
        },
        new ToolModel
        {
          Id=-1,
          Name="高级数据编辑器2015(汉化)",
          Path=@"AGE\AdvancedGenieEditor3CHS.exe",
          IconPath="/resources;component/Resources/AGE2.png",
          ToolTip="打开【高级数据编辑器】(Advanced Genie Editor，AGE) 2015 (汉化)版。\n该程序可以修改帝国时代的数据库DAT文件。",
        },
        //new ToolModel
        //{
        //  Id=-1,
        //  Name="语言DLL编辑器",
        //  Path="",
        //  IconPath="/resources;component/Resources/语言DLL.png",
        //  ToolTip="打开【语言DLL编辑器】内置工具。\n该程序可以修改帝国时代的语言DLL文件。",
        //},
        new ToolModel
        {
          Id=-1,
          Name="单位ID对应表",
          Path=@"aokunit.xls",
          IconPath="/resources;component/Resources/ID修改器.png",
          ToolTip="打开【单位 ID 对应表】Excel 文档。\n该表包括帝国时代单位名称、内部名称、单位ID。",
        },
      })
      {
        Id=-2,
        Name="MOD",
      },
      new ToolGroupModel(new []
      {
        new ToolModel
        {
          Id=-1,
          Name="录像浏览器",
          Path=@"Recorded Game Explorer\RecordedGameExplorer.exe",
          IconPath="/resources;component/Resources/录像1.png",
          ToolTip="打开【录像浏览器】程序。\n本程序可以读取录像文件的玩家名称、民族、组队、聊天内容、地图等信息。",
        },
        new ToolModel
        {
          Id=-1,
          Name="录像分析器",
          Path=@"AoERecAnalyzer.exe",
          IconPath="/resources;component/Resources/录像1.png",
          ToolTip="打开【录像分析器】程序。\n本程序可以解析录像文件的格式内容，分析录像文件中的部分信息，支持文件重命名功能，并可以自定命名格式。",
        },
        new ToolModel
        {
          Id=-1,
          Name="1.0C 数据修改器",
          Path=@"AoCEditor\AOCEditor1.5.exe",
          IconPath="/resources;component/Resources/修改器.png",
          ToolTip="打开【1.0C 数据修改器】程序。\n本程序必须在帝国时代2主程序运行时才可以打开！\n本程序仅能应用于 1.0C 版！\n本程序通过修改内存，可以修改帝国时代2的实时运行数据，用于场景调试及单机游戏作弊。\n禁止在多人游戏中使用，会产生同步错误。",
        },
      })
      {
        Id=-1,
        Name="录像及其他",
      },
    };
}
}
