using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluzzBot
{
    public class Setlist
    {

        List<Song> SongSetlist;
        Song _CurrentSong;



        public Setlist()
        {
            SongSetlist = new List<Song>();
        }
        public bool AddSongToSetlist(Song s)
        {
            try
            {
                SongSetlist.Add(s);
                if (SongSetlist.Count == 1)
                    _CurrentSong = s;
            }
            catch (Exception)
            {

                return false;
            }
            return true;
        }

        public Song CurrentSong()
        {
            return _CurrentSong;
        }


        public Song NextSong()
        {
            string justFinised = _CurrentSong.Name;

           var conn = new MySql.Data.MySqlClient.MySqlConnection();
            var connString = String.Format("server={0};uid={1};pwd={2};database={3}", Credentials.DatabaseHost, Credentials.DatabaseUsername, Credentials.DatabasePassword, Credentials.DatabaseName);
            conn.ConnectionString = connString;
            conn.Open();
            string queury = "SET SQL_SAFE_UPDATES = 0;DELETE FROM current_setlist WHERE song_id LIKE(SELECT id FROM Songs WHERE Songs.title LIKE '" + justFinised + "') AND user_id LIKE(SELECT Usernames.user_id FROM Usernames WHERE Usernames.username like '" + Credentials.ChannelName + "')";

            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = queury;
            cmd.Connection = conn;


            cmd.BeginExecuteNonQuery();
            if (SongSetlist.Count >= 2)
                {
                    SongSetlist.RemoveRange(0, 1);
                    _CurrentSong = SongSetlist[0];
                }
                else
                {
                    if (SongSetlist.Count == 1)
                        SongSetlist.RemoveRange(0, 1);
                    _CurrentSong = null;

                }
                return _CurrentSong;
            

        }

        public TimeSpan SetlistLength()
        {
            TimeSpan ts = new TimeSpan();

            foreach (Song s in SongSetlist)
            {
                try
                {
                    ts = ts.Add(new TimeSpan(0, 0, s.Duration));
                }
                catch (NullReferenceException ex)
                {

                    Console.WriteLine(s.Name + " did not have a duration!");
                }
                
            }

            return ts;
        }

        internal void LoadSetlistFromDatabase()
        {
            var conn = new MySql.Data.MySqlClient.MySqlConnection();
            var connString = String.Format("server={0};uid={1};pwd={2};database={3}", Credentials.DatabaseHost, Credentials.DatabaseUsername, Credentials.DatabasePassword, Credentials.DatabaseName);
            conn.ConnectionString = connString;
            conn.Open();
            string queury = "SELECT * FROM Songs s WHERE s.id IN (SELECT song_id FROM current_setlist WHERE user_id LIKE(SELECT Usernames.user_id FROM Usernames WHERE Usernames.username like '" + Credentials.ChannelName + "'))";
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = queury;
            cmd.Connection = conn;

            MySqlDataReader dataReader = cmd.ExecuteReader();

            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {
                    Song s = new Song();
                    s.Name = (string)(dataReader["title"]);
                    s.Guitar = (int)dataReader["guitar"];
                    s.Bass = (int)dataReader["bass"];
                    s.Drums = (int)dataReader["drums"];
                    s.Vocals = (int)dataReader["vocals"];
                    s.Artist = (string)dataReader["artist"];
                    s.BPM = int.Parse((string)dataReader["bpm"]);
                    s.Gender = ((string)dataReader["gender"])[0];
                    s.Genre = (string)dataReader["genre"];
                    s.Released = DateTime.Parse((string)dataReader["released"]);
                    s.VocalParts = (int)dataReader["vocalParts"];
                    s.FreestyleGuitar = (string)dataReader["freestyleGuitar"] == "yes";
                    s.FreestyleVocals = (string)dataReader["freestyleVocals"] == "yes";
                    s.Duration = (int)TimeSpan.Parse((string)("00:" + dataReader["duration"])).TotalSeconds;
                    SongSetlist.Add(s);
                }
                SongSetlist.Reverse();
                dataReader.Close();
            }

            conn.Close();
        }
    }
}
