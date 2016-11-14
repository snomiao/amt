using System;
using System.Data.SQLite;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YTY
{
  public class SqliteOp
  {
    private string database;
    private string connectionString;

    public SqliteOp(string database)
    {
      this.database = database;
      connectionString = new SQLiteConnectionStringBuilder() { DataSource = database }.ConnectionString;
    }

    public T ExecuteScalar<T>(string sql)
    {
      using (var connection = new SQLiteConnection(connectionString))
      {
        connection.Open();
        using (var command = new SQLiteCommand(sql, connection))
        {
          return (T)command.ExecuteScalar();
        }
      }
    }

    public SQLiteDataReader ExecuteReader(string sql)
    {
      var connection = new SQLiteConnection(connectionString);
      connection.Open();
      using (var command = new SQLiteCommand(sql, connection))
      {
        return command.ExecuteReader();
      }
    }

    public DataTable GetDataTable(string sql)
    {
      using (var connection = new SQLiteConnection(connectionString))
      {
        connection.Open();
        using (var adapter = new SQLiteDataAdapter(sql, connection))
        {
          var dataTable = new DataTable();
          adapter.Fill(dataTable);
          return dataTable;
        }
      }
    }

    public int ExecuteNonQuery(string sql)
    {
      using (var connection = new SQLiteConnection(connectionString))
      {
        connection.Open();
        using (var command = new SQLiteCommand(sql, connection))
        {
          return command.ExecuteNonQuery();
        }
      }
    }

    public void ExecuteNonQueryTransaction(IEnumerable<string> sqls)
    {
      using (var connection = new SQLiteConnection(connectionString))
      {
        connection.Open();
        using (var transaction = connection.BeginTransaction())
        {
          using (var command = new SQLiteCommand())
          {
            command.Transaction = transaction;
            foreach (var sql in sqls)
            {
              command.CommandText = sql;
              command.ExecuteNonQuery();
            }
          }
          transaction.Commit();
        }
      }
    }
  }
}
