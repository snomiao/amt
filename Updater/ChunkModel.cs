using System.ComponentModel;

namespace YTY.amt
{
  public class ChunkModel:INotifyPropertyChanged
  {
    private ChunkStatus status;

    public int FileId { get; set; }

    public int Id { get; set; }

    public ChunkStatus Status
    {
      get { return status; }
      set
      {
        status = value;
        OnPropertyChanged(nameof(Status));
        DatabaseClient.UpdateChunk(this);
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }

  public enum ChunkStatus
  {
    New,
    Done,
  }
}