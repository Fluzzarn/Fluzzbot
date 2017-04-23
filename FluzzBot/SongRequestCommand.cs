using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluzzBot
{
    class SongRequestCommand : Command
    {
        private string _commandName = "!request";
        public string CommandName { get => _commandName; set => throw new NotImplementedException(); }

        public bool Execute(FluzzBot bot,string message)
        {
            string song = message.Substring(message.IndexOf(' ') + 1);
            Song dbSong= GetSongFromDatabase(song);

            if(dbSong != null)
            {
                bot.SongList.AddSongToSetlist(dbSong);
                bot.ConstructAndEnqueueMessage(dbSong.Name + " has been added to the setlist");
            }
            else
            {
                bot.ConstructAndEnqueueMessage("Could not find the song " + song + " in the database");
            }
            return true;
        }

        private Song GetSongFromDatabase(string message)
        {
            Song s = null;
            MySql.Data.MySqlClient.MySqlConnection conn;
            var connString = String.Format("server={0};uid={1};pwd={2};database={3}",Credentials.DatabaseHost,Credentials.DatabaseUsername,Credentials.DatabasePassword,Credentials.DatabaseName);

            try
            {
                conn = new MySql.Data.MySqlClient.MySqlConnection();
                conn.ConnectionString = connString;
                conn.Open();

                string queury = "SELECT * FROM Songs WHERE title LIKE '" + message + "'";


                Console.WriteLine(queury);

                MySqlCommand cmd = new MySqlCommand();
                cmd.CommandText = queury;
                cmd.Connection = conn;


                MySqlDataReader dataReader = cmd.ExecuteReader();
                
                while (dataReader.Read())
                {
                    s = new Song();
                    s.Name = (string)(dataReader["title"]);
                    s.Guitar = (int)dataReader["guitar"];
                    s.Bass = (int)dataReader["bass"];
                    s.Drums = (int)dataReader["drums"];
                    s.Vocals = (int)dataReader["vocals"];
                    s.Artist = (string)dataReader["artist"];
                    s.BPM = int.Parse((string)dataReader["bpm"]);
                    s.Gender = ((string)dataReader["gender"])[0];
                    s.Genre = (string)dataReader["genre"];
                    s.Released = DateTime.Parse( (string)dataReader["released"]);
                    s.VocalParts = (int)dataReader["vocalParts"];
                    s.FreestyleGuitar = (string)dataReader["freestyleGuitar"] == "yes" ;
                    s.FreestyleVocals = (string)dataReader["freestyleVocals"] == "yes";
                    s.Duration = (int)TimeSpan.Parse((string)("00:" + dataReader["duration"])).TotalSeconds;
                }
                conn.Close();
            }
            catch (Exception ex)
            {

                throw ex;
            }
            Console.WriteLine(message);
            
            return s;
        }
    }
}
