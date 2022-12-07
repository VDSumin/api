using System;
using System.Data.Common;
using Npgsql;
using System.IO;
using SFAA.Entities;

namespace SFAA.DBAdapter
{
    public class PgSqlDbContext : IDisposable
    {
        private readonly NpgsqlConnection _connection;

        private NpgsqlDataReader _reader;

        private Object _scalar;

        private NpgsqlTransaction _transaction;
        
        public PgSqlDbContext()
        {
            _connection = new NpgsqlConnection();

            if (_connection == null)
            {
                throw new Exception("Ошибка PostgreSQL");
            }
            
            _connection.ConnectionString = string.Format("127.0.0.1");
            _connection.Open();
        }

        public void ExecuteNonQuery(string query, NpgsqlParameter[] parameters = null)
        {
            var command = new NpgsqlCommand() { CommandText = query, Connection = this._connection };
            if (parameters != null)
            {
                foreach (var npgsqlParameter in parameters)
                {
                    command.Parameters.Add(npgsqlParameter);
                }
            }
            command.ExecuteNonQuery();
        }

        public NpgsqlDataReader ExecuteQuery(string query, NpgsqlParameter[] parameters = null)
        {
            var command = new NpgsqlCommand { CommandText = query, Connection = this._connection, Transaction = this._transaction};

            if (parameters != null)
            {
                foreach (var npgsqlParameter in parameters)
                {
                    command.Parameters.Add(npgsqlParameter);
                }
            }
            
            this._reader = command.ExecuteReader();

            return _reader;
        }

        public Object ExecuteScalarQuery(string query)
        {
            var command = new NpgsqlCommand(query, _connection);
            this._scalar = command.ExecuteScalar();
            return this._scalar;
        }

        public void Dispose()
        {
            if (null != this._reader)
            {
                this._reader.Dispose();
            }
            _connection.Close();
        }
    }
}