using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTY.amt
{
  public class ResourceFileModel
  {
    public uint Id { get; set; }

    public uint Size { get; set; }

    public string Path { get; set; }

    public DateTime UpdateDate { get; set; }

    public string Sha1 { get; set; }

    public bool Finished { get; set; }

    public List<FileChunkModel> Chunks { get; set; }

    public async Task DownloadAsync()
    {
      
    }

    public void MakeChunks()
    {
      Chunks = Enumerable.Range(0, (int)(Size + DAL.CHUNKSIZE - 1) / DAL.CHUNKSIZE).Select(i => new FileChunkModel() { FileId = Id, Id = i, Finished = false }).ToList();
    }
  }
}
