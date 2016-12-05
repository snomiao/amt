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

    public int FileId { get; set; }

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

    public void UpdateFinished(bool finished)
    {
      Finished = finished;
      DAL.UpdateFileChunkFinished(FileId, Id, finished);
    }

    public byte[] Data { get; private set; }

    public async Task<FileChunkModel> DownloadAsync()
    {
      Data = await DAL.GetChunk(FileId, Id);
      Finished = true;
      return this;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
