using System;

namespace YTY
{
  public class DownloadChunkEventArgs : EventArgs
  {
    public int Index { get; set; }
    public byte[] Data { get; set; }
  }
}
