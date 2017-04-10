namespace YTY.amt.Model
{
  internal class ResourceFileDto
  {
    public int Id { get; set; }

    public int ResourceId { get; set; }

    public int Size { get; set; }

    public string Path { get; set; }

    public int UpdateDate { get; set; }

    public string Sha1 { get; set; }

    public int Status { get; set; }
  }

  internal enum FileServerStatus
  {
    Alive,
    Deleted,
  }
}
