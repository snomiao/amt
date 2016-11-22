using System;
using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;


namespace YTY.amt
{
  public class MySqlOp
  {
    private string connectionString;

    public MySqlOp(IPEndPoint server, string userId, string password, string database)
    {
      var builder = new MySqlConnectionStringBuilder();
      builder.Server = server.Address.ToString();
      builder.Port = (uint)server.Port;
      builder.UserID = userId;
      builder.Password = password;
      builder.Database = database;
      connectionString = builder.ConnectionString;
    }

    public Task<T> ExecuteScalar<T>(string sql)
    {
      return Task.Run(() =>
      {
        using (var connection = new MySqlConnection(connectionString))
        {
          connection.Open();
          using (var command = new MySqlCommand(sql, connection))
          {
            return (T)command.ExecuteScalar();
          }
        }
      });
    }

    public Task<MySqlDataReader> ExecuteReaderAsync(string sql)
    {
      return Task.Run(() =>
      {
        var connection = new MySqlConnection(connectionString);
        connection.Open();
        using (var command = new MySqlCommand(sql, connection))
        {
          return command.ExecuteReader();
        }
      });
    }

    public Task<int> ExecuteNonQueryAsync(string sql)
    {
      return Task.Run(() =>
      {
        using (var connection = new MySqlConnection(connectionString))
        {
          connection.Open();
          using (var command = new MySqlCommand(sql, connection))
          {
            return command.ExecuteNonQuery();
          }
        }
      });
    }

    public Task ExecuteNonQueryTransaction(IEnumerable<string> sqls)
    {
      return Task.Run(() =>
      {
        using (var connection = new MySqlConnection(connectionString))
        {
          connection.Open();
          using (var transaction = connection.BeginTransaction())
          {
            using (var command = new MySqlCommand())
            {
              command.Connection = connection;
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
      });
    }
  }
}
