using System;
using System.Data.SQLite;
using System.Data;
using System.Collections.Generic;

namespace YTY.amt
{
  public class SqliteOp : IDisposable
  {
    private string database;
    private SQLiteConnection connection;

    public SqliteOp(string database)
    {
      this.database = database;
      connection = new SQLiteConnection(new SQLiteConnectionStringBuilder() { DataSource = database }.ConnectionString);
      connection.Open();
    }

    public T ExecuteScalar<T>(string sql)
    {
      using (var command = new SQLiteCommand(sql, connection))
      {
        return (T)command.ExecuteScalar();
      }
    }

    public DataTable GetDataTable(string sql)
    {
      using (var adapter = new SQLiteDataAdapter(sql, connection))
      {
        var dataTable = new DataTable();
        adapter.Fill(dataTable);
        return dataTable;
      }
    }

    public int ExecuteNonQuery(string sql)
    {
      using (var command = new SQLiteCommand(sql, connection))
      {
        return command.ExecuteNonQuery();
      }
    }

    public void ExecuteNonQueryTransaction(IEnumerable<string> sqls)
    {
      using (var transaction = connection.BeginTransaction())
      {
        using (var command = new SQLiteCommand())
        {
          command.Transaction = transaction;
          foreach(var sql in sqls )
          {
            command.CommandText = sql;
            command.ExecuteNonQuery();
          }
        }
        transaction.Commit();
      }
    }

    private bool disposedValue = false;

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          connection.Close();
        }
        disposedValue = true;
      }
    }

    public void Dispose()
    {
      Dispose(true);
    }
  }
}
