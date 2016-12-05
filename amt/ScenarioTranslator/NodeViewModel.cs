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
    private string header;
    private byte[] sourceBytes;
    private string source;
    private byte[] toBytes;
    private string to;
    private List<NodeViewModel> children;
    private Visibility visibility;
    private string toolTipText;

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

    public string To
    {
      get { return to; }
      set
      {
        to = value;
        OnPropertyChanged(nameof(To));
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

    public string ToolTipText
    {
      get { return toolTipText; }
      set
      {
        toolTipText = value;
        OnPropertyChanged(nameof(ToolTipText));
      }
    }

    public NodeViewModel(bool hasContent=true)
    {
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
        ToolTipText = $"【警告】位于字节流位置 {ex.Index} 处的字节 {BitConverter.ToString(ex.BytesUnknown)} 无法使用 {My.ScenarioTranslatorViewModel.FromEncoding.EncodingName} 解码。\n若这是一个错误，请考虑改变原文编码。";
        Source = Encoding.GetEncoding(My.ScenarioTranslatorViewModel.FromEncoding.CodePage).GetString(sourceBytes).TrimEnd('\0');
      }
    }

    private void ScenarioTranslatorViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(ScenarioTranslatorViewModel.FromEncoding))
      {
        SetSource();
        OnPropertyChanged(nameof(SourceError));
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
