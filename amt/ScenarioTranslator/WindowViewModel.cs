using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace YTY.amt
{
  public class ScenarioTranslatorViewModel : INotifyPropertyChanged
  {
    private List<NodeViewModel> nodes;
    private Encoding fromEncoding;
    private Encoding toEncoding;
    private List<Encoding> fromEncodings;
    private List<Encoding> toEncodings;
    private ScxFile scx;
    private bool hide;
    private bool forgot;

    public bool FileOpened { get; set; }
    public string Prefix { get; set; }

    public ScxFile Scx
    {
      get { return scx; }
      set
      {
        scx = value;
        if (scx == null)
        {
          Nodes = null;
          FileOpened = false;
        }
        else
        {
          var nodes = new List<NodeViewModel>();
          nodes.Add(new NodeViewModel() { Header = "剧情任务指示", SourceBytes = scx.Instruction });
          nodes.Add(new NodeViewModel() { Header = "任务", SourceBytes = scx.StringInfos[0] });
          nodes.Add(new NodeViewModel() { Header = "提示", SourceBytes = scx.StringInfos[1] });
          nodes.Add(new NodeViewModel() { Header = "胜利", SourceBytes = scx.StringInfos[2] });
          nodes.Add(new NodeViewModel() { Header = "失败", SourceBytes = scx.StringInfos[3] });
          nodes.Add(new NodeViewModel() { Header = "历史", SourceBytes = scx.StringInfos[4] });
          nodes.Add(new NodeViewModel() { Header = "侦察", SourceBytes = scx.StringInfos[5] });
          for (var i = 0; i < scx.PlayerCount; i++)
          {
            nodes.Add(new NodeViewModel() { Header = $"玩家 {i + 1} 名称", SourceBytes = scx.Players[i].Name });
          }
          for (var i = 0; i < scx.Triggers.Count; i++)
          {
            var node = new NodeViewModel(false) { Header = $"触发 {i + 1}", Flags = NodeType.Trigger };
            if (Convert.ToBoolean(scx.Triggers[i].IsObjective))
              node.Flags |= NodeType.IsObjective;
            node.Children.Add(new NodeViewModel() { Header = "名称", SourceBytes = scx.Triggers[i].Name, Flags = NodeType.TriggerName });
            node.Children.Add(new NodeViewModel() { Header = "描述", SourceBytes = scx.Triggers[i].Discription,Flags= NodeType.TriggerDesc });
            for (var j = 0; j < scx.Triggers[i].Effects.Count; j++)
            {
              switch (scx.Triggers[i].Effects[j].Type)
              {
                case EffectType.SendChat:
                  node.Children.Add(new NodeViewModel() { Header = $"效果 {j}：送出聊天", SourceBytes = scx.Triggers[i].Effects[j].Text, Flags= NodeType.TriggerContent });
                  break;
                case EffectType.DisplayInstructions:
                  node.Children.Add(new NodeViewModel() { Header = $"效果 {j}：显示指示", SourceBytes = scx.Triggers[i].Effects[j].Text, Flags = NodeType.TriggerContent });
                  break;
                case EffectType.ChangeObjectName:
                  node.Children.Add(new NodeViewModel() { Header = $"效果 {j}：改变物件名称", SourceBytes = scx.Triggers[i].Effects[j].Text, Flags = NodeType.TriggerContent });
                  break;
              }
            }
            nodes.Add(node);
          }
          Nodes = nodes;
          FileOpened = true;
        }
        OnPropertyChanged(nameof(Scx));
        OnPropertyChanged(nameof(FileOpened));
      }
    }

    public List<NodeViewModel> GetAllNodes()
    {
      return nodes.Concat(nodes.SelectMany(n => n.Children)).ToList();
    }

    public List<Encoding> FromEncodings
    {
      get
      {
        if (fromEncodings == null)
        {
          fromEncodings = Encoding.GetEncodings().Select(e => Encoding.GetEncoding(e.CodePage, EncoderFallback.ReplacementFallback, DecoderFallback.ExceptionFallback)).ToList();
        }
        return fromEncodings;
      }
    }

    public List<Encoding> ToEncodings
    {
      get
      {
        if (toEncodings == null)
        {
          toEncodings = Encoding.GetEncodings().Select(e => Encoding.GetEncoding(e.CodePage, EncoderFallback.ExceptionFallback, DecoderFallback.ReplacementFallback)).ToList();
        }
        return toEncodings;
      }
    }

    public Encoding FromEncoding
    {
      get { return fromEncoding; }
      set
      {
        fromEncoding = value;
        OnPropertyChanged(nameof(FromEncoding));
      }
    }

    public Encoding ToEncoding
    {
      get { return toEncoding; }
      set
      {
        toEncoding = value;
        OnPropertyChanged(nameof(ToEncoding));
      }
    }

    public List<NodeViewModel> Nodes
    {
      get { return nodes; }
      set
      {
        nodes = value;
        OnPropertyChanged(nameof(Nodes));
      }
    }

    public bool Hide
    {
      get { return hide; }
      set
      {
        hide = value;
        OnPropertyChanged(nameof(Hide));
      }
    }

    public bool Forgot
    {
      get { return forgot; }
      set
      {
        forgot = value;
        OnPropertyChanged(nameof(Forgot));
      }
    }

    public ScenarioTranslatorViewModel()
    {
      Prefix = "触发事件 ";
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
