using FluzzBot;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluzzBotCore
{
    class JustDanceNextSongCommand : Command
    {
        public string CommandName { get => "!jdNext"; set => throw new NotImplementedException(); }
        public bool RequireMod { get => true; set => throw new NotImplementedException(); }

        public bool Execute(FluzzBot.FluzzBot bot, string message)
        {
            string song = bot.JustDanceSetlist.NextSong();

            if (song == null)
                bot.ConstructAndEnqueueMessage("Setlist is empty!");
            else
                bot.ConstructAndEnqueueMessage("Next song is " + song);
            return true;
        }
    }
}
