using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace YTY.amt
{
  public static class ProgramModel
  {
    private static int build;
    private static readonly Lazy<int> build_Db = new Lazy<int>(() => DatabaseClient.Build);
    private static readonly Lazy<ObservableCollection<FileModel>> files = new Lazy<ObservableCollection<FileModel>>(
      () => new ObservableCollection<FileModel>(DatabaseClient.GetFiles().Select(FileModel.FromDto)));

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
        Build = UpdateServerModel.Build;
        var newFiles = new List<FileModel>();
        foreach (var serverFile in UpdateServerModel.ServerFiles)
        {
          var localFile = Files.FirstOrDefault(l => l.Id == serverFile.Id);
          if (localFile == null)
          {
            var newFile = FileModel.FromDto(serverFile);
            newFile.Status = FileStatus.Ready;
            newFiles.Add(newFile);
            Files.Add(newFile);
          }
          else
          {
            if (Version.Parse( serverFile.Version )> localFile.Version)
            {
              localFile.Size = serverFile.Size;
              localFile.Md5 = serverFile.Md5;
              localFile.Version =Version.Parse( serverFile.Version);
              localFile.Status = FileStatus.Ready;
            }
          }
        }
        DatabaseClient.SaveFiles(newFiles.Select(n => n.ToDto()));
      }
      if (UpdateServerModel.Status != UpdateServerStatus.ConnectFailed && UpdateServerModel.Status != UpdateServerStatus.ServerError)
      {
        foreach (var file in Files.Where(f => f.Status == FileStatus.Ready || f.Status == FileStatus.Downloading || f.Status == FileStatus.Error))
        {
          await file.DownloadAsync();
        }
      }
    }
  }
}
