using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Threading;

namespace YTY.amt.Model
{
  public class FileChunkModel : INotifyPropertyChanged
  {
    private bool finished;

    public int FileId { get; set; }

    public int Id { get; set; }

    public bool Finished
    {
      get { return finished; }
      set
      {
        finished = value;
        OnPropertyChanged(nameof(Finished));
      }
    }

    public async Task<(int Id,byte[] Data)> DownloadAsync(CancellationToken cancellationToken)
    {
      var data = await WebServiceClient.GetChunk(FileId, Id);
      cancellationToken.ThrowIfCancellationRequested();
      Finished = true;
      return (Id,data);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
