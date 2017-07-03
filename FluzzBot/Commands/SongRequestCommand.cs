using FluzzBot.Commands;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluzzBot
{
    class SongRequestCommand : Command,ICommand
    {
        private string _commandName = "!rbRequest";
        public string CommandName { get => _commandName; set => throw new NotImplementedException(); }
        public bool RequireMod { get => true; set => throw new NotImplementedException(); }
        public bool HasCooldown { get => false; set => throw new NotImplementedException(); }
        public int Cooldown { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool Execute(FluzzBot bot,string message,string username)
        {
            string song = message.Substring(message.IndexOf(' ') + 1);
            Song dbSong= GetSongFromDatabase(song, bot,username);

            if(dbSong != null)
            {
                bot.SongList.AddSongToSetlist(dbSong);
                bot.ConstructAndEnqueueMessage(dbSong.Name + " has been added to the setlist",username);
            }
            else
            {
                bot.ConstructAndEnqueueMessage("Could not find the song " + song + " in the database or " + username + " does not own the song!",username);
            }
            return true;
        }

        private Song GetSongFromDatabase(string message, FluzzBot bot,string username)
        {
            Song s = null;
            try
            {

                MySql.Data.MySqlClient.MySqlConnection conn;
                MySqlDataReader dataReader= MySQLHelper.GetSQLDataFromDatabase("SELECT * FROM(User_Songs JOIN Usernames ON User_Songs.user_id = Usernames.user_id JOIN Songs ON User_Songs.song_id = Songs.id) WHERE title LIKE @message AND username LIKE @username", new Dictionary<string, string>() { {"@message",message },{"@username",username } },out conn);
                
                if(dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        s = new Song()
                        {
                            Name = (string)(dataReader["title"]),
                            Guitar = (int)dataReader["guitar"],
                            Bass = (int)dataReader["bass"],
                            Drums = (int)dataReader["drums"],
                            Vocals = (int)dataReader["vocals"],
                            Artist = (string)dataReader["artist"],
                            BPM = int.Parse((string)dataReader["bpm"]),
                            Gender = ((string)dataReader["gender"])[0],
                            Genre = (string)dataReader["genre"],
                            Released = DateTime.Parse((string)dataReader["released"]),
                            VocalParts = (int)dataReader["vocalParts"],
                            FreestyleGuitar = (string)dataReader["freestyleGuitar"] == "yes",
                            FreestyleVocals = (string)dataReader["freestyleVocals"] == "yes",
                            Duration = (int)TimeSpan.Parse((string)("00:" + dataReader["duration"])).TotalSeconds
                        };
                    }

                    dataReader.Close();
                    string setListUpdate = "INSERT INTO current_setlist (user_id,song_id) SELECT Usernames.user_id,Songs.id FROM(User_Songs JOIN Usernames ON User_Songs.user_id = Usernames.user_id JOIN Songs ON User_Songs.song_id = Songs.id) WHERE title LIKE '" + message + "' AND username LIKE '" + username + "'";

                    MySQLHelper.RunSQLRequest("INSERT INTO current_setlist (user_id,song_id) SELECT Usernames.user_id,Songs.id FROM(User_Songs JOIN Usernames ON User_Songs.user_id = Usernames.user_id JOIN Songs ON User_Songs.song_id = Songs.id) WHERE title LIKE @message AND username LIKE @username", new Dictionary<string, string> { { "@message", message }, { "@username", username } });

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
