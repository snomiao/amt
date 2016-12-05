using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows;

namespace YTY.amt
{
  public class NodeViewModel : INotifyPropertyChanged
  {
    private bool sourceError;
    private bool destError;
    private string header;
    private byte[] sourceBytes;
    private string source;
    private byte[] toBytes;
    private string to;
    private List<NodeViewModel> children;
    private Visibility visibility;
    private string sourceErrorText;
    private string destErrorText;

    public bool HasContent { get; set; }

    public string Header
    {
      get { return header; }
      set
      {
        header = value;
        OnPropertyChanged(nameof(Header));
      }
    }

    public byte[] SourceBytes
    {
      get { return sourceBytes; }
      set
      {
        sourceBytes = value;
        SetSource();
      }
    }

    public string Source
    {
      get { return source; }
      set
      {
        source = value;
        OnPropertyChanged(nameof(Source));
      }
    }

    public bool SourceError
    {
      get { return sourceError; }
      set
      {
        sourceError = value;
        OnPropertyChanged(nameof(SourceError));
      }
    }

    public bool DestError
    {
      get { return destError; }
      set
      {
        destError = value;
        OnPropertyChanged(nameof(DestError));
      }
    }

    public string To
    {
      get { return to; }
      set
      {
        to = value;
        OnPropertyChanged(nameof(To));
        OnSetDest();
      }
    }

    public List<NodeViewModel> Children
    {
      get { return children; }
      set
      {
        children = value;
        OnPropertyChanged(nameof(Children));
      }
    }

    public Visibility Visibility
    {
      get { return visibility; }
      set
      {
        visibility = value;
        OnPropertyChanged(nameof(Visibility));
      }
    }

    public string SourceErrorText
    {
      get { return sourceErrorText; }
      set
      {
        sourceErrorText = value;
        OnPropertyChanged(nameof(SourceErrorText));
      }
    }

    public string DestErrorText
    {
      get { return destErrorText; }
      set
      {
        destErrorText = value;
        OnPropertyChanged(nameof(DestErrorText));
      }
    }

    public NodeType Flags { get; set; }

    public NodeViewModel(bool hasContent = true)
    {
      children = new List<NodeViewModel>();
      HasContent = hasContent;
      My.ScenarioTranslatorViewModel.PropertyChanged += ScenarioTranslatorViewModel_PropertyChanged;
    }

    private void SetSource()
    {
      if (!HasContent) return;
      try
      {
        SourceError = false;
        Source = My.ScenarioTranslatorViewModel.FromEncoding.GetString(sourceBytes).TrimEnd('\0');
      }
      catch (DecoderFallbackException ex)
      {
        SourceError = true;
        SourceErrorText = $"【警告】位于字节流位置 {ex.Index} 处的字节 {BitConverter.ToString(ex.BytesUnknown)} 无法使用 {My.ScenarioTranslatorViewModel.FromEncoding.EncodingName} 解码。\n请检查原文编码是否合适。";
        Source = Encoding.GetEncoding(My.ScenarioTranslatorViewModel.FromEncoding.CodePage).GetString(sourceBytes).TrimEnd('\0');
      }
    }

    private void OnSetDest()
    {
      try
      {
        DestError = false;
        toBytes = My.ScenarioTranslatorViewModel.ToEncoding.GetBytes(To + '\0');
      }
      catch (EncoderFallbackException ex)
      {
        DestError = true;
        DestErrorText = $"【警告】位于字符串位置 {ex.Index} 处的字符 {ex.CharUnknown} 无法使用 {My.ScenarioTranslatorViewModel.ToEncoding.EncodingName} 编码。\n请检查译文字符串是否错误，译文编码是否合适。";
        toBytes = Encoding.GetEncoding(My.ScenarioTranslatorViewModel.ToEncoding.CodePage).GetBytes(To + '\0');
      }
    }

    private void ScenarioTranslatorViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      switch (e.PropertyName)
      {
        case nameof(ScenarioTranslatorViewModel.FromEncoding):
          SetSource();
          OnPropertyChanged(nameof(SourceError));
          break;
        case nameof(ScenarioTranslatorViewModel.ToEncoding):
          OnSetDest();
          OnPropertyChanged(nameof(DestError));
          break;
        case nameof(ScenarioTranslatorViewModel.Hide):
          Visibility = CalcVisibility();
          break;
      }
    }

    private Visibility CalcVisibility()
    {
      if (!My.ScenarioTranslatorViewModel.Hide)
        return Visibility.Visible;
      if (Flags.HasFlag(NodeType.TriggerName))
        return Visibility.Collapsed;
      if (Flags.HasFlag(NodeType.TriggerDesc) || Flags.HasFlag(NodeType.TriggerContent))
        return string.IsNullOrWhiteSpace(Source) ? Visibility.Collapsed : Visibility.Visible;
      if (!Flags.HasFlag(NodeType.Trigger))
        return Visibility.Visible;
      if (Flags.HasFlag(NodeType.IsObjective))
        return Visibility.Visible;
      if (Children.Where(n => n.Flags.HasFlag(NodeType.TriggerContent) || n.Flags.HasFlag(NodeType.TriggerDesc)).Any(n => !string.IsNullOrWhiteSpace(n.Source)))
        return Visibility.Visible;
      return Visibility.Collapsed;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }

  [Flags]
  public enum NodeType
  {
    None = 0,
    Trigger = 1,
    TriggerName = 2,
    TriggerContent = 4,
    IsObjective = 8,
    TriggerDesc = 0x10
  }
}
