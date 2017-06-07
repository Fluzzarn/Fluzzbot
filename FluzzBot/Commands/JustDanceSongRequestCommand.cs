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

        public bool Execute(FluzzBot.FluzzBot bot, string message,string username)
        {
            Console.WriteLine("Starting JustDanceSongRequest Command");
            string song = message.Substring(message.IndexOf(' ') + 1);
            bot.JustDanceSetlist.AddSong(song);



                string setListUpdate = "INSERT INTO justdance_setlist (user_id,song_name) SELECT Usernames.user_id, @song FROM Usernames WHERE username LIKE @username";

            int rows =MySQLHelper.RunSQLRequest(setListUpdate, new Dictionary<string, string>() { { "@song", song }, { "@username", username } });

            bot.ConstructAndEnqueueMessage(song + " has been added to the queue",username);
            return true;
        }
    }
}
