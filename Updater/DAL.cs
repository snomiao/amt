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

    public async Task CreateUpdateItem(UpdateItemViewModel updateItem)
    {
      await sqlite.ExecuteNonQueryAsync($"INSERT INTO Files(id,sourceUri,fileName,status) VALUES({updateItem.Id},'{updateItem.SourceUri}','{updateItem.FileName}',{(int)updateItem.Status})").ConfigureAwait(false);
    }

    public async Task SetUpdateItemSize(UpdateItemViewModel updateItem)
    {
      await sqlite.ExecuteNonQueryAsync($"UPDATE Files SET size={updateItem.Size} WHERE id={updateItem.Id}").ConfigureAwait(false);
    }

    public async Task SetUpdateItemStatus(UpdateItemViewModel updateItem)
    {
      await sqlite.ExecuteNonQueryAsync($"UPDATE Files SET status={(int)updateItem.Status} WHERE id={updateItem.Id}").ConfigureAwait(false);
    }

    public async Task SetUpdateItemVersion(UpdateItemViewModel updateItem)
    {
      await sqlite.ExecuteNonQueryAsync($"UPDATE Files SET version='{updateItem.Version}' WHERE id={updateItem.Id}").ConfigureAwait(false);
    }

    public async Task SetUpdateItemMD5(UpdateItemViewModel updateItem)
    {
      await sqlite.ExecuteNonQueryAsync($"UPDATE Files SET md5='{updateItem.MD5}' WHERE id={updateItem.Id}").ConfigureAwait(false);
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

    public async Task SaveUpdateItems(IEnumerable<UpdateItemViewModel> updateItems)
    {
      await sqlite.ExecuteNonQueryTransactionAsync(updateItems.Select(item => $"INSERT INTO Files(id,sourceUri,fileName,status,size,version,md5) VALUES({item.Id},'{item.SourceUri.Replace("'","''")}','{item.FileName.Replace("'", "''")}',{(int)item.Status},{item.Size},'{item.Version}','{item.MD5}')")).ConfigureAwait(false);
    }

    public async Task SaveChunks(IEnumerable<ChunkViewModel> chunks)
    {
      await sqlite.ExecuteNonQueryTransactionAsync(chunks.Select(item => $"INSERT INTO Chunks(updateItemId,idx,status) VALUES({item.UpdateItemId},{item.Index},{(int)item.Status})")).ConfigureAwait(false);
    }

    public IEnumerable<ChunkViewModel> GetChunks(int updateItemId)
    {
      return sqlite.GetDataTable($"SELECT idx,status FROM Chunks WHERE updateItemId={updateItemId}").AsEnumerable().ToList().
        Select(row => new ChunkViewModel(
          updateItemId: updateItemId,
          index: row.Field<int>("idx"),
          status: row.Field<DownloadChunkStatus>("status")));
    }

    public async Task SetChunkStatus(ChunkViewModel chunk, DownloadChunkStatus status)
    {
      await sqlite.ExecuteNonQueryAsync($"UPDATE Chunks SET status={(int)status} WHERE updateItemId={chunk.UpdateItemId} AND idx={chunk.Index}").ConfigureAwait(false);
    }

    public async Task CreateChunk(ChunkViewModel chunk)
    {
      await sqlite.ExecuteNonQueryAsync($"INSERT INTO Chunks(updateItemId,idx,status) VALUES({chunk.UpdateItemId},{chunk.Index},{(int)chunk.Status})").ConfigureAwait(false);
    }

    public async Task DeleteChunks(UpdateItemViewModel updateItem)
    {
      await sqlite.ExecuteNonQueryAsync($"DELETE FROM Chunks WHERE updateItemId={updateItem.Id}").ConfigureAwait(false);
    }
  }
}
