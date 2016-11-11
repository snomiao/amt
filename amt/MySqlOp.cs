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

    public async Task<T> ExecuteScalar<T>(string sql)
    {
      using (var connection = new MySqlConnection(connectionString))
      {
        await connection.OpenAsync();
        using (var command = new MySqlCommand(sql, connection))
        {
          return (T)await command.ExecuteScalarAsync();
        }
      }
    }

    public async Task<DbDataReader> ExecuteReaderAsync(string sql)
    {
      var connection = new MySqlConnection(connectionString);
      await connection.OpenAsync();
      using (var command = new MySqlCommand(sql, connection))
      {
        return await command.ExecuteReaderAsync();
      }
    }

    public async Task<int> ExecuteNonQueryAsync(string sql)
    {
      using (var connection = new MySqlConnection(connectionString))
      {
        await connection.OpenAsync();
        using (var command = new MySqlCommand(sql, connection))
        {
          return await command.ExecuteNonQueryAsync();
        }
      }
    }

    public async Task ExecuteNonQueryTransaction(IEnumerable<string> sqls)
    {
      using (var connection = new MySqlConnection(connectionString))
      {
        await connection.OpenAsync();
        using (var transaction = connection.BeginTransaction())
        {
          using (var command = new MySqlCommand())
          {
            command.Connection = connection;
            command.Transaction = transaction;
            foreach (var sql in sqls)
            {
              command.CommandText = sql;
              await command.ExecuteNonQueryAsync();
            }
          }
          transaction.Commit();
        }
      }
    }
  }
}
