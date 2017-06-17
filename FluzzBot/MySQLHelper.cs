using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluzzBot
{
    static class MySQLHelper
    {
        
        public static MySqlDataReader GetSQLDataFromDatabase(string command, Dictionary<string,string> paramaters, out MySql.Data.MySqlClient.MySqlConnection conn)
        {
            string connString = String.Format("server={0};uid={1};pwd={2};database={3};SslMode=None", DatabaseCredentials.DatabaseHost, DatabaseCredentials.DatabaseUsername, DatabaseCredentials.DatabasePassword, DatabaseCredentials.DatabaseName);
            MySqlDataReader reader;
            conn = new MySql.Data.MySqlClient.MySqlConnection()
            {
                ConnectionString = connString
            };
            conn.Open();
            MySqlCommand cmd = new MySqlCommand()
            {
                CommandText = command,
                Connection = conn
            };
            foreach (var param in paramaters.ToList())
            {
                MySqlParameter sqlParam = new MySqlParameter()
                {
                    ParameterName = param.Key,
                    Value = param.Value
                };
                cmd.Parameters.Add(sqlParam);
            }

            Console.WriteLine(cmd.CommandText);

            reader = cmd.ExecuteReader();
            return reader ;
        }

        public static int RunSQLRequest(string command, Dictionary<string, string> paramaters)
        {
            string connString = String.Format("server={0};uid={1};pwd={2};database={3};SslMode=None", DatabaseCredentials.DatabaseHost, DatabaseCredentials.DatabaseUsername, DatabaseCredentials.DatabasePassword, DatabaseCredentials.DatabaseName);
            var conn = new MySql.Data.MySqlClient.MySqlConnection()
            {
                ConnectionString = connString
            };
            conn.Open();
            MySqlCommand cmd = new MySqlCommand()
            {
                CommandText = command,
                Connection = conn
            };
            foreach (var param in paramaters.ToList())
            {
                MySqlParameter sqlParam = new MySqlParameter()
                {
                    ParameterName = param.Key,
                    Value = param.Value
                };
                cmd.Parameters.Add(sqlParam);
            }
           int rows= cmd.ExecuteNonQuery();
            conn.Close();
            return rows;
        }

    }
}
