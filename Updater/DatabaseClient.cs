using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;


namespace YTY.amt
{
  public static class DatabaseClient
  {
    private static readonly SQLiteConnectionStringBuilder connectionStringBuilder = new SQLiteConnectionStringBuilder
    {
      Pooling = true,
      DataSource =Util.MakeQualifiedPath( CONFIGFILE),
      JournalMode = SQLiteJournalModeEnum.Persist,
    };
    private const string CONFIGFILE = "Updater.db";

    private static SQLiteConnection GetConnection()
    {
      return new SQLiteConnection(connectionStringBuilder.ConnectionString).OpenAndReturn();
    }

    private static SQLiteTransaction GetTransaction()
    {
      return GetConnection().BeginTransaction();
    }

    static DatabaseClient()
    {
      if (!System.IO.File.Exists(CONFIGFILE))
      {
        InitializeDatabase();
      }
    }

    private static void InitializeDatabase()
    {
      using (var connection = GetConnection())
      {
        connection.Execute(@"
CREATE TABLE IF NOT EXISTS Meta(
Build INTEGER NOT NULL);
CREATE TABLE IF NOT EXISTS File(
Id INTEGER PRIMARY KEY,
SourceUri TEXT NOT NULL,
FileName TEXT NOT NULL,
Status INTEGER NOT NULL,
Size INTEGER NOT NULL,
Version INTEGER NOT NULL,
Md5 TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS Chunk(
FileId INTEGER NOT NULL,
Id INTEGER NOT NULL,
Status INTEGER NOT NULL,
PRIMARY KEY(FileId,Id));
INSERT INTO Meta(Build) VALUES(0);");
      }
    }

    public static int Build
    {
      get { return GetConnection().QueryFirstOrDefault<int>("SELECT Build FROM Meta"); }
      set
      {
        using (var transaction = GetTransaction())
        {
          transaction.Connection.Execute("UPDATE Meta SET Build=@Build", new { Build = value }, transaction);
          transaction.Commit();
        }
      }
    }

    public static IEnumerable<FileDto> GetFiles()
    {
      return GetConnection().Query<FileDto>("SELECT Id,SourceUri,FileName,Status,Size,Version,Md5 FROM File");
    }

    public static void SaveFiles(IEnumerable<FileDto> dtos)
    {
      using (var transaction = GetTransaction())
      {
        transaction.Connection.Execute("INSERT OR REPLACE INTO File(Id,SourceUri,FileName,Status,Size,Version,Md5) VALUES(@Id,@SourceUri,@FileName,@Status,@Size,@Version,@Md5)", dtos, transaction);
        transaction.Commit();
      }
    }

    public static IEnumerable<ChunkModel> GetChunks(int fileId)
    {
      return GetConnection().Query<ChunkModel>("SELECT Id,Status FROM Chunk WHERE FileId=@fileId ORDER BY Id", new { fileId });
    }

    public static void SaveChunks(IEnumerable<ChunkModel> chunks)
    {
      using (var transaction = GetTransaction())
      {
        transaction.Connection.Execute("INSERT OR REPLACE INTO Chunk(FileId,Id,Status) VALUES(@FileId,@Id,@Status)", chunks, transaction);
        transaction.Commit();
      }
    }

    public static void DeleteChunks(IEnumerable<ChunkModel> chunks)
    {
      using (var transaction = GetTransaction())
      {
        transaction.Connection.Execute("DELETE FROM Chunk WHERE FileId=@FileId AND Id=@Id", chunks, transaction);
        transaction.Commit();
      }
    }

    public static void UpdateChunk(ChunkModel chunk)
    {
      using (var transaction = GetTransaction())
      {
        transaction.Connection.Execute("UPDATE Chunk SET Status=@Status WHERE FileId=@FileId AND Id=@Id", chunk, transaction);
        transaction.Commit();
      }
    }

    public static void UpdateFileStatus(FileDto dto)
    {
      using (var transaction = GetTransaction())
      {
        transaction.Connection.Execute("UPDATE File SET Status=@Status WHERE Id=@Id", dto, transaction);
        transaction.Commit();
      }
    }
  }
}
