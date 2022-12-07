using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SFAA.Entities;

namespace SFAA.DBAdapter
{
    public class DBAdapterLocalDB
    {
        /// <summary>
        /// Данный метод ищет в локальной базе данные авторизации
        /// </summary>
        /// <param name="apikey"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetGalAuthDataByApiKey(string apikey)
        {
            using (var context = new PgSqlDbContext())
            {
                var query =
                    "SELECT * FROM public.\"ApiUsers\"" + $"WHERE apikey = '{apikey}'" + " LIMIT 1 ";

                var reader = context.ExecuteQuery(query);

                if (reader.HasRows == true)
                {
                    var authData = new Dictionary<string, string>();
                    
                    while (reader.Read())
                    {
                        authData.Add(reader.GetString(reader.GetOrdinal("gallogin")), reader.GetString(reader.GetOrdinal("galpass")));
                       
                        GlobalSettings.Instance.CurrentApiUser = new ApiUsers()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            GalStatus = reader.GetInt32(reader.GetOrdinal("galstatus")),
                        };
                    }


                    return authData;
                }
                else
                {
                    reader.Dispose();

                    return null;

                }
            }
        }

        /// <summary>
        /// Данный метод вставляет в таблицу доступа nrec ведомостей, с которыми можно работать пользователю. Предварительно происходит удаление предществующих записей.
        /// </summary>
        /// <param name="actionDataApiKey"></param>
        /// <param name="listAllNrec"></param>
        /// <returns></returns>
        public bool InsertListNrecIntoAccessTable(string apikey, List<string> listAllNrec)
        {
            var result = false;
            using (var context = new PgSqlDbContext())
            {
                var query =
                    "SELECT * FROM public.\"ApiUsers\"" + $"WHERE apikey = '{apikey}'" + " LIMIT 1 ";

                var reader = context.ExecuteQuery(query);

                if (reader.HasRows != true) return result;
                var id = 0;

                while (reader.Read())
                {
                    id = reader.GetInt32(reader.GetOrdinal("id"));
                }

                reader.Dispose();

                if (id == 0) return result;

                query = $"DELETE FROM public.\"ApiUsersAccessUList\" WHERE \"apiUsersId\" = {id}";

                context.ExecuteQuery(query);
                
                reader.Dispose();
                var res = 0;
                foreach (var oneRec in listAllNrec)
                {
                    query = $"INSERT INTO public.\"ApiUsersAccessUList\" (id, \"apiUsersId\", \"nrecList\") VALUES (default, {id}, '{oneRec}')";
                    context.ExecuteQuery(query);
                    reader.Dispose();
                    res++;
                }

                if (res == 0) return result;
                
                result = true;


            }

            return result;
        }

        /// <summary>
        /// Данный метод проверяет, возможно ли любые манипуляции с ведомостью.
        /// </summary>
        /// <param name="apikey">Ключ пользователя</param>
        /// <param name="listNrec">Nrec ведомости</param>
        /// <returns></returns>
        public bool CheckAccessToWorkWithList(string apikey, string listNrec)
        {
            var result = false;
            using (var context = new PgSqlDbContext())
            {
                var query =
                    "SELECT * FROM public.\"ApiUsers\"" + $"WHERE \"apikey\" = '{apikey}'" + " LIMIT 1 ";
                var reader = context.ExecuteQuery(query);

                if (reader.HasRows != true) return result;
                var id = 0;

                while (reader.Read())
                {
                    id = reader.GetInt32(reader.GetOrdinal("id"));
                }
                reader.Dispose();

                if (id == 0) return result;

                query = "SELECT id " + "FROM public.\"ApiUsersAccessUList\" " + $"WHERE \"apiUsersId\" = {id} and \"nrecList\" = '{listNrec}'" + " LIMIT 1 ";
                reader = context.ExecuteQuery(query);

                if (reader.HasRows != true) return result;
                long temp = 0;
                while (reader.Read())
                {
                    temp = reader.GetInt64(reader.GetOrdinal("id"));
                }

                if (temp == 0) return result;
                
                result = true;


            }

            return result;
        }

        /// <summary>
        /// Данный метод вставляет в таблицу лога информацию о подключении.
        /// </summary>
        /// <param name="actionDataApiKey"></param>
        /// <param name="listAllNrec"></param>
        /// <returns></returns>
        public bool InsertInfoConnectnioIntoLogConnection(int type)
        {
            var result = false;
            using (var context = new PgSqlDbContext())
            {
                var query =
                    $"INSERT INTO public.\"LogConnections\" (id, \"ApiUsersId\", \"type\", \"dateTime\") VALUES (default, {GlobalSettings.Instance.CurrentApiUser.Id}, '{type}', '{DateTime.Now:yyyy-MM-dd HH:mm:ss}')";

                var reader = context.ExecuteQuery(query);

                reader.Dispose();
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Данный метод выполняет блокировку api key
        /// </summary>
        /// <param name="actionDataQueryParamNum1"></param>
        /// <returns></returns>
        public bool BlockApiKey(string actionDataQueryParamNum1)
        {
            var result = false;
            using (var context = new PgSqlDbContext())
            {
                var query =
                    $"UPDATE public.\"ApiUsers\" SET \"galstatus\" = 0 WHERE \"apikey\" = '{actionDataQueryParamNum1}'";

                var reader = context.ExecuteQuery(query);
                result = reader.RecordsAffected >= 1;

                reader.Dispose();
                
            }

            return result;
        }

        /// <summary>
        /// Данный метод создает пользователя
        /// </summary>
        /// <param name="actionDataQueryParamNum1"></param>
        /// <returns></returns>
        public string CreateUserByLoginIntoDb(string actionDataQueryParamNum1)
        {
            var login = string.Concat("OMGTU910#", actionDataQueryParamNum1);
            using (var context = new PgSqlDbContext())
            {
                var query =
                    "SELECT * FROM public.\"ApiUsers\"" + $"WHERE \"gallogin\" = '{login}'" + " LIMIT 1 ";
                var reader = context.ExecuteQuery(query);
                if (reader.HasRows == true) return string.Empty;

                var key = new byte[36];

                using (var generator = RandomNumberGenerator.Create())
                    generator.GetBytes(key);

                string apiKey = Convert.ToBase64String(key);

                apiKey.Replace('\\', '1');

                reader.Dispose();
                query =
                    $"INSERT INTO public.\"ApiUsers\" (id, \"apikey\", \"gallogin\", \"galpass\") VALUES (default, '{apiKey}', '{login}', 'covid-19')";

                reader = context.ExecuteQuery(query);

                if (reader.RecordsAffected == 1)
                {
                    reader.Dispose();
                    query =
                        "SELECT * FROM public.\"ApiUsers\"" + $"WHERE \"gallogin\" = '{login}'" + " LIMIT 1 ";
                    reader = context.ExecuteQuery(query);
                    if (reader.HasRows != true) return string.Empty;
                    while (reader.Read())
                    {
                        return reader.GetString(reader.GetOrdinal("apikey"));
                    }
                }
                else
                {
                    return string.Empty;
                }

            }

            return string.Empty;
        }
    }
}
