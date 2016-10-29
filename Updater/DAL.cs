using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;

namespace YTY.amt
{
  public class DAL
  {
    private static string CONFIGDATABASEPATH;
    private SqliteOp sqlite;

    private int? build;

    static DAL()
    {
      CONFIGDATABASEPATH = Util.MakeQualifiedPath("Config.db");
    }

    public DAL()
    {
      if (System.IO.File.Exists(CONFIGDATABASEPATH))
        sqlite = new SqliteOp(CONFIGDATABASEPATH);
      else
      {
        sqlite = new SqliteOp(CONFIGDATABASEPATH);
        sqlite.ExecuteNonQuery(@"CREATE TABLE Meta(build int);
CREATE TABLE Files(id int PRIMARY KEY,sourceUri text,fileName text,status int,size int,version text,md5 text);
CREATE TABLE Chunks(updateItemId int,idx int,status int,PRIMARY KEY(updateItemId,idx));
INSERT INTO Meta(build) VALUES(0);");
      }
      sqlite.ExecuteNonQuery("PRAGMA journal_mode=memory;PRAGMA synchronous=off;");
    }

    public int Build
    {
      get
      {
        if (!build.HasValue)
          build = sqlite.ExecuteScalar<int>("SELECT build FROM Meta");
        return build.Value;
      }
      set
      {
        build = value;
        sqlite.ExecuteNonQuery($"UPDATE Meta SET build={build}");
      }
    }

    public void CreateUpdateItem(UpdateItemViewModel updateItem)
    {
      sqlite.ExecuteNonQuery($"INSERT INTO Files(id,sourceUri,fileName,status) VALUES({updateItem.Id},'{updateItem.SourceUri}','{updateItem.FileName}',{(int)updateItem.Status})");
    }

    public void SetUpdateItemSize(UpdateItemViewModel updateItem)
    {
      sqlite.ExecuteNonQuery($"UPDATE Files SET size={updateItem.Size} WHERE id={updateItem.Id}");
    }

    public void SetUpdateItemStatus(UpdateItemViewModel updateItem)
    {
      sqlite.ExecuteNonQuery($"UPDATE Files SET status={(int)updateItem.Status} WHERE id={updateItem.Id}");
    }

    public void SetUpdateItemVersion(UpdateItemViewModel updateItem)
    {
      sqlite.ExecuteNonQuery($"UPDATE Files SET version='{updateItem.Version}' WHERE id={updateItem.Id}");
    }

    public void SetUpdateItemMD5(UpdateItemViewModel updateItem)
    {
      sqlite.ExecuteNonQuery($"UPDATE Files SET md5='{updateItem.MD5}' WHERE id={updateItem.Id}");
    }

    public IEnumerable<UpdateItemViewModel> GetUpdateItems()
    {
      return sqlite.GetDataTable("SELECT * FROM Files").AsEnumerable().ToList().
         Select(row => new UpdateItemViewModel(
           id: row.Field<int>("id"),
           sourceUri: row.Field<string>("sourceUri"),
           fileName: row.Field<string>("fileName"),
           size: row.Field<int>("size"),
           version: new Version(row.Field<string>("version")),
           md5: row.Field<string>("md5"),
           status: row.Field<UpdateItemStatus>("status"),
           chunks: GetChunks(row.Field<int>("id"))));
    }

    public void SaveUpdateItems(IEnumerable<UpdateItemViewModel> updateItems)
    {
      sqlite.ExecuteNonQueryTransaction(updateItems.Select(item => $"INSERT INTO Files(id,sourceUri,fileName,status,size,version,md5) VALUES({item.Id},'{item.SourceUri.Replace("'", "''")}','{item.FileName.Replace("'", "''")}',{(int)item.Status},{item.Size},'{item.Version}','{item.MD5}')"));
    }

    public void SaveChunks(IEnumerable<ChunkViewModel> chunks)
    {
      sqlite.ExecuteNonQueryTransaction(chunks.Select(item => $"INSERT INTO Chunks(updateItemId,idx,status) VALUES({item.UpdateItemId},{item.Index},{(int)item.Status})"));
    }

    public IEnumerable<ChunkViewModel> GetChunks(int updateItemId)
    {
      return sqlite.GetDataTable($"SELECT idx,status FROM Chunks WHERE updateItemId={updateItemId}").AsEnumerable().ToList().
        Select(row => new ChunkViewModel(
          updateItemId: updateItemId,
          index: row.Field<int>("idx"),
          status: row.Field<DownloadChunkStatus>("status")));
    }

    public void SetChunkStatus(ChunkViewModel chunk, DownloadChunkStatus status)
    {
      sqlite.ExecuteNonQuery($"UPDATE Chunks SET status={(int)status} WHERE updateItemId={chunk.UpdateItemId} AND idx={chunk.Index}");
    }

    public void CreateChunk(ChunkViewModel chunk)
    {
      sqlite.ExecuteNonQuery($"INSERT INTO Chunks(updateItemId,idx,status) VALUES({chunk.UpdateItemId},{chunk.Index},{(int)chunk.Status})");
    }

    public void DeleteChunks(UpdateItemViewModel updateItem)
    {
      sqlite.ExecuteNonQuery($"DELETE FROM Chunks WHERE updateItemId={updateItem.Id}");
    }
  }
}
