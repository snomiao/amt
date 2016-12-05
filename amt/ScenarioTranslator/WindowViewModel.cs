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
    private List<Encoding> encodings;
    private ScxFile scx;

    public ScxFile Scx
    {
      get { return scx; }
      set
      {
        scx = value;
        var nodes = new List<NodeViewModel>();
        nodes.Add(new NodeViewModel() { Header = "剧情任务指示", SourceBytes = scx.Instruction });
        nodes.Add(new NodeViewModel() { Header = "任务", SourceBytes = scx.StringInfos[0] });
        nodes.Add(new NodeViewModel() { Header = "提示", SourceBytes = scx.StringInfos[1] });
        nodes.Add(new NodeViewModel() { Header = "胜利", SourceBytes = scx.StringInfos[2] });
        nodes.Add(new NodeViewModel() { Header = "失败", SourceBytes = scx.StringInfos[3] });
        nodes.Add(new NodeViewModel() { Header = "历史", SourceBytes = scx.StringInfos[4] });
        nodes.Add(new NodeViewModel() { Header = "侦察", SourceBytes = scx.StringInfos[5] });
        for(var i = 0;i<scx.PlayerCount;i++)
        {
          nodes.Add(new NodeViewModel() { Header = $"玩家 {i+1} 名称", SourceBytes = scx.Players[i].Name });
        }
        for (var i = 0; i < scx.Triggers.Count; i++)
        {
          var node = new NodeViewModel(false) { Header = $"触发 {i + 1}" };
          node.Children = new List<NodeViewModel>();
          node.Children.Add(new NodeViewModel() { Header = "名称", SourceBytes = scx.Triggers[i].Name });
          node.Children.Add(new NodeViewModel() { Header = "描述", SourceBytes = scx.Triggers[i].Discription });
          for (var j = 0; j < scx.Triggers[i].Effects.Count; j++)
          {
            switch (scx.Triggers[i].Effects[j].Type)
            {
              case EffectType.SendChat:
                node.Children.Add(new NodeViewModel() { Header = $"效果 {j}：送出聊天", SourceBytes = scx.Triggers[i].Effects[j].Text });
                break;
              case EffectType.DisplayInstructions:
                node.Children.Add(new NodeViewModel() { Header = $"效果 {j}：显示指示", SourceBytes = scx.Triggers[i].Effects[j].Text });
                break;
              case EffectType.ChangeObjectName:
                node.Children.Add(new NodeViewModel() { Header = $"效果 {j}：改变物件名称", SourceBytes = scx.Triggers[i].Effects[j].Text });
                break;
            }
          }
          nodes.Add(node);
        }
        Nodes = nodes;
      }
    }

    public List<Encoding> Encodings
    {
      get
      {
        if (encodings == null)
        {
          encodings = Encoding.GetEncodings().Select(e => Encoding.GetEncoding(e.CodePage, EncoderFallback.ReplacementFallback, DecoderFallback.ExceptionFallback)).ToList();
        }
        return encodings;
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

    public List<NodeViewModel> Nodes
    {
      get { return nodes; }
      set
      {
        nodes = value;
        OnPropertyChanged(nameof(Nodes));
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
