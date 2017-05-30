using FluzzBot;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluzzBot
{
    internal class JustDanceSetlist
    {
        List<string> Songs = new List<string>();
        String current_song;
        FluzzBot bot;


        public JustDanceSetlist(FluzzBot b)
        {
            bot = b;
        }

        public void AddSong(string songName)
        {
            Songs.Add(songName);
        }

        public string NextSong()
        {
            if(Songs.Count == 1)
            {
                current_song = Songs[0];
                Songs.RemoveRange(0, 1);
            }
            else
            if (Songs.Count >= 2)
            {
                Songs.RemoveRange(0, 1);
                current_song = Songs[0];
            }
            else
            {
                if (Songs.Count == 1)
                    Songs.RemoveRange(0, 1);
                current_song = null;

            }

            var connString = String.Format("server={0};uid={1};pwd={2};database={3};SslMode=None", DatabaseCredentials.DatabaseHost, DatabaseCredentials.DatabaseUsername, DatabaseCredentials.DatabasePassword, DatabaseCredentials.DatabaseName);
            var conn = new MySql.Data.MySqlClient.MySqlConnection();
            conn.ConnectionString = connString;
            conn.Open();
            string queury = "SET SQL_SAFE_UPDATES = 0;DELETE FROM justdance_setlist WHERE song_name LIKE'" + current_song + "' AND user_id LIKE(SELECT Usernames.user_id FROM Usernames WHERE Usernames.username like '" + bot.Credentials.ChannelName + "')";

            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = queury;
            cmd.Connection = conn;


            cmd.ExecuteNonQuery();
            conn.Close();
            return current_song;
        }

        public void LoadSetlistFromDatabase()
        {
            var conn = new MySql.Data.MySqlClient.MySqlConnection();
            var connString = String.Format("server={0};uid={1};pwd={2};database={3};SslMode=None", DatabaseCredentials.DatabaseHost, DatabaseCredentials.DatabaseUsername, DatabaseCredentials.DatabasePassword, DatabaseCredentials.DatabaseName);
            conn.ConnectionString = connString;
            conn.Open();
            string queury = "SELECT * FROM justdance_setlist WHERE user_id LIKE(SELECT Usernames.user_id FROM Usernames WHERE Usernames.username like '" + bot.Credentials.ChannelName + "')";
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = queury;
            cmd.Connection = conn;

            MySqlDataReader dataReader = cmd.ExecuteReader();

            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {
                    Songs.Add((string)dataReader["song_name"]);

                }
                Songs.Reverse();
                dataReader.Close();
            }

            conn.Close();
        }

        public List<string> GetSetlist()
        {
            return Songs;
        }

        public bool RemoveSong(string name)
        {
            if (Songs.Contains(name))
            {
                Songs.RemoveAll((x) => x == name);
                var connString = String.Format("server={0};uid={1};pwd={2};database={3};SslMode=None", DatabaseCredentials.DatabaseHost, DatabaseCredentials.DatabaseUsername, DatabaseCredentials.DatabasePassword, DatabaseCredentials.DatabaseName);
                var conn = new MySql.Data.MySqlClient.MySqlConnection();
                conn.ConnectionString = connString;
                conn.Open();
                string queury = "SET SQL_SAFE_UPDATES = 0;DELETE FROM justdance_setlist WHERE song_name LIKE'" + name + "' AND user_id LIKE(SELECT Usernames.user_id FROM Usernames WHERE Usernames.username like '" + bot.Credentials.ChannelName + "')";
                MySqlCommand cmd = new MySqlCommand();
                cmd.CommandText = queury;
                cmd.Connection = conn;


               int rows = cmd.ExecuteNonQuery();
                conn.Close();


                return rows>=1;
            }
            return false;
        }
    }
}
