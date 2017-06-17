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
        FluzzBot bot;

        public Setlist(FluzzBot b)
        {
            bot = b;
            SongSetlist = new List<Song>();
        }
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


        public Song NextSong(string username)
        {
            string justFinised = _CurrentSong.Name;
            string queury = "SET SQL_SAFE_UPDATES = 0;DELETE FROM current_setlist WHERE song_id LIKE(SELECT id FROM Songs WHERE Songs.title LIKE @justFinished) AND user_id LIKE(SELECT Usernames.user_id FROM Usernames WHERE Usernames.username like @username)";

            MySQLHelper.RunSQLRequest(queury, new Dictionary<string, string>() { { "@justFinished", justFinised }, { "@username", username } });
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
                catch (NullReferenceException)
                {

                    Console.WriteLine(s.Name + " did not have a duration!");
                }

            }

            return ts;
        }

        internal void LoadSetlistFromDatabase(string username)
        {

            string queury = "SELECT * FROM Songs s WHERE s.id IN (SELECT song_id FROM current_setlist WHERE user_id LIKE(SELECT Usernames.user_id FROM Usernames WHERE Usernames.username like @username))";

            //MySqlDataReader dataReader = MySQLHelper.GetSQLDataFromDatabase(queury, new Dictionary<string, string>() { { "@username", username } });

            //if (dataReader.HasRows)
            //{
            //    while (dataReader.Read())
            //    {
            //        Song s = new Song()
            //        {
            //            Name = (string)(dataReader["title"]),
            //            Guitar = (int)dataReader["guitar"],
            //            Bass = (int)dataReader["bass"],
            //            Drums = (int)dataReader["drums"],
            //            Vocals = (int)dataReader["vocals"],
            //            Artist = (string)dataReader["artist"],
            //            BPM = int.Parse((string)dataReader["bpm"]),
            //            Gender = ((string)dataReader["gender"])[0],
            //            Genre = (string)dataReader["genre"],
            //            Released = DateTime.Parse((string)dataReader["released"]),
            //            VocalParts = (int)dataReader["vocalParts"],
            //            FreestyleGuitar = (string)dataReader["freestyleGuitar"] == "yes",
            //            FreestyleVocals = (string)dataReader["freestyleVocals"] == "yes",
            //            Duration = (int)TimeSpan.Parse((string)("00:" + dataReader["duration"])).TotalSeconds
            //        };
            //        SongSetlist.Add(s);
            //    }
            //    SongSetlist.Reverse();
            //    dataReader.Close();
            //}
        }

        internal List<Song> GetSetlist()
        {
            return SongSetlist;
        }
    }
}
