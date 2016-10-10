using System;

namespace YTY
{
  public class DownloadChunkEventArgs : EventArgs
  {
    public int Index;
    public byte[] Data;
  }
}
