using FluzzBot;
using FluzzBot.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluzzBotCore
{
    class JustDanceNextSongCommand : Command, ICommand
    {
        public string CommandName { get => "!jdNext"; set => throw new NotImplementedException(); }
        public bool RequireMod { get => true; set => throw new NotImplementedException(); }
        public bool HasCooldown { get => false; set => throw new NotImplementedException(); }
        public int Cooldown { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool Execute(FluzzBot.FluzzBot bot, string message,string username)
        {
            string song = bot.JustDanceDict[username].NextSong(username);

            if (song == null)
                bot.ConstructAndEnqueueMessage("Setlist is empty!",username);
            else
                bot.ConstructAndEnqueueMessage("Next song is " + song,username);
            return true;
        }
    }
}
