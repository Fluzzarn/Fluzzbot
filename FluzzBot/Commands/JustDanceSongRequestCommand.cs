using FluzzBot;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluzzBotCore
{
    class JustDanceSongRequestCommand : ICommand
    {
        private string _commandName = "!jdRequest";
        public string CommandName { get => _commandName; set => throw new NotImplementedException(); }
        public bool RequireMod { get => true; set => throw new NotImplementedException(); }
        public bool HasCooldown { get => false; set => throw new NotImplementedException(); }
        public int Cooldown { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool Execute(FluzzBot.FluzzBot bot, string message)
        {
            Console.WriteLine("Starting JustDanceSongRequest Command");
            string song = message.Substring(message.IndexOf(' ') + 1);
            bot.JustDanceSetlist.AddSong(song);

            MySql.Data.MySqlClient.MySqlConnection conn;
            var connString = String.Format("server={0};uid={1};pwd={2};database={3};SslMode=None", DatabaseCredentials.DatabaseHost, DatabaseCredentials.DatabaseUsername, DatabaseCredentials.DatabasePassword, DatabaseCredentials.DatabaseName);
                conn = new MySql.Data.MySqlClient.MySqlConnection();
                conn.ConnectionString = connString;
                conn.Open();


                string setListUpdate = "INSERT INTO justdance_setlist (user_id,song_name) SELECT Usernames.user_id,'" + song + "' FROM Usernames WHERE username LIKE '" + bot.Credentials.ChannelName + "'";

            Console.WriteLine(setListUpdate);
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = setListUpdate;
            cmd.Connection = conn;


            cmd.ExecuteNonQuery();
            bot.ConstructAndEnqueueMessage(song + " has been added to the queue");
            conn.Close();
            return true;
        }
    }
}
