namespace YTY.amt.Model
{
  internal class WorkshopResourceDto
  {
    public int Id { get; set; }

    public WorkshopResourceType Type { get; set; }

    public int CreateDate { get; set; }

    public string Name { get; set; }

    public int Rating { get; set; }

    public long TotalSize { get; set; }

    public int LastChangeDate { get; set; }

    public int LastFileChangeDate { get; set; }

    public int AuthorId { get; set; }

    public string AuthorName { get; set; }

    public string Description { get; set; }

    public GameVersion GameVersion { get; set; }

    public int DownloadCount { get; set; }

    public string SourceUrl { get; set; }

    public int Status { get; set; }

    public bool DeletePending { get; set; }

    public bool IsActivated { get; set; }

    public string ExePath { get; set; }

    public string XmlPath { get; set; }

    public string FolderPath { get; set; }
  }
}
