using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.IO;

namespace YTY.amt
{
  public static class ProgramModel
  {
    private static int build;
    private static readonly Lazy<int> build_Db = new Lazy<int>(() => DatabaseClient.Build);
    private static readonly Lazy<ObservableCollection<FileModel>> files = new Lazy<ObservableCollection<FileModel>>(
      () => new ObservableCollection<FileModel>(DatabaseClient.GetFiles().Select(f =>
      {
        var ret = FileModel.FromDto(f);
        if (ret.Status == FileStatus.Downloading)
        {
          foreach (var chunk in DatabaseClient.GetChunks(ret.Id))
          {
            chunk.FileId = ret.Id;
            ret.Chunks.Add(chunk);
          }
          ret.FinishedSize = ret.Chunks.Count(c => c.Status == ChunkStatus.Done) * WebServiceClient.CHUNKSIZE;
        }
        return ret;
      })));

    public static UpdateServerModel UpdateServerModel { get; } = new UpdateServerModel();

    public static int Build
    {
      get
      {
        if (!build_Db.IsValueCreated)
        {
          build = build_Db.Value;
        }
        return build;
      }
      set
      {
        build = value;
        DatabaseClient.Build = build;
      }
    }

    public static ObservableCollection<FileModel> Files => files.Value;

    public static async Task StartUpdate()
    {
      await UpdateServerModel.GetUpdateSourcesAsync();
      if (UpdateServerModel.Status == UpdateServerStatus.NeedUpdate)
      {
        var toSave = new List<FileModel>();
        foreach (var serverFile in UpdateServerModel.ServerFiles)
        {
          var localFile = Files.FirstOrDefault(l => l.Id == serverFile.Id);
          if (localFile == null)
          {
            var newFile = FileModel.FromDto(serverFile);
            newFile.Status = FileStatus.NotDownloaded;
            toSave.Add(newFile);
            Files.Add(newFile);
          }
          else
          {
            if (serverFile.Version > localFile.Version)
            {
              localFile.Size = serverFile.Size;
              localFile.Md5 = serverFile.Md5;
              localFile.Version = serverFile.Version;
              localFile.Status = FileStatus.NotDownloaded;
              toSave.Add(localFile);
            }
          }
        }
        DatabaseClient.SaveFiles(toSave.Select(n => n.ToDto()));
        var toDownload = new List<FileModel>();
        foreach (var file in Files.Where(f => f.Status == FileStatus.NotDownloaded || f.Status == FileStatus.Downloading || f.Status == FileStatus.Error).ToList())
        {
          toDownload.Add(file);
          await file.DownloadAsync();
        }
        if (Files.All(f => f.Status == FileStatus.Finished))
        {
          foreach (var file in Files)
          {
            if (File.Exists(file.FullFileName + ".downloading"))
            {
              File.Delete(file.FullFileName);
              File.Move(file.FullFileName + ".downloading", file.FullFileName);
            }
          }
          Build = UpdateServerModel.Build;
          UpdateServerModel.Status = UpdateServerStatus.UpToDate;
        }
      }
      Util.CreateShortcut(Util.MakeQualifiedPath("amt.exe"), Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "帝国时代管家.lnk"), "帝国时代管家");
    }
  }
}
