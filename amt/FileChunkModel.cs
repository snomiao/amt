using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace YTY.amt
{
  public class FileChunkModel : INotifyPropertyChanged
  {
    private bool finished;

    public uint FileId { get; set; }

    public int Id { get; set; }

    public bool Finished
    {
      get { return finished; }
      private set
      {
        finished = value;
        OnPropertyChanged(nameof(Finished));
      }
    }

    public byte[] Data { get; private set; }

    public async Task DownloadAsync()
    {
      Data = await DAL.GetChunk(FileId, Id);
      Finished = true;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
