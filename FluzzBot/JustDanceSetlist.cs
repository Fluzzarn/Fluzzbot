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

        public string NextSong(string username)
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
            string queury = "SET SQL_SAFE_UPDATES = 0;DELETE FROM justdance_setlist WHERE song_name LIKE @current_song AND user_id LIKE(SELECT Usernames.user_id FROM Usernames WHERE Usernames.username like @username)";
            MySQLHelper.RunSQLRequest(queury, new Dictionary<string, string>() { { "@current_song", current_song }, { "@username", username } });

            return current_song;
        }

        public void LoadSetlistFromDatabase(string username)
        {
            string queury = "SELECT * FROM justdance_setlist WHERE user_id LIKE(SELECT Usernames.user_id FROM Usernames WHERE Usernames.username like @username)";


            MySqlDataReader dataReader = MySQLHelper.GetSQLDataFromDatabase(queury, new Dictionary<string, string>() { { "@username", username } });
            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {
                    Songs.Add((string)dataReader["song_name"]);
                    Console.WriteLine("Adding {0} to just dance setlist", (string)dataReader["song_name"]);
                }
                dataReader.Close();
            }

        }

        public List<string> GetSetlist()
        {
            return Songs;
        }

        public bool RemoveSong(string name,string username)
        {
            if (Songs.Contains(name))
            {
                Songs.RemoveAll((x) => x == name);
                string queury = "SET SQL_SAFE_UPDATES = 0;DELETE FROM justdance_setlist WHERE song_name LIKE @name AND user_id LIKE(SELECT Usernames.user_id FROM Usernames WHERE Usernames.username like @username)";
                return MySQLHelper.RunSQLRequest(queury, new Dictionary<string, string>() { { "@name", name }, { "@username", username } })>= 1;
            }
            return false;
        }
    }
}
