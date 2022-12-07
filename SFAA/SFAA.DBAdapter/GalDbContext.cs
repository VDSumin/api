using System;
using System.Data.SqlClient;
using System.IO;
using SFAA.Entities;

namespace SFAA.DBAdapter
{
    public class GalDbContext: IDisposable
    {
        private readonly SqlConnection _connection;

        private SqlDataReader _reader;

        private Object _scalar;

        public GalDbContext()
        {
            var st1 = @"Data source=";
            var st2 = GlobalSettings.Instance.DnsGal;
            var st3 = ";initial catalog=";
            var st4 = GlobalSettings.Instance.GalDb;
            var st5 = ";persist security info=True;user id=";
            var st6 = GlobalSettings.Instance.AuthDataGalLogin;
            var st7 = ";password=";
            var st8 = GlobalSettings.Instance.AuthDataGalPass;
            var st9 = ";MultipleActiveResultSets=True";
            
            _connection = new SqlConnection();

            this._connection.ConnectionString = string.Concat(st1, st2, st3, st4, st5, st6, st7, st8, st9); 
            this._connection.Open();
        }

        public void ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            var command = new SqlCommand { CommandText = query, Connection = this._connection };
            if (parameters != null)
            {
                foreach (var sqlParameter in parameters)
                {
                    command.Parameters.Add(sqlParameter);
                }
            }
            command.ExecuteNonQuery();
        }

        public SqlDataReader ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            //var transaction = this._connection.BeginTransaction();

            var command = new SqlCommand
            {
                CommandText = query, Connection = this._connection //, Transaction = transaction
            };


            if (parameters != null)
            {
                foreach (var sqlParameter in parameters)
                {
                    command.Parameters.Add(sqlParameter);
                }
            }

            try
            {
                this._reader = command.ExecuteReader();
               // transaction.Commit();
                return _reader;
            }
            catch (Exception exception)
            {
               // transaction.Rollback();
                Logger.Log.Error($"При попытке выполнить команду SQL возникла ошибка: {exception}");
                return null;
            }
            
        }

        public Object ExecuteScalarQuery(string query)
        {
            var command = new SqlCommand(query, _connection);
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